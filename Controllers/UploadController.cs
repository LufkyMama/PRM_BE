using PRM_BE.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PRM_BE.Controllers
{
    [Authorize(Policy = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly FirebaseStorageService _storageService;

        public UploadController(FirebaseStorageService storageService)
        {
            _storageService = storageService;
        }

                [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var result = await _storageService.UploadImageAsync(file);
                return Ok(new { 
                    imageUrl = result.url,
                    fileName = result.fileName 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("image/{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var imageBytes = await _storageService.GetImageAsync(fileName);
                return File(imageBytes, "image/jpeg"); // Có thể cần detect content type
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("image/{fileName}")]
        public async Task<IActionResult> UpdateImage(string fileName, IFormFile file)
        {
            try
            {
                var url = await _storageService.UpdateImageAsync(fileName, file);
                return Ok(new { imageUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("image/{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            try
            {
                var result = await _storageService.DeleteImageAsync(fileName);
                if (result)
                {
                    return Ok(new { message = "File deleted successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to delete file" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("images")]
        public async Task<IActionResult> ListImages()
        {
            try
            {
                var images = await _storageService.ListImagesAsync();
                return Ok(new { images = images });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
