using MafiaPedia.Api.DTOs.Phase2.Finance;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int clubId);
    Task<ProductDto> CreateAsync(int clubId, int userId, CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int clubId, int productId, int userId, UpdateProductDto dto);
    Task DeleteAsync(int clubId, int productId, int userId);
}
