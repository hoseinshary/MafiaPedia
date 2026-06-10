namespace MafiaPedia.Api.DTOs;

public class UserListDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Mobile { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserListResponseDto
{
    public List<UserListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserDetailDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Mobile { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? MobileVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Mobile { get; set; }
    public string Role { get; set; } = "user";
}

public class UpdateUserDto
{
    public string? DisplayName { get; set; }
    public string? Mobile { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; }
}
