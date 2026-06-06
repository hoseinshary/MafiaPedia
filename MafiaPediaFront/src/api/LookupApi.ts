import apiClient from './apiClient'
import type { Club, Event, Scenario, Role } from '@/types'

export const LookupApi = {
  getClubs() {
    return apiClient.get<Club[]>('/clubs')
  },

  getEvents() {
    return apiClient.get<Event[]>('/events')
  },

  getScenarios() {
    return apiClient.get<Scenario[]>('/scenarios')
  },

  getRoles() {
    return apiClient.get<Role[]>('/roles')
  },
}
