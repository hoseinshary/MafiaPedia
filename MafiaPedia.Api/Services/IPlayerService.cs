using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IPlayerService
{
    Task<PlayerProfileDto?> GetProfileAsync(int playerId);
    Task<IEnumerable<PlayerSearchDto>> SearchPlayersAsync(string query, int limit = 10);
}
