import apiClient from './apiClient'

export interface AccountDto {
  id: number
  username: string
  displayName: string
  mobile: string
  role: string
  player: { id: number; name: string; picture: string | null } | null
  clubplayer: { id: number; name: string; picture: string | null } | null
  master: { id: number; name: string; photo: string | null } | null
}

export async function getMyAccount(): Promise<AccountDto> {
  const res = await apiClient.get<AccountDto>('/account/me')
  return res.data
}

export async function updateMyAccount(payload: { username?: string; displayName?: string }): Promise<AccountDto> {
  const res = await apiClient.put<AccountDto>('/account/me', payload)
  return res.data
}

export async function changePassword(payload: { oldPassword: string; newPassword: string }): Promise<void> {
  await apiClient.put('/account/change-password', payload)
}

export async function uploadLinkedPicture(target: 'player' | 'clubplayer' | 'master', file: File): Promise<{ path: string }> {
  const formData = new FormData()
  formData.append('target', target)
  formData.append('file', file)
  const res = await apiClient.put<{ path: string }>('/account/me/picture', formData)
  return res.data
}
