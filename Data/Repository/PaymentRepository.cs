// ================== PaymentRepository.cs ==================
using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;
using PRM_BE.Model.Enums;

namespace PRM_BE.Data.Repository
{
    public class PaymentRepository
    {
        private readonly AppDbContext _db;

        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Payment?> GetByIdAsync(int id)
        {
            return _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }

        public async Task MarkStatusAsync(int paymentId, PaymentStatus status, string? failureReason = null)
        {
            var p = await _db.Payments.FindAsync(paymentId);
            if (p == null) return;

            p.Status = status;
            if (!string.IsNullOrWhiteSpace(failureReason))
                p.FailureReason = failureReason;

            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
