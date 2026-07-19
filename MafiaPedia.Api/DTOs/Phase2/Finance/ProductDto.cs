namespace MafiaPedia.Api.DTOs.Phase2.Finance;

public record CreateProductDto(string Name, int CategoryId, decimal Price, bool? IsActive = true);

public record UpdateProductDto(string Name, int CategoryId, decimal Price, bool? IsActive = true);

public record ProductDto(int Id, string Name, int CategoryId, string CategoryName, decimal Price, bool? IsActive);
