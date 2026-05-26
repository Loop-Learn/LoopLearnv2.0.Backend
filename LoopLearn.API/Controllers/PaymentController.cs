using LoopLearn.Entities.DTOs;
using LoopLearn.Entities.DTOs.Payment;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;

		public PaymentController(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
		}

		private string GetUserId() =>
			User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException();

		// =============================================
		// POST /api/payment/checkout
		// بيعمل Stripe Checkout Session ويرجع الـ URL
		// =============================================
		[HttpPost("checkout")]
		[Authorize]
		public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDTO model)
		{
			try
			{
				var studentId = GetUserId();

				var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == model.CourseId, ignoreQueryFilters: true);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				if (course.Status != CourseStatus.Published)
					return BadRequest(new { success = false, message = "Course is not available." });

				if (course.IsFree)
					return BadRequest(new
					{
						success = false,
						message = "This course is free. Use the enrollment endpoint directly.",
						enrollEndpoint = $"/api/enrollment/courses/{model.CourseId}"
					});

				var alreadyEnrolled = await _unitOfWork.Enrollments
					.ExistsAsync(e => e.StudentId == studentId && e.CourseId == model.CourseId);

				if (alreadyEnrolled)
					return Conflict(new { success = false, message = "You are already enrolled in this course." });

				// إنشاء Stripe Session
				StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string> { "card" },
					LineItems = new List<SessionLineItemOptions>
					{
						new SessionLineItemOptions
						{
							PriceData = new SessionLineItemPriceDataOptions
							{
								Currency = "usd",
								UnitAmount = (long)(course.Price * 100), // Stripe بيشتغل بالسنت
                                ProductData = new SessionLineItemPriceDataProductDataOptions
								{
									Name = course.Title,
									Description = course.Subtitle,
									Images = course.ThumbnailUrl is not null
										? new List<string> { course.ThumbnailUrl }
										: null
								}
							},
							Quantity = 1
						}
					},
					Mode = "payment",
					SuccessUrl = _configuration["Stripe:SuccessUrl"],
					CancelUrl = _configuration["Stripe:CancelUrl"],
					Metadata = new Dictionary<string, string>
					{
						{ "studentId", studentId },
						{ "courseId", model.CourseId.ToString() }
					}
				};

				var service = new SessionService();
				var session = await service.CreateAsync(options);

				// سجل الـ Payment بـ Pending في الـ DB
				var payment = new Payment
				{
					StudentId = studentId,
					CourseId = model.CourseId,
					Amount = course.Price,
					Currency = "usd",
					StripeSessionId = session.Id,
					Status = PaymentStatus.Pending,
					CreatedAt = DateTime.UtcNow
				};

				await _unitOfWork.Payments.AddAsync(payment);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					data = new CheckoutSessionResultDTO
					{
						SessionId = session.Id,
						CheckoutUrl = session.Url
					}
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (StripeException ex)
			{
				return StatusCode(500, new { success = false, message = ex.StripeError.Message });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// POST /api/payment/webhook
		// Stripe بيكلم الـ Endpoint ده بعد الدفع
		// [AllowAnonymous] لأن Stripe مش بيبعت JWT
		// =============================================
		[HttpPost("webhook")]
		[AllowAnonymous]
		public async Task<IActionResult> StripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			try
			{
				var webhookSecret = _configuration["Stripe:WebhookSecret"];
				var stripeSignature = Request.Headers["Stripe-Signature"];

				// Stripe بيتحقق إن الـ request جاي منه فعلاً
				var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

				if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
				{
					var session = stripeEvent.Data.Object as Session;

					var studentId = session.Metadata["studentId"];
					var courseId = int.Parse(session.Metadata["courseId"]);

					await using var transaction = await _unitOfWork.BeginTransactionAsync();

					try
					{
						// 1. حدّث الـ Payment
						var payment = await _unitOfWork.Payments
							.GetFirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

						if (payment is not null)
						{
							payment.Status = PaymentStatus.Succeeded;
							payment.StripePaymentIntentId = session.PaymentIntentId;
							payment.PaidAt = DateTime.UtcNow;
							_unitOfWork.Payments.Update(payment);
						}

						// 2. تأكد مش enrolled قبل كده (edge case)
						var alreadyEnrolled = await _unitOfWork.Enrollments
							.ExistsAsync(e => e.StudentId == studentId && e.CourseId == courseId);

						if (!alreadyEnrolled)
						{
							// 3. Enroll الـ Student تلقائياً
							var enrollment = new Enrollment
							{
								StudentId = studentId,
								CourseId = courseId,
								EnrolledAt = DateTime.UtcNow,
								ProgressPercentage = 0,
								IsCompleted = false,
								LastAccessAt = DateTime.UtcNow
							};

							await _unitOfWork.Enrollments.AddAsync(enrollment);
						}

						await _unitOfWork.SaveAsync();
						await transaction.CommitAsync();
					}
					catch
					{
						await transaction.RollbackAsync();
						throw;
					}
				}

				return Ok();
			}
			catch (StripeException ex)
			{
				return BadRequest(new { success = false, message = ex.StripeError.Message});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}

		// =============================================
		// GET /api/payment/history
		// =============================================
		[HttpGet("history")]
		[Authorize]
		public async Task<IActionResult> GetPaymentHistory()
		{
			try
			{
				var studentId = GetUserId();

				var payments = await _unitOfWork.Payments
					.GetAllAsync(
						p => p.StudentId == studentId,
						includes: "Course"
					);

				var result = payments
					.OrderByDescending(p => p.CreatedAt)
					.Select(p => new PaymentHistoryDTO
					{
						Id = p.Id,
						CourseId = p.CourseId,
						CourseTitle = p.Course.Title,
						Amount = p.Amount,
						Currency = p.Currency,
						Status = p.Status.ToString(),
						CreatedAt = p.CreatedAt
					});

				return Ok(new { success = true, data = result });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}
	}
}