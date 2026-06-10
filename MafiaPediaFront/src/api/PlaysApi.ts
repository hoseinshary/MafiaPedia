import apiClient from './apiClient'
import type { CreatePlayPayload, PlayDto, PaginatedPlays } from '@/types'

export const PlaysApi = {
  createPlay(payload: CreatePlayPayload) {
    return apiClient.post('/plays', payload)
  },

  getPlays(page: number, pageSize: number, search?: string) {
    return apiClient.get<PaginatedPlays>('/plays', {
      params: { page, pageSize, search: search || undefined },
    })
  },

  getPlay(playId: number) {
    return apiClient.get<PlayDto>(`/plays/${playId}`)
  },

  updatePlay(playId: number, payload: CreatePlayPayload) {
    return apiClient.put(`/plays/${playId}`, payload)
  },

  deletePlay(playId: number) {
    return apiClient.delete(`/plays/${playId}`)
  },
}
