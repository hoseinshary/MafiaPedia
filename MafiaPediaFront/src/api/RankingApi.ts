import apiClient from './apiClient'
import type { OverallRankingEntry, SideRankingEntry, SideRankingParams, OverallRankingFilterDto } from '@/types'

export const RankingApi = {
  getOverallRanking(filter: OverallRankingFilterDto = {}) {
    return apiClient.get<OverallRankingEntry[]>('/rankings/overall', {
      params: filter,
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
