using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IStatisticsService
{
    Task<StatisticsDto> GetStatisticsAsync(StatisticsFilterDto filter);
    Task<StatisticsHomeDto> GetStatisticsHomeAsync();
}