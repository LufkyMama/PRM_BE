using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRM_BE.Model.Enums;

namespace PRM_BE.Model
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public PaymentMethod Method { get; set; } = PaymentMethod.Unknown;

         public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RefundedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
