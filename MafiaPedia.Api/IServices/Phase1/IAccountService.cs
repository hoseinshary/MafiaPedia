using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IAccountService
{
    Task<AccountDto> GetMyAccountAsync(int userId);
    Task<AccountDto> UpdateAccountAsync(int userId, UpdateAccountDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<string> UpdateLinkedPictureAsync(int userId, string target, IFormFile file);
}
