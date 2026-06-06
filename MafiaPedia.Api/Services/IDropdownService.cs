using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface IDropdownService
{
    Task<IEnumerable<DropdownDto>> GetClubsAsync();
    Task<IEnumerable<EventDropdownDto>> GetEventsAsync();
    Task<IEnumerable<DropdownDto>> GetScenariosAsync();
    Task<IEnumerable<RoleDropdownDto>> GetRolesAsync();
}
