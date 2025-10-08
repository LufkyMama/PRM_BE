using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM_BE.Service;
using PRM_BE.Controllers.ViewModel;
using PRM_BE.Service.Models;
namespace PRM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _userService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password,
                request.PhoneNumber,
                request.LastName,
                request.FirstName,
                request.Address);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _userService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                return BadRequest(new { result.Message });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new { Message = "Authenticated user", Username = User.Identity?.Name });
        }
    }
}