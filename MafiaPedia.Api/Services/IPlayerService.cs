using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IPlayerService
{
    Task<PlayerProfileDto?> GetProfileAsync(int playerId);
}
