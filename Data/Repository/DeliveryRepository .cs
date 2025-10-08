using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;
using PRM_BE.Model.Enums;

namespace PRM_BE.Data.Repository
{
    public class DeliveryRepository 
    {
        private readonly AppDbContext _db;

        public DeliveryRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Delivery?> GetByIdAsync(int id)
        {
            return _db.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public Task<Delivery?> GetByOrderIdAsync(int orderId)
        {
            return _db.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);
        }

        public async Task<Delivery> AddAsync(Delivery delivery)
        {
            _db.Deliveries.Add(delivery);
            await _db.SaveChangesAsync();
            return delivery;
        }

        public async Task UpdateAsync(Delivery delivery)
        {
            _db.Deliveries.Update(delivery);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int orderId, DeliveryStatus status, DateTime? deliveredAt = null, string? proofPhotoUrl = null)
        {
            var d = await _db.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (d == null) return;

            d.Status = status;
            d.DeliveredAt = deliveredAt;
            d.ProofPhotoUrl = proofPhotoUrl;

            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
