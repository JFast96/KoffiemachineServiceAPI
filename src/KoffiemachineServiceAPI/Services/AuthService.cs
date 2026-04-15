using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KoffiemachineServiceAPI.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace KoffiemachineServiceAPI.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    // Hardcoded users for demonstration purposes.
    // In production, these would come from a database.
    private static readonly Dictionary<string, (string Password, string Role)> Users = new()
    {
        ["admin"] = ("admin123", "Admin"),
        ["technicus"] = ("tech123", "Technicus"),
        ["viewer"] = ("view123", "Viewer")
    };

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse? Authenticate(LoginRequest request)
    {
        // Validate credentials
        if (!Users.TryGetValue(request.Username.ToLower(), out var user))
            return null;

        if (user.Password != request.Password)
            return null;

        // Generate JWT token
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(
            double.Parse(jwtSettings["ExpiresInHours"] ?? "1"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
