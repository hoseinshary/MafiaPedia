using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Master;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class MasterAuthService : IMasterAuthService
{
    private readonly MafiaDbContext _context;

    public MasterAuthService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<MasterContextDto?> GetMasterContextByUserIdAsync(int userId)
    {
        var master = await _context.Masters
            .Include(m => m.Club)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (master is null) return null;

        return new MasterContextDto(
            master.Id,
            master.Name ?? "",
            master.ClubId,
            master.Club.Name ?? ""
        );
    }
}
