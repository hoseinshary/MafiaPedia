using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IPlayWriteService
{
    Task<int> AddPlayAsync(CreatePlayDto dto);
    Task<bool> UpdatePlayAsync(int playId, UpdatePlayDto dto);
    Task<(bool Success, string? Error)> DeletePlayAsync(int playId);
}
