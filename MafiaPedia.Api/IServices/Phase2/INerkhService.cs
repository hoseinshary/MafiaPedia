using MafiaPedia.Api.DTOs.Phase2.Finance;

namespace MafiaPedia.Api.IServices.Phase2;

public interface INerkhService
{
    Task<List<NerkhDto>> GetAllAsync(int clubId);
    Task<NerkhDto> CreateAsync(int clubId, int userId, CreateNerkhDto dto);
    Task<NerkhDto> UpdateAsync(int clubId, int nerkhId, int userId, UpdateNerkhDto dto);
    Task DeleteAsync(int clubId, int nerkhId, int userId);
}
