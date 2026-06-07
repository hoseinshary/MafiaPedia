import apiClient from './apiClient'
import type { OverallRankingEntry, SideRankingEntry, SideRankingParams } from '@/types'

export const RankingApi = {
  getOverallRanking(clubId?: number) {
    return apiClient.get<OverallRankingEntry[]>('/rankings/overall', {
      params: clubId !== undefined ? { clubId } : undefined,
    })
  },

  getCitizenRanking(params: SideRankingParams) {
    return apiClient.get<SideRankingEntry[]>('/rankings/side', {
      params: { ...params, sideId: params.sideId },
    })
  },

  getMafiaRanking(params: SideRankingParams) {
    return apiClient.get<SideRankingEntry[]>('/rankings/side', {
      params: { ...params, sideId: params.sideId },
    })
  },
}
