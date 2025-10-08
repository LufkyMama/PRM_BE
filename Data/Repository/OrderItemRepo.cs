using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;

namespace PRM_BE.Data.Repository
{
    public class OrderItemRepository 
    {
        private readonly AppDbContext _db;

        public OrderItemRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<OrderItem?> GetByIdAsync(int id)
        {
            return _db.OrderItems
                .Include(i => i.Flower)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<List<OrderItem>> GetByOrderAsync(int orderId)
        {
            return _db.OrderItems
                .Where(i => i.OrderId == orderId)
                .Include(i => i.Flower)
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<OrderItem> AddAsync(OrderItem item)
        {
            _db.OrderItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task UpdateAsync(OrderItem item)
        {
            _db.OrderItems.Update(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.OrderItems.FindAsync(id);
            if (entity != null)
            {
                _db.OrderItems.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
