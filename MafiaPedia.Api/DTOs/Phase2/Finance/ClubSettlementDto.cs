namespace MafiaPedia.Api.DTOs.Phase2.Finance;

public record CreateSettlementDto(int ClubPlayerId, decimal Amount, string PaymentMethod, string? Note, int? OrderId = null);

public record SettlementDto(int Id, int ClubPlayerId, decimal Amount, string PaymentMethod, string? Note, DateTime CreatedAt, int CreatedByUserId, string? CreatedByDisplayName, int? OrderId = null);

public record BalanceGameItemDto(int ClubPlayId, string? Title, string? NerkhName, decimal Price, string? RoomName);

public record BalanceOrderItemDto(int OrderId, List<OrderItemDto> Items);

public record ClubPlayerBalanceDto(
    int ClubPlayerId,
    string? ClubPlayerName,
    List<BalanceGameItemDto> TodayGames,
    List<BalanceOrderItemDto> TodayOrders,
    decimal TodayGamesTotal,
    decimal TodayOrdersTotal,
    decimal PreviousBalance,
    decimal TotalDue,
    decimal TodaySubtotal,
    decimal? VatPercent,
    decimal VatAmount,
    decimal TodayDue
);

public record TodayOverviewDto(
    int ClubPlayerId,
    string Name,
    string Mobile,
    int GamesCountToday,
    decimal TodayDue,
    decimal PreviousBalance,
    decimal PaidToday,
    string Status
);

public record DebtorDto(
    int ClubPlayerId,
    string Name,
    string Mobile,
    decimal TotalDebt,
    string? OldestUnpaidDate
);

public record LedgerEntryDto(
    string EntryType,
    string? Description,
    decimal Amount,
    DateTime? DateTime,
    DateOnly? BusinessDate,
    int? RelatedId
);

public record LedgerResponseDto(
    List<LedgerEntryDto> Entries,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance
);