namespace MafiaPedia.Api.DTOs.Phase1;

public class DropdownDto
{
    public List<DropdownItemDto> Clubs { get; set; } = new();
    public List<DropdownItemDto> Events { get; set; } = new();
    public List<DropdownItemDto> Senarios { get; set; } = new();
    public List<DropdownItemDto> Sides { get; set; } = new();
    public List<DropdownItemDto> Masters { get; set; } = new();
    public List<DropdownItemDto> Rooms { get; set; } = new();
    public List<DropdownEventDto> EventsWithClub { get; set; } = new();
}

public class DropdownItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class DropdownEventDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? ClubId { get; set; }
}
