import apiClient from './apiClient'

export const AuthApi = {
  login(username: string, password: string) {
    return apiClient.post('/auth/login', { username, password })
  },
  register(username: string, mobile: string, password: string) {
    return apiClient.post('/auth/register', { username, mobile, password })
  },
  refresh(refreshToken: string) {
    return apiClient.post('/auth/refresh', { refreshToken })
  },
}
