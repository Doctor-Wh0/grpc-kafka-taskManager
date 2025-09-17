using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagement.Api.Models;
using TaskManagement.Core.Models;
using TaskManagement.Infrastructure.Data;  // Для RefreshToken

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TaskNoteDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser> userManager, TaskNoteDbContext context, IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Добавь роль, если нужно: await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { Message = "User registered" });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized("Invalid login");
        }

        var accessToken = await GenerateAccessTokenAsync(user!);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);  // ← СТРОКА!
        var refreshToken = GenerateRefreshToken();

        // Сохрани refresh в БД
        var refreshEntity = new RefreshToken
        {
            Token = refreshToken,
            JwtId = accessToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value,
            UserId = user!.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!)),
            Device = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        };
        _context.RefreshTokens.Add(refreshEntity);
        await _context.SaveChangesAsync();

        // Cookies (Secure=false для dev)
        Response.Cookies.Append("accessToken", tokenString,  // ← tokenString, а не объект!
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // ← false для HTTP dev!
                SameSite = SameSiteMode.Lax,  // ← Lax вместо Strict
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!))
            });

        Response.Cookies.Append("refreshToken", refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // ← false для dev
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
            });

        // JSON с СТРОКОЙ токена
        return Ok(new
        {
            Message = "Login successful",
            AccessToken = tokenString,  // ← СТРОКА, а не объект!
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!) * 60
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("No refresh token");

        var refreshEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsUsed && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

        if (refreshEntity == null)
            return Unauthorized("Invalid refresh token");

        var user = await _userManager.FindByIdAsync(refreshEntity.UserId);
        if (user == null)
            return Unauthorized("User not found");

        // Ротация
        refreshEntity.IsUsed = true;
        var newRefreshToken = GenerateRefreshToken();
        refreshEntity.ReplacedByToken = newRefreshToken;
        await _context.SaveChangesAsync();

        var newAccessToken = await GenerateAccessTokenAsync(user);
        var newTokenString = new JwtSecurityTokenHandler().WriteToken(newAccessToken);  // ← СТРОКА!

        // Сохрани новый refresh
        var newRefreshEntity = new RefreshToken
        {
            Token = newRefreshToken,
            JwtId = newAccessToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!)),
            Device = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        };
        _context.RefreshTokens.Add(newRefreshEntity);
        await _context.SaveChangesAsync();

        // Обнови cookies
        Response.Cookies.Append("accessToken", newTokenString,  // ← СТРОКА!
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // dev
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!))
            });

        Response.Cookies.Append("refreshToken", newRefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
            });

        return Ok(new
        {
            Message = "Token refreshed",
            AccessToken = newTokenString,  // ← СТРОКА!
            RefreshToken = newRefreshToken,
            TokenType = "Bearer",
            ExpiresIn = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!) * 60
        });
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(user))
        {
            var refreshTokens = _context.RefreshTokens.Where(rt => rt.UserId == user);
            foreach (var rt in refreshTokens)
                rt.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");

        return Ok("Logged out");
    }

    // Вспомогательные методы
    private async Task<JwtSecurityToken> GenerateAccessTokenAsync(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  // Unique ID
        };

        // Добавь роли
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiryMinutes"]!)),
            signingCredentials: creds
        );
    }

    private string GenerateRefreshToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);  // Безопасный random
        return Convert.ToBase64String(tokenBytes);
    }
}
