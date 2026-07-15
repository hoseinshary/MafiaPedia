using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.ClubPlay;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubPlayService : IClubPlayService
{
    private readonly MafiaDbContext _context;
    private readonly ILogger<ClubPlayService> _logger;
    private readonly IEventService _eventService;

    public ClubPlayService(MafiaDbContext context, ILogger<ClubPlayService> logger, IEventService eventService)
    {
        _context = context;
        _logger = logger;
        _eventService = eventService;
    }

    public async Task<ClubPlayDetailDto> CreateClubPlayAsync(int clubId, int masterId, int userId, CreateClubPlayDto dto)
    {
        var masterExists = await _context.Masters.AnyAsync(m => m.Id == masterId && m.ClubId == clubId);
        if (!masterExists)
            throw new KeyNotFoundException("گرداننده یافت نشد");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.ClubId == clubId);
            if (room is null)
                throw new KeyNotFoundException("اتاق یافت نشد");

            if (dto.Participants.Count != dto.PlayersCount)
                throw new InvalidOperationException(
                    $"تعداد بازیکنان انتخاب‌شده ({dto.Participants.Count}) با PlayersCount ({dto.PlayersCount}) مطابقت ندارد");

            var participantIds = dto.Participants.Select(p => p.ClubPlayerId).ToList();

            var distinctIds = participantIds.Distinct().Count();
            if (distinctIds != participantIds.Count)
                throw new InvalidOperationException("بازیکن تکراری در لیست شرکت‌کنندگان وجود دارد");

            var membershipIds = await _context.ClubClubplayers
                .Where(cc => cc.ClubId == clubId)
                .Select(cc => cc.ClubplayerId)
                .ToListAsync();

            var invalidIds = participantIds
                .Where(id => !membershipIds.Contains(id))
                .ToList();

            if (invalidIds.Count != 0)
                throw new InvalidOperationException(
                    $"این بازیکنان عضو این کافه نیستند: {string.Join("، ", invalidIds)}");

            var roleSet = await _context.SenarioRoleSets
                .FirstOrDefaultAsync(rs => rs.SenarioId == dto.SenarioId && rs.PlayerCount == dto.PlayersCount);

            if (roleSet is null)
                throw new InvalidOperationException(
                    $"این سناریو برای {dto.PlayersCount} نفر هنوز پیکربندی نشده است");

            var roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
            if (roleIds is null || roleIds.Count != dto.PlayersCount)
            {
                _logger.LogError(
                    "SenarioRoleSet Id={Id}: RoleIds array length ({Actual}) does not match PlayerCount ({Expected})",
                    roleSet.Id, roleIds?.Count ?? 0, dto.PlayersCount);
                throw new InvalidOperationException("خطای یکپارچگی داده‌های پیکربندی سناریو");
            }

            int eventId;
            if (dto.EventId.HasValue)
            {
                var eventExists = await _context.Events.AnyAsync(e => e.Id == dto.EventId.Value && e.ClubId == clubId);
                if (!eventExists)
                    throw new InvalidOperationException("رویداد انتخاب‌شده برای این کافه معتبر نیست");
                eventId = dto.EventId.Value;
            }
            else
            {
                var defaultEvent = await _eventService.GetDefaultEventAsync(clubId);
                if (defaultEvent is null)
                    throw new InvalidOperationException("این کافه هنوز Event پیش‌فرض ندارد — از پنل مدیریت یک فصل بسازید");
                eventId = defaultEvent.Id;
            }

            var guestCount = dto.Participants.Count(p => p.IsGuest);

            // Auto-generate title if empty
            var title = dto.Title?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                var businessDate = BusinessDateHelper.Compute(dto.DateTime);
                var dayStart = businessDate.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)));
                var dayEnd = businessDate.AddDays(1).ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)));

                var countToday = await _context.Clubplays
                    .CountAsync(p => p.MasterId == masterId
                                  && p.DateTime >= dayStart
                                  && p.DateTime < dayEnd);

                var masterName = await _context.Masters
                    .Where(m => m.Id == masterId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync() ?? "بدون نام";

                title = $"بازی {PersianOrdinal.ToOrdinal(countToday + 1)} {masterName}";
            }

            var clubPlay = new Clubplay
            {
                Title = title,
                DateTime = dto.DateTime,
                PlayersCount = dto.PlayersCount,
                GuestCount = guestCount,
                Desc = dto.Desc,
                RoomId = dto.RoomId,
                SenarioId = dto.SenarioId,
                MasterId = masterId,
                WinnersideId = null,
                UserId = userId,
                EventId = eventId,
                Link = dto.Link,
                PlayType = dto.PlayType,
                Status = dto.PlayType == "normal" ? "done" : "pending"
            };

            _context.Clubplays.Add(clubPlay);
            await _context.SaveChangesAsync();

            var participants = await CreateClubPlayParticipantsAsync(
                clubPlay.Id, dto.Participants, roleIds, dto.ShuffleRoles);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var senario = await _context.Senarios.FindAsync(dto.SenarioId);
            var evt = await _context.Events.FindAsync(eventId);

            return new ClubPlayDetailDto(
                clubPlay.Id,
                clubPlay.Title,
                clubPlay.DateTime ?? dto.DateTime,
                room.Id,
                room.Name ?? "",
                dto.SenarioId,
                senario?.Name ?? "",
                dto.PlayersCount,
                guestCount,
                clubPlay.Desc,
                clubPlay.Link,
                clubPlay.PlayType,
                clubPlay.Status,
                masterId,
                (await _context.Masters.FindAsync(masterId))?.Name ?? "",
                clubPlay.WinnersideId,
                eventId,
                evt?.Name ?? "",
                participants
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ClubPlayDetailDto?> GetClubPlayDetailAsync(int clubId, int playId)
    {
        var play = await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Event)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return null;

        if (play.Room.ClubId != clubId) return null;

        var participants = play.Clubplayplayers
            .OrderBy(pp => pp.Id)
            .Select(pp => new ClubPlayParticipantDto(
                pp.PlayerId,
                pp.Player.Name ?? "",
                pp.RoleId,
                pp.Role.Name ?? "",
                pp.Role.SideId,
                pp.Role.Photo,
                pp.IsGuest
            ))
            .ToList();

        return new ClubPlayDetailDto(
            play.Id,
            play.Title,
            play.DateTime ?? default,
            play.RoomId,
            play.Room.Name ?? "",
            play.SenarioId,
            play.Senario.Name ?? "",
            play.PlayersCount ?? 0,
            play.GuestCount ?? 0,
            play.Desc,
            play.Link,
            play.PlayType,
            play.Status,
            play.MasterId,
            play.Master.Name ?? "",
            play.WinnersideId,
            play.EventId,
            play.Event?.Name ?? "",
            participants
        );
    }

    public async Task<ClubPlayDetailDto?> ReshuffleRolesAsync(int clubId, int playId, int? restrictToMasterId)
    {
        var play = await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;
        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید نقش‌های بازی دیگری را پخش کنید");

        if (play.Status != "pending")
            throw new InvalidOperationException("پس از پایان بازی امکان پخش دوباره نیست");

        var roleSet = await _context.SenarioRoleSets
            .FirstOrDefaultAsync(rs => rs.SenarioId == play.SenarioId && rs.PlayerCount == play.PlayersCount);

        if (roleSet is null)
            throw new InvalidOperationException("پیکربندی سناریو برای این تعداد بازیکن یافت نشد");

        var roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
        if (roleIds is null || roleIds.Count != play.PlayersCount)
        {
            _logger.LogError(
                "SenarioRoleSet Id={Id}: RoleIds array length ({Actual}) does not match PlayerCount ({Expected})",
                roleSet.Id, roleIds?.Count ?? 0, play.PlayersCount);
            throw new InvalidOperationException("خطای یکپارچگی داده‌های پیکربندی سناریو");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var shuffled = ShuffleCopy(roleIds);
            var existingPlayers = play.Clubplayplayers.OrderBy(pp => pp.Id).ToList();

            for (int i = 0; i < existingPlayers.Count; i++)
            {
                existingPlayers[i].RoleId = shuffled[i];
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var participants = existingPlayers
                .Select(pp => new ClubPlayParticipantDto(
                    pp.PlayerId,
                    pp.Player.Name ?? "",
                    pp.RoleId,
                    pp.Role.Name ?? "",
                    pp.Role.SideId,
                    pp.Role.Photo,
                    pp.IsGuest
                ))
                .ToList();

        return new ClubPlayDetailDto(
            play.Id,
            play.Title,
            play.DateTime ?? default,
            play.RoomId,
            play.Room.Name ?? "",
            play.SenarioId,
            play.Senario.Name ?? "",
            play.PlayersCount ?? 0,
            play.GuestCount ?? 0,
            play.Desc,
            play.Link,
            play.PlayType,
            play.Status,
            play.MasterId,
            play.Master.Name ?? "",
            play.WinnersideId,
            play.EventId,
            play.Event?.Name ?? "",
            participants
        );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> GetPlayCountByDateAsync(int clubId, int masterId, DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)));
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)));

        return await _context.Clubplays
            .CountAsync(p => p.Room.ClubId == clubId
                          && p.MasterId == masterId
                          && p.DateTime >= dayStart
                          && p.DateTime < dayEnd);
    }

    public async Task<ClubPlayDetailDto?> ConfirmRoleRevealAsync(int clubId, int playId, int? restrictToMasterId)
    {
        var play = await LoadPlayWithIncludesAsync(playId);
        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید نقش‌های بازی دیگری را پخش کنید");

        // Normal games start as "done" — no-op, just return current state
        if (play.PlayType == "normal" && play.Status == "done")
            return MapToDetailDto(play);

        if (play.Status != "pending")
            throw new InvalidOperationException("این بازی در وضعیت مناسب برای این عملیات نیست");

        play.Status = "notwinside";
        await _context.SaveChangesAsync();

        return MapToDetailDto(play);
    }

    public async Task<ClubPlayDetailDto?> SubmitWinnersideAsync(int clubId, int playId, int? restrictToMasterId, int winnersideId)
    {
        var play = await LoadPlayWithIncludesAsync(playId);
        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید برای بازی دیگری برنده ثبت کنید");

        if (play.Status != "notwinside")
            throw new InvalidOperationException("این بازی در وضعیت مناسب برای ثبت برنده نیست");

        if (play.PlayType == "normal")
            throw new InvalidOperationException("این بازی در وضعیت مناسب برای این عملیات نیست");

        play.WinnersideId = winnersideId;

        play.Status = play.PlayType switch
        {
            "rank" or "etc" => "done",
            "superrank" => "notrank",
            _ => throw new InvalidOperationException("نوع بازی نامعتبر است")
        };

        await _context.SaveChangesAsync();
        return MapToDetailDto(play);
    }

    public async Task<ClubPlayDetailDto?> SubmitRanksAsync(int clubId, int playId, int? restrictToMasterId, List<ParticipantRankDto> ranks)
    {
        var play = await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید برای بازی دیگری رتبه ثبت کنید");

        if (play.Status != "notrank")
            throw new InvalidOperationException("این بازی در وضعیت مناسب برای ثبت رتبه نیست");

        if (ranks.Count != (play.PlayersCount ?? 0))
            throw new InvalidOperationException(
                $"تعداد رتبه‌ها ({ranks.Count}) با تعداد بازیکنان ({play.PlayersCount}) مطابقت ندارد");

        var duplicateIds = ranks.GroupBy(r => r.ClubPlayerId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateIds.Count != 0)
            throw new InvalidOperationException(
                $"بازیکن تکراری در لیست رتبه‌ها: {string.Join("، ", duplicateIds)}");

        var existingPlayerIds = play.Clubplayplayers.Select(pp => pp.PlayerId).ToHashSet();
        var missingIds = ranks.Select(r => r.ClubPlayerId).Where(id => !existingPlayerIds.Contains(id)).ToList();
        if (missingIds.Count != 0)
            throw new InvalidOperationException(
                $"این بازیکنان در این بازی نیستند: {string.Join("، ", missingIds)}");

        var rankById = ranks.ToDictionary(r => r.ClubPlayerId, r => r.Rank);
        foreach (var pp in play.Clubplayplayers)
        {
            if (rankById.TryGetValue(pp.PlayerId, out var rank))
            {
                pp.Rank = rank;
            }
        }

        play.Status = "done";
        await _context.SaveChangesAsync();
        return MapToDetailDto(play);
    }

    public async Task<List<ClubPlayListItemDto>> GetPlaysByBusinessDateAsync(int clubId, int masterId, DateOnly businessDate)
    {
        return await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && p.BusinessDate == businessDate)
            .OrderBy(p => p.DateTime)
            .Select(p => new ClubPlayListItemDto(
                p.Id,
                p.Title,
                p.DateTime ?? default,
                p.BusinessDate ?? default,
                p.Room.Name ?? "",
                p.Senario.Name ?? "",
                p.PlayersCount ?? 0,
                p.GuestCount ?? 0,
                p.Status,
                p.PlayType,
                null
            ))
            .ToListAsync();
    }

    public async Task<List<ClubPlayListItemDto>> GetClubPlaysByBusinessDateAsync(int clubId, DateOnly businessDate)
    {
        return await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.BusinessDate == businessDate)
            .OrderBy(p => p.DateTime)
            .Select(p => new ClubPlayListItemDto(
                p.Id,
                p.Title,
                p.DateTime ?? default,
                p.BusinessDate ?? default,
                p.Room.Name ?? "",
                p.Senario.Name ?? "",
                p.PlayersCount ?? 0,
                p.GuestCount ?? 0,
                p.Status,
                p.PlayType,
                p.Master.Name
            ))
            .ToListAsync();
    }

    public async Task<List<ClubPlayListItemDto>> GetOpenPlaysAsync(int clubId, int masterId)
    {
        return await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && p.Status != "done")
            .OrderBy(p => p.DateTime)
            .Select(p => new ClubPlayListItemDto(
                p.Id,
                p.Title,
                p.DateTime ?? default,
                p.BusinessDate ?? default,
                p.Room.Name ?? "",
                p.Senario.Name ?? "",
                p.PlayersCount ?? 0,
                p.GuestCount ?? 0,
                p.Status,
                p.PlayType,
                null
            ))
            .ToListAsync();
    }

    public async Task<(List<ClubPlayListItemDto> Items, int Total)> GetMyPlaysAsync(
        int clubId, int masterId, int page, int pageSize, DateTime? dateFrom, DateTime? dateTo, string? status)
    {
        var query = _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId);

        if (dateFrom.HasValue)
        {
            var from = BusinessDateHelper.Compute(dateFrom.Value);
            query = query.Where(p => p.BusinessDate >= from);
        }

        if (dateTo.HasValue)
        {
            var to = BusinessDateHelper.Compute(dateTo.Value);
            query = query.Where(p => p.BusinessDate <= to);
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ClubPlayListItemDto(
                p.Id,
                p.Title,
                p.DateTime ?? default,
                p.BusinessDate ?? default,
                p.Room.Name ?? "",
                p.Senario.Name ?? "",
                p.PlayersCount ?? 0,
                p.GuestCount ?? 0,
                p.Status,
                p.PlayType,
                null
            ))
            .ToListAsync();

        return (items, total);
    }

    public async Task<MasterStatsDto> GetClubStatsAsync(int clubId, string period)
    {
        var today = BusinessDateHelper.Today();
        DateOnly from;

        if (period == "week") from = today.AddDays(-6);
        else if (period == "month") from = new DateOnly(today.Year, today.Month, 1);
        else throw new InvalidOperationException("پریود نامعتبر است. از week یا month استفاده کنید");

        var playIdsQuery = _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.BusinessDate >= from && p.BusinessDate <= today);

        var totalPlays = await playIdsQuery.CountAsync();
        var playIds = await playIdsQuery.Select(p => p.Id).ToListAsync();

        var totalEntries = await _context.Clubplayplayers.CountAsync(pp => playIds.Contains(pp.PlayId) && !pp.IsGuest);
        var totalGuestEntries = await _context.Clubplayplayers.CountAsync(pp => playIds.Contains(pp.PlayId) && pp.IsGuest);

        return new MasterStatsDto(totalPlays, totalEntries, totalGuestEntries);
    }

    public async Task<List<MasterPerformanceDto>> GetMasterPerformanceAsync(int clubId, string period)
    {
        var today = BusinessDateHelper.Today();
        DateOnly from;

        if (period == "week") from = today.AddDays(-6);
        else if (period == "month") from = new DateOnly(today.Year, today.Month, 1);
        else throw new InvalidOperationException("پریود نامعتبر است. از week یا month استفاده کنید");

        var plays = await _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.BusinessDate >= from && p.BusinessDate <= today)
            .Include(p => p.Master)
            .Select(p => new { p.Id, p.MasterId, MasterName = p.Master.Name })
            .ToListAsync();

        var playIds = plays.Select(p => p.Id).ToList();

        var entryCounts = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId))
            .GroupBy(pp => pp.PlayId)
            .Select(g => new { PlayId = g.Key, Entries = g.Count(x => !x.IsGuest), Guests = g.Count(x => x.IsGuest) })
            .ToListAsync();

        var entryLookup = entryCounts.ToDictionary(x => x.PlayId);

        return plays
            .GroupBy(p => new { p.MasterId, p.MasterName })
            .Select(g => new MasterPerformanceDto(
                g.Key.MasterId,
                g.Key.MasterName ?? "",
                g.Count(),
                g.Sum(p => entryLookup.TryGetValue(p.Id, out var e) ? e.Entries : 0),
                g.Sum(p => entryLookup.TryGetValue(p.Id, out var e) ? e.Guests : 0)
            ))
            .OrderByDescending(x => x.PlayCount)
            .ToList();
    }

    public async Task<MasterStatsDto> GetMyStatsAsync(int clubId, int masterId, string period)
    {
        var today = BusinessDateHelper.Today();
        DateOnly from;

        if (period == "week")
        {
            from = today.AddDays(-6);
        }
        else if (period == "month")
        {
            from = new DateOnly(today.Year, today.Month, 1);
        }
        else
        {
            throw new InvalidOperationException("پریود نامعتبر است. از week یا month استفاده کنید");
        }

        var playIdsQuery = _context.Clubplays
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && p.BusinessDate >= from && p.BusinessDate <= today);

        var totalPlays = await playIdsQuery.CountAsync();

        var playIds = await playIdsQuery.Select(p => p.Id).ToListAsync();

        var totalEntries = await _context.Clubplayplayers
            .CountAsync(pp => playIds.Contains(pp.PlayId) && !pp.IsGuest);

        var totalGuestEntries = await _context.Clubplayplayers
            .CountAsync(pp => playIds.Contains(pp.PlayId) && pp.IsGuest);

        return new MasterStatsDto(totalPlays, totalEntries, totalGuestEntries);
    }

    public async Task<ClubPlayDetailDto?> UpdateClubPlayAsync(int clubId, int playId, int? restrictToMasterId, UpdateClubPlayDto dto)
    {
        var play = await LoadPlayWithIncludesAsync(playId);

        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید بازی دیگری را ویرایش کنید");

        if (restrictToMasterId.HasValue && play.Status == "done")
            throw new InvalidOperationException("بازی‌های تکمیل‌شده قابل ویرایش نیستند");

        var statusBeforeChanges = play.Status;

        // Validate room
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.ClubId == clubId);
        if (room is null)
            throw new KeyNotFoundException("اتاق یافت نشد");

        if (dto.Participants.Count == 0)
            throw new InvalidOperationException("لیست شرکت‌کنندگان خالی است");

        // Validate no duplicates
        var participantIds = dto.Participants.Select(p => p.ClubPlayerId).ToList();
        var distinctIds = participantIds.Distinct().Count();
        if (distinctIds != participantIds.Count)
            throw new InvalidOperationException("بازیکن تکراری در لیست شرکت‌کنندگان وجود دارد");

        // Validate all are club members
        var membershipIds = await _context.ClubClubplayers
            .Where(cc => cc.ClubId == clubId)
            .Select(cc => cc.ClubplayerId)
            .ToListAsync();

        var invalidIds = participantIds.Where(id => !membershipIds.Contains(id)).ToList();
        if (invalidIds.Count != 0)
            throw new InvalidOperationException($"این بازیکنان عضو این کافه نیستند: {string.Join("، ", invalidIds)}");

        // ═══════════════════════════════════════════
        // Determine update tier
        // ═══════════════════════════════════════════
        var currentPlayerIds = play.Clubplayplayers.Select(pp => pp.PlayerId).OrderBy(x => x).ToList();
        var incomingPlayerIds = participantIds.OrderBy(x => x).ToList();

        bool sameSenario = play.SenarioId == dto.SenarioId;
        bool sameCount = play.PlayersCount == dto.Participants.Count;
        bool samePlayerSet = currentPlayerIds.SequenceEqual(incomingPlayerIds);
        bool sameGuests = samePlayerSet && play.Clubplayplayers
            .OrderBy(pp => pp.PlayerId)
            .Select(pp => pp.IsGuest)
            .SequenceEqual(dto.Participants.OrderBy(p => p.ClubPlayerId).Select(p => p.IsGuest));

        bool needsFullRebuild = !sameSenario || !sameCount;
        bool needsParticipantSwap = !needsFullRebuild && !samePlayerSet;
        bool metadataOnly = sameSenario && sameCount && samePlayerSet && sameGuests;

        // Validate role set exists (skip only for metadata-only — participant/senario didn't change)
        List<int>? roleIds = null;
        if (!metadataOnly)
        {
            var roleSet = await _context.SenarioRoleSets
                .FirstOrDefaultAsync(rs => rs.SenarioId == dto.SenarioId && rs.PlayerCount == dto.Participants.Count);

            if (roleSet is null)
                throw new InvalidOperationException($"این سناریو برای {dto.Participants.Count} نفر هنوز پیکربندی نشده است");

            roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
            if (roleIds is null || roleIds.Count != dto.Participants.Count)
            {
                _logger.LogError(
                    "SenarioRoleSet Id={Id}: RoleIds array length ({Actual}) does not match PlayerCount ({Expected})",
                    roleSet.Id, roleIds?.Count ?? 0, dto.Participants.Count);
                throw new InvalidOperationException("خطای یکپارچگی داده‌های پیکربندی سناریو");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            List<ClubPlayParticipantDto> participants;

            if (needsFullRebuild)
            {
                // ──────────────────────────────────────
                // Tier C: SenarioId or player-count changed
                //         → full role rebuild from scratch
                // ──────────────────────────────────────
                _context.Clubplayplayers.RemoveRange(play.Clubplayplayers);
                participants = await CreateClubPlayParticipantsAsync(
                    playId, dto.Participants, roleIds!, shuffle: true);
                play.WinnersideId = null;
            }
            else if (needsParticipantSwap)
            {
                // ──────────────────────────────────────
                // Tier B: Same scenario + count, different members
                //         → merge participants preserving roles
                // ──────────────────────────────────────
                participants = await MergeParticipantsPreservingRoles(
                    playId, play.Clubplayplayers, dto.Participants, roleIds!);
                play.WinnersideId = null;
            }
            else
            {
                // ──────────────────────────────────────
                // Tier A: Metadata-only (or IsGuest-only)
                //         → update fields, touch nothing related to roles/status
                // ──────────────────────────────────────
                if (!sameGuests)
                {
                    // IsGuest flags changed — update existing records
                    var incomingMap = dto.Participants.ToDictionary(p => p.ClubPlayerId);
                    foreach (var pp in play.Clubplayplayers)
                    {
                        pp.IsGuest = incomingMap[pp.PlayerId].IsGuest;
                    }
                }
                participants = play.Clubplayplayers
                    .OrderBy(pp => pp.Id)
                    .Select(pp => new ClubPlayParticipantDto(
                        pp.PlayerId,
                        pp.Player.Name ?? "",
                        pp.RoleId,
                        pp.Role.Name ?? "",
                        pp.Role.SideId,
                        pp.Role.Photo,
                        pp.IsGuest
                    ))
                    .ToList();
            }

            // ──────────────────────────────────────
            // Common: update metadata fields
            // ──────────────────────────────────────
            play.Title = dto.Title;
            play.DateTime = dto.DateTime;
            play.RoomId = dto.RoomId;
            play.SenarioId = dto.SenarioId;
            play.PlayersCount = dto.Participants.Count;
            play.GuestCount = dto.Participants.Count(p => p.IsGuest);
            play.Desc = dto.Desc;
            play.Link = dto.Link;
            play.PlayType = dto.PlayType;
            play.EventId = dto.EventId;

            play.Status = ClubPlayStatusResolver.Resolve(
                statusBeforeChanges,
                play.PlayType,
                play.WinnersideId,
                play.Clubplayplayers.Select(pp => pp.Rank));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var senario = await _context.Senarios.FindAsync(dto.SenarioId);

            return new ClubPlayDetailDto(
                play.Id,
                play.Title,
                play.DateTime ?? dto.DateTime,
                room.Id,
                room.Name ?? "",
                dto.SenarioId,
                senario?.Name ?? "",
                dto.Participants.Count,
                play.GuestCount ?? 0,
                play.Desc,
                play.Link,
                play.PlayType,
                play.Status,
                play.MasterId,
                (await _context.Masters.FindAsync(play.MasterId))?.Name ?? "",
                play.WinnersideId,
                play.EventId,
                play.Event?.Name ?? "",
                participants
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ClubPlayParticipantDto> ReplaceParticipantAsync(
        int clubId, int playId, int? restrictToMasterId, int currentClubPlayerId, ReplaceParticipantDto dto)
    {
        var play = await _context.Clubplays
            .Include(p => p.Clubplayplayers)
            .Include(p => p.Room)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null)
            throw new KeyNotFoundException("بازی یافت نشد");

        if (play.Room.ClubId != clubId)
            throw new KeyNotFoundException("بازی در این کافه یافت نشد");

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید بازی دیگری را ویرایش کنید");

        if (restrictToMasterId.HasValue && play.Status == "done")
            throw new InvalidOperationException("بازی‌های تکمیل‌شده قابل ویرایش نیستند");

        var target = play.Clubplayplayers.FirstOrDefault(pp => pp.PlayerId == currentClubPlayerId);
        if (target is null)
            throw new KeyNotFoundException("این بازیکن در این بازی یافت نشد");

        // Validate new player is a club member
        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == dto.NewClubPlayerId);

        if (!isMember)
            throw new InvalidOperationException("بازیکن جدید عضو این کافه نیست");

        // Validate no duplicate
        var isDuplicate = play.Clubplayplayers.Any(pp => pp.PlayerId == dto.NewClubPlayerId);
        if (isDuplicate)
            throw new InvalidOperationException("این بازیکن قبلاً در این بازی ثبت شده است");

        // Swap: update PlayerId and IsGuest only — RoleId, Rank, Action untouched
        target.PlayerId = dto.NewClubPlayerId;
        target.IsGuest = dto.IsGuest;

        await _context.SaveChangesAsync();

        // Load new player + existing role for DTO
        var newPlayer = await _context.Clubplayers.FindAsync(dto.NewClubPlayerId);
        var role = await _context.Roles
            .Include(r => r.Side)
            .FirstOrDefaultAsync(r => r.Id == target.RoleId);

        return new ClubPlayParticipantDto(
            target.PlayerId,
            newPlayer?.Name ?? "",
            target.RoleId,
            role?.Name ?? "",
            role?.SideId ?? 0,
            role?.Photo,
            target.IsGuest
        );
    }

    private async Task<Clubplay?> LoadPlayWithIncludesAsync(int playId)
    {
        return await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Event)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId);
    }

    private static ClubPlayDetailDto MapToDetailDto(Clubplay play)
    {
        var participants = play.Clubplayplayers
            .OrderBy(pp => pp.Id)
            .Select(pp => new ClubPlayParticipantDto(
                pp.PlayerId,
                pp.Player.Name ?? "",
                pp.RoleId,
                pp.Role.Name ?? "",
                pp.Role.SideId,
                pp.Role.Photo,
                pp.IsGuest
            ))
            .ToList();

        return new ClubPlayDetailDto(
            play.Id,
            play.Title,
            play.DateTime ?? default,
            play.RoomId,
            play.Room.Name ?? "",
            play.SenarioId,
            play.Senario.Name ?? "",
            play.PlayersCount ?? 0,
            play.GuestCount ?? 0,
            play.Desc,
            play.Link,
            play.PlayType,
            play.Status,
            play.MasterId,
            play.Master.Name ?? "",
            play.WinnersideId,
            play.EventId,
            play.Event?.Name ?? "",
            participants
        );
    }

    private async Task<List<ClubPlayParticipantDto>> CreateClubPlayParticipantsAsync(
        int playId, List<ParticipantInputDto> participantInputs, List<int> roleIds, bool shuffle)
    {
        var assignedRoleIds = shuffle ? ShuffleCopy(roleIds) : new List<int>(roleIds);
        var participantIds = participantInputs.Select(p => p.ClubPlayerId).ToList();

        var clubPlayerLookup = await _context.Clubplayers
            .Where(p => participantIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var roleLookup = await _context.Roles
            .Where(r => assignedRoleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        var result = new List<ClubPlayParticipantDto>();

        for (int i = 0; i < participantInputs.Count; i++)
        {
            var input = participantInputs[i];
            var roleId = assignedRoleIds[i];

            _context.Clubplayplayers.Add(new Clubplayplayer
            {
                PlayerId = input.ClubPlayerId,
                RoleId = roleId,
                PlayId = playId,
                Rank = null,
                Action = null,
                IsGuest = input.IsGuest
            });

            var player = clubPlayerLookup[input.ClubPlayerId];
            var role = roleLookup[roleId];

            result.Add(new ClubPlayParticipantDto(
                input.ClubPlayerId,
                player.Name ?? "",
                roleId,
                role.Name ?? "",
                role.SideId,
                role.Photo,
                input.IsGuest
            ));
        }

        return result;
    }

    private async Task<List<ClubPlayParticipantDto>> MergeParticipantsPreservingRoles(
        int playId,
        ICollection<Clubplayplayer> currentPlayers,
        List<ParticipantInputDto> incomingParticipants,
        List<int> allRoleIds)
    {
        var currentMap = currentPlayers.ToDictionary(pp => pp.PlayerId);
        var incomingIds = incomingParticipants.Select(p => p.ClubPlayerId).ToHashSet();

        var removed = currentMap.Keys.Where(id => !incomingIds.Contains(id)).ToList();
        var added = incomingParticipants.Where(p => !currentMap.ContainsKey(p.ClubPlayerId)).ToList();

        // Collect freed RoleIds from removed players
        var freedRoles = removed.Select(id => currentMap[id].RoleId).ToList();

        // Remove old records
        foreach (var id in removed)
            _context.Clubplayplayers.Remove(currentMap[id]);

        // Update IsGuest for common players
        var incomingMap = incomingParticipants.ToDictionary(p => p.ClubPlayerId);
        foreach (var id in currentMap.Keys.Where(incomingIds.Contains))
            currentMap[id].IsGuest = incomingMap[id].IsGuest;

        // Add new records — assign freed roles deterministically (first-freed → first-new)
        var newPlayerRoles = new Dictionary<int, int>();
        for (int i = 0; i < added.Count; i++)
        {
            var roleId = i < freedRoles.Count ? freedRoles[i] : allRoleIds[i];
            newPlayerRoles[added[i].ClubPlayerId] = roleId;
            _context.Clubplayplayers.Add(new Clubplayplayer
            {
                PlayerId = added[i].ClubPlayerId,
                RoleId = roleId,
                PlayId = playId,
                Rank = null,
                Action = null,
                IsGuest = added[i].IsGuest
            });
        }

        // Fetch player & role data for the DTO
        var allPlayerIds = incomingParticipants.Select(p => p.ClubPlayerId).ToList();
        var players = await _context.Clubplayers
            .Where(p => allPlayerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
        var roles = await _context.Roles
            .Where(r => allRoleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        var result = new List<ClubPlayParticipantDto>();
        foreach (var input in incomingParticipants)
        {
            var roleId = newPlayerRoles.TryGetValue(input.ClubPlayerId, out var fromNew)
                ? fromNew
                : currentMap[input.ClubPlayerId].RoleId;

            var player = players[input.ClubPlayerId];
            var role = roles[roleId];

            result.Add(new ClubPlayParticipantDto(
                input.ClubPlayerId,
                player.Name ?? "",
                roleId,
                role.Name ?? "",
                role.SideId,
                role.Photo,
                input.IsGuest
            ));
        }

        return result;
    }

    private static List<int> ShuffleCopy(List<int> source)
    {
        var copy = new List<int>(source);
        using var rng = RandomNumberGenerator.Create();
        var n = copy.Count;
        while (n > 1)
        {
            n--;
            var byteBuffer = new byte[4];
            rng.GetBytes(byteBuffer);
            var k = BitConverter.ToInt32(byteBuffer, 0) & 0x7FFFFFFF;
            k %= (n + 1);
            (copy[n], copy[k]) = (copy[k], copy[n]);
        }
        return copy;
    }
}
