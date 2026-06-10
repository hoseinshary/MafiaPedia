using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IUserService
{
    Task<UserListResponseDto> GetUsersAsync(int page, int pageSize, string? search);
    Task<UserDetailDto?> GetUserDetailAsync(int userId);
    Task<UserDetailDto> CreateUserAsync(CreateUserDto dto);
    Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task<(bool Success, string? Error)> DeleteUserAsync(int userId);
}
