namespace PRM_BE.Service
{
    public class FlowerService
    {
        private readonly Data.Repository.FlowerRepo _flowerRepo;
        public FlowerService(Data.Repository.FlowerRepo flowerRepo)
        {
            _flowerRepo = flowerRepo;
        }
        public List<Model.Flower> GetAllFlowers()
        {
            return _flowerRepo.GetAllFlowers();
        }
        public Model.Flower GetFlowerById(int id)
        {
            return _flowerRepo.GetFlowerById(id);
        }
        public void AddFlower(Model.Flower flower)
        {
            _flowerRepo.AddFlower(flower);
        }
        public void UpdateFlower(Model.Flower flower)
        {
            _flowerRepo.UpdateFlower(flower);
        }
        public void DeleteFlower(int id)
        {
            _flowerRepo.DeleteFlower(id);
        }
    }
}
