using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Service;
using PRM_BE.Service.Models;
using PRM_BE.Model.Enums;

namespace PRM_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/order
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<List<OrderDto>>> GetAll(
            [FromQuery] int? userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] OrderStatus? status)
        {
            var orders = await _orderService.ListAsync(userId, from, to, status);
            return Ok(orders);
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<OrderDto>> Get(int id)
        {
            var order = await _orderService.GetAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST: api/order
        [HttpPost]
        [Authorize(Roles = "Customer,Staff,Admin")]
        public async Task<ActionResult<OrderDto>> Create([FromBody] OrderCreateDto dto)
        {
            var created = await _orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT: api/order/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto)
        {
            await _orderService.UpdateStatusAsync(id, dto);
            return NoContent();
        }

        // DELETE: api/order/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Customer,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
    }
}
