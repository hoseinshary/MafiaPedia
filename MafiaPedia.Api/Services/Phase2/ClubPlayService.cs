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

            var clubPlay = new Clubplay
            {
                Title = dto.Title,
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

    public async Task<ClubPlayDetailDto?> ReshuffleRolesAsync(int clubId, int playId, int masterId)
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
        if (play.MasterId != masterId)
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
        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);

        return await _context.Clubplays
            .CountAsync(p => p.Room.ClubId == clubId
                          && p.MasterId == masterId
                          && p.DateTime >= dateStart
                          && p.DateTime <= dateEnd);
    }

    public async Task<ClubPlayDetailDto?> ConfirmRoleRevealAsync(int clubId, int playId, int masterId)
    {
        var play = await LoadPlayWithIncludesAsync(playId);
        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (play.MasterId != masterId)
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

    public async Task<ClubPlayDetailDto?> SubmitWinnersideAsync(int clubId, int playId, int masterId, int winnersideId)
    {
        var play = await LoadPlayWithIncludesAsync(playId);
        if (play is null) return null;
        if (play.Room.ClubId != clubId) return null;

        if (play.MasterId != masterId)
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

    public async Task<ClubPlayDetailDto?> SubmitRanksAsync(int clubId, int playId, int masterId, List<ParticipantRankDto> ranks)
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

        if (play.MasterId != masterId)
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
                p.PlayType
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
                p.PlayType
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
                p.PlayType
            ))
            .ToListAsync();

        return (items, total);
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

    public async Task<ClubPlayDetailDto?> UpdateClubPlayAsync(int clubId, int playId, int masterId, UpdateClubPlayDto dto)
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

        if (play.MasterId != masterId)
            throw new InvalidOperationException("شما نمی‌توانید بازی دیگری را ویرایش کنید");

        if (play.Status == "done")
            throw new InvalidOperationException("بازی‌های تکمیل‌شده قابل ویرایش نیستند");

        // Validate room
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.ClubId == clubId);
        if (room is null)
            throw new KeyNotFoundException("اتاق یافت نشد");

        // Validate participants count matches PlayersCount
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

        // Validate role set exists
        var roleSet = await _context.SenarioRoleSets
            .FirstOrDefaultAsync(rs => rs.SenarioId == dto.SenarioId && rs.PlayerCount == dto.Participants.Count);

        if (roleSet is null)
            throw new InvalidOperationException($"این سناریو برای {dto.Participants.Count} نفر هنوز پیکربندی نشده است");

        var roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
        if (roleIds is null || roleIds.Count != dto.Participants.Count)
        {
            _logger.LogError(
                "SenarioRoleSet Id={Id}: RoleIds array length ({Actual}) does not match PlayerCount ({Expected})",
                roleSet.Id, roleIds?.Count ?? 0, dto.Participants.Count);
            throw new InvalidOperationException("خطای یکپارچگی داده‌های پیکربندی سناریو");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Delete existing Clubplayplayer rows
            _context.Clubplayplayers.RemoveRange(play.Clubplayplayers);

            // Update Clubplay fields
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
            play.WinnersideId = null;

            // Reset status based on PlayType (roles were just reshuffled)
            play.Status = dto.PlayType == "normal" ? "done" : "pending";

            // Reassign roles from scratch
            var participants = await CreateClubPlayParticipantsAsync(
                playId, dto.Participants, roleIds, dto.ShuffleRoles);

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
                masterId,
                (await _context.Masters.FindAsync(masterId))?.Name ?? "",
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
