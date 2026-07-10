using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IPlayWriteService
{
    Task<int> AddPlayAsync(CreatePlayDto dto);
    Task<bool> UpdatePlayAsync(int playId, UpdatePlayDto dto);
    Task<(bool Success, string? Error)> DeletePlayAsync(int playId);
}
