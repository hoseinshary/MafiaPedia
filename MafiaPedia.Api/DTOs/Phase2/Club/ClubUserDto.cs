namespace MafiaPedia.Api.DTOs.Phase2.Club;

public record NewClubUserAccountDto(string Username, string Password, string Mobile, string? DisplayName);
public record CreateClubUserDto(string ClubuserRole, int? ExistingUserId, NewClubUserAccountDto? NewUser);
public record UpdateClubUserRoleDto(string ClubuserRole);
public record ClubUserDto(
    int Id, int UserId, string? UserDisplayName, string? UserMobile,
    string ClubuserRole, int ClubId,
    int? MasterId, string? MasterName
);
public record ClubUserContextDto(int ClubId, string ClubName, string ClubuserRole);
