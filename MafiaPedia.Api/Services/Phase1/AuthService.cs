using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase1;

public class AuthService : IAuthService
{
    private readonly MafiaDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(MafiaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            throw new InvalidOperationException("Username already exists.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Mobile = dto.Mobile,
            DisplayName = PersianTextNormalizer.Normalize(dto.DisplayName ?? dto.Username),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (refreshToken is null || refreshToken.IsRevoked == true || refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(refreshToken.User);
    }

    public async Task SendOtpAsync(SendOtpRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Mobile == dto.Mobile);
        if (user is null)
            throw new InvalidOperationException("Mobile not found.");

        user.OtpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Mobile == dto.Mobile);
        if (user is null || user.OtpCode != dto.OtpCode || user.OtpExpiresAt < DateTime.UtcNow)
            return false;

        user.MobileVerified = true;
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes"))
        };
    }

    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "User"),
            new Claim("mobile", user.Mobile ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                jwtSettings.GetValue<int>("AccessTokenExpiryMinutes")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var tokenString = Convert.ToBase64String(tokenBytes);

        var expiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays");

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return tokenString;
    }
}
