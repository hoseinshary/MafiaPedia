using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IPlayerService
{
    Task<PlayerProfileDto?> GetProfileAsync(int playerId);
    Task<IEnumerable<PlayerSearchDto>> SearchPlayersAsync(string query, int limit = 10);
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto);
    Task<PlayerListResponseDto> GetPlayersAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<bool> UpdatePlayerAsync(int playerId, UpdatePlayerDto dto);
    Task<PlayerDetailDto?> GetPlayerDetailAsync(int playerId);
    Task<(bool Success, string? Error)> DeletePlayerAsync(int playerId);
}
