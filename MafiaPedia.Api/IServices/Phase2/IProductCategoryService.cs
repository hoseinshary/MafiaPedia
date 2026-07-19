using MafiaPedia.Api.DTOs.Phase2.Finance;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IProductCategoryService
{
    Task<List<ProductCategoryDto>> GetAllAsync(int clubId);
    Task<ProductCategoryDto> CreateAsync(int clubId, int userId, CreateProductCategoryDto dto);
    Task<ProductCategoryDto> UpdateAsync(int clubId, int categoryId, UpdateProductCategoryDto dto);
    Task DeleteAsync(int clubId, int categoryId, int userId);
}
