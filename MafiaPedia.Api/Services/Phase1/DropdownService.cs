using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.DTOs.Senario;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Services.Phase1;

public class DropdownService : IDropdownService
{
    private readonly MafiaDbContext _context;

    public DropdownService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<DropdownDto> GetAllAsync()
    {
        return new DropdownDto
        {
            Clubs = await _context.Clubs
                .Select(c => new DropdownItemDto { Id = c.Id, Name = c.Name })
                .ToListAsync(),

            Events = await _context.Events
                .Select(e => new DropdownItemDto { Id = e.Id, Name = e.Name })
                .ToListAsync(),

            EventsWithClub = await _context.Events
                .Select(e => new DropdownEventDto { Id = e.Id, Name = e.Name, ClubId = e.ClubId })
                .ToListAsync(),

            Senarios = await _context.Senarios
                .Select(s => new DropdownItemDto { Id = s.Id, Name = s.Name })
                .ToListAsync(),

            Sides = await _context.Sides
                .Select(s => new DropdownItemDto { Id = s.Id, Name = s.Name })
                .ToListAsync(),

            Masters = await _context.Masters
                .Select(m => new DropdownItemDto { Id = m.Id, Name = m.Name })
                .ToListAsync(),

            Rooms = await _context.Rooms
                .Select(r => new DropdownItemDto { Id = r.Id, Name = r.Name })
                .ToListAsync(),
        };
    }

    public async Task<IEnumerable<DropdownItemDto>> GetClubsAsync()
    {
        return await _context.Clubs
            .Select(c => new DropdownItemDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<IEnumerable<EventDropdownDto>> GetEventsAsync()
    {
        return await _context.Events
            .Select(e => new EventDropdownDto { Id = e.Id, Name = e.Name, ClubId = e.ClubId })
            .ToListAsync();
    }

    public async Task<IEnumerable<DropdownItemDto>> GetScenariosAsync()
    {
        return await _context.Senarios
            .Select(s => new DropdownItemDto { Id = s.Id, Name = s.Name })
            .ToListAsync();
    }

    public async Task<IEnumerable<RoleDropdownDto>> GetRolesAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDropdownDto { Id = r.Id, Name = r.Name, SenarioId = r.SenarioId, SideId = r.SideId, Photo = r.Photo })
            .ToListAsync();
    }

    public async Task<List<RoleSetEntryDto>?> GetRoleSetAsync(int senarioId, int playerCount)
    {
        var roleSet = await _context.SenarioRoleSets
            .FirstOrDefaultAsync(rs => rs.SenarioId == senarioId && rs.PlayerCount == playerCount);

        if (roleSet is null) return null;

        var roleIds = JsonSerializer.Deserialize<List<int>>(roleSet.RoleIds);
        if (roleIds is null) return null;

        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        return roleIds
            .Select(id => roles.TryGetValue(id, out var role)
                ? new RoleSetEntryDto(role.Id, role.Name ?? "", role.Photo, role.SideId)
                : null)
            .Where(e => e is not null)
            .ToList()!;
    }
}
