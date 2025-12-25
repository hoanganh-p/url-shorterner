using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Models;
using UrlShortener.Services;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IJwtService jwtService, IUserService userService, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            // Validate credentials with database
            var isValid = await _userService.ValidateUserCredentialsAsync(request.Username, request.Password);

            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Get user info
            var user = await _userService.GetUserByUsernameAsync(request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Generate token
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes");
            var token = _jwtService.GenerateToken(
                userId: user.UserId,
                username: user.Username,
                roles: new[] { user.Role }
            );

            return Ok(new LoginResponse
            {
                Token = token,
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password, // Will be hashed in CreateUserAsync
                Role = "User"
            };

            var success = await _userService.CreateUserAsync(user);

            if (!success)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            return Ok(new { message = "User registered successfully", userId = user.UserId });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }
    }
}