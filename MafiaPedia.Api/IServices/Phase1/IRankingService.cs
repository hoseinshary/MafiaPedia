using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetOverallRankingAsync(OverallRankingFilterDto filter);
    Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter);
}
