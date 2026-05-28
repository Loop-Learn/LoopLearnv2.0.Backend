namespace LoopLearn.Entities.DTOs.Payment
{
    public class RefundResultDTO
    {
        public int PaymentId { get; set; }
        public string CourseTitle { get; set; }
        public decimal AmountRefunded { get; set; }
        public string Currency { get; set; }
        public string StripeRefundId { get; set; }
        public DateTime RefundedAt { get; set; }
    }
}
