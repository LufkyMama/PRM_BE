using PRM_BE.Model.Enums;

namespace PRM_BE.Service.Models
{
    public class PaymentCreateDto
    {
        public int OrderId { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Unknown;
        public decimal Amount { get; set; }
    }

    public class PaymentStatusUpdateDto
    {
        public PaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
