using LoopLearn.Entities.DTOs.Payment;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Helpers.Models;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly StripeSettings _stripeSettings;

        public PaymentController(IUnitOfWork unitOfWork, IOptions<StripeSettings> stripeOptions)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        // =============================================
        // POST /api/payment/checkout
        // Creates a Stripe Checkout Session and returns the URL
        // =============================================
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDTO model)
        {
            try
            {
                var studentId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == model.CourseId);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                //if (course.Status != CourseStatus.Published)
                //    return BadRequest(new { success = false, message = "Course is not available." });

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

                // Create Stripe Session
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
                                UnitAmount = (long)(course.Price * 100), // Stripe works in cents
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
                    SuccessUrl = _stripeSettings.SuccessUrl,
                    CancelUrl = _stripeSettings.CancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "studentId", studentId },
                        { "courseId", model.CourseId.ToString() }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Save Payment record with Pending status
                var payment = new Payment
                {
                    StudentId = studentId,
                    CourseId = model.CourseId,
                    Amount = course.Price,
                    Currency = "USD",
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
        // Stripe calls this endpoint after payment
        // [AllowAnonymous] because Stripe doesn't send a JWT
        // =============================================
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var webhookSecret = _stripeSettings.WebhookSecret;
                var stripeSignature = Request.Headers["Stripe-Signature"];

                // Verify the event came from Stripe
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                // Use the exact event name string (version‑proof and safe)
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                        return BadRequest(new { success = false, message = "Invalid session object." });

                    // Safely extract metadata
                    if (!session.Metadata.TryGetValue("studentId", out var studentId) ||
                        !session.Metadata.TryGetValue("courseId", out var courseIdStr))
                    {
                        return BadRequest(new { success = false, message = "Missing metadata in session." });
                    }

                    if (!int.TryParse(courseIdStr, out var courseId))
                        return BadRequest(new { success = false, message = "Invalid course ID in metadata." });

                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // 1. Update Payment status
                        var payment = await _unitOfWork.Payments
                            .GetFirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

                        if (payment is not null)
                        {
                            payment.Status = PaymentStatus.Succeeded;
                            payment.StripePaymentIntentId = session.PaymentIntentId;
                            payment.PaidAt = DateTime.UtcNow;
                            _unitOfWork.Payments.Update(payment);
                        }

                        // 2. Prevent duplicate enrollment (edge case)
                        var alreadyEnrolled = await _unitOfWork.Enrollments
                            .ExistsAsync(e => e.StudentId == studentId && e.CourseId == courseId);

                        if (!alreadyEnrolled)
                        {
                            // 3. Automatically enroll the student
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
                return BadRequest(new { success = false, message = ex.StripeError.Message });
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