using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetOverallRankingsAsync(int? clubId = null);
    Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter);
}
