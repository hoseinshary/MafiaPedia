using MafiaPedia.Api.DTOs.Phase2.ClubPlay;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubPlayService
{
    Task<ClubPlayDetailDto> CreateClubPlayAsync(int clubId, int masterId, int userId, CreateClubPlayDto dto);
    Task<ClubPlayDetailDto?> GetClubPlayDetailAsync(int clubId, int playId);
    Task<int> GetPlayCountByDateAsync(int clubId, int masterId, DateOnly date);

    // Status transitions
    Task<ClubPlayDetailDto?> ReshuffleRolesAsync(int clubId, int playId, int? restrictToMasterId);
    Task<ClubPlayDetailDto?> ConfirmRoleRevealAsync(int clubId, int playId, int? restrictToMasterId);
    Task<ClubPlayDetailDto?> SubmitWinnersideAsync(int clubId, int playId, int? restrictToMasterId, int winnersideId);
    Task<ClubPlayDetailDto?> SubmitRanksAsync(int clubId, int playId, int? restrictToMasterId, List<ParticipantRankDto> ranks);

    // Dashboard reads
    Task<List<ClubPlayListItemDto>> GetPlaysByBusinessDateAsync(int clubId, int masterId, DateOnly businessDate);
    Task<List<ClubPlayListItemDto>> GetClubPlaysByBusinessDateAsync(int clubId, DateOnly businessDate);
    Task<List<ClubPlayListItemDto>> GetOpenPlaysAsync(int clubId, int masterId);
    Task<(List<ClubPlayListItemDto> Items, int Total)> GetMyPlaysAsync(int clubId, int masterId, int page, int pageSize, DateTime? dateFrom, DateTime? dateTo, string? status);
    Task<MasterStatsDto> GetMyStatsAsync(int clubId, int masterId, string period);
    Task<MasterStatsDto> GetClubStatsAsync(int clubId, string period);
    Task<List<MasterPerformanceDto>> GetMasterPerformanceAsync(int clubId, string period);

    // Editing
    Task<ClubPlayDetailDto?> UpdateClubPlayAsync(int clubId, int playId, int? restrictToMasterId, string? actingClubRole, UpdateClubPlayDto dto);
    Task<ClubPlayParticipantDto> ReplaceParticipantAsync(
        int clubId, int playId, int? restrictToMasterId, int participantRowId, ReplaceParticipantDto dto);

    // Soft delete
    Task<bool> DeleteClubPlayAsync(int clubId, int playId, int actingUserId, bool isAdmin, int? restrictToMasterId);
    Task<(List<ClubPlayDeletedListItemDto> Items, int Total)> GetDeletedPlaysAsync(int clubId, int page, int pageSize);
}
