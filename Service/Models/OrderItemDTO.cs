using PRM_BE.Model.Enums;

namespace PRM_BE.Service.Models
{
    // ----- OrderItem DTOs -----
    public class OrderItemCreateDto
    {
        public int FlowerId { get; set; }
        public int Quantity { get; set; }
        // Nếu null -> dùng Flower.Price hiện tại
        public decimal? UnitPrice { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int FlowerId { get; set; }
        public string? FlowerName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ----- Order DTOs -----
    public class OrderCreateDto
    {
        public int? CustomerUserId { get; set; }
        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;
        public string SenderPhone { get; set; } = null!;
        public string? Recipient { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DeliveryTimeWindow DeliveryTimeWindow { get; set; } = DeliveryTimeWindow.Anytime;

        public decimal ShippingFee { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int? CustomerUserId { get; set; }

        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;
        public string SenderPhone { get; set; } = null!;
        public string? Recipient { get; set; }

        public DateTime DeliveryDate { get; set; }
        public DeliveryTimeWindow DeliveryTimeWindow { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
        public PaymentDto? Payment { get; set; }
        public DeliveryDto? Delivery { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        public OrderStatus Status { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }
}
