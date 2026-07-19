using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Common.Exceptions;
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

            var totalEntryCount = dto.Participants.Sum(p => p.EntryCount);
            if (totalEntryCount != dto.PlayersCount)
                throw new InvalidOperationException(
                    $"تعداد کل ورودی‌ها ({totalEntryCount}) با PlayersCount ({dto.PlayersCount}) مطابقت ندارد");

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

                int? nerkhId;
                string? nerkhName;
                if (dto.NerkhId.HasValue)
                {
                    var nerkh = await _context.Nerkhs
                        .FirstOrDefaultAsync(n => n.Id == dto.NerkhId.Value && n.ClubId == clubId && !n.IsDeleted);
                    if (nerkh is null)
                        throw new InvalidOperationException("نرخ انتخاب‌شده برای این کافه معتبر نیست");
                    nerkhId = nerkh.Id;
                    nerkhName = nerkh.Name;
                }
                else
                {
                    var defaultNerkh = await _context.Nerkhs
                        .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
                        .FirstOrDefaultAsync();
                    if (defaultNerkh is null)
                        throw new InvalidOperationException("این کافه هیچ نرخ پیش‌فرضی تعریف نکرده است");
                    nerkhId = defaultNerkh.Id;
                    nerkhName = defaultNerkh.Name;
                }

                var guestCount = dto.Participants.Where(p => p.IsGuest).Sum(p => p.EntryCount);

            // Validate EntryCount rules
            ValidateEntryCountRules(dto.PlayType, dto.Participants);

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
                Status = dto.PlayType == "normal" ? "done" : "pending",
                NerkhId = nerkhId
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
                nerkhId,
                nerkhName,
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
            .Include(p => p.Nerkh)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId && !p.IsDeleted);

        if (play is null) return null;

        if (play.Room.ClubId != clubId) return null;

        var playersList = play.Clubplayplayers.OrderBy(pp => pp.Id).ToList();
        var ecMap = playersList.GroupBy(pp => pp.PlayerId).ToDictionary(g => g.Key, g => g.Count());
        var participants = playersList
            .Select(pp => new ClubPlayParticipantDto(
                pp.Id,
                pp.PlayerId,
                pp.Player.Name ?? "",
                pp.RoleId,
                pp.Role.Name ?? "",
                pp.Role.SideId,
                pp.Role.Photo,
                pp.IsGuest,
                ecMap.GetValueOrDefault(pp.PlayerId, 1)
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
            play.NerkhId,
            play.Nerkh?.Name,
            participants
        );
    }

    public async Task<ClubPlayDetailDto?> ReshuffleRolesAsync(int clubId, int playId, int? restrictToMasterId)
    {
        var play = await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Nerkh)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId && !p.IsDeleted);

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

            var ecMap = existingPlayers.GroupBy(pp => pp.PlayerId).ToDictionary(g => g.Key, g => g.Count());
            var participants = existingPlayers
                .Select(pp => new ClubPlayParticipantDto(
                    pp.Id,
                    pp.PlayerId,
                    pp.Player.Name ?? "",
                    pp.RoleId,
                    pp.Role.Name ?? "",
                    pp.Role.SideId,
                    pp.Role.Photo,
                    pp.IsGuest,
                    ecMap.GetValueOrDefault(pp.PlayerId, 1)
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
            play.NerkhId,
            play.Nerkh?.Name,
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
                          && !p.IsDeleted
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
            .FirstOrDefaultAsync(p => p.Id == playId && !p.IsDeleted);

        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید برای بازی دیگری رتبه ثبت کنید");

        if (play.Status != "notrank")
            throw new InvalidOperationException("این بازی در وضعیت مناسب برای ثبت رتبه نیست");

        if (ranks.Count != (play.PlayersCount ?? 0))
            throw new InvalidOperationException(
                $"تعداد رتبه‌ها ({ranks.Count}) با تعداد بازیکنان ({play.PlayersCount}) مطابقت ندارد");

        var duplicateIds = ranks.GroupBy(r => r.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateIds.Count != 0)
            throw new InvalidOperationException(
                $"ردیف تکراری در لیست رتبه‌ها: {string.Join("، ", duplicateIds)}");

        var existingRowIds = play.Clubplayplayers.Select(pp => pp.Id).ToHashSet();
        var missingIds = ranks.Select(r => r.Id).Where(id => !existingRowIds.Contains(id)).ToList();
        if (missingIds.Count != 0)
            throw new InvalidOperationException(
                $"این ردیف‌ها در این بازی نیستند: {string.Join("، ", missingIds)}");

        var rankByRowId = ranks.ToDictionary(r => r.Id, r => r.Rank);
        foreach (var pp in play.Clubplayplayers)
        {
            if (rankByRowId.TryGetValue(pp.Id, out var rank))
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
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && !p.IsDeleted && p.BusinessDate == businessDate)
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
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate == businessDate)
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
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && !p.IsDeleted && p.Status != "done")
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
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && !p.IsDeleted);

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
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate >= from && p.BusinessDate <= today);

        var totalPlays = await playIdsQuery.CountAsync();
        var playIds = await playIdsQuery.Select(p => p.Id).ToListAsync();

        var totalEntries = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId) && !pp.IsGuest)
            .SumAsync(pp => pp.EntryCount);
        var totalGuestEntries = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId) && pp.IsGuest)
            .SumAsync(pp => pp.EntryCount);

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
            .Where(p => p.Room.ClubId == clubId && !p.IsDeleted && p.BusinessDate >= from && p.BusinessDate <= today)
            .Include(p => p.Master)
            .Select(p => new { p.Id, p.MasterId, MasterName = p.Master.Name })
            .ToListAsync();

        var playIds = plays.Select(p => p.Id).ToList();

        var entryCounts = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId))
            .GroupBy(pp => pp.PlayId)
            .Select(g => new { PlayId = g.Key, Entries = g.Where(x => !x.IsGuest).Sum(x => x.EntryCount), Guests = g.Where(x => x.IsGuest).Sum(x => x.EntryCount) })
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
            .Where(p => p.Room.ClubId == clubId && p.MasterId == masterId && !p.IsDeleted && p.BusinessDate >= from && p.BusinessDate <= today);

        var totalPlays = await playIdsQuery.CountAsync();

        var playIds = await playIdsQuery.Select(p => p.Id).ToListAsync();

        var totalEntries = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId) && !pp.IsGuest)
            .SumAsync(pp => pp.EntryCount);

        var totalGuestEntries = await _context.Clubplayplayers
            .Where(pp => playIds.Contains(pp.PlayId) && pp.IsGuest)
            .SumAsync(pp => pp.EntryCount);

        return new MasterStatsDto(totalPlays, totalEntries, totalGuestEntries);
    }

    public async Task<ClubPlayDetailDto?> UpdateClubPlayAsync(int clubId, int playId, int? restrictToMasterId, string? actingClubRole, UpdateClubPlayDto dto)
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

        var incomingEntryTotal = dto.Participants.Sum(p => p.EntryCount);

        // Validate no duplicate distinct PlayerIds in the DTO
        var distinctDtoPlayerIds = dto.Participants.Select(p => p.ClubPlayerId).Distinct().ToList();
        if (distinctDtoPlayerIds.Count != dto.Participants.Count)
            throw new InvalidOperationException("بازیکن تکراری در لیست شرکت‌کنندگان وجود دارد");

        // Validate all are club members
        var membershipIds = await _context.ClubClubplayers
            .Where(cc => cc.ClubId == clubId)
            .Select(cc => cc.ClubplayerId)
            .ToListAsync();

        var invalidIds = distinctDtoPlayerIds.Where(id => !membershipIds.Contains(id)).ToList();
        if (invalidIds.Count != 0)
            throw new InvalidOperationException($"این بازیکنان عضو این کافه نیستند: {string.Join("، ", invalidIds)}");

        // Validate EntryCount rules against the new playType
        ValidateEntryCountRulesForUpdate(dto.PlayType, dto.Participants, play.Clubplayplayers);

        // Supervisor permission check: reject if supervisor tries to change entryCount for any player
        var currentRowCountByPlayer = play.Clubplayplayers
            .GroupBy(pp => pp.PlayerId)
            .ToDictionary(g => g.Key, g => g.Count());
        if (actingClubRole == "supervisor")
        {
            foreach (var p in dto.Participants)
            {
                var currentRowCount = currentRowCountByPlayer.GetValueOrDefault(p.ClubPlayerId, 0);
                if (p.EntryCount != currentRowCount)
                    throw new ForbiddenAppException("سوپروایزر مجاز به تغییر تعداد ورودی نیست");
            }
        }

        // ═══════════════════════════════════════════
        // Determine update tier
        // ═══════════════════════════════════════════
        var currentDistinctPlayers = play.Clubplayplayers
            .Select(pp => pp.PlayerId).Distinct().OrderBy(x => x).ToList();
        var incomingDistinctPlayers = distinctDtoPlayerIds.OrderBy(x => x).ToList();

        bool sameSenario = play.SenarioId == dto.SenarioId;
        bool sameTotalCount = play.PlayersCount == incomingEntryTotal;
        bool sameDistinctPlayerSet = currentDistinctPlayers.SequenceEqual(incomingDistinctPlayers);
        bool sameRowDistribution = sameDistinctPlayerSet
            && currentDistinctPlayers.All(id => currentRowCountByPlayer[id] ==
                dto.Participants.First(p => p.ClubPlayerId == id).EntryCount);
        bool sameGuests = sameDistinctPlayerSet
            && currentDistinctPlayers.All(id =>
                play.Clubplayplayers.First(pp => pp.PlayerId == id).IsGuest ==
                dto.Participants.First(p => p.ClubPlayerId == id).IsGuest);

        bool needsFullRebuild = !sameSenario || !sameTotalCount;
        bool needsParticipantSwap = !needsFullRebuild && !sameDistinctPlayerSet;
        bool needsEntryCountAdjust = !needsFullRebuild && sameDistinctPlayerSet && !sameRowDistribution;
        bool metadataOnly = sameSenario && sameTotalCount && sameDistinctPlayerSet && sameRowDistribution && sameGuests;

        // Validate role set exists (skip only for metadata-only — participant/senario didn't change)
        List<int>? roleIds = null;
        if (!metadataOnly)
        {
            var roleSet = await _context.SenarioRoleSets
                .FirstOrDefaultAsync(rs => rs.SenarioId == dto.SenarioId && rs.PlayerCount == incomingEntryTotal);

            if (roleSet is null)
                throw new InvalidOperationException($"این سناریو برای {incomingEntryTotal} نفر هنوز پیکربندی نشده است");

            roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
            if (roleIds is null || roleIds.Count != incomingEntryTotal)
            {
                _logger.LogError(
                    "SenarioRoleSet Id={Id}: RoleIds array length ({Actual}) does not match PlayerCount ({Expected})",
                    roleSet.Id, roleIds?.Count ?? 0, incomingEntryTotal);
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
                // Tier C: SenarioId or total entry-count changed
                //         → full role rebuild from scratch
                // ──────────────────────────────────────
                _context.Clubplayplayers.RemoveRange(play.Clubplayplayers);
                participants = await CreateClubPlayParticipantsAsync(
                    playId, dto.Participants, roleIds!, shuffle: true);
                play.WinnersideId = null;
                play.Status = dto.PlayType == "normal" ? "done" : "pending";
            }
            else if (needsParticipantSwap)
            {
                // ──────────────────────────────────────
                // Tier B: Same scenario + total count, different member set
                //         → merge participants preserving roles, handling row expansion
                // ──────────────────────────────────────
                participants = await MergeParticipantsPreservingRoles(
                    playId, play.Clubplayplayers, dto.Participants, roleIds!);
                play.WinnersideId = null;
            }
            else if (needsEntryCountAdjust)
            {
                // ──────────────────────────────────────
                // Tier B2: Same scenario + total count + same players,
                //          but entryCount distribution changed
                //         → adjust row counts per PlayerId preserving roles
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
                    var incomingMap = dto.Participants.ToDictionary(p => p.ClubPlayerId);
                    foreach (var pp in play.Clubplayplayers)
                    {
                        pp.IsGuest = incomingMap[pp.PlayerId].IsGuest;
                        pp.EntryCount = 1;
                    }
                }
                participants = play.Clubplayplayers
                    .OrderBy(pp => pp.Id)
                    .Select(pp => new ClubPlayParticipantDto(
                        pp.Id,
                        pp.PlayerId,
                        pp.Player.Name ?? "",
                        pp.RoleId,
                        pp.Role.Name ?? "",
                        pp.Role.SideId,
                        pp.Role.Photo,
                        pp.IsGuest,
                        0
                    ))
                    .ToList();
                // Compute and set entryCount (row-group count) on each result row
                var ecMap = participants.GroupBy(r => r.ClubPlayerId).ToDictionary(g => g.Key, g => g.Count());
                for (int i = 0; i < participants.Count; i++)
                {
                    var r = participants[i];
                    participants[i] = r with { EntryCount = ecMap[r.ClubPlayerId] };
                }
            }

            // ──────────────────────────────────────
            // Common: update metadata fields
            // ──────────────────────────────────────
            play.Title = dto.Title;
            play.DateTime = dto.DateTime;
            play.RoomId = dto.RoomId;
            play.SenarioId = dto.SenarioId;
            play.PlayersCount = incomingEntryTotal;
            play.GuestCount = dto.Participants.Where(p => p.IsGuest).Sum(p => p.EntryCount);
            play.Desc = dto.Desc;
            play.Link = dto.Link;
            play.PlayType = dto.PlayType;
            play.EventId = dto.EventId;

            if (dto.NerkhId.HasValue)
            {
                var nerkh = await _context.Nerkhs
                    .FirstOrDefaultAsync(n => n.Id == dto.NerkhId.Value && n.ClubId == clubId && !n.IsDeleted);
                if (nerkh is null)
                    throw new InvalidOperationException("نرخ انتخاب‌شده برای این کافه معتبر نیست");
                play.NerkhId = nerkh.Id;
            }
            else
            {
                var defaultNerkh = await _context.Nerkhs
                    .Where(n => n.ClubId == clubId && n.IsDefault && !n.IsDeleted)
                    .FirstOrDefaultAsync();
                if (defaultNerkh is null)
                    throw new InvalidOperationException("این کافه هیچ نرخ پیش‌فرضی تعریف نکرده است");
                play.NerkhId = defaultNerkh.Id;
            }

            if (!needsFullRebuild)
            {
                play.Status = ClubPlayStatusResolver.Resolve(
                    statusBeforeChanges,
                    play.PlayType,
                    play.WinnersideId,
                    play.Clubplayplayers.Select(pp => pp.Rank));
            }

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
                incomingEntryTotal,
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
                play.NerkhId,
                play.Nerkh?.Name,
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
        int clubId, int playId, int? restrictToMasterId, int participantRowId, ReplaceParticipantDto dto)
    {
        var play = await _context.Clubplays
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .Include(p => p.Room)
            .FirstOrDefaultAsync(p => p.Id == playId && !p.IsDeleted);

        if (play is null)
            throw new KeyNotFoundException("بازی یافت نشد");

        if (play.Room.ClubId != clubId)
            throw new KeyNotFoundException("بازی در این کافه یافت نشد");

        if (restrictToMasterId.HasValue && play.MasterId != restrictToMasterId.Value)
            throw new InvalidOperationException("شما نمی‌توانید بازی دیگری را ویرایش کنید");

        if (restrictToMasterId.HasValue && play.Status == "done")
            throw new InvalidOperationException("بازی‌های تکمیل‌شده قابل ویرایش نیستند");

        var targetRow = play.Clubplayplayers.FirstOrDefault(pp => pp.Id == participantRowId);
        if (targetRow is null)
            throw new KeyNotFoundException("این ردیف شرکت‌کننده در این بازی یافت نشد");

        var oldPlayerId = targetRow.PlayerId;
        var oldRoleId = targetRow.RoleId;

        // Validate new player is a club member
        var isMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == dto.NewClubPlayerId);

        if (!isMember)
            throw new InvalidOperationException("بازیکن جدید عضو این کافه نیست");

        // Validate EntryCount rules
        if (dto.EntryCount < 1 || dto.EntryCount > 10)
            throw new ValidationAppException("EntryCount باید بین ۱ تا ۱۰ باشد");

        if (dto.EntryCount > 1 && play.PlayType != "normal")
            throw new ValidationAppException("EntryCount بیشتر از ۱ فقط در بازی‌های normal مجاز است");

        if (dto.IsGuest && dto.EntryCount != 1)
            throw new ValidationAppException("ورودی مهمان (IsGuest) فقط با EntryCount=۱ مجاز است");

        // Per-row replace: swap PlayerId and IsGuest on the single target row
        targetRow.PlayerId = dto.NewClubPlayerId;
        targetRow.IsGuest = dto.IsGuest;
        targetRow.EntryCount = 1;

        // If there's already a row for the new player in this play (duplicate check),
        // we allow it only if entryCount > 1 scenario (different physical rows for same player ID).
        // In the per-row replace, having the same PlayerId on another row is OK.

        await _context.SaveChangesAsync();

        var newPlayer = await _context.Clubplayers.FindAsync(dto.NewClubPlayerId);
        var role = await _context.Roles
            .Include(r => r.Side)
            .FirstOrDefaultAsync(r => r.Id == oldRoleId);

        return new ClubPlayParticipantDto(
            targetRow.Id,
            targetRow.PlayerId,
            newPlayer?.Name ?? "",
            targetRow.RoleId,
            role?.Name ?? "",
            role?.SideId ?? 0,
            role?.Photo,
            targetRow.IsGuest,
            dto.EntryCount
        );
    }

    public async Task<bool> DeleteClubPlayAsync(int clubId, int playId, int actingUserId, bool isAdmin, int? restrictToMasterId)
    {
        var play = await _context.Clubplays
            .Include(p => p.Room)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null || play.IsDeleted) return false;
        if (!isAdmin && play.Room.ClubId != clubId) return false;

        if (restrictToMasterId.HasValue)
        {
            if (play.MasterId != restrictToMasterId.Value)
                throw new InvalidOperationException("شما نمی‌توانید بازی دیگری را حذف کنید");

            if (play.Status == "done")
                throw new InvalidOperationException("بازی‌های تکمیل‌شده توسط گرداننده قابل حذف نیستند — از مدیر یا سوپروایزر کافه بخواهید");
        }

        play.IsDeleted = true;
        play.DeletedAt = DateTime.UtcNow;
        play.DeletedByUserId = actingUserId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(List<ClubPlayDeletedListItemDto> Items, int Total)> GetDeletedPlaysAsync(int clubId, int page, int pageSize)
    {
        var query = _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Where(p => p.Room.ClubId == clubId && p.IsDeleted);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ClubPlayDeletedListItemDto(
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
                p.Master.Name,
                p.DeletedAt,
                p.DeletedByUserId,
                p.DeletedByUserId != null
                    ? _context.Users.Where(u => u.Id == p.DeletedByUserId).Select(u => u.DisplayName).FirstOrDefault()
                    : null
            ))
            .ToListAsync();

        return (items, total);
    }

    private async Task<Clubplay?> LoadPlayWithIncludesAsync(int playId)
    {
        return await _context.Clubplays
            .Include(p => p.Room)
            .Include(p => p.Senario)
            .Include(p => p.Master)
            .Include(p => p.Event)
            .Include(p => p.Nerkh)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Clubplayplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId && !p.IsDeleted);
    }

    private static ClubPlayDetailDto MapToDetailDto(Clubplay play)
    {
        var playersList = play.Clubplayplayers.OrderBy(pp => pp.Id).ToList();
        var ecMap = playersList.GroupBy(pp => pp.PlayerId).ToDictionary(g => g.Key, g => g.Count());
        var participants = playersList
            .Select(pp => new ClubPlayParticipantDto(
                pp.Id,
                pp.PlayerId,
                pp.Player.Name ?? "",
                pp.RoleId,
                pp.Role.Name ?? "",
                pp.Role.SideId,
                pp.Role.Photo,
                pp.IsGuest,
                ecMap.GetValueOrDefault(pp.PlayerId, 1)
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
            play.NerkhId,
            play.Nerkh?.Name,
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
        var addedEntities = new List<Clubplayplayer>();
        int roleIndex = 0;

        foreach (var input in participantInputs)
        {
            for (int copy = 0; copy < input.EntryCount; copy++)
            {
                var roleId = assignedRoleIds[roleIndex++];

                var entity = new Clubplayplayer
                {
                    PlayerId = input.ClubPlayerId,
                    RoleId = roleId,
                    PlayId = playId,
                    Rank = null,
                    Action = null,
                    IsGuest = input.IsGuest,
                    EntryCount = 1
                };
                _context.Clubplayplayers.Add(entity);
                addedEntities.Add(entity);

                var player = clubPlayerLookup[input.ClubPlayerId];
                var role = roleLookup[roleId];

                result.Add(new ClubPlayParticipantDto(
                    0,
                    input.ClubPlayerId,
                    player.Name ?? "",
                    roleId,
                    role.Name ?? "",
                    role.SideId,
                    role.Photo,
                    input.IsGuest,
                    0
                ));
            }
        }

        // Save to populate entity Ids
        await _context.SaveChangesAsync();

        // Set Id and entryCount on result DTOs
        var entryCountMap = addedEntities.GroupBy(e => e.PlayerId).ToDictionary(g => g.Key, g => g.Count());
        for (int i = 0; i < result.Count; i++)
        {
            result[i] = result[i] with { Id = addedEntities[i].Id, EntryCount = entryCountMap[addedEntities[i].PlayerId] };
        }

        return result;
    }

    private async Task<List<ClubPlayParticipantDto>> MergeParticipantsPreservingRoles(
        int playId,
        ICollection<Clubplayplayer> currentPlayers,
        List<ParticipantInputDto> incomingParticipants,
        List<int> allRoleIds)
    {
        var currentRowsByPlayer = currentPlayers.GroupBy(pp => pp.PlayerId).ToDictionary(g => g.Key, g => g.OrderBy(pp => pp.Id).ToList());
        var incomingIds = incomingParticipants.Select(p => p.ClubPlayerId).ToHashSet();
        var incomingMap = incomingParticipants.ToDictionary(p => p.ClubPlayerId);
        var allRolePool = new List<int>(allRoleIds);

        var removedIds = currentRowsByPlayer.Keys.Where(id => !incomingIds.Contains(id)).ToList();
        var addedInputs = incomingParticipants.Where(p => !currentRowsByPlayer.ContainsKey(p.ClubPlayerId)).ToList();
        var commonIds = currentRowsByPlayer.Keys.Where(incomingIds.Contains).ToList();

        // Collect freed RoleIds from ALL rows of removed players
        var freedRoles = new List<int>();
        foreach (var id in removedIds)
        {
            foreach (var row in currentRowsByPlayer[id])
            {
                freedRoles.Add(row.RoleId);
                _context.Clubplayplayers.Remove(row);
            }
        }

        // Adjust row count for common players, update IsGuest on each group
        var roleIndex = 0;
        foreach (var id in commonIds)
        {
            var targetCount = incomingMap[id].EntryCount;
            var existingRows = currentRowsByPlayer[id];
            var currentCount = existingRows.Count;

            foreach (var row in existingRows)
                row.IsGuest = incomingMap[id].IsGuest;

            if (targetCount > currentCount)
            {
                var extra = targetCount - currentCount;
                for (int i = 0; i < extra; i++)
                {
                    var roleId = roleIndex < freedRoles.Count ? freedRoles[roleIndex++] : allRolePool[0];
                    _context.Clubplayplayers.Add(new Clubplayplayer
                    {
                        PlayerId = id,
                        RoleId = roleId,
                        PlayId = playId,
                        Rank = null,
                        Action = null,
                        IsGuest = incomingMap[id].IsGuest,
                        EntryCount = 1
                    });
                }
            }
            else if (targetCount < currentCount)
            {
                var removeCount = currentCount - targetCount;
                var toRemove = existingRows.Skip(currentCount - removeCount).ToList();
                foreach (var row in toRemove)
                {
                    freedRoles.Add(row.RoleId);
                    _context.Clubplayplayers.Remove(row);
                }
            }
        }

        // Assign remaining freed roles to added players
        foreach (var input in addedInputs)
        {
            for (int copy = 0; copy < input.EntryCount; copy++)
            {
                var roleId = roleIndex < freedRoles.Count ? freedRoles[roleIndex++] : allRolePool[0];
                _context.Clubplayplayers.Add(new Clubplayplayer
                {
                    PlayerId = input.ClubPlayerId,
                    RoleId = roleId,
                    PlayId = playId,
                    Rank = null,
                    Action = null,
                    IsGuest = input.IsGuest,
                    EntryCount = 1
                });
            }
        }

        // Save to persist all changes and populate row Ids
        await _context.SaveChangesAsync();

        // Reload all rows for this play to get final state
        var savedRows = await _context.Clubplayplayers
            .Where(pp => pp.PlayId == playId)
            .OrderBy(pp => pp.Id)
            .ToListAsync();

        // Fetch player and role data for DTO
        var allPlayerIds = incomingParticipants.Select(p => p.ClubPlayerId).ToList();
        var players = await _context.Clubplayers
            .Where(p => allPlayerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
        var usedRoleIds = savedRows.Select(r => r.RoleId).Distinct().ToList();
        var roleLookup = await _context.Roles
            .Where(r => usedRoleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        var entryPerPlayer = savedRows.GroupBy(pp => pp.PlayerId).ToDictionary(g => g.Key, g => g.Count());
        var result = savedRows
            .Select(pp =>
            {
                var role = roleLookup.GetValueOrDefault(pp.RoleId);
                return new ClubPlayParticipantDto(
                    pp.Id,
                    pp.PlayerId,
                    players.GetValueOrDefault(pp.PlayerId)?.Name ?? "",
                    pp.RoleId,
                    role?.Name ?? "",
                    role?.SideId ?? 0,
                    role?.Photo,
                    pp.IsGuest,
                    entryPerPlayer.GetValueOrDefault(pp.PlayerId, 1)
                );
            })
            .ToList();

        return result;
    }

    // TODO: use EntryCount as quantity multiplier when computing game admission fee in club_order
    private static void ValidateEntryCountRules(string playType, List<ParticipantInputDto> participants)
    {
        foreach (var p in participants)
        {
            if (p.EntryCount < 1 || p.EntryCount > 10)
                throw new ValidationAppException("EntryCount باید بین ۱ تا ۱۰ باشد");

            if (p.EntryCount > 1 && playType != "normal")
                throw new ValidationAppException("EntryCount بیشتر از ۱ فقط در بازی‌های normal مجاز است");

            if (p.IsGuest && p.EntryCount != 1)
                throw new ValidationAppException("ورودی مهمان (IsGuest) فقط با EntryCount=۱ مجاز است");
        }
    }

    private static void ValidateEntryCountRulesForUpdate(
        string playType, List<ParticipantInputDto> participants,
        ICollection<Clubplayplayer> existingPlayers)
    {
        // Re-validate all three rules
        foreach (var p in participants)
        {
            if (p.EntryCount < 1 || p.EntryCount > 10)
                throw new ValidationAppException("EntryCount باید بین ۱ تا ۱۰ باشد");

            if (p.EntryCount > 1 && playType != "normal")
                throw new ValidationAppException("EntryCount بیشتر از ۱ فقط در بازی‌های normal مجاز است");

            if (p.IsGuest && p.EntryCount != 1)
                throw new ValidationAppException("ورودی مهمان (IsGuest) فقط با EntryCount=۱ مجاز است");
        }
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
