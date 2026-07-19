using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class NerkhService : INerkhService
{
    private readonly MafiaDbContext _context;

    public NerkhService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<NerkhDto>> GetAllAsync(int clubId)
    {
        return await _context.Nerkhs
            .Where(n => n.ClubId == clubId && !n.IsDeleted)
            .OrderByDescending(n => n.IsDefault)
            .ThenBy(n => n.Name)
            .Select(n => new NerkhDto(n.Id, n.Name, n.Price, n.IsDefault, n.IsActive))
            .ToListAsync();
    }

    public async Task<NerkhDto> CreateAsync(int clubId, int userId, CreateNerkhDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (dto.IsDefault)
            {
                var currentDefault = await _context.Nerkhs
                    .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
                    .FirstOrDefaultAsync();

                if (currentDefault is not null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            var nerkh = new Nerkh
            {
                ClubId = clubId,
                Name = dto.Name,
                Price = dto.Price,
                IsDefault = dto.IsDefault,
                IsActive = true
            };

            _context.Nerkhs.Add(nerkh);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new NerkhDto(nerkh.Id, nerkh.Name, nerkh.Price, nerkh.IsDefault, nerkh.IsActive);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<NerkhDto> UpdateAsync(int clubId, int nerkhId, int userId, UpdateNerkhDto dto)
    {
        var nerkh = await _context.Nerkhs
            .FirstOrDefaultAsync(n => n.Id == nerkhId && n.ClubId == clubId && !n.IsDeleted);

        if (nerkh is null)
            throw new NotFoundAppException("نرخ مورد نظر یافت نشد");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (dto.IsDefault && !nerkh.IsDefault)
            {
                var currentDefault = await _context.Nerkhs
                    .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted && n.Id != nerkhId)
                    .FirstOrDefaultAsync();

                if (currentDefault is not null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            nerkh.Name = dto.Name;
            nerkh.Price = dto.Price;
            nerkh.IsDefault = dto.IsDefault;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new NerkhDto(nerkh.Id, nerkh.Name, nerkh.Price, nerkh.IsDefault, nerkh.IsActive);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int clubId, int nerkhId, int userId)
    {
        var nerkh = await _context.Nerkhs
            .FirstOrDefaultAsync(n => n.Id == nerkhId && n.ClubId == clubId && !n.IsDeleted);

        if (nerkh is null)
            throw new NotFoundAppException("نرخ مورد نظر یافت نشد");

        if (nerkh.IsDefault)
            throw new ConflictAppException("نرخ پیش‌فرض را نمی‌توان حذف کرد. لطفاً ابتدا یک نرخ دیگر را به‌عنوان پیش‌فرض انتخاب کنید");

        nerkh.IsDeleted = true;
        nerkh.DeletedAt = DateTime.UtcNow;
        nerkh.DeletedByUserId = userId;
        nerkh.IsActive = false;

        await _context.SaveChangesAsync();
    }
}
