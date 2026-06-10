using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetOverallRankingsAsync(int? clubId = null);
    Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter);
}
