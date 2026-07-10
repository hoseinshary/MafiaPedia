import apiClient from './apiClient'
import type {
  ClubDto,
  ClubDetailDto,
  CreateClubDto,
  UpdateClubDto,
  RoomDto,
  CreateRoomDto,
  UpdateRoomDto,
  MasterDto,
  CreateMasterDto,
  UpdateMasterDto,
  ClubListItem,
} from '@/types/club'

function toFormData(obj: Record<string, unknown>, prefix = ''): FormData {
  const fd = new FormData()
  for (const [key, val] of Object.entries(obj)) {
    const name = prefix ? `${prefix}.${key}` : key
    if (val === null || val === undefined) continue
    if (typeof val === 'object' && !(val instanceof File) && !(val instanceof Blob)) {
      if (Array.isArray(val)) {
        val.forEach((v, i) => { fd.append(`${name}[${i}]`, String(v)) })
      } else {
        for (const [k, v] of Object.entries(val as Record<string, unknown>)) {
          const nested = toFormData({ [k]: v }, name)
          for (const [nk, nv] of nested.entries()) { fd.append(nk, nv) }
        }
      }
    } else {
      fd.append(name, val instanceof File || val instanceof Blob ? val : String(val))
    }
  }
  return fd
}

export const ClubApi = {
  getClubs() {
    return apiClient.get<ClubListItem[]>('/clubs')
  },

  getClubDetail(clubId: number) {
    return apiClient.get<ClubDetailDto>(`/clubs/${clubId}`)
  },

  createClub(dto: CreateClubDto, logo?: File) {
    const fd = toFormData(dto as unknown as Record<string, unknown>)
    if (logo) fd.append('logo', logo)
    return apiClient.post<ClubDto>('/clubs', fd)
  },

  updateClub(clubId: number, dto: UpdateClubDto, logo?: File) {
    const fd = toFormData(dto as unknown as Record<string, unknown>)
    if (logo) fd.append('logo', logo)
    return apiClient.put<ClubDto>(`/clubs/${clubId}`, fd)
  },

  deleteClub(clubId: number) {
    return apiClient.delete(`/clubs/${clubId}`)
  },

  createRoom(clubId: number, dto: CreateRoomDto) {
    return apiClient.post<RoomDto>(`/clubs/${clubId}/rooms`, dto)
  },

  updateRoom(clubId: number, roomId: number, dto: UpdateRoomDto) {
    return apiClient.put<RoomDto>(`/clubs/${clubId}/rooms/${roomId}`, dto)
  },

  deleteRoom(clubId: number, roomId: number) {
    return apiClient.delete(`/clubs/${clubId}/rooms/${roomId}`)
  },

  createMaster(clubId: number, dto: CreateMasterDto, photo?: File) {
    const fd = toFormData(dto as unknown as Record<string, unknown>)
    if (photo) fd.append('photo', photo)
    return apiClient.post<MasterDto>(`/clubs/${clubId}/masters`, fd)
  },

  updateMaster(clubId: number, masterId: number, dto: UpdateMasterDto, photo?: File) {
    const fd = toFormData(dto as unknown as Record<string, unknown>)
    if (photo) fd.append('photo', photo)
    return apiClient.put<MasterDto>(`/clubs/${clubId}/masters/${masterId}`, fd)
  },

  deleteMaster(clubId: number, masterId: number) {
    return apiClient.delete(`/clubs/${clubId}/masters/${masterId}`)
  },
}
