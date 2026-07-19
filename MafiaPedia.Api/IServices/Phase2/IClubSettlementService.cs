using MafiaPedia.Api.DTOs.Phase2.Finance;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubSettlementService
{
    Task<ClubPlayerBalanceDto> GetBalanceAsync(int clubId, int clubPlayerId, DateOnly? businessDate = null);
    Task<SettlementDto> CreateAsync(int clubId, int userId, CreateSettlementDto dto);
    Task<LedgerResponseDto> GetLedgerAsync(int clubId, int clubPlayerId);
    Task DeleteAsync(int clubId, int settlementId, int userId);
    Task<List<TodayOverviewDto>> GetTodayOverviewAsync(int clubId, string status, string? date = null);
    Task<List<DebtorDto>> GetDebtorsAsync(int clubId);
}