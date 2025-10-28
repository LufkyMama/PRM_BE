using System.Text.RegularExpressions;
﻿using PRM_BE.Model;          
using PRM_BE.Model.Enums;     

namespace PRM_BE.Service
{
    public class FlowerService
    {
        private readonly Data.Repository.FlowerRepo _flowerRepo;
        private readonly FirebaseStorageService _firebaseStorageService;

        public FlowerService(Data.Repository.FlowerRepo flowerRepo, FirebaseStorageService firebaseStorageService)
        {
            _flowerRepo = flowerRepo;
            _firebaseStorageService = firebaseStorageService;
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

        public async Task DeleteFlowerAsync(int id)
        {
            var flower = _flowerRepo.GetFlowerById(id);
            if (flower != null)
            {
                if (!string.IsNullOrEmpty(flower.ImageUrl))
                {
                    var fileName = GetFileNameFromUrl(flower.ImageUrl);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        await _firebaseStorageService.DeleteImageAsync(fileName);
                    }
                }
                _flowerRepo.DeleteFlower(id);
            }
        }

        public async Task DeleteFlowerImageAsync(int id)
        {
            var flower = _flowerRepo.GetFlowerById(id);
            if (flower != null && !string.IsNullOrEmpty(flower.ImageUrl))
            {
                var fileName = GetFileNameFromUrl(flower.ImageUrl);
                if (!string.IsNullOrEmpty(fileName))
                {
                    await _firebaseStorageService.DeleteImageAsync(fileName);
                }

                flower.ImageUrl = ""; // Set URL to empty
                _flowerRepo.UpdateFlower(flower); // Save the change
            }
        }

        private string? GetFileNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');
            var fileNameWithToken = pathSegments.LastOrDefault();
            if (fileNameWithToken != null)
            {
                var fileName = Uri.UnescapeDataString(fileNameWithToken);
                return fileName;
            }
            return null;
        }
        public List<Model.Flower> GetByCategory(FlowerCategory category)
        {
            return _flowerRepo.GetByCategory(category);
        }
    }
}