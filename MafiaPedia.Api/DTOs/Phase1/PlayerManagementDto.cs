namespace MafiaPedia.Api.DTOs.Phase1;

public class PlayerListDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Picture { get; set; }
    public string? Mobile { get; set; }
    public int TotalGames { get; set; }
}

public class PlayerListResponseDto
{
    public List<PlayerListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UpdatePlayerDto
{
    public string? Name { get; set; }
    public string? Mobile { get; set; }
    public string? Code { get; set; }
    public string? Birthday { get; set; }
    public string? Desc { get; set; }
    public IFormFile? Picture { get; set; }
}

public class PlayerDetailDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Mobile { get; set; }
    public string? Code { get; set; }
    public string? Birthday { get; set; }
    public string? Desc { get; set; }
    public string? Picture { get; set; }
}
