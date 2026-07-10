namespace MafiaPedia.Api.DTOs.Phase1;

public class CreatePlayerDto
{
    public string Name { get; set; } = null!;
    public string? Mobile { get; set; }
    public string? Code { get; set; }
    public string? Birthday { get; set; }
    public string? Desc { get; set; }
    public IFormFile? Picture { get; set; }
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Picture { get; set; }
}
