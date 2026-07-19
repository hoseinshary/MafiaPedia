namespace MafiaPedia.Api.DTOs.Phase2.Finance;

public record CreateNerkhDto(string Name, decimal Price, bool IsDefault = false);

public record UpdateNerkhDto(string Name, decimal Price, bool IsDefault = false);

public record NerkhDto(int Id, string Name, decimal Price, bool IsDefault, bool? IsActive);
