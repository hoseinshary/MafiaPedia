namespace MafiaPedia.Api.DTOs.Phase2.Club;

public record CreateClubDto(string Name, string? Address, string? Phone, string? City, string? Description);
public record UpdateClubDto(string? Name, string? Address, string? Phone, string? City, string? Description, decimal? VatPercent);
public record ClubDto(int Id, string Name, string? Address, string? Phone, string? City, string? Description, string? Logo, decimal? VatPercent);

public record CreateRoomDto(string Name, bool IsActive = true);
public record UpdateRoomDto(string? Name, bool? IsActive);
public record RoomDto(int Id, string Name, int ClubId, bool IsActive);

public record NewMasterUserDto(string Username, string Password, string Mobile, string? DisplayName);
public record CreateMasterDto(string Name, decimal? RatePerGame, int? ExistingUserId, NewMasterUserDto? NewUser);
public record UpdateMasterDto(string? Name, decimal? RatePerGame, int? ExistingUserId, bool UnlinkUser = false);

public record MasterDto(
    int Id, string Name, int ClubId,
    int? UserId, string? UserDisplayName, string? UserMobile,
    decimal? RatePerGame, string? Photo, string? Bio
);

public record ClubDetailDto(
    int Id, string Name,
    string? Address, string? Phone, string? City, string? Description, string? Logo, decimal? VatPercent,
    List<RoomDto> Rooms, List<MasterDto> Masters
);
