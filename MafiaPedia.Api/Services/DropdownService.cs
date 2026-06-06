using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public class DropdownService : IDropdownService
{
    private readonly MafiaDbContext _context;

    public DropdownService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DropdownDto>> GetClubsAsync()
    {
        return await _context.Clubs
            .Select(c => new DropdownDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<IEnumerable<EventDropdownDto>> GetEventsAsync()
    {
        return await _context.Events
            .Select(e => new EventDropdownDto { Id = e.Id, Name = e.Name, ClubId = e.ClubId })
            .ToListAsync();
    }

    public async Task<IEnumerable<DropdownDto>> GetScenariosAsync()
    {
        return await _context.Senarios
            .Select(s => new DropdownDto { Id = s.Id, Name = s.Name })
            .ToListAsync();
    }

    public async Task<IEnumerable<RoleDropdownDto>> GetRolesAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDropdownDto { Id = r.Id, Name = r.Name, SenarioId = r.SenarioId, SideId = r.SideId })
            .ToListAsync();
    }
}
