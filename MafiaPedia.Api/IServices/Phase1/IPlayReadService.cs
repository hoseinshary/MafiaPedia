using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IPlayReadService
{
    Task<PlayListResponseDto> GetPlaysAsync(PlayFilterDto filter);
    Task<PlayDetailDto?> GetPlayByIdAsync(int playId);
    Task<HeadToHeadDto?> GetHeadToHeadAsync(int player1Id, int player2Id);
}
