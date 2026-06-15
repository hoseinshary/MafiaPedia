using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IStatisticsService
{
    Task<StatisticsDto> GetStatisticsAsync(StatisticsFilterDto filter);
    Task<StatisticsHomeDto> GetStatisticsHomeAsync();
}