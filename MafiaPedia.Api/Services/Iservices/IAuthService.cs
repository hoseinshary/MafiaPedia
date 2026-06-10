using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task SendOtpAsync(SendOtpRequestDto dto);
    Task<bool> VerifyOtpAsync(VerifyOtpRequestDto dto);
}
