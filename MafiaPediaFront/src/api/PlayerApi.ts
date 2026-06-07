import apiClient from './apiClient'
import type { PlayerProfile, PlayerSearchResult } from '@/types'

export const PlayerApi = {
  getPlayerProfile(playerId: number) {
    return apiClient.get<PlayerProfile>(`/players/${playerId}`)
  },

  searchPlayers(query: string) {
    return apiClient.get<PlayerSearchResult[]>('/players/search', {
      params: { query },
    })
  },
}
