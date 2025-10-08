using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Service;
using PRM_BE.Service.Models;

namespace PRM_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderItemController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orderitem/{orderId}
        [HttpPost("{orderId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> AddItem(int orderId, [FromBody] OrderItemCreateDto dto)
        {
            await _orderService.AddItemAsync(orderId, dto);
            return Ok(new { message = "Item added successfully" });
        }

        // PUT: api/orderitem/{orderItemId}/quantity?value=5
        [HttpPut("{orderItemId}/quantity")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> UpdateQuantity(int orderItemId, [FromQuery] int value)
        {
            await _orderService.UpdateItemQuantityAsync(orderItemId, value);
            return Ok(new { message = "Quantity updated" });
        }

        // DELETE: api/orderitem/{orderItemId}
        [HttpDelete("{orderItemId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<IActionResult> Delete(int orderItemId)
        {
            await _orderService.RemoveItemAsync(orderItemId);
            return Ok(new { message = "Item removed" });
        }
    }
}
