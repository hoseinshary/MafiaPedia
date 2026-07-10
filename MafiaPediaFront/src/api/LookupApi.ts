import apiClient from './apiClient'
import type { Club, Event, Senario, Role, DropdownData, RoleSetEntryDto } from '@/types'

export const LookupApi = {
  getClubs() {
    return apiClient.get<Club[]>('/dropdown/clubs')
  },

  getEvents() {
    return apiClient.get<Event[]>('/events')
  },

  getScenarios() {
    return apiClient.get<Senario[]>('/scenarios')
  },

  getRoles() {
    return apiClient.get<Role[]>('/roles')
  },

  getDropdown() {
    return apiClient.get<DropdownData>('/dropdown')
  },

  getRoleSet(senarioId: number, playerCount: number) {
    return apiClient.get<RoleSetEntryDto[]>(`/senarios/${senarioId}/role-set`, { params: { playerCount } })
  },
}
