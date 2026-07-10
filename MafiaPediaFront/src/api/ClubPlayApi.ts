import apiClient from './apiClient'
import type { ClubPlayDetailDto, ClubPlayListItemDto, CreateClubPlayDto, EventDto, MasterStatsDto } from '@/types/clubPlay'

export const ClubPlayApi = {
  createClubPlay(clubId: number, dto: CreateClubPlayDto) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays`, dto)
  },

  getClubPlayDetail(clubId: number, playId: number) {
    return apiClient.get<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}`)
  },

  getPlayCountByDate(clubId: number, date: string) {
    return apiClient.get<{ count: number }>(`/clubs/${clubId}/clubplays/count-by-date`, { params: { date } })
  },

  getClubEvents(clubId: number) {
    return apiClient.get<EventDto[]>(`/clubs/${clubId}/events`)
  },

  reshuffleRoles(clubId: number, playId: number) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}/reshuffle-roles`)
  },

  // Status transitions
  confirmReveal(clubId: number, playId: number) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}/confirm-reveal`)
  },

  submitWinnerside(clubId: number, playId: number, winnersideId: number) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}/submit-winnerside`, { winnersideId })
  },

  submitRanks(clubId: number, playId: number, ranks: { clubPlayerId: number; rank: number }[]) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}/submit-ranks`, ranks)
  },

  // Dashboard reads
  getPlaysByBusinessDate(clubId: number, date?: string) {
    const params = date ? { date } : undefined
    return apiClient.get<ClubPlayListItemDto[]>(`/clubs/${clubId}/clubplays/by-date`, { params })
  },

  getOpenPlays(clubId: number) {
    return apiClient.get<ClubPlayListItemDto[]>(`/clubs/${clubId}/clubplays/open`)
  },

  getMyPlays(clubId: number, params: { page?: number; pageSize?: number; dateFrom?: string; dateTo?: string; status?: string }) {
    return apiClient.get<{ items: ClubPlayListItemDto[]; total: number; page: number; pageSize: number }>(`/clubs/${clubId}/clubplays/mine`, { params })
  },

  getMyStats(clubId: number, period: 'week' | 'month') {
    return apiClient.get<MasterStatsDto>(`/clubs/${clubId}/clubplays/my-stats`, { params: { period } })
  },

  // Editing
  updateClubPlay(clubId: number, playId: number, dto: CreateClubPlayDto) {
    return apiClient.put<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}`, dto)
  },
}
