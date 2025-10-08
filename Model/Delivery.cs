using System.ComponentModel.DataAnnotations;
using PRM_BE.Model.Enums;

namespace PRM_BE.Model
{
    public class Delivery
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public DeliveryStatus Status { get; set; } = DeliveryStatus.NotStarted;
        public DateTime? DeliveredAt { get; set; } 
        public string? ProofPhotoUrl { get; set; }
    }
}
