import apiClient from './apiClient'
import type { ClubPlayerDto, ClubPlayerJoinResult, CustomerSearchResultDto } from '@/types/clubPlayer'

export const ClubPlayerApi = {
  getClubPlayers(clubId: number, page: number, pageSize: number, search?: string) {
    return apiClient.get<{ items: ClubPlayerDto[]; total: number; page: number; pageSize: number; totalPages: number }>(
      `/clubs/${clubId}/customers`,
      { params: { page, pageSize, search } }
    )
  },

  getClubPlayerDetail(clubId: number, customerId: number) {
    return apiClient.get<ClubPlayerDto>(`/clubs/${clubId}/customers/${customerId}`)
  },

  createOrJoin(clubId: number, formData: FormData) {
    return apiClient.post<ClubPlayerJoinResult>(`/clubs/${clubId}/customers`, formData)
  },

  updateClubPlayer(clubId: number, customerId: number, formData: FormData) {
    return apiClient.put<ClubPlayerDto>(`/clubs/${clubId}/customers/${customerId}`, formData)
  },

  removeFromClub(clubId: number, customerId: number) {
    return apiClient.delete(`/clubs/${clubId}/customers/${customerId}`)
  },

  searchByMobile(mobile: string) {
    return apiClient.get<ClubPlayerDto>('/customers/search-by-mobile', { params: { mobile } })
  },

  searchAllCustomers(clubId: number, query: string) {
    return apiClient.get<CustomerSearchResultDto>(`/clubs/${clubId}/customers/search-all`, { params: { query } })
  },
}
