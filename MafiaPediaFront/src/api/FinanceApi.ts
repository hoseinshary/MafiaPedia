import apiClient from './apiClient'
import type {
  NerkhDto, CreateNerkhDto, UpdateNerkhDto,
  ProductCategoryDto, CreateProductCategoryDto, UpdateProductCategoryDto,
  ProductDto, CreateProductDto, UpdateProductDto,
  ClubOrderDto, ClubOrderListItemDto, CreateClubOrderDto,
  AddItemRequestDto, AddItemResponseDto, UpdateItemQuantityRequestDto,
  UpdateItemQuantityResponseDto, RemoveItemResponseDto,
  ClubPlayerBalanceDto, CreateSettlementDto, SettlementDto, LedgerResponseDto,
  TodayOverviewDto, DebtorDto, FinanceDeletedRecordDto,
} from '@/types/finance'

export const FinanceApi = {
  // Nerkh
  getNerkhs(clubId: number) {
    return apiClient.get<NerkhDto[]>('/club/nerkh', { params: { clubId } })
  },
  createNerkh(clubId: number, dto: CreateNerkhDto) {
    return apiClient.post<NerkhDto>('/club/nerkh', dto, { params: { clubId } })
  },
  updateNerkh(clubId: number, id: number, dto: UpdateNerkhDto) {
    return apiClient.put<NerkhDto>(`/club/nerkh/${id}`, dto, { params: { clubId } })
  },
  deleteNerkh(clubId: number, id: number) {
    return apiClient.delete(`/club/nerkh/${id}`, { params: { clubId } })
  },

  // Product Categories
  getProductCategories(clubId: number) {
    return apiClient.get<ProductCategoryDto[]>('/club/product-category', { params: { clubId } })
  },
  createProductCategory(clubId: number, dto: CreateProductCategoryDto) {
    return apiClient.post<ProductCategoryDto>('/club/product-category', dto, { params: { clubId } })
  },
  updateProductCategory(clubId: number, id: number, dto: UpdateProductCategoryDto) {
    return apiClient.put<ProductCategoryDto>(`/club/product-category/${id}`, dto, { params: { clubId } })
  },
  deleteProductCategory(clubId: number, id: number) {
    return apiClient.delete(`/club/product-category/${id}`, { params: { clubId } })
  },

  // Products
  getProducts(clubId: number) {
    return apiClient.get<ProductDto[]>('/club/product', { params: { clubId } })
  },
  createProduct(clubId: number, dto: CreateProductDto) {
    return apiClient.post<ProductDto>('/club/product', dto, { params: { clubId } })
  },
  updateProduct(clubId: number, id: number, dto: UpdateProductDto) {
    return apiClient.put<ProductDto>(`/club/product/${id}`, dto, { params: { clubId } })
  },
  deleteProduct(clubId: number, id: number) {
    return apiClient.delete(`/club/product/${id}`, { params: { clubId } })
  },

  // Orders
  getOrderById(clubId: number, orderId: number) {
    return apiClient.get<ClubOrderDto>(`/club/order/${orderId}`, { params: { clubId } })
  },
  searchOrders(clubId: number, params: { query?: string; fromDate?: string; toDate?: string }) {
    return apiClient.get<ClubOrderListItemDto[]>('/club/order/search', { params: { clubId, ...params } })
  },
  createOrder(clubId: number, dto: CreateClubOrderDto) {
    return apiClient.post<ClubOrderDto>('/club/order', dto, { params: { clubId } })
  },
  getOrdersByClubPlayerAndDate(clubId: number, clubPlayerId: number, businessDate: string) {
    return apiClient.get<ClubOrderDto[]>(`/club/order/by-clubplayer/${clubPlayerId}`, {
      params: { clubId, businessDate },
    })
  },
  getOpenOrdersForCustomer(clubId: number, clubPlayerId: number) {
    return apiClient.get<ClubOrderDto[]>(`/club/order/open/${clubPlayerId}`, { params: { clubId } })
  },
  addItem(clubId: number, dto: AddItemRequestDto) {
    return apiClient.post<AddItemResponseDto>('/club/order/add-item', dto, { params: { clubId } })
  },
  updateItemQuantity(clubId: number, orderItemId: number, dto: UpdateItemQuantityRequestDto) {
    return apiClient.put<UpdateItemQuantityResponseDto>(`/club/order/items/${orderItemId}/quantity`, dto, { params: { clubId } })
  },
  removeItem(clubId: number, orderItemId: number) {
    return apiClient.delete<RemoveItemResponseDto>(`/club/order/items/${orderItemId}`, { params: { clubId } })
  },
  deleteOrder(clubId: number, id: number) {
    return apiClient.delete(`/club/order/${id}`, { params: { clubId } })
  },

  // Settlements
  getBalance(clubId: number, clubPlayerId: number, businessDate?: string) {
    return apiClient.get<ClubPlayerBalanceDto>(`/club/settlement/balance/${clubPlayerId}`, {
      params: { clubId, businessDate: businessDate || undefined },
    })
  },
  createSettlement(clubId: number, dto: CreateSettlementDto) {
    return apiClient.post<SettlementDto>('/club/settlement', dto, { params: { clubId } })
  },
  getLedger(clubId: number, clubPlayerId: number) {
    return apiClient.get<LedgerResponseDto>(`/club/settlement/ledger/${clubPlayerId}`, { params: { clubId } })
  },
  deleteSettlement(clubId: number, id: number) {
    return apiClient.delete(`/club/settlement/${id}`, { params: { clubId } })
  },
  getTodayOverview(clubId: number, status: string = 'all', date?: string) {
    return apiClient.get<TodayOverviewDto[]>('/club/settlement/today-overview', { params: { clubId, status, date } })
  },
  getDebtors(clubId: number) {
    return apiClient.get<DebtorDto[]>('/club/settlement/debtors', { params: { clubId } })
  },

  // Finance Audit
  getDeletedFinanceRecords(clubId: number, type: string) {
    return apiClient.get<FinanceDeletedRecordDto[]>('/club/finance-audit', { params: { clubId, type } })
  },
}
