using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IDropdownService
{
    Task<IEnumerable<DropdownDto>> GetClubsAsync();
    Task<IEnumerable<EventDropdownDto>> GetEventsAsync();
    Task<IEnumerable<DropdownDto>> GetScenariosAsync();
    Task<IEnumerable<RoleDropdownDto>> GetRolesAsync();
}
