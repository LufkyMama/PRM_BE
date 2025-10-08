// ================== PaymentService.cs ==================
using PRM_BE.Data.Repository;
using PRM_BE.Model;
using PRM_BE.Model.Enums;
using PRM_BE.Service.Models;

namespace PRM_BE.Service
{
    public class PaymentService 
    {
        private readonly OrderRepository _orderRepo;
        private readonly PaymentRepository _paymentRepo;

        public PaymentService(OrderRepository orderRepo, PaymentRepository paymentRepo)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<PaymentDto?> GetByOrderAsync(int orderId)
        {
            var p = await _paymentRepo.GetByOrderIdAsync(orderId);
            return p is null ? null : Map(p);
        }

        public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId)
                        ?? throw new KeyNotFoundException("Order not found.");

            var p = new Payment
            {
                OrderId = order.Id,
                Method = dto.Method,
                Amount = dto.Amount,
                Status = PaymentStatus.Unpaid
            };

            p = await _paymentRepo.AddAsync(p);
            return Map(p);
        }

        public async Task UpdateStatusAsync(int paymentId, PaymentStatusUpdateDto dto)
        {
            await _paymentRepo.MarkStatusAsync(paymentId, dto.Status, dto.FailureReason);

            // sync Order status theo Payment
            var p = await _paymentRepo.GetByIdAsync(paymentId);
            if (p?.OrderId != null)
            {
                var o = await _orderRepo.GetByIdAsync(p.OrderId);
                if (o != null)
                {
                    if (dto.Status == PaymentStatus.Paid)
                    {
                        o.Status = Model.Enums.OrderStatus.Confirmed;
                        o.ConfirmedAt = DateTime.UtcNow;
                        await _orderRepo.UpdateAsync(o);
                    }
                    else if (dto.Status == PaymentStatus.Refunded)
                    {
                        o.Status = Model.Enums.OrderStatus.Refunded;
                        await _orderRepo.UpdateAsync(o);
                    }
                    else if (dto.Status == PaymentStatus.Failed)
                    {
                        // Giữ nguyên hoặc set Pending tuỳ chính sách
                    }
                }
            }
        }

        private static PaymentDto Map(Payment p) => new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Method = p.Method,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            RefundedAt = p.RefundedAt,
            FailureReason = p.FailureReason
        };
    }
}
