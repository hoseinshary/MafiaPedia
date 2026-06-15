using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IPlayReadService
{
    Task<PlayListResponseDto> GetPlaysAsync(PlayFilterDto filter);
    Task<PlayDetailDto?> GetPlayByIdAsync(int playId);
    Task<HeadToHeadDto?> GetHeadToHeadAsync(int player1Id, int player2Id);
}
