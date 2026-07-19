using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class FinanceAuditService : IFinanceAuditService
{
    private readonly MafiaDbContext _context;

    public FinanceAuditService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> GetDeletedRecordsAsync(int clubId, string type)
    {
        var validTypes = new[] { "nerkh", "product", "order", "settlement" };
        if (!validTypes.Contains(type))
            throw new ConflictAppException("نوع نامعتبر. گزینه‌های مجاز: nerkh, product, order, settlement");

        return type switch
        {
            "nerkh" => await GetDeletedNerkhsAsync(clubId),
            "product" => await GetDeletedProductsAsync(clubId),
            "order" => await GetDeletedOrdersAsync(clubId),
            "settlement" => await GetDeletedSettlementsAsync(clubId),
            _ => new List<object>()
        };
    }

    private async Task<List<object>> GetDeletedNerkhsAsync(int clubId)
    {
        return await _context.Nerkhs
            .Where(n => n.ClubId == clubId && n.IsDeleted)
            .Select(n => (object)new
            {
                n.Id,
                n.Name,
                n.Price,
                n.IsDefault,
                n.DeletedAt,
                n.DeletedByUserId,
                DeletedByDisplayName = n.DeletedByUser != null ? n.DeletedByUser.DisplayName : null
            })
            .ToListAsync();
    }

    private async Task<List<object>> GetDeletedProductsAsync(int clubId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.ClubId == clubId && p.IsDeleted)
            .Select(p => (object)new
            {
                p.Id,
                p.Name,
                CategoryName = p.Category.Name,
                p.Price,
                p.DeletedAt,
                p.DeletedByUserId,
                DeletedByDisplayName = p.DeletedByUser != null ? p.DeletedByUser.DisplayName : null
            })
            .ToListAsync();
    }

    private async Task<List<object>> GetDeletedOrdersAsync(int clubId)
    {
        return await _context.ClubOrders
            .Include(o => o.ClubPlayer)
            .Where(o => o.ClubId == clubId && o.IsDeleted)
            .Select(o => (object)new
            {
                o.Id,
                ClubPlayerName = o.ClubPlayer.Name,
                o.BusinessDate,
                o.DeletedAt,
                o.DeletedByUserId,
                DeletedByDisplayName = o.DeletedByUser != null ? o.DeletedByUser.DisplayName : null
            })
            .ToListAsync();
    }

    private async Task<List<object>> GetDeletedSettlementsAsync(int clubId)
    {
        return await _context.ClubSettlements
            .Include(s => s.ClubPlayer)
            .Where(s => s.ClubId == clubId && s.IsDeleted)
            .Select(s => (object)new
            {
                s.Id,
                ClubPlayerName = s.ClubPlayer.Name,
                s.Amount,
                s.PaymentMethod,
                s.DeletedAt,
                s.DeletedByUserId,
                DeletedByDisplayName = s.DeletedByUser != null ? s.DeletedByUser.DisplayName : null
            })
            .ToListAsync();
    }
}
