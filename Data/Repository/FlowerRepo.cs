using PRM_BE.Data;             
using PRM_BE.Model;           
using PRM_BE.Model.Enums;
namespace PRM_BE.Data.Repository
{
    public class FlowerRepo
    {
        private readonly AppDbContext _context;
        public FlowerRepo(AppDbContext context)
        {
            _context = context;
        }
        public List<Model.Flower> GetAllFlowers()
        {
            return _context.Flowers.ToList();
        }
        public List<Flower> GetByCategory(FlowerCategory category)
        {
            return _context.Flowers
                .Where(f => f.Category == category)
                .OrderBy(f => f.Name)
                .ToList();
        }
        public Model.Flower GetFlowerById(int id)
        {
            return _context.Flowers.Find(id);
        }
        public void AddFlower(Model.Flower flower)
        {
            _context.Flowers.Add(flower);
            _context.SaveChanges();
        }
        public void UpdateFlower(Model.Flower flower)
        {
            var existingFlower = _context.Flowers.Find(flower.Id);
            if (existingFlower != null)
            {
                existingFlower.Name = flower.Name;
                existingFlower.Description = flower.Description;
                existingFlower.Price = flower.Price;
                existingFlower.Stock = flower.Stock;
                existingFlower.Category = flower.Category;
                existingFlower.ImageUrl = flower.ImageUrl;

                _context.SaveChanges();
            }
        }
        public void DeleteFlower(int id)
        {
            var flower = _context.Flowers.Find(id);
            if (flower != null)
            {
                _context.Flowers.Remove(flower);
                _context.SaveChanges();
            }
        }

    }
}
