using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services.Iservices;

namespace MafiaPedia.Api.Services;

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
            .Select(r => new RoleDropdownDto { Id = r.Id, Name = r.Name, SenarioId = r.SenarioId, SideId = r.SideId })
            .ToListAsync();
    }
}
