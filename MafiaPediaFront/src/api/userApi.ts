import apiClient from './apiClient'
import type { UserDto, PaginatedUsers, CreateUserDto, UpdateUserDto } from '@/types'

export const UserApi = {
  searchUsers(query: string) {
    return apiClient.get<PaginatedUsers>('/users', {
      params: { page: 1, pageSize: 10, search: query },
    })
  },

  getUsers(page: number, pageSize: number, search?: string) {
    return apiClient.get<PaginatedUsers>('/users', {
      params: { page, pageSize, search: search || undefined },
    })
  },

  getUserDetail(id: number) {
    return apiClient.get<UserDto>(`/users/${id}`)
  },

  createUser(dto: CreateUserDto) {
    return apiClient.post('/users', dto)
  },

  updateUser(id: number, dto: UpdateUserDto) {
    return apiClient.put(`/users/${id}`, dto)
  },

  deleteUser(id: number) {
    return apiClient.delete(`/users/${id}`)
  },
}
