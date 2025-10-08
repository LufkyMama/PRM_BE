// ================== DeliveryDtos.cs ==================
using PRM_BE.Model.Enums;

namespace PRM_BE.Service.Models
{
    public class DeliveryCreateDto
    {
        public int OrderId { get; set; }
    }

    public class DeliveryStatusUpdateDto
    {
        public int OrderId { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ProofPhotoUrl { get; set; }
    }

    public class DeliveryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ProofPhotoUrl { get; set; }
    }
}
