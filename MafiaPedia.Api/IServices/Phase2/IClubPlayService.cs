using MafiaPedia.Api.DTOs.Phase2.ClubPlay;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubPlayService
{
    Task<ClubPlayDetailDto> CreateClubPlayAsync(int clubId, int masterId, int userId, CreateClubPlayDto dto);
    Task<ClubPlayDetailDto?> GetClubPlayDetailAsync(int clubId, int playId);
    Task<int> GetPlayCountByDateAsync(int clubId, int masterId, DateOnly date);
    Task<ClubPlayDetailDto?> ReshuffleRolesAsync(int clubId, int playId, int masterId);

    // Status transitions
    Task<ClubPlayDetailDto?> ConfirmRoleRevealAsync(int clubId, int playId, int masterId);
    Task<ClubPlayDetailDto?> SubmitWinnersideAsync(int clubId, int playId, int masterId, int winnersideId);
    Task<ClubPlayDetailDto?> SubmitRanksAsync(int clubId, int playId, int masterId, List<ParticipantRankDto> ranks);

    // Dashboard reads
    Task<List<ClubPlayListItemDto>> GetPlaysByBusinessDateAsync(int clubId, int masterId, DateOnly businessDate);
    Task<List<ClubPlayListItemDto>> GetOpenPlaysAsync(int clubId, int masterId);
    Task<(List<ClubPlayListItemDto> Items, int Total)> GetMyPlaysAsync(int clubId, int masterId, int page, int pageSize, DateTime? dateFrom, DateTime? dateTo, string? status);
    Task<MasterStatsDto> GetMyStatsAsync(int clubId, int masterId, string period);

    // Editing
    Task<ClubPlayDetailDto?> UpdateClubPlayAsync(int clubId, int playId, int masterId, UpdateClubPlayDto dto);
}
