using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Service;
using PRM_BE.Service.Models;

namespace PRM_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: api/payment/by-order/{orderId}
        [HttpGet("by-order/{orderId}")]
        [Authorize(Roles = "Admin,Staff,Customer")]
        public async Task<ActionResult<PaymentDto>> GetByOrder(int orderId)
        {
            var p = await _paymentService.GetByOrderAsync(orderId);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // POST: api/payment
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto dto)
        {
            var created = await _paymentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByOrder), new { orderId = created.OrderId }, created);
        }

        // PUT: api/payment/{id}/status
        [HttpPut("{paymentId}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int paymentId, [FromBody] PaymentStatusUpdateDto dto)
        {
            await _paymentService.UpdateStatusAsync(paymentId, dto);
            return Ok(new { message = "Payment status updated" });
        }
    }
}
