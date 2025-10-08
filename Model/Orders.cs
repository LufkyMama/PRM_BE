using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRM_BE.Model.Enums;

namespace PRM_BE.Model
{
    public class Order
    {
        public int Id { get; set; }

        // Người gửi có thể là user có tài khoản (nullable)
        public int? CustomerUserId { get; set; }
        public User? CustomerUser { get; set; }

        // Guest info (khi không có tài khoản)
        [MaxLength(120)] public string SenderName { get; set; } = null!;
        [MaxLength(255)] public string SenderEmail { get; set; } = null!;
        [MaxLength(20)]  public string SenderPhone { get; set; } = null!;

        // Người nhận
        public string ?Recipient { get; set; }
 

        // Thời gian giao
        public DateTime DeliveryDate { get; set; }                     // dùng Date-only semantics theo app
        public DeliveryTimeWindow DeliveryTimeWindow { get; set; } = DeliveryTimeWindow.Anytime;


        // Tài chính
         public decimal ShippingFee { get; set; }
         public decimal Total { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public Delivery? Delivery { get; set; }
    }
}
