using faspark_be.Database;
using faspark_be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace faspark_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserAuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // REGISTER
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterRequest request)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username sudah digunakan" });
            }

            var newUser = new User
            {
                Username = request.Username,
                Password = request.Password, // disarankan nanti pakai hashing
                Role = request.Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "Registrasi sukses" });
        }

        // LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Username atau password salah" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToLower())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return Ok(new
            {
                message = "Login sukses",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role,
                username = user.Username,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    role = user.Role
                }
            });
        }

        // UPDATE
        [HttpPut("update-profile/{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UserUpdateProfileRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User tidak ditemukan" });
            }

            user.Username = request.Username;
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = request.Password;
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new { message = "Profil berhasil diupdate" });
        }

        // DTOs
        public class UserRegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }

        public class UserLoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class UserUpdateProfileRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
