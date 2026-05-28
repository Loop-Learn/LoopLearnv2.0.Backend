using LoopLearn.DataAccess.Services.Enroll;
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
        private readonly EnrollmentService _enrollmentService;

        public PaymentController(
            IUnitOfWork unitOfWork,
            IOptions<StripeSettings> stripeOptions,
            EnrollmentService enrollmentService)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeOptions.Value;
            _enrollmentService = enrollmentService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        // =============================================
        // POST /api/payment/checkout
        // Creates a Stripe Checkout Session and returns the URL.
        // Free courses are rejected here and routed to /api/enrollment/courses/{id}.
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
                                UnitAmount = (long)(course.Price * 100),
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
        // Stripe calls this after payment completion.
        // [AllowAnonymous] — Stripe sends no JWT; security is the webhook signature.
        // EnableBuffering() in Program.cs ensures the raw body is readable here.
        // =============================================
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            string json;

            // Read the raw body — EnableBuffering() in Program.cs keeps it available
            // as a byte stream so Stripe's signature check works correctly
            using (var reader = new StreamReader(HttpContext.Request.Body,
                       leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }

            try
            {
                var stripeSignature = Request.Headers["Stripe-Signature"];

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _stripeSettings.WebhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session is null)
                        return BadRequest(new { success = false, message = "Invalid session object." });

                    if (!session.Metadata.TryGetValue("studentId", out var studentId) ||
                        !session.Metadata.TryGetValue("courseId", out var courseIdStr))
                        return BadRequest(new { success = false, message = "Missing metadata in session." });

                    if (!int.TryParse(courseIdStr, out var courseId))
                        return BadRequest(new { success = false, message = "Invalid course ID in metadata." });

                    // ── Idempotency guard ──────────────────────────────────────
                    // Stripe guarantees at-least-once delivery, so this event can
                    // arrive more than once. If we already processed it, acknowledge
                    // immediately without doing any work.
                    var alreadyProcessed = await _unitOfWork.Payments
                        .ExistsAsync(p => p.StripeSessionId == session.Id
                                       && p.Status == PaymentStatus.Succeeded);

                    if (alreadyProcessed)
                        return Ok(); // Idempotent — safe to acknowledge again
                                     // ──────────────────────────────────────────────────────────

                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // 1. Update Payment record
                        var payment = await _unitOfWork.Payments
                            .GetFirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

                        if (payment is not null)
                        {
                            payment.Status = PaymentStatus.Succeeded;
                            payment.StripePaymentIntentId = session.PaymentIntentId;
                            payment.PaidAt = DateTime.UtcNow;
                            _unitOfWork.Payments.Update(payment);
                        }

                        // 2. Enroll the student via EnrollmentService —
                        //    same logic path as free courses, including future
                        //    side-effects (emails, analytics, etc.)
                        await _enrollmentService.EnrollStudentAsync(
                            studentId,
                            courseId,
                            paymentId: payment?.Id
                        );

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
        // Returns all payments for the authenticated student,
        // ordered by date, filtered at the DB level.
        // =============================================
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetPaymentHistory()
        {
            try
            {
                var studentId = GetUserId();

                // Ordering happens inside GetAsync at the IQueryable level,
                // not in-memory after loading all records
                var payments = await _unitOfWork.Payments
                    .GetAsync(
                        predicate: p => p.StudentId == studentId,
                        selector: p => new PaymentHistoryDTO
                        {
                            Id = p.Id,
                            CourseId = p.CourseId,
                            CourseTitle = p.Course.Title,
                            Amount = p.Amount,
                            Currency = p.Currency,
                            Status = p.Status.ToString(),
                            CreatedAt = p.CreatedAt
                        },
                        include: "Course",
                        orderBy: q => q.OrderByDescending(p => p.CreatedAt)
                    );

                return Ok(new { success = true, data = payments });
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