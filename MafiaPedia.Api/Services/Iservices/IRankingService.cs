using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetOverallRankingAsync(OverallRankingFilterDto filter);
    Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter);
}
