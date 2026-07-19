using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubOrderService : IClubOrderService
{
    private readonly MafiaDbContext _context;

    public ClubOrderService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<ClubOrderDto> CreateAsync(int clubId, int userId, CreateClubOrderDto dto)
    {
        var clubPlayerExists = await _context.Clubplayers
            .AnyAsync(cp => cp.Id == dto.ClubPlayerId);

        if (!clubPlayerExists)
            throw new NotFoundAppException("مشتری مورد نظر یافت نشد");

        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == dto.ClubPlayerId);

        if (!isMember)
            throw new ConflictAppException("این مشتری عضو این کافه نیست");

        if (dto.Items is null || dto.Items.Count == 0)
            throw new ConflictAppException("سفارش باید حداقل یک آیتم داشته باشد");

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.ClubId == clubId && !p.IsDeleted && p.IsActive == true)
            .ToDictionaryAsync(p => p.Id);

        var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count != 0)
            throw new NotFoundAppException($"محصولات زیر یافت نشد یا غیرفعال هستند: {string.Join("، ", missing)}");

        var businessDate = BusinessDateHelper.Today();

        var order = new ClubOrder
        {
            ClubId = clubId,
            ClubPlayerId = dto.ClubPlayerId,
            BusinessDate = businessDate,
            RegisteredByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = "open"
        };

        _context.ClubOrders.Add(order);
        await _context.SaveChangesAsync();

        List<OrderItemDto> items = new();
        foreach (var item in dto.Items)
        {
            var product = products[item.ProductId];
            var qty = item.Quantity > 0 ? item.Quantity : 1;

            var orderItem = new ClubOrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = qty,
                UnitPrice = product.Price
            };

            _context.ClubOrderItems.Add(orderItem);

            items.Add(new OrderItemDto(
                orderItem.Id,
                item.ProductId,
                product.Name,
                qty,
                product.Price,
                qty * product.Price
            ));
        }

        await _context.SaveChangesAsync();

        var total = items.Sum(i => i.LineTotal);
        await RecalculateOrderStatusAsync(order.Id);

        var clubPlayerName = await _context.Clubplayers
            .Where(cp => cp.Id == dto.ClubPlayerId)
            .Select(cp => cp.Name)
            .FirstOrDefaultAsync() ?? "";

        return new ClubOrderDto(order.Id, dto.ClubPlayerId, clubPlayerName, businessDate, total, "open", items);
    }

    public async Task<ClubOrderDto> GetByIdAsync(int clubId, int orderId)
    {
        var order = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ClubPlayer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order is null)
            throw new NotFoundAppException("سفارش مورد نظر یافت نشد");

        if (order.ClubId != clubId)
            throw new ForbiddenAppException();

        var items = order.ClubOrderItems
            .Where(oi => !oi.IsDeleted)
            .Select(oi => new OrderItemDto(
                oi.Id,
                oi.ProductId,
                oi.Product.Name ?? "",
                oi.Quantity,
                oi.UnitPrice,
                oi.Quantity * oi.UnitPrice
            ))
            .ToList();

        var total = items.Sum(i => i.LineTotal);

        return new ClubOrderDto(
            order.Id,
            order.ClubPlayerId,
            order.ClubPlayer.Name ?? "",
            order.BusinessDate,
            total,
            order.Status,
            items
        );
    }

    public async Task<List<ClubOrderDto>> GetByClubPlayerAndDateAsync(int clubId, int clubPlayerId, DateOnly businessDate)
    {
        var orders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ClubPlayer)
            .Where(o => o.ClubId == clubId
                     && o.ClubPlayerId == clubPlayerId
                     && o.BusinessDate == businessDate
                     && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o =>
        {
            var items = o.ClubOrderItems
                .Where(oi => !oi.IsDeleted)
                .Select(oi => new OrderItemDto(
                    oi.Id,
                    oi.ProductId,
                    oi.Product.Name ?? "",
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice
                ))
                .ToList();
            var total = items.Sum(i => i.LineTotal);
            return new ClubOrderDto(
                o.Id,
                o.ClubPlayerId,
                o.ClubPlayer.Name ?? "",
                o.BusinessDate,
                total,
                o.Status,
                items
            );
        }).ToList();
    }

    public async Task<List<ClubOrderDto>> GetOpenOrdersForCustomerAsync(int clubId, int clubPlayerId)
    {
        var today = BusinessDateHelper.Today();

        var orders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ClubPlayer)
            .Where(o => o.ClubId == clubId
                     && o.ClubPlayerId == clubPlayerId
                     && o.BusinessDate == today
                     && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o =>
        {
            var items = o.ClubOrderItems
                .Where(oi => !oi.IsDeleted)
                .Select(oi => new OrderItemDto(
                    oi.Id,
                    oi.ProductId,
                    oi.Product.Name ?? "",
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice
                ))
                .ToList();
            var total = items.Sum(i => i.LineTotal);
            return new ClubOrderDto(
                o.Id,
                o.ClubPlayerId,
                o.ClubPlayer.Name ?? "",
                o.BusinessDate,
                total,
                o.Status,
                items
            );
        }).ToList();
    }

    public async Task<List<ClubOrderListItemDto>> SearchAsync(int clubId, string? query, DateOnly? fromDate, DateOnly? toDate, int limit = 200)
    {
        // ── 1. Order rows ──
        var orderQuery = _context.ClubOrders
            .Include(o => o.ClubPlayer)
            .Include(o => o.ClubOrderItems)
            .Where(o => o.ClubId == clubId && !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim();
            orderQuery = orderQuery.Where(o => o.ClubPlayer.Name.Contains(trimmed)
                                            || o.ClubPlayer.Mobile.Contains(trimmed));
        }

        if (fromDate.HasValue)
            orderQuery = orderQuery.Where(o => o.BusinessDate >= fromDate.Value);

        if (toDate.HasValue)
            orderQuery = orderQuery.Where(o => o.BusinessDate <= toDate.Value);

        var orders = await orderQuery
            .OrderByDescending(o => o.BusinessDate)
            .ThenByDescending(o => o.Id)
            .Take(limit)
            .ToListAsync();

        var orderRows = orders.Select(o =>
        {
            var items = o.ClubOrderItems.Where(oi => !oi.IsDeleted).ToList();
            var total = items.Sum(oi => oi.Quantity * oi.UnitPrice);
            return new ClubOrderListItemDto(
                o.Id,
                o.ClubPlayerId,
                o.ClubPlayer.Name ?? "",
                o.ClubPlayer.Mobile ?? "",
                o.BusinessDate,
                items.Count,
                total,
                o.Status
            );
        }).ToList();

        // Track (clubPlayerId, businessDate) combos that already have order rows
        var orderKeys = new HashSet<(int, DateOnly)>();
        foreach (var r in orderRows)
            if (r.OrderId.HasValue)
                orderKeys.Add((r.ClubPlayerId, r.BusinessDate));

        // ── 2. Game-only rows (clubplayplayer entries with no corresponding order) ──
        var defaultNerkhPrice = await _context.Nerkhs
            .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
            .Select(n => (decimal?)n.Price)
            .FirstOrDefaultAsync() ?? 0m;

        var gameQuery = _context.Clubplayplayers
            .Include(pp => pp.Play).ThenInclude(p => p.Nerkh)
            .Include(pp => pp.Player)
            .Where(pp => pp.Play.Room.ClubId == clubId
                      && !pp.Play.IsDeleted
                      && !pp.IsGuest
                      && pp.Play.BusinessDate != null);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var trimmed = query.Trim();
            gameQuery = gameQuery.Where(pp => pp.Player.Name.Contains(trimmed)
                                           || pp.Player.Mobile.Contains(trimmed));
        }

        if (fromDate.HasValue)
            gameQuery = gameQuery.Where(pp => pp.Play.BusinessDate >= fromDate.Value);

        if (toDate.HasValue)
            gameQuery = gameQuery.Where(pp => pp.Play.BusinessDate <= toDate.Value);

        var participants = await gameQuery.ToListAsync();

        var gameGroups = participants
            .GroupBy(pp => (
                pp.PlayerId,
                pp.Player.Name ?? "",
                pp.Player.Mobile ?? "",
                BusinessDate: pp.Play.BusinessDate!.Value
            ))
            .Select(g => new
            {
                ClubPlayerId = g.Key.PlayerId,
                ClubPlayerName = g.Key.Item2,
                ClubPlayerMobile = g.Key.Item3,
                BusinessDate = g.Key.BusinessDate,
                GameCount = g.Count(),
                Total = g.Sum(pp => pp.Play.Nerkh?.Price ?? defaultNerkhPrice)
            })
            .ToList();

        var gameOnlyRows = gameGroups
            .Where(g => !orderKeys.Contains((g.ClubPlayerId, g.BusinessDate)))
            .Select(g => new ClubOrderListItemDto(
                null,
                g.ClubPlayerId,
                g.ClubPlayerName,
                g.ClubPlayerMobile,
                g.BusinessDate,
                g.GameCount,
                g.Total,
                "game_only"
            ))
            .ToList();

        // ── 3. Merge and sort ──
        var allRows = orderRows
            .Concat(gameOnlyRows)
            .OrderByDescending(r => r.BusinessDate)
            .ThenBy(r => r.ClubPlayerName)
            .Take(limit)
            .ToList();

        return allRows;
    }

    public async Task DeleteAsync(int clubId, int orderId, int userId)
    {
        var order = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ClubId == clubId && !o.IsDeleted);

        if (order is null)
            throw new NotFoundAppException("سفارش مورد نظر یافت نشد");

        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        order.DeletedByUserId = userId;

        foreach (var item in order.ClubOrderItems.Where(i => !i.IsDeleted))
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserId = userId;
        }

        await _context.SaveChangesAsync();
    }

    // ── New item-level operations ──

    public async Task<AddItemResponseDto> AddItemAsync(int clubId, int userId, int? orderId, int clubPlayerId, int productId, int quantity, bool forceNewOrder)
    {
        if (quantity <= 0)
            throw new ConflictAppException("تعداد باید بزرگتر از صفر باشد");

        var clubPlayerExists = await _context.Clubplayers.AnyAsync(cp => cp.Id == clubPlayerId);
        if (!clubPlayerExists)
            throw new NotFoundAppException("مشتری مورد نظر یافت نشد");

        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == clubPlayerId);
        if (!isMember)
            throw new ConflictAppException("این مشتری عضو این کافه نیست");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.ClubId == clubId && !p.IsDeleted && p.IsActive == true);
        if (product is null)
            throw new NotFoundAppException("محصول مورد نظر یافت نشد یا غیرفعال است");

        var today = BusinessDateHelper.Today();
        bool wasSettled = false;

        ClubOrder order;
        if (orderId.HasValue)
        {
            order = await _context.ClubOrders
                .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.ClubId == clubId && !o.IsDeleted);
            if (order is null)
                throw new NotFoundAppException("سفارش مورد نظر یافت نشد");

            if (order.ClubPlayerId != clubPlayerId)
                throw new ConflictAppException("این سفارش متعلق به این مشتری نیست");

            if (order.Status == "settled")
                wasSettled = true;
        }
        else
        {
            order = await ResolveOrCreateOpenOrderInternalAsync(clubId, clubPlayerId, userId, today, forceNewOrder);
        }

        // Check if same product already exists in this order (non-deleted)
        var existingItem = await _context.ClubOrderItems
            .FirstOrDefaultAsync(oi => oi.OrderId == order.Id && oi.ProductId == productId && !oi.IsDeleted);

        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var newItem = new ClubOrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            _context.ClubOrderItems.Add(newItem);
        }

        await _context.SaveChangesAsync();
        await RecalculateOrderStatusAsync(order.Id);

        // Reload to get updated total and status
        var freshOrder = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == order.Id);

        var orderTotal = freshOrder.ClubOrderItems
            .Where(oi => !oi.IsDeleted)
            .Sum(oi => oi.Quantity * oi.UnitPrice);

        var addedItem = await _context.ClubOrderItems
            .Include(oi => oi.Product)
            .FirstAsync(oi => oi.OrderId == order.Id && oi.ProductId == productId && !oi.IsDeleted);

        string? warning = wasSettled
            ? "این سفارش قبلاً تسویه شده بود. آیتم جدید به آن اضافه شد."
            : null;

        return new AddItemResponseDto(
            addedItem.Id,
            order.Id,
            orderTotal,
            freshOrder.Status,
            wasSettled,
            warning
        );
    }

    public async Task<UpdateItemQuantityResponseDto> UpdateItemQuantityAsync(int clubId, int userId, int orderItemId, int newQuantity)
    {
        var item = await _context.ClubOrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && !oi.IsDeleted);

        if (item is null)
            throw new NotFoundAppException("آیتم سفارش یافت نشد");

        if (item.Order.ClubId != clubId)
            throw new ForbiddenAppException();

        bool wasSettled = item.Order.Status == "settled";

        if (newQuantity <= 0)
        {
            // Soft-delete the item
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserId = userId;
        }
        else
        {
            item.Quantity = newQuantity;
        }

        await _context.SaveChangesAsync();
        await RecalculateOrderStatusAsync(item.OrderId);

        var freshOrder = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == item.OrderId);

        var orderTotal = freshOrder.ClubOrderItems
            .Where(oi => !oi.IsDeleted)
            .Sum(oi => oi.Quantity * oi.UnitPrice);

        return new UpdateItemQuantityResponseDto(
            item.Id,
            newQuantity > 0 ? newQuantity : 0,
            orderTotal,
            freshOrder.Status,
            wasSettled
        );
    }

    public async Task<RemoveItemResponseDto> RemoveItemAsync(int clubId, int userId, int orderItemId)
    {
        var item = await _context.ClubOrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && !oi.IsDeleted);

        if (item is null)
            throw new NotFoundAppException("آیتم سفارش یافت نشد");

        if (item.Order.ClubId != clubId)
            throw new ForbiddenAppException();

        bool wasSettled = item.Order.Status == "settled";

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedByUserId = userId;
        await _context.SaveChangesAsync();

        await RecalculateOrderStatusAsync(item.OrderId);

        var freshOrder = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == item.OrderId);

        var orderTotal = freshOrder.ClubOrderItems
            .Where(oi => !oi.IsDeleted)
            .Sum(oi => oi.Quantity * oi.UnitPrice);

        return new RemoveItemResponseDto(
            item.Id,
            item.OrderId,
            orderTotal,
            freshOrder.Status,
            wasSettled
        );
    }

    public async Task RecalculateOrderStatusAsync(int orderId)
    {
        var order = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
            .Include(o => o.ClubSettlements)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null) return;

        var orderTotal = order.ClubOrderItems
            .Where(oi => !oi.IsDeleted)
            .Sum(oi => oi.Quantity * oi.UnitPrice);

        var orderPaid = order.ClubSettlements
            .Where(s => !s.IsDeleted)
            .Sum(s => s.Amount);

        if (orderPaid <= 0m)
        {
            order.Status = "open";
            order.SettledAt = null;
        }
        else if (orderPaid < orderTotal)
        {
            order.Status = "partial";
            order.SettledAt = null;
        }
        else
        {
            order.Status = "settled";
            if (order.SettledAt is null)
                order.SettledAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // ── Internal helpers ──

    private async Task<ClubOrder> ResolveOrCreateOpenOrderInternalAsync(int clubId, int clubPlayerId, int userId, DateOnly today, bool forceNew)
    {
        if (!forceNew)
        {
            var existing = await _context.ClubOrders
                .Where(o => o.ClubId == clubId
                         && o.ClubPlayerId == clubPlayerId
                         && o.BusinessDate == today
                         && !o.IsDeleted
                         && (o.Status == "open" || o.Status == "partial"))
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing is not null)
                return existing;
        }

        var order = new ClubOrder
        {
            ClubId = clubId,
            ClubPlayerId = clubPlayerId,
            BusinessDate = today,
            RegisteredByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = "open"
        };

        _context.ClubOrders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }
}