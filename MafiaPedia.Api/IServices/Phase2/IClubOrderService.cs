using MafiaPedia.Api.DTOs.Phase2.Finance;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IClubOrderService
{
    Task<ClubOrderDto> CreateAsync(int clubId, int userId, CreateClubOrderDto dto);
    Task<ClubOrderDto> GetByIdAsync(int clubId, int orderId);
    Task<List<ClubOrderDto>> GetByClubPlayerAndDateAsync(int clubId, int clubPlayerId, DateOnly businessDate);
    Task<List<ClubOrderDto>> GetOpenOrdersForCustomerAsync(int clubId, int clubPlayerId);
    Task<List<ClubOrderListItemDto>> SearchAsync(int clubId, string? query, DateOnly? fromDate, DateOnly? toDate, int limit = 200);
    Task DeleteAsync(int clubId, int orderId, int userId);

    Task<AddItemResponseDto> AddItemAsync(int clubId, int userId, int? orderId, int clubPlayerId, int productId, int quantity, bool forceNewOrder);
    Task<UpdateItemQuantityResponseDto> UpdateItemQuantityAsync(int clubId, int userId, int orderItemId, int newQuantity);
    Task<RemoveItemResponseDto> RemoveItemAsync(int clubId, int userId, int orderItemId);
    Task RecalculateOrderStatusAsync(int orderId);
}