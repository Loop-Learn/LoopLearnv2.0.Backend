using LoopLearn.Entities.Enums;
namespace LoopLearn.Entities.Models
{
	public class Payment
	{
		public int Id { get; set; }

		public string StudentId { get; set; }
		public ApplicationUser Student { get; set; }

		public int CourseId { get; set; }
		public Course Course { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; } = "usd";

		// Stripe IDs — محتاجينهم للـ Webhook
		public string StripeSessionId { get; set; }
		public string? StripePaymentIntentId { get; set; }

		public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? PaidAt { get; set; }
	}
}
