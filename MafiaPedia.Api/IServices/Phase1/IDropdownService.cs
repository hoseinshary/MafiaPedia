using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.DTOs.Senario;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IDropdownService
{
    Task<DropdownDto> GetAllAsync();
    Task<IEnumerable<DropdownItemDto>> GetClubsAsync();
    Task<IEnumerable<EventDropdownDto>> GetEventsAsync();
    Task<IEnumerable<DropdownItemDto>> GetScenariosAsync();
    Task<IEnumerable<RoleDropdownDto>> GetRolesAsync();

    Task<List<RoleSetEntryDto>?> GetRoleSetAsync(int senarioId, int playerCount);
}
