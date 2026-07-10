using MafiaPedia.Api.DTOs.Phase2.Master;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IMasterAuthService
{
    Task<MasterContextDto?> GetMasterContextByUserIdAsync(int userId);
}
