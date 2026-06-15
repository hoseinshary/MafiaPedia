import apiClient from './apiClient'
import type { PlayerProfile, PlayerSearchResult, PaginatedPlayers, PlayerDetail, HeadToHeadDto } from '@/types'

export const PlayerApi = {
  getPlayerProfile(playerId: number) {
    return apiClient.get<PlayerProfile>(`/players/${playerId}`)
  },

  searchPlayers(query: string) {
    return apiClient.get<PlayerSearchResult[]>('/players/search', {
      params: { query },
    })
  },

  createPlayer(formData: FormData) {
    return apiClient.post('/players', formData)
  },

  getPlayers(page: number, pageSize: number, search?: string) {
    return apiClient.get<PaginatedPlayers>('/players', {
      params: { page, pageSize, search: search || undefined },
    })
  },

  getPlayerDetail(id: number) {
    return apiClient.get<PlayerDetail>(`/players/${id}/detail`)
  },

  updatePlayer(id: number, formData: FormData) {
    return apiClient.put(`/players/${id}`, formData)
  },

  deletePlayer(id: number) {
    return apiClient.delete(`/players/${id}`)
  },

  getHeadToHead(player1Id: number, player2Id: number) {
    return apiClient.get<HeadToHeadDto>('/players/head-to-head', {
      params: { player1Id, player2Id },
    })
  },
}
