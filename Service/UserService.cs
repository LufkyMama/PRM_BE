using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PRM_BE.Data.Repository;
using PRM_BE.Model;
using PRM_BE.Model.Enums;
using PRM_BE.Service.Models;

namespace PRM_BE.Service
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<Auth>> RegisterAsync(string username, string email, string password, string? phoneNumber, string firstname, string lastname, string address = null)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return ServiceResponse<Auth>.FailureResponse("Username, email, and password are required");
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(email))
            {
                return ServiceResponse<Auth>.FailureResponse("Email already in use");
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(username))
            {
                return ServiceResponse<Auth>.FailureResponse("Username already in use");
            }

            // Create new user
            var user = new User
            {
                UserName = username,
                Email = email,
                Password = HashPassword(password),
                PhoneNumber = phoneNumber,
                FirstName = firstname,
                LastName = lastname,
                Role = Model.Enums.UserRole.Customer,
                Address = address,
            };

            // Save user to database
            await _userRepository.CreateAsync(user);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Create Auth object
            var auth = new Auth
            {
                User = user,
                Token = token
            };

            return ServiceResponse<Auth>.SuccessResponse(auth, "Registration successful");
        }

        public async Task<ServiceResponse<Auth>> LoginAsync(string email, string password)
        {
            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ServiceResponse<Auth>.FailureResponse("Invalid email or password");
            }

            // Verify password
            if (!VerifyPassword(password, user.Password))
            {
                return ServiceResponse<Auth>.FailureResponse("Invalid email or password");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Create Auth object
            var auth = new Auth
            {
                User = user,
                Token = token
            };

            return ServiceResponse<Auth>.SuccessResponse(auth, "Login successful");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string GenerateJwtToken(User user)
        {
            var issuer = _configuration["Jwt:Issuer"] ?? "exeapi";
            var audience = _configuration["Jwt:Audience"] ?? "exeusers";
            var key = _configuration["Jwt:Key"]
                           ?? throw new Exception("JWT Key missing in configuration");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        // Role: dùng ClaimTypes.Role để khớp RoleClaimType = ClaimTypes.Role trong TokenValidationParameters
        new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), user.Role) ?? "User")
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,              // <-- bắt buộc khi ValidateAudience = true
                claims: claims,
                notBefore: now,
                expires: now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
