import apiClient from './apiClient'
import type {
  ClubUserDto,
  CreateClubUserDto,
  UpdateClubUserRoleDto,
  ClubUserContextDto,
} from '@/types/club'

export const ClubUserApi = {
  getMembers(clubId: number) {
    return apiClient.get<ClubUserDto[]>(`/clubs/${clubId}/members`)
  },
  createMember(clubId: number, dto: CreateClubUserDto) {
    return apiClient.post<ClubUserDto>(`/clubs/${clubId}/members`, dto)
  },
  updateMemberRole(clubId: number, memberId: number, dto: UpdateClubUserRoleDto) {
    return apiClient.put<ClubUserDto>(`/clubs/${clubId}/members/${memberId}`, dto)
  },
  deleteMember(clubId: number, memberId: number) {
    return apiClient.delete(`/clubs/${clubId}/members/${memberId}`)
  },
  getMyClubs() {
    return apiClient.get<ClubUserContextDto[]>('/clubusers/me')
  },
}
