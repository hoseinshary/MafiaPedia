using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubSettlementService : IClubSettlementService
{
    private readonly MafiaDbContext _context;
    private readonly IClubOrderService _clubOrderService;

    public ClubSettlementService(MafiaDbContext context, IClubOrderService clubOrderService)
    {
        _context = context;
        _clubOrderService = clubOrderService;
    }

    public async Task<ClubPlayerBalanceDto> GetBalanceAsync(int clubId, int clubPlayerId, DateOnly? businessDate = null)
    {
        var clubPlayer = await _context.Clubplayers
            .FirstOrDefaultAsync(cp => cp.Id == clubPlayerId);

        if (clubPlayer is null)
            throw new NotFoundAppException("مشتری مورد نظر یافت نشد");

        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == clubPlayerId);

        if (!isMember)
            throw new ConflictAppException("این مشتری عضو این کافه نیست");

        var targetDate = businessDate ?? BusinessDateHelper.Today();

        // ── Games for target date ──
        var dayPlayIds = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate == targetDate)
            .Select(p => p.Id)
            .ToListAsync();

        var dayPlayParticipants = await _context.Clubplayplayers
            .Include(pp => pp.Play)
                .ThenInclude(p => p.Nerkh)
            .Include(pp => pp.Play)
                .ThenInclude(p => p.Room)
            .Where(pp => dayPlayIds.Contains(pp.PlayId)
                      && pp.PlayerId == clubPlayerId
                      && !pp.IsGuest)
            .ToListAsync();

        var defaultNerkhPrice = await _context.Nerkhs
            .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
            .Select(n => (decimal?)n.Price)
            .FirstOrDefaultAsync() ?? 0m;

        var dayGames = dayPlayParticipants.Select(pp =>
        {
            var nerkh = pp.Play.Nerkh;
            var price = nerkh is not null ? nerkh.Price : defaultNerkhPrice;

            return new BalanceGameItemDto(
                pp.PlayId,
                pp.Play.Title,
                nerkh?.Name ?? "نرخ پیش‌فرض",
                price,
                pp.Play.Room?.Name
            );
        }).ToList();

        var dayGamesTotal = dayGames.Sum(g => g.Price);

        // ── Orders for target date ──
        var dayOrders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.ClubId == clubId
                     && o.ClubPlayerId == clubPlayerId
                     && o.BusinessDate == targetDate
                     && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var dayOrderDtos = dayOrders.Select(o => new BalanceOrderItemDto(
            o.Id,
            o.ClubOrderItems
                .Where(oi => !oi.IsDeleted)
                .Select(oi => new OrderItemDto(
                    oi.Id,
                    oi.ProductId,
                    oi.Product.Name ?? "",
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice
                ))
                .ToList()
        )).ToList();

        var dayOrdersTotal = dayOrders.Sum(o =>
            o.ClubOrderItems.Where(oi => !oi.IsDeleted).Sum(oi => oi.Quantity * oi.UnitPrice));

        // ── VAT ──
        var vatPercent = await GetClubVatPercentAsync(clubId);

        var daySubtotal = dayGamesTotal + dayOrdersTotal;
        var dayDue = ApplyVat(daySubtotal, vatPercent);
        var vatAmount = dayDue - daySubtotal;

        // ── Previous balance (always based on real "now" — NOT the requested date) ──
        var realToday = BusinessDateHelper.Today();
        var prevGamesTotal = await CalculatePreviousGamesBalanceAsync(clubId, clubPlayerId, realToday);
        var prevOrdersTotal = await CalculatePreviousOrdersBalanceAsync(clubId, clubPlayerId, realToday);
        var prevPaymentsTotal = await CalculatePreviousPaymentsAsync(clubId, clubPlayerId, realToday);

        var previousBalance = ApplyVat(prevGamesTotal + prevOrdersTotal, vatPercent) - prevPaymentsTotal;

        var totalDue = previousBalance + dayDue;

        return new ClubPlayerBalanceDto(
            clubPlayerId,
            clubPlayer.Name,
            dayGames,
            dayOrderDtos,
            dayGamesTotal,
            dayOrdersTotal,
            previousBalance,
            totalDue,
            daySubtotal,
            vatPercent,
            vatAmount,
            dayDue
        );
    }

    public async Task<SettlementDto> CreateAsync(int clubId, int userId, CreateSettlementDto dto)
    {
        if (dto.Amount <= 0)
            throw new ConflictAppException("مبلغ پرداختی باید بیشتر از صفر باشد");

        var validMethods = new[] { "cash", "pos", "card" };
        if (!validMethods.Contains(dto.PaymentMethod))
            throw new ConflictAppException("روش پرداخت نامعتبر است. گزینه‌های مجاز: cash, pos, card");

        var clubPlayerExists = await _context.Clubplayers
            .AnyAsync(cp => cp.Id == dto.ClubPlayerId);

        if (!clubPlayerExists)
            throw new NotFoundAppException("مشتری مورد نظر یافت نشد");

        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == dto.ClubPlayerId);

        if (!isMember)
            throw new ConflictAppException("این مشتری عضو این کافه نیست");

        if (dto.OrderId.HasValue)
        {
            var orderExists = await _context.ClubOrders
                .AnyAsync(o => o.Id == dto.OrderId.Value && o.ClubId == clubId && !o.IsDeleted);
            if (!orderExists)
                throw new NotFoundAppException("سفارش مورد نظر یافت نشد");
        }

        var settlement = new ClubSettlement
        {
            ClubId = clubId,
            ClubPlayerId = dto.ClubPlayerId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Note = dto.Note,
            OrderId = dto.OrderId
        };

        _context.ClubSettlements.Add(settlement);
        await _context.SaveChangesAsync();

        // Recalculate order status if this settlement is tied to an order
        if (dto.OrderId.HasValue)
        {
            await _clubOrderService.RecalculateOrderStatusAsync(dto.OrderId.Value);
        }

        var displayName = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync();

        return new SettlementDto(
            settlement.Id,
            settlement.ClubPlayerId,
            settlement.Amount,
            settlement.PaymentMethod,
            settlement.Note,
            settlement.CreatedAt,
            settlement.CreatedByUserId,
            displayName,
            settlement.OrderId
        );
    }

    public async Task<LedgerResponseDto> GetLedgerAsync(int clubId, int clubPlayerId)
    {
        var clubPlayer = await _context.Clubplayers
            .FirstOrDefaultAsync(cp => cp.Id == clubPlayerId);

        if (clubPlayer is null)
            throw new NotFoundAppException("مشتری مورد نظر یافت نشد");

        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == clubPlayerId);

        if (!isMember)
            throw new ConflictAppException("این مشتری عضو این کافه نیست");

        var debitEntries = new List<LedgerEntryDto>();
        var creditEntries = new List<LedgerEntryDto>();

        // Games (debit)
        var playIds = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var gameParticipants = await _context.Clubplayplayers
            .Include(pp => pp.Play)
                .ThenInclude(p => p.Nerkh)
            .Where(pp => playIds.Contains(pp.PlayId)
                      && pp.PlayerId == clubPlayerId
                      && !pp.IsGuest)
            .ToListAsync();

        foreach (var pp in gameParticipants)
        {
            var nerkh = pp.Play.Nerkh;
            var price = nerkh is not null
                ? nerkh.Price
                : await _context.Nerkhs
                    .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
                    .Select(n => (decimal?)n.Price)
                    .FirstOrDefaultAsync() ?? 0m;

            debitEntries.Add(new LedgerEntryDto(
                "game",
                pp.Play.Title,
                price,
                pp.Play.DateTime,
                pp.Play.BusinessDate,
                pp.PlayId
            ));
        }

        // Orders (debit)
        var orders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.ClubId == clubId
                     && o.ClubPlayerId == clubPlayerId
                     && !o.IsDeleted)
            .ToListAsync();

        foreach (var order in orders)
        {
            var total = order.ClubOrderItems
                .Where(oi => !oi.IsDeleted)
                .Sum(oi => oi.Quantity * oi.UnitPrice);

            debitEntries.Add(new LedgerEntryDto(
                "order",
                $"سفارش {order.ClubOrderItems.Count(oi => !oi.IsDeleted)} قلم",
                total,
                order.CreatedAt,
                order.BusinessDate,
                order.Id
            ));
        }

        // Settlements (credit, positive amount)
        var settlements = await _context.ClubSettlements
            .Where(s => s.ClubId == clubId
                     && s.ClubPlayerId == clubPlayerId
                     && !s.IsDeleted)
            .ToListAsync();

        foreach (var s in settlements)
        {
            creditEntries.Add(new LedgerEntryDto(
                "settlement",
                s.PaymentMethod == "cash" ? "پرداخت نقدی"
                    : s.PaymentMethod == "pos" ? "پرداخت کارتخوان"
                    : s.PaymentMethod == "card_to_card" ? "کارت به کارت"
                    : "پرداخت",
                s.Amount,
                s.CreatedAt,
                null,
                s.Id
            ));
        }

        var allDebit = debitEntries
            .OrderByDescending(e => e.DateTime)
            .ThenByDescending(e => e.BusinessDate)
            .ToList();

        var allCredit = creditEntries
            .OrderByDescending(e => e.DateTime)
            .ThenByDescending(e => e.BusinessDate)
            .ToList();

        // ── VAT ──
        var vatPercent = await GetClubVatPercentAsync(clubId);

        var totalDebit = ApplyVat(allDebit.Sum(e => e.Amount), vatPercent);
        var totalCredit = allCredit.Sum(e => e.Amount);
        var balance = totalDebit - totalCredit;

        var merged = new List<LedgerEntryDto>();
        merged.AddRange(allDebit);
        merged.AddRange(allCredit.Select(c => c with { Amount = -c.Amount }));

        return new LedgerResponseDto(
            merged.OrderByDescending(e => e.DateTime)
                  .ThenByDescending(e => e.BusinessDate)
                  .ToList(),
            totalDebit,
            totalCredit,
            balance
        );
    }

    public async Task DeleteAsync(int clubId, int settlementId, int userId)
    {
        var settlement = await _context.ClubSettlements
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == settlementId && s.ClubId == clubId && !s.IsDeleted);

        if (settlement is null)
            throw new NotFoundAppException("تسویه مورد نظر یافت نشد");

        var orderId = settlement.OrderId;

        settlement.IsDeleted = true;
        settlement.DeletedAt = DateTime.UtcNow;
        settlement.DeletedByUserId = userId;

        await _context.SaveChangesAsync();

        // Recalculate order status if this settlement was tied to an order
        if (orderId.HasValue)
        {
            await _clubOrderService.RecalculateOrderStatusAsync(orderId.Value);
        }
    }

    // ── New: Today overview ──

    public async Task<List<TodayOverviewDto>> GetTodayOverviewAsync(int clubId, string statusFilter, string? dateStr = null)
    {
        var today = dateStr is not null && DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var parsed)
            ? parsed
            : BusinessDateHelper.Today();

        Console.WriteLine($"[DEBUG] clubId={clubId}, today={today}");
        // Get all club players who have activity today (games or orders)
        var todayPlayIds = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate == today)
            .Select(p => p.Id)
            .ToListAsync();

        var playerIdsWithGamesToday = await _context.Clubplayplayers
            .Where(pp => todayPlayIds.Contains(pp.PlayId) && !pp.IsGuest)
            .Select(pp => pp.PlayerId)
            .Distinct()
            .ToListAsync();

        var playerIdsWithOrdersToday = await _context.ClubOrders
            .Where(o => o.ClubId == clubId && o.BusinessDate == today && !o.IsDeleted)
            .Select(o => o.ClubPlayerId)
            .Distinct()
            .ToListAsync();
        var query = _context.ClubOrders
    .Where(o => o.ClubId == clubId && o.BusinessDate == today && !o.IsDeleted);
        Console.WriteLine(query.ToQueryString());

        var allPlayerIds = playerIdsWithGamesToday
            .Union(playerIdsWithOrdersToday)
            .Distinct()
            .ToList();

        if (allPlayerIds.Count == 0)
            return new List<TodayOverviewDto>();

        var clubPlayers = await _context.Clubplayers
            .Where(cp => allPlayerIds.Contains(cp.Id))
            .ToDictionaryAsync(cp => cp.Id);

        // Preload data for calculation
        var allNerkhs = await _context.Nerkhs
            .Where(n => n.ClubId == clubId && !n.IsDeleted)
            .ToDictionaryAsync(n => n.Id);

        var defaultNerkh = allNerkhs.Values.FirstOrDefault(n => n.IsDefault);

        var allPlaysToday = await _context.Clubplays
            .Where(p => todayPlayIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var allParticipantsToday = await _context.Clubplayplayers
            .Where(pp => todayPlayIds.Contains(pp.PlayId) && !pp.IsGuest)
            .ToListAsync();

        var allOrdersToday = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
            .Where(o => o.ClubId == clubId && o.BusinessDate == today && !o.IsDeleted)
            .ToListAsync();

        // Previous balance calculations
        var prevPlays = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate < today)
            .Select(p => p.Id)
            .ToListAsync();

        var prevParticipants = await _context.Clubplayplayers
            .Include(pp => pp.Play).ThenInclude(p => p.Nerkh)
            .Where(pp => prevPlays.Contains(pp.PlayId) && !pp.IsGuest)
            .ToListAsync();

        var allSettlementsToday = await _context.ClubSettlements
            .Where(s => s.ClubId == clubId && !s.IsDeleted
                     && s.CreatedAt >= today.ToDateTime(TimeOnly.MinValue)
                     && s.CreatedAt < today.ToDateTime(TimeOnly.MinValue).AddDays(1))
            .ToListAsync();

        var allPrevSettlements = await _context.ClubSettlements
            .Where(s => s.ClubId == clubId && !s.IsDeleted
                     && s.CreatedAt < today.ToDateTime(TimeOnly.MinValue).AddDays(1))
            .ToListAsync();

        var prevOrders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
            .Where(o => o.ClubId == clubId && o.BusinessDate < today && !o.IsDeleted)
            .ToListAsync();

        // ── VAT ──
        var vatPercent = await GetClubVatPercentAsync(clubId);

        var result = new List<TodayOverviewDto>();

        foreach (var playerId in allPlayerIds)
        {
            var cp = clubPlayers.GetValueOrDefault(playerId);
            if (cp is null) continue;

            // Today games
            var playerGamesToday = allParticipantsToday.Where(pp => pp.PlayerId == playerId).ToList();
            var gamesCountToday = playerGamesToday.Count;
            decimal todayGamesTotal = 0;
            foreach (var pp in playerGamesToday)
            {
                var play = allPlaysToday.GetValueOrDefault(pp.PlayId);
                if (play is null) continue;
                var nerkh = play.NerkhId.HasValue ? allNerkhs.GetValueOrDefault(play.NerkhId.Value) : defaultNerkh;
                todayGamesTotal += nerkh?.Price ?? 0m;
            }

            // Today orders
            var playerOrdersToday = allOrdersToday.Where(o => o.ClubPlayerId == playerId).ToList();
            var todayOrdersTotal = playerOrdersToday.Sum(o =>
                o.ClubOrderItems.Where(oi => !oi.IsDeleted).Sum(oi => oi.Quantity * oi.UnitPrice));

            var todayDue = ApplyVat(todayGamesTotal + todayOrdersTotal, vatPercent);

            // Previous balance
            decimal prevGamesTotal = 0;
            var prevPlayerParts = prevParticipants.Where(pp => pp.PlayerId == playerId).ToList();
            foreach (var pp in prevPlayerParts)
            {
                var nerkh = pp.Play.Nerkh;
                var price = nerkh is not null
                    ? nerkh.Price
                    : defaultNerkh?.Price ?? 0m;
                prevGamesTotal += price;
            }

            var prevOrdersTotal = prevOrders
                .Where(o => o.ClubPlayerId == playerId)
                .Sum(o => o.ClubOrderItems.Where(oi => !oi.IsDeleted).Sum(oi => oi.Quantity * oi.UnitPrice));

            var prevPaymentsTotal = allPrevSettlements
                .Where(s => s.ClubPlayerId == playerId)
                .Sum(s => s.Amount);

            var previousBalance = prevGamesTotal + prevOrdersTotal - prevPaymentsTotal;

            // Paid today
            var paidToday = allSettlementsToday
                .Where(s => s.ClubPlayerId == playerId)
                .Sum(s => s.Amount);

            // Compute status
            var status = ComputeTodayStatus(paidToday, todayDue, previousBalance);

            // Apply filter
            if (statusFilter != "all" && status != statusFilter)
                continue;

            result.Add(new TodayOverviewDto(
                playerId,
                cp.Name ?? "",
                cp.Mobile ?? "",
                gamesCountToday,
                todayDue,
                previousBalance,
                paidToday,
                status
            ));
        }

        return result.OrderByDescending(r => r.TodayDue).ToList();
    }

    public async Task<List<DebtorDto>> GetDebtorsAsync(int clubId)
    {
        var today = BusinessDateHelper.Today();

        var clubPlayerIds = await _context.ClubClubplayers
            .Where(cc => cc.ClubId == clubId)
            .Select(cc => cc.ClubplayerId)
            .ToListAsync();

        if (clubPlayerIds.Count == 0)
            return new List<DebtorDto>();

        var clubPlayers = await _context.Clubplayers
            .Where(cp => clubPlayerIds.Contains(cp.Id))
            .ToDictionaryAsync(cp => cp.Id);

        var allPlayIds = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var allNerkhs = await _context.Nerkhs
            .Where(n => n.ClubId == clubId && !n.IsDeleted)
            .ToDictionaryAsync(n => n.Id);

        var defaultNerkh = allNerkhs.Values.FirstOrDefault(n => n.IsDefault);

        var allParticipants = await _context.Clubplayplayers
            .Include(pp => pp.Play).ThenInclude(p => p.Nerkh)
            .Where(pp => allPlayIds.Contains(pp.PlayId) && !pp.IsGuest)
            .ToListAsync();

        var allOrders = await _context.ClubOrders
            .Include(o => o.ClubOrderItems)
            .Where(o => o.ClubId == clubId && !o.IsDeleted)
            .ToListAsync();

        var allSettlements = await _context.ClubSettlements
            .Where(s => s.ClubId == clubId && !s.IsDeleted)
            .ToListAsync();

        var allPlays = await _context.Clubplays
            .Where(p => allPlayIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // ── VAT ──
        var vatPercent = await GetClubVatPercentAsync(clubId);

        var result = new List<DebtorDto>();

        foreach (var playerId in clubPlayerIds)
        {
            var cp = clubPlayers.GetValueOrDefault(playerId);
            if (cp is null) continue;

            // Total game balance
            decimal totalGameBalance = 0;
            var playerParts = allParticipants.Where(pp => pp.PlayerId == playerId).ToList();
            DateOnly? oldestUnpaidDate = null;

            foreach (var pp in playerParts)
            {
                var nerkh = pp.Play.Nerkh;
                var price = nerkh is not null
                    ? nerkh.Price
                    : defaultNerkh?.Price ?? 0m;
                totalGameBalance += price;

                var play = allPlays.GetValueOrDefault(pp.PlayId);
                if (play is not null)
                {
                    if (oldestUnpaidDate is null || play.BusinessDate < oldestUnpaidDate)
                        oldestUnpaidDate = play.BusinessDate;
                }
            }

            // Total orders
            var totalOrderBalance = allOrders
                .Where(o => o.ClubPlayerId == playerId)
                .Sum(o => o.ClubOrderItems.Where(oi => !oi.IsDeleted).Sum(oi => oi.Quantity * oi.UnitPrice));

            // Total payments
            var totalPaid = allSettlements
                .Where(s => s.ClubPlayerId == playerId)
                .Sum(s => s.Amount);

            var totalDebt = ApplyVat(totalGameBalance + totalOrderBalance, vatPercent) - totalPaid;

            if (totalDebt <= 0)
                continue;

            result.Add(new DebtorDto(
                playerId,
                cp.Name ?? "",
                cp.Mobile ?? "",
                totalDebt,
                oldestUnpaidDate?.ToString("yyyy-MM-dd")
            ));
        }

        return result.OrderByDescending(r => r.TotalDebt).ToList();
    }

    // ── Private helpers ──

    private static decimal ApplyVat(decimal subtotal, decimal? vatPercent)
    {
        var rate = vatPercent ?? 0m;
        return subtotal * (1 + rate / 100m);
    }

    private async Task<decimal?> GetClubVatPercentAsync(int clubId)
    {
        return await _context.Clubs
            .Where(c => c.Id == clubId)
            .Select(c => c.VatPercent)
            .FirstOrDefaultAsync();
    }

    private static string ComputeTodayStatus(decimal paidToday, decimal todayDue, decimal previousBalance)
    {
        var effectiveDue = todayDue + Math.Max(previousBalance, 0);
        if (paidToday <= 0 && effectiveDue > 0)
            return "unpaid";
        if (paidToday > 0 && paidToday < effectiveDue)
            return "partial";
        if (paidToday >= effectiveDue && effectiveDue > 0)
            return "settled";
        return "settled";
    }

    private async Task<decimal> CalculatePreviousGamesBalanceAsync(int clubId, int clubPlayerId, DateOnly today)
    {
        var prevPlayIds = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate < today)
            .Select(p => p.Id)
            .ToListAsync();

        if (prevPlayIds.Count == 0) return 0m;

        var participants = await _context.Clubplayplayers
            .Include(pp => pp.Play)
                .ThenInclude(p => p.Nerkh)
            .Where(pp => prevPlayIds.Contains(pp.PlayId)
                      && pp.PlayerId == clubPlayerId
                      && !pp.IsGuest)
            .ToListAsync();

        var total = 0m;
        foreach (var pp in participants)
        {
            var nerkh = pp.Play.Nerkh;
            var price = nerkh is not null
                ? nerkh.Price
                : await _context.Nerkhs
                    .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
                    .Select(n => (decimal?)n.Price)
                    .FirstOrDefaultAsync() ?? 0m;
            total += price;
        }

        return total;
    }

    private async Task<decimal> CalculatePreviousOrdersBalanceAsync(int clubId, int clubPlayerId, DateOnly today)
    {
        return await _context.ClubOrders
            .Where(o => o.ClubId == clubId
                     && o.ClubPlayerId == clubPlayerId
                     && o.BusinessDate < today
                     && !o.IsDeleted)
            .SelectMany(o => o.ClubOrderItems.Where(oi => !oi.IsDeleted))
            .SumAsync(oi => (decimal?)oi.Quantity * oi.UnitPrice) ?? 0m;
    }

    private async Task<decimal> CalculatePreviousPaymentsAsync(int clubId, int clubPlayerId, DateOnly today)
    {
        return await _context.ClubSettlements
            .Where(s => s.ClubId == clubId
                     && s.ClubPlayerId == clubPlayerId
                     && !s.IsDeleted
                     && s.CreatedAt < today.ToDateTime(TimeOnly.MinValue).AddDays(1))
            .SumAsync(s => (decimal?)s.Amount) ?? 0m;
    }
}