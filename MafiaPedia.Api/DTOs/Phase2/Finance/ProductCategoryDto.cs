namespace MafiaPedia.Api.DTOs.Phase2.Finance;

public record CreateProductCategoryDto(string Name);

public record UpdateProductCategoryDto(string Name);

public record ProductCategoryDto(int Id, string Name);
