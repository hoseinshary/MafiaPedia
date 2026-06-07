using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IPlayWriteService
{
    Task<int> AddPlayAsync(CreatePlayDto dto);
}
