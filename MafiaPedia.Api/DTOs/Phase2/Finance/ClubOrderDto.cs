namespace MafiaPedia.Api.DTOs.Phase2.Finance;

public record CreateOrderItemDto(int ProductId, int Quantity = 1);

public record CreateClubOrderDto(int ClubPlayerId, List<CreateOrderItemDto> Items);

public record OrderItemDto(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record ClubOrderDto(int Id, int ClubPlayerId, string ClubPlayerName, DateOnly BusinessDate, decimal Total, string Status, List<OrderItemDto> Items);

public record ClubOrderListItemDto(int? OrderId, int ClubPlayerId, string ClubPlayerName, string ClubPlayerMobile, DateOnly BusinessDate, int ItemCount, decimal Total, string Status);

public record AddItemRequestDto(int ProductId, int ClubPlayerId, int Quantity = 1, int? OrderId = null, bool ForceNewOrder = false);

public record UpdateItemQuantityRequestDto(int NewQuantity);

public record AddItemResponseDto(int OrderItemId, int OrderId, decimal OrderTotal, string OrderStatus, bool WasSettledOrder, string? Warning);

public record UpdateItemQuantityResponseDto(int OrderItemId, int NewQuantity, decimal OrderTotal, string OrderStatus, bool WasSettledOrder);

public record RemoveItemResponseDto(int OrderItemId, int OrderId, decimal OrderTotal, string OrderStatus, bool WasSettledOrder);