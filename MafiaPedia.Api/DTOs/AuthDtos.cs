namespace MafiaPedia.Api.DTOs;

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class SendOtpRequestDto
{
    public string Mobile { get; set; } = string.Empty;
}

public class VerifyOtpRequestDto
{
    public string Mobile { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
