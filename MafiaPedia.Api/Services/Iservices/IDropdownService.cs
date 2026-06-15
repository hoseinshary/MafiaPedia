using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IDropdownService
{
    Task<DropdownDto> GetAllAsync();
    Task<IEnumerable<DropdownItemDto>> GetClubsAsync();
    Task<IEnumerable<EventDropdownDto>> GetEventsAsync();
    Task<IEnumerable<DropdownItemDto>> GetScenariosAsync();
    Task<IEnumerable<RoleDropdownDto>> GetRolesAsync();
}
