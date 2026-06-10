using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IPlayReadService
{
    Task<PlayListResponseDto> GetPlaysAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<PlayDetailDto?> GetPlayByIdAsync(int playId);
}
