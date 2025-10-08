// ================== DeliveryService.cs ==================
using PRM_BE.Data.Repository;
using PRM_BE.Model;
using PRM_BE.Model.Enums;
using PRM_BE.Service.Models;

namespace PRM_BE.Service
{
    public class DeliveryService 
    {
        private readonly OrderRepository _orderRepo;
        private readonly DeliveryRepository _deliveryRepo;

        public DeliveryService(OrderRepository orderRepo, DeliveryRepository deliveryRepo)
        {
            _orderRepo = orderRepo;
            _deliveryRepo = deliveryRepo;
        }

        public async Task<DeliveryDto?> GetByOrderAsync(int orderId)
        {
            var d = await _deliveryRepo.GetByOrderIdAsync(orderId);
            return d is null ? null : Map(d);
        }

        public async Task<DeliveryDto> CreateAsync(DeliveryCreateDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId)
                        ?? throw new KeyNotFoundException("Order not found.");

            var d = new Delivery
            {
                OrderId = order.Id,
                Status = DeliveryStatus.NotStarted
            };

            d = await _deliveryRepo.AddAsync(d);
            return Map(d);
        }

        public async Task UpdateStatusAsync(DeliveryStatusUpdateDto dto)
        {
            await _deliveryRepo.UpdateStatusAsync(
                dto.OrderId, dto.Status, dto.DeliveredAt, dto.ProofPhotoUrl);

            // Sync Order.Status
            var o = await _orderRepo.GetByIdAsync(dto.OrderId);
            if (o != null)
            {
                o.Status = dto.Status switch
                {
                    DeliveryStatus.Assigned => Model.Enums.OrderStatus.Preparing,
                    DeliveryStatus.PickedUp => Model.Enums.OrderStatus.OutForDelivery,
                    DeliveryStatus.InTransit => Model.Enums.OrderStatus.OutForDelivery,
                    DeliveryStatus.Delivered => Model.Enums.OrderStatus.Delivered,
                    DeliveryStatus.Failed => Model.Enums.OrderStatus.Cancelled,
                    _ => o.Status
                };

                await _orderRepo.UpdateAsync(o);
            }
        }

        private static DeliveryDto Map(Delivery d) => new DeliveryDto
        {
            Id = d.Id,
            OrderId = d.OrderId,
            Status = d.Status,
            DeliveredAt = d.DeliveredAt,
            ProofPhotoUrl = d.ProofPhotoUrl
        };
    }
}
