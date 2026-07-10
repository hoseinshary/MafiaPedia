using MafiaPedia.Api.DTOs.Phase2.Club;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubManagementService
{
    Task<List<ClubDto>> GetAllClubsAsync();
    Task<ClubDetailDto?> GetClubDetailAsync(int clubId);
    Task<ClubDto> CreateClubAsync(CreateClubDto dto);
    Task<ClubDto?> UpdateClubAsync(int clubId, UpdateClubDto dto);
    Task<bool> DeleteClubAsync(int clubId);

    Task<RoomDto> CreateRoomAsync(int clubId, CreateRoomDto dto);
    Task<RoomDto?> UpdateRoomAsync(int roomId, UpdateRoomDto dto);
    Task<bool> DeleteRoomAsync(int roomId);

    Task<MasterDto> CreateMasterAsync(int clubId, CreateMasterDto dto);
    Task<MasterDto?> UpdateMasterAsync(int masterId, UpdateMasterDto dto);
    Task<bool> DeleteMasterAsync(int masterId);

    Task<string?> SaveLogoAsync(int clubId, IFormFile file);
    Task<string?> SaveMasterPhotoAsync(int masterId, IFormFile file);
}
