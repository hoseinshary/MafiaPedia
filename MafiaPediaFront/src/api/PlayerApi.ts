import apiClient from './apiClient'
import type { PlayerProfile } from '@/types'

export const PlayerApi = {
  getPlayerProfile(playerId: number) {
    return apiClient.get<PlayerProfile>(`/players/${playerId}`)
  },
}
