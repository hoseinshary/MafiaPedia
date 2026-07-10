import apiClient from './apiClient'
import type { MasterContextDto } from '@/types/clubPlay'

export const MasterApi = {
  getMasterContext() {
    return apiClient.get<MasterContextDto>('/masters/me')
  },
}
