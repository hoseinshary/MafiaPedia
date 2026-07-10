using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task SendOtpAsync(SendOtpRequestDto dto);
    Task<bool> VerifyOtpAsync(VerifyOtpRequestDto dto);
}
