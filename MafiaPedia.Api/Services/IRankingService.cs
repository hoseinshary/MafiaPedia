using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetOverallRankingsAsync();
    Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter);
}
