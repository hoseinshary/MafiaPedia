import apiClient from './apiClient'
import type { CreatePlayPayload } from '@/types'

export const PlaysApi = {
  createPlay(payload: CreatePlayPayload) {
    return apiClient.post('/plays', payload)
  },
}
