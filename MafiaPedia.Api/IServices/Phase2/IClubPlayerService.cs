using MafiaPedia.Api.DTOs.Phase2.ClubPlayer;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubPlayerService
{
    Task<(List<ClubPlayerDto> Items, int Total)> GetClubPlayersAsync(int clubId, int page, int pageSize, string? search);

    Task<ClubPlayerDto?> GetClubPlayerDetailAsync(int clubId, int customerId);

    Task<ClubPlayerDto?> SearchByMobileAsync(string mobile);

    Task<ClubPlayerJoinResultDto> CreateOrJoinAsync(int clubId, CreateOrJoinClubPlayerDto dto, string? picturePath);

    Task<ClubPlayerDto> UpdateClubPlayerAsync(int customerId, UpdateClubPlayerDto dto, string? newPicturePath);

    Task RemoveFromClubAsync(int clubId, int customerId);

    Task<CustomerSearchResultDto> SearchAllAsync(int clubId, string query);
}
