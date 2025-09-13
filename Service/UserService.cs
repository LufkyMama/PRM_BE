using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PRM_BE.Data.Repository;
using PRM_BE.Model;
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

        public async Task<ServiceResponse<Auth>> RegisterAsync(string username, string email, string password, string? phoneNumber, string firstname, string lastname)
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
                Role = user_role.User,
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
            var jwtKey = _configuration["Jwt:Key"] ?? "DefaultKeyForDevelopmentPurposesOnly12345678901234";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, Enum.GetName(typeof(user_role), user.Role)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "exeapi",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
