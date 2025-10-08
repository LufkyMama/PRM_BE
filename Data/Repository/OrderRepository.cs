using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;
using PRM_BE.Model.Enums;

namespace PRM_BE.Data.Repository
{
    public class OrderRepository 
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Order?> GetByIdAsync(int id, bool includeRelated = true)
        {
            IQueryable<Order> q = _db.Orders.AsQueryable();

            if (includeRelated)
            {
                q = q
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Flower)
                    .Include(o => o.Payment)
                    .Include(o => o.Delivery)
                    .Include(o => o.CustomerUser);
            }

            return await q.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserAsync(int? userId, DateTime? from = null, DateTime? to = null, OrderStatus? status = null)
        {
            var q = _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .AsQueryable();

            if (userId.HasValue)
                q = q.Where(o => o.CustomerUserId == userId.Value);

            if (from.HasValue)
                q = q.Where(o => o.CreatedAt >= from.Value);

            if (to.HasValue)
                q = q.Where(o => o.CreatedAt <= to.Value);

            if (status.HasValue)
                q = q.Where(o => o.Status == status.Value);

            return await q
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order> AddAsync(Order order)
        {
            // đảm bảo Total ban đầu đúng
            if (order.Items != null && order.Items.Count > 0)
            {
                var subtotal = order.Items.Sum(i => i.LineTotal);
                order.Total = subtotal + order.ShippingFee;
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Orders.FindAsync(id);
            if (entity != null)
            {
                _db.Orders.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RecalculateTotalsAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            var subtotal = order.Items?.Sum(i => i.LineTotal) ?? 0m;
            order.Total = subtotal + order.ShippingFee;

            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
