using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace RealtimeChat.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string AccessToken, string Username);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    // User hardcode cho demo, Ngày 3 sẽ thay bằng DB
    private static readonly Dictionary<string, string> Users = new()
    {
        ["alice"] = "123456",
        ["bob"] = "123456",
        ["charlie"] = "123456"
    };

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username và password không được rỗng." });
        }

        var username = request.Username.Trim().ToLowerInvariant();

        if (!Users.TryGetValue(username, out var password) || password != request.Password)
        {
            return Unauthorized(new { message = "Sai username hoặc password." });
        }

        var token = GenerateJwtToken(username);
        return Ok(new LoginResponse(token, username));
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}