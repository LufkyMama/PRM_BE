using FirebaseAdmin;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PRM_BE.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PRM_BE.Service
{
    public class FirebaseStorageService
    {
        private readonly string _bucketName = "flower-shop-af959.firebasestorage.app";
        private readonly IConfiguration _configuration;

        public FirebaseStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private GoogleCredential GetFirebaseCredential()
        {
            var firebaseConfig = _configuration.GetSection("Firebase").Get<FirebaseConfig>();
            if (firebaseConfig == null)
                throw new Exception("Firebase configuration not found");

            // Tạo JSON string từ configuration
            var serviceAccountJson = $@"{{
                ""type"": ""service_account"",
                ""project_id"": ""{firebaseConfig.ProjectId}"",
                ""private_key_id"": ""{firebaseConfig.PrivateKeyId}"",
                ""private_key"": ""{firebaseConfig.PrivateKey}"",
                ""client_email"": ""{firebaseConfig.ClientEmail}"",
                ""client_id"": ""{firebaseConfig.ClientId}"",
                ""auth_uri"": ""{firebaseConfig.AuthUri}"",
                ""token_uri"": ""{firebaseConfig.TokenUri}"",
                ""auth_provider_x509_cert_url"": ""{firebaseConfig.AuthProviderX509CertUrl}"",
                ""client_x509_cert_url"": ""{firebaseConfig.ClientX509CertUrl}"",
                ""universe_domain"": ""{firebaseConfig.UniverseDomain}""
            }}";

            return GoogleCredential.FromJson(serviceAccountJson);
        }

        public async Task<(string url, string fileName)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File không hợp lệ.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            // Sử dụng credentials từ configuration
            var credential = GetFirebaseCredential();
            var storage = StorageClient.Create(credential);
            
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            await storage.UploadObjectAsync(_bucketName, fileName, null, stream);

            var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{fileName}?alt=media";
            return (url, fileName);
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Tên file không hợp lệ.");

            var credential = GetFirebaseCredential();
            var storage = StorageClient.Create(credential);

            using var stream = new MemoryStream();
            await storage.DownloadObjectAsync(_bucketName, fileName, stream);
            
            return stream.ToArray();
        }

        public async Task<string> UpdateImageAsync(string fileName, IFormFile newFile)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Tên file không hợp lệ.");
            
            if (newFile == null || newFile.Length == 0)
                throw new Exception("File mới không hợp lệ.");

            var credential = GetFirebaseCredential();
            var storage = StorageClient.Create(credential);

            // Xóa file cũ
            try
            {
                await storage.DeleteObjectAsync(_bucketName, fileName);
            }
            catch (Exception ex)
            {
                // Log warning nếu file không tồn tại
                Console.WriteLine($"Warning: Could not delete old file {fileName}: {ex.Message}");
            }

            // Upload file mới
            using var stream = new MemoryStream();
            await newFile.CopyToAsync(stream);
            stream.Position = 0;

            await storage.UploadObjectAsync(_bucketName, fileName, null, stream);

            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{fileName}?alt=media";
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Tên file không hợp lệ.");

            var credential = GetFirebaseCredential();
            var storage = StorageClient.Create(credential);

            try
            {
                await storage.DeleteObjectAsync(_bucketName, fileName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {fileName}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> ListImagesAsync()
        {
            var credential = GetFirebaseCredential();
            var storage = StorageClient.Create(credential);

            var objects = new List<string>();
            await foreach (var obj in storage.ListObjectsAsync(_bucketName))
            {
                objects.Add(obj.Name);
            }

            return objects;
        }
    }
}
