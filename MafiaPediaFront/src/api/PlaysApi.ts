import apiClient from './apiClient'
import type { CreatePlayPayload, PlayDetailDto, PaginatedPlays, PlayFilterParams } from '@/types'

export const PlaysApi = {
  createPlay(payload: CreatePlayPayload) {
    return apiClient.post('/plays', payload)
  },

  getPlays(page: number, pageSize: number, search?: string, filter?: PlayFilterParams) {
    const params: Record<string, string | number | undefined> = {
      page, pageSize, search: search || undefined,
    }
    if (filter) {
      if (filter.clubId) params.clubId = filter.clubId
      if (filter.eventId) params.eventId = filter.eventId
      if (filter.senarioId) params.senarioId = filter.senarioId
      if (filter.playerId) params.playerId = filter.playerId
      if (filter.winnersideId) params.winnersideId = filter.winnersideId
    }
    return apiClient.get<PaginatedPlays>('/plays', { params })
  },

  getPlaysPublic(filter: PlayFilterParams = {}) {
    const params: Record<string, string | number | undefined> = {}
    if (filter.clubId) params.clubId = filter.clubId
    if (filter.eventId) params.eventId = filter.eventId
    if (filter.senarioId) params.senarioId = filter.senarioId
    if (filter.playerId) params.playerId = filter.playerId
    if (filter.winnersideId) params.winnersideId = filter.winnersideId
    if (filter.search) params.search = filter.search
    if (filter.page) params.page = filter.page
    if (filter.pageSize) params.pageSize = filter.pageSize
    return apiClient.get<PaginatedPlays>('/plays', { params })
  },

  getPlay(playId: number) {
    return apiClient.get<PlayDetailDto>(`/plays/${playId}`)
  },

  updatePlay(playId: number, payload: CreatePlayPayload) {
    return apiClient.put(`/plays/${playId}`, payload)
  },

  deletePlay(playId: number) {
    return apiClient.delete(`/plays/${playId}`)
  },
}
