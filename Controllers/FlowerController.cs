using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Model;           
using PRM_BE.Model.Enums;     
using PRM_BE.Service;
namespace PRM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowerController : ControllerBase
    {
        private readonly Service.FlowerService _flowerService;
        public FlowerController(Service.FlowerService flowerService)
        {
            _flowerService = flowerService;
        }
        [HttpGet ("all")]
        public ActionResult<List<Model.Flower>> GetAllFlowers()
        {
            return _flowerService.GetAllFlowers();
        }
        [HttpGet("{id}")]
        public ActionResult<Model.Flower> GetFlowerById(int id)
        {
            var flower = _flowerService.GetFlowerById(id);
            if (flower == null)
            {
                return NotFound();
            }
            return flower;
        }
        [HttpPost]
        public ActionResult AddFlower(Model.Flower flower)
        {
            _flowerService.AddFlower(flower);
            return CreatedAtAction(nameof(GetFlowerById), new { id = flower.Id }, flower);
        }
        [HttpPut("{id}")]
        public ActionResult UpdateFlower(int id, Model.Flower flower)
        {
            if (id != flower.Id)
            {
                return BadRequest();
            }
            var existingFlower = _flowerService.GetFlowerById(id);
            if (existingFlower == null)
            {
                return NotFound();
            }
            _flowerService.UpdateFlower(flower);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteFlower(int id)
        {
            var existingFlower = _flowerService.GetFlowerById(id);
            if (existingFlower == null)
            {
                return NotFound();
            }
            _flowerService.DeleteFlower(id);
            return NoContent();
        }
        [HttpGet]
        public ActionResult<List<Flower>> Get([FromQuery] FlowerCategory? category)
        {
            if (category is null)
                return _flowerService.GetAllFlowers();

            var data = _flowerService.GetByCategory(category.Value);
            return Ok(data);
        }
    }
}
