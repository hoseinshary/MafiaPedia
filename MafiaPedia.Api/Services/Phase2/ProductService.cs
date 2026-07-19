using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class ProductService : IProductService
{
    private readonly MafiaDbContext _context;

    public ProductService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync(int clubId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.ClubId == clubId && !p.IsDeleted)
            .OrderBy(p => p.Category.Name)
            .ThenBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.CategoryId, p.Category.Name, p.Price, p.IsActive))
            .ToListAsync();
    }

    public async Task<ProductDto> CreateAsync(int clubId, int userId, CreateProductDto dto)
    {
        var category = await _context.ProductCategories
            .AnyAsync(pc => pc.Id == dto.CategoryId && pc.ClubId == clubId && !pc.IsDeleted);

        if (!category)
            throw new NotFoundAppException("دسته‌بندی مورد نظر یافت نشد");

        var product = new Product
        {
            ClubId = clubId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Price = dto.Price,
            IsActive = dto.IsActive ?? true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var catName = await _context.ProductCategories
            .Where(pc => pc.Id == dto.CategoryId)
            .Select(pc => pc.Name)
            .FirstOrDefaultAsync() ?? "";

        return new ProductDto(product.Id, product.Name, product.CategoryId, catName, product.Price, product.IsActive);
    }

    public async Task<ProductDto> UpdateAsync(int clubId, int productId, int userId, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && p.ClubId == clubId && !p.IsDeleted);

        if (product is null)
            throw new NotFoundAppException("محصول مورد نظر یافت نشد");

        var category = await _context.ProductCategories
            .AnyAsync(pc => pc.Id == dto.CategoryId && pc.ClubId == clubId && !pc.IsDeleted);

        if (!category)
            throw new NotFoundAppException("دسته‌بندی مورد نظر یافت نشد");

        product.Name = dto.Name;
        product.CategoryId = dto.CategoryId;
        product.Price = dto.Price;
        product.IsActive = dto.IsActive ?? true;

        await _context.SaveChangesAsync();

        var catName = await _context.ProductCategories
            .Where(pc => pc.Id == dto.CategoryId)
            .Select(pc => pc.Name)
            .FirstOrDefaultAsync() ?? "";

        return new ProductDto(product.Id, product.Name, product.CategoryId, catName, product.Price, product.IsActive);
    }

    public async Task DeleteAsync(int clubId, int productId, int userId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.ClubId == clubId && !p.IsDeleted);

        if (product is null)
            throw new NotFoundAppException("محصول مورد نظر یافت نشد");

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedByUserId = userId;
        product.IsActive = false;

        await _context.SaveChangesAsync();
    }
}
