using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class ProductCategoryService : IProductCategoryService
{
    private readonly MafiaDbContext _context;

    public ProductCategoryService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategoryDto>> GetAllAsync(int clubId)
    {
        return await _context.ProductCategories
            .Where(pc => pc.ClubId == clubId && !pc.IsDeleted)
            .OrderBy(pc => pc.Name)
            .Select(pc => new ProductCategoryDto(pc.Id, pc.Name))
            .ToListAsync();
    }

    public async Task<ProductCategoryDto> CreateAsync(int clubId, int userId, CreateProductCategoryDto dto)
    {
        var category = new ProductCategory
        {
            ClubId = clubId,
            Name = dto.Name
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return new ProductCategoryDto(category.Id, category.Name);
    }

    public async Task<ProductCategoryDto> UpdateAsync(int clubId, int categoryId, UpdateProductCategoryDto dto)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(pc => pc.Id == categoryId && pc.ClubId == clubId && !pc.IsDeleted);

        if (category is null)
            throw new NotFoundAppException("دسته‌بندی مورد نظر یافت نشد");

        category.Name = dto.Name;
        await _context.SaveChangesAsync();

        return new ProductCategoryDto(category.Id, category.Name);
    }

    public async Task DeleteAsync(int clubId, int categoryId, int userId)
    {
        var category = await _context.ProductCategories
            .Include(pc => pc.Products)
            .FirstOrDefaultAsync(pc => pc.Id == categoryId && pc.ClubId == clubId && !pc.IsDeleted);

        if (category is null)
            throw new NotFoundAppException("دسته‌بندی مورد نظر یافت نشد");

        if (category.Products.Any(p => !p.IsDeleted))
            throw new ConflictAppException("این دسته‌بندی دارای محصول فعال است. ابتدا محصولات آن را حذف کنید");

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedByUserId = userId;

        await _context.SaveChangesAsync();
    }
}
