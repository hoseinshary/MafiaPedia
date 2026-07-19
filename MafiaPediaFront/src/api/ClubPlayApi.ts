import apiClient from './apiClient'
import type { ClubPlayDetailDto, ClubPlayListItemDto, ClubPlayDeletedListItemDto, CreateClubPlayDto, EventDto, MasterStatsDto, MasterPerformanceDto, ClubPlayParticipantDto, ReplaceParticipantDto } from '@/types/clubPlay'

export type { ClubPlayParticipantDto }

export const ClubPlayApi = {
  createClubPlay(clubId: number, dto: CreateClubPlayDto) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays`, dto)
  },

  getClubPlayDetail(clubId: number, playId: number) {
    return apiClient.get<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}`)
  },

  getPlayCountByDate(clubId: number, date: string, masterId?: number) {
    const params: Record<string, string | number> = { date }
    if (masterId !== undefined) params.masterId = masterId
    return apiClient.get<{ count: number }>(`/clubs/${clubId}/clubplays/count-by-date`, { params })
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

  submitRanks(clubId: number, playId: number, ranks: { id: number; rank: number }[]) {
    return apiClient.post<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}/submit-ranks`, ranks)
  },

  // Dashboard reads
  getPlaysByBusinessDate(clubId: number, date?: string) {
    const params = date ? { date } : undefined
    return apiClient.get<ClubPlayListItemDto[]>(`/clubs/${clubId}/clubplays/by-date`, { params })
  },

  getClubPlaysByBusinessDate(clubId: number, date: string) {
    return apiClient.get<ClubPlayListItemDto[]>(`/clubs/${clubId}/clubplays/club-by-date`, {
      params: { date },
    })
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

  getClubStats(clubId: number, period: 'week' | 'month') {
    return apiClient.get<MasterStatsDto>(`/clubs/${clubId}/clubplays/club-stats`, { params: { period } })
  },
  getMasterPerformance(clubId: number, period: 'week' | 'month') {
    return apiClient.get<MasterPerformanceDto[]>(`/clubs/${clubId}/clubplays/master-performance`, { params: { period } })
  },

  // Editing
  updateClubPlay(clubId: number, playId: number, dto: CreateClubPlayDto) {
    return apiClient.put<ClubPlayDetailDto>(`/clubs/${clubId}/clubplays/${playId}`, dto)
  },

  replaceParticipant(clubId: number, playId: number, participantRowId: number, payload: ReplaceParticipantDto) {
    return apiClient.put<ClubPlayParticipantDto>(
      `/clubs/${clubId}/clubplays/${playId}/participants/${participantRowId}`,
      payload
    )
  },

  // Delete
  deleteClubPlay(clubId: number, playId: number) {
    return apiClient.delete(`/clubs/${clubId}/clubplays/${playId}`)
  },

  // Deleted plays audit
  getDeletedPlays(clubId: number, params: { page?: number; pageSize?: number }) {
    return apiClient.get<{ items: ClubPlayDeletedListItemDto[]; total: number; page: number; pageSize: number }>(
      `/clubs/${clubId}/clubplays/deleted`, { params }
    )
  },
}
