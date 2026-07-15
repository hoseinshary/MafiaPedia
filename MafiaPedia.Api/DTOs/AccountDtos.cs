namespace MafiaPedia.Api.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string Role { get; set; } = null!;
    public LinkedPlayerDto? Player { get; set; }
    public LinkedClubplayerDto? Clubplayer { get; set; }
    public LinkedMasterDto? Master { get; set; }
}

public class LinkedPlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Picture { get; set; }
}

public class LinkedClubplayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Picture { get; set; }
}

public class LinkedMasterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Photo { get; set; }
}

public class UpdateAccountDto
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
}

public class ChangePasswordDto
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
