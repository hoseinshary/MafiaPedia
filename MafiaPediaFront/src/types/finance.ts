export interface NerkhDto {
  id: number; name: string; price: number; isDefault: boolean; isActive: boolean | null
}
export interface CreateNerkhDto { name: string; price: number; isDefault?: boolean }
export interface UpdateNerkhDto { name: string; price: number; isDefault?: boolean }

export interface ProductCategoryDto { id: number; name: string }
export interface CreateProductCategoryDto { name: string }
export interface UpdateProductCategoryDto { name: string }

export interface ProductDto { id: number; name: string; categoryId: number; categoryName: string; price: number; isActive: boolean | null }
export interface CreateProductDto { name: string; categoryId: number; price: number; isActive?: boolean }
export interface UpdateProductDto { name: string; categoryId: number; price: number; isActive?: boolean }

export interface CreateOrderItemDto { productId: number; quantity?: number }
export interface CreateClubOrderDto { clubPlayerId: number; items: CreateOrderItemDto[] }

export interface OrderItemDto { id: number; productId: number; productName: string; quantity: number; unitPrice: number; lineTotal: number }

export interface ClubOrderDto {
  id: number; clubPlayerId: number; clubPlayerName: string; businessDate: string
  total: number; status: string; items: OrderItemDto[]
}

export interface ClubOrderListItemDto { orderId: number | null; clubPlayerId: number; clubPlayerName: string; clubPlayerMobile: string; businessDate: string; itemCount: number; total: number; status: string }

export interface AddItemRequestDto { productId: number; clubPlayerId: number; quantity?: number; orderId?: number | null; forceNewOrder?: boolean }
export interface AddItemResponseDto { orderItemId: number; orderId: number; orderTotal: number; orderStatus: string; wasSettledOrder: boolean; warning: string | null }
export interface UpdateItemQuantityRequestDto { newQuantity: number }
export interface UpdateItemQuantityResponseDto { orderItemId: number; newQuantity: number; orderTotal: number; orderStatus: string; wasSettledOrder: boolean }
export interface RemoveItemResponseDto { orderItemId: number; orderId: number; orderTotal: number; orderStatus: string; wasSettledOrder: boolean }

export interface CreateSettlementDto { clubPlayerId: number; amount: number; paymentMethod: string; note?: string; orderId?: number | null }
export interface SettlementDto { id: number; clubPlayerId: number; amount: number; paymentMethod: string; note: string | null; createdAt: string; createdByUserId: number; createdByDisplayName: string | null; orderId: number | null }

export interface BalanceGameItemDto { clubPlayId: number; title: string | null; nerkhName: string | null; price: number; roomName: string | null }
export interface BalanceOrderItemDto { orderId: number; items: OrderItemDto[] }

export interface ClubPlayerBalanceDto {
  clubPlayerId: number; clubPlayerName: string | null
  todayGames: BalanceGameItemDto[]; todayOrders: BalanceOrderItemDto[]
  todayGamesTotal: number; todayOrdersTotal: number; previousBalance: number; totalDue: number
  todaySubtotal: number; vatPercent: number | null; vatAmount: number; todayDue: number
}

export interface TodayOverviewDto {
  clubPlayerId: number; name: string; mobile: string
  gamesCountToday: number; todayDue: number; previousBalance: number; paidToday: number; status: string
}

export interface DebtorDto {
  clubPlayerId: number; name: string; mobile: string; totalDebt: number; oldestUnpaidDate: string | null
}

export interface LedgerEntryDto { entryType: string; description: string | null; amount: number; dateTime: string | null; businessDate: string | null; relatedId: number | null }
export interface LedgerResponseDto { entries: LedgerEntryDto[]; totalDebit: number; totalCredit: number; balance: number }
export interface FinanceDeletedRecordDto { id: number; name: string; deletedAt: string; deletedBy: string }