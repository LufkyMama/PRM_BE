using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Service;
using PRM_BE.Service.Models;

namespace PRM_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly DeliveryService _deliveryService;

        public DeliveryController(DeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        // GET: api/delivery/by-order/{orderId}
        [HttpGet("by-order/{orderId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<DeliveryDto>> GetByOrder(int orderId)
        {
            var d = await _deliveryService.GetByOrderAsync(orderId);
            if (d == null) return NotFound();
            return Ok(d);
        }

        // POST: api/delivery
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<DeliveryDto>> Create([FromBody] DeliveryCreateDto dto)
        {
            var created = await _deliveryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByOrder), new { orderId = created.OrderId }, created);
        }

        // PUT: api/delivery/status
        [HttpPut("status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus([FromBody] DeliveryStatusUpdateDto dto)
        {
            await _deliveryService.UpdateStatusAsync(dto);
            return Ok(new { message = "Delivery status updated" });
        }
    }
}
