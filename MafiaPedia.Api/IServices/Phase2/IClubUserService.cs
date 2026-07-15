using MafiaPedia.Api.DTOs.Phase2.Club;
using MafiaPedia.Api.Entities;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubUserService
{
    Task<List<ClubUserDto>> GetMembersAsync(int clubId);
    Task<ClubUserDto> CreateMemberAsync(int clubId, CreateClubUserDto dto, bool callerIsAdmin);
    Task<ClubUserDto?> UpdateMemberRoleAsync(int clubId, int clubUserId, UpdateClubUserRoleDto dto, bool callerIsAdmin);
    Task<bool> DeleteMemberAsync(int clubId, int clubUserId);
    Task<Clubuser?> GetClubUserAsync(int userId, int clubId);
    Task<List<ClubUserContextDto>> GetMyClubsAsync(int userId);
}
