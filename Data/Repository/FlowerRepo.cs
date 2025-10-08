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
            _context.Flowers.Update(flower);
            _context.SaveChanges();
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
