// ================== OrderService.cs ==================
using Microsoft.EntityFrameworkCore;
using PRM_BE.Data;
using PRM_BE.Data.Repository;
using PRM_BE.Model;
using PRM_BE.Model.Enums;
using PRM_BE.Service.Models;

namespace PRM_BE.Service
{
    public class OrderService 
    {
        private readonly AppDbContext _db;
        private readonly OrderRepository _orderRepo;
        private readonly OrderItemRepository _itemRepo;
        private readonly PaymentRepository _paymentRepo;
        private readonly DeliveryRepository _deliveryRepo;

        public OrderService(
            AppDbContext db,
            OrderRepository orderRepo,
            OrderItemRepository itemRepo,
            PaymentRepository paymentRepo,
            DeliveryRepository deliveryRepo)
        {
            _db = db;
            _orderRepo = orderRepo;
            _itemRepo = itemRepo;
            _paymentRepo = paymentRepo;
            _deliveryRepo = deliveryRepo;
        }

        // ===== Queries =====
        public async Task<OrderDto?> GetAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id, includeRelated: true);
            return order is null ? null : MapOrder(order);
        }

        public async Task<List<OrderDto>> ListAsync(int? userId = null, DateTime? from = null, DateTime? to = null, OrderStatus? status = null)
        {
            var list = await _orderRepo.GetByUserAsync(userId, from, to, status);
            return list.Select(MapOrder).ToList();
        }

        // ===== Commands =====
        public async Task<OrderDto> CreateAsync(OrderCreateDto dto)
        {
            if (dto.DeliveryDate.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("DeliveryDate cannot be in the past.");

            var order = new Order
            {
                CustomerUserId = dto.CustomerUserId,
                SenderName = dto.SenderName,
                SenderEmail = dto.SenderEmail,
                SenderPhone = dto.SenderPhone,
                Recipient = dto.Recipient,
                DeliveryDate = dto.DeliveryDate,
                DeliveryTimeWindow = dto.DeliveryTimeWindow,
                ShippingFee = dto.ShippingFee,
                Status = OrderStatus.Pending
            };

            // Add items
            foreach (var it in dto.Items)
            {
                var flower = await _db.Flowers.FirstOrDefaultAsync(f => f.Id == it.FlowerId)
                             ?? throw new KeyNotFoundException($"Flower {it.FlowerId} not found.");

                var unitPrice = it.UnitPrice ?? flower.Price;

                order.Items.Add(new OrderItem
                {
                    FlowerId = it.FlowerId,
                    Quantity = it.Quantity,
                    UnitPrice = unitPrice
                    // LineTotal là computed column trên DB
                });
            }

            // Lưu order + items, EF sẽ tính LineTotal
            order = await _orderRepo.AddAsync(order);
            await _orderRepo.RecalculateTotalsAsync(order.Id);

            // Tạo Payment & Delivery mặc định
            var payment = new Payment
            {
                OrderId = order.Id,
                Method = PaymentMethod.Unknown,
                Amount = order.Total,
                Status = PaymentStatus.Unpaid
            };
            await _paymentRepo.AddAsync(payment);

            var delivery = new Delivery
            {
                OrderId = order.Id,
                Status = DeliveryStatus.NotStarted
            };
            await _deliveryRepo.AddAsync(delivery);

            // reload
            var saved = await _orderRepo.GetByIdAsync(order.Id, includeRelated: true)
                       ?? throw new Exception("Cannot reload saved order.");

            return MapOrder(saved);
        }

        public async Task AddItemAsync(int orderId, OrderItemCreateDto itemDto)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, includeRelated: true)
                        ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot modify items of a finalized order.");

            var flower = await _db.Flowers.FirstOrDefaultAsync(f => f.Id == itemDto.FlowerId)
                         ?? throw new KeyNotFoundException("Flower not found.");

            var unitPrice = itemDto.UnitPrice ?? flower.Price;

            await _itemRepo.AddAsync(new OrderItem
            {
                OrderId = orderId,
                FlowerId = itemDto.FlowerId,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice
            });

            await _orderRepo.RecalculateTotalsAsync(orderId);
        }

        public async Task UpdateItemQuantityAsync(int orderItemId, int newQuantity)
        {
            var item = await _itemRepo.GetByIdAsync(orderItemId)
                       ?? throw new KeyNotFoundException("OrderItem not found.");

            var order = await _orderRepo.GetByIdAsync(item.OrderId)
                        ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot modify items of a finalized order.");

            item.Quantity = newQuantity;
            await _itemRepo.UpdateAsync(item);
            await _orderRepo.RecalculateTotalsAsync(item.OrderId);
        }

        public async Task RemoveItemAsync(int orderItemId)
        {
            var item = await _itemRepo.GetByIdAsync(orderItemId)
                       ?? throw new KeyNotFoundException("OrderItem not found.");

            var order = await _orderRepo.GetByIdAsync(item.OrderId)
                        ?? throw new KeyNotFoundException("Order not found.");

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot modify items of a finalized order.");

            await _itemRepo.DeleteAsync(orderItemId);
            await _orderRepo.RecalculateTotalsAsync(item.OrderId);
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatusUpdateDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(orderId)
                        ?? throw new KeyNotFoundException("Order not found.");

            order.Status = dto.Status;
            order.ConfirmedAt = dto.ConfirmedAt;

            await _orderRepo.UpdateAsync(order);
        }

        public Task DeleteAsync(int orderId) => _orderRepo.DeleteAsync(orderId);

        // ===== Mapping helpers =====
        private static OrderDto MapOrder(Order o)
        {
            return new OrderDto
            {
                Id = o.Id,
                CustomerUserId = o.CustomerUserId,
                SenderName = o.SenderName,
                SenderEmail = o.SenderEmail,
                SenderPhone = o.SenderPhone,
                Recipient = o.Recipient,
                DeliveryDate = o.DeliveryDate,
                DeliveryTimeWindow = o.DeliveryTimeWindow,
                ShippingFee = o.ShippingFee,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ConfirmedAt = o.ConfirmedAt,
                Items = o.Items?.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    FlowerId = i.FlowerId,
                    FlowerName = i.Flower?.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList() ?? new(),

                Payment = o.Payment == null ? null : new PaymentDto
                {
                    Id = o.Payment.Id,
                    OrderId = o.Payment.OrderId,
                    Method = o.Payment.Method,
                    Amount = o.Payment.Amount,
                    Status = o.Payment.Status,
                    CreatedAt = o.Payment.CreatedAt,
                    RefundedAt = o.Payment.RefundedAt,
                    FailureReason = o.Payment.FailureReason
                },

                Delivery = o.Delivery == null ? null : new DeliveryDto
                {
                    Id = o.Delivery.Id,
                    OrderId = o.Delivery.OrderId,
                    Status = o.Delivery.Status,
                    DeliveredAt = o.Delivery.DeliveredAt,
                    ProofPhotoUrl = o.Delivery.ProofPhotoUrl
                }
            };
        }
    }
}
