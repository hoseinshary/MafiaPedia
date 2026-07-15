import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthApi } from '@/api/AuthApi'
import type { ClubUserContextDto } from '@/types/club'
import { ClubUserApi } from '@/api/ClubUserApi'

function parseJwt(token: string) {
  const base64Url = token.split('.')[1]
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
  const jsonPayload = decodeURIComponent(
    atob(base64).split('').map(c =>
      '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    ).join('')
  )
  return JSON.parse(jsonPayload)
}

function extractRole(token: string): string {
  try {
    const payload = parseJwt(token)
    return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''
  } catch {
    return ''
  }
}

function extractUserId(token: string): number | null {
  try {
    const payload = parseJwt(token)
    const id = payload.nameid || payload.sub
    return id ? Number(id) : null
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  let useLocalStorage = true

  function getInitialToken(key: string): string {
    const local = localStorage.getItem(key)
    if (local) {
      useLocalStorage = true
      return local
    }
    const session = sessionStorage.getItem(key)
    if (session) {
      useLocalStorage = false
      return session
    }
    return ''
  }

  const accessToken = ref(getInitialToken('accessToken'))
  const refreshToken = ref(getInitialToken('refreshToken'))
  const displayName = ref(getInitialToken('displayName'))

  const role = computed(() => accessToken.value ? extractRole(accessToken.value) : '')
  const isAuthenticated = computed(() => !!accessToken.value)
  const isAdmin = computed(() => role.value === 'admin')
  const isClub = computed(() => role.value === 'club')

  const userId = computed(() => accessToken.value ? extractUserId(accessToken.value) : null)

  const clubContexts = ref<ClubUserContextDto[]>([])
  const clubContextsLoaded = ref(false)

  const activeClubId = ref<number | null>(
    localStorage.getItem('activeClubId') ? Number(localStorage.getItem('activeClubId')) : null
  )

  const activeClubContext = computed(() =>
    clubContexts.value.find(c => c.clubId === activeClubId.value) ?? null
  )

  const activeClubRole = computed(() => activeClubContext.value?.clubuserRole ?? '')
  const isMaster = computed(() => activeClubRole.value === 'master')
  const isOwner = computed(() => activeClubRole.value === 'owner')
  const isSupervisor = computed(() => activeClubRole.value === 'supervisor')
  const isCashier = computed(() => activeClubRole.value === 'cashier')

  function getStorage() {
    return useLocalStorage ? localStorage : sessionStorage
  }

  function persist() {
    const storage = getStorage()
    if (accessToken.value) {
      storage.setItem('accessToken', accessToken.value)
    } else {
      storage.removeItem('accessToken')
    }
    if (refreshToken.value) {
      storage.setItem('refreshToken', refreshToken.value)
    } else {
      storage.removeItem('refreshToken')
    }
    if (displayName.value) {
      storage.setItem('displayName', displayName.value)
    } else {
      storage.removeItem('displayName')
    }
  }

  async function login(mobile: string, password: string, rememberMe = false) {
    const res = await AuthApi.login(mobile, password)
    const data = res.data
    useLocalStorage = rememberMe
    accessToken.value = data.accessToken
    refreshToken.value = data.refreshToken
    displayName.value = data.displayName
    persist()
  }

  async function register(username: string, mobile: string, password: string) {
    await AuthApi.register(username, mobile, password)
    await login(username, password)
  }

  async function refreshAccessToken(): Promise<string> {
    if (!refreshToken.value) throw new Error('No refresh token')
    const res = await AuthApi.refresh(refreshToken.value)
    const data = res.data
    accessToken.value = data.accessToken
    refreshToken.value = data.refreshToken
    persist()
    return data.accessToken
  }

  async function loadClubContexts() {
    if (!isClub.value) {
      clubContextsLoaded.value = true
      return
    }
    try {
      const res = await ClubUserApi.getMyClubs()
      clubContexts.value = res.data
      if (res.data.length === 1) {
        setActiveClub(res.data[0].clubId)
      } else if (res.data.length > 1 && !res.data.some(c => c.clubId === activeClubId.value)) {
        activeClubId.value = null
        localStorage.removeItem('activeClubId')
      }
    } catch {
      clubContexts.value = []
    } finally {
      clubContextsLoaded.value = true
    }
  }

  function setActiveClub(clubId: number) {
    activeClubId.value = clubId
    localStorage.setItem('activeClubId', String(clubId))
  }

  function logout() {
    accessToken.value = ''
    refreshToken.value = ''
    displayName.value = ''
    clubContexts.value = []
    activeClubId.value = null
    clubContextsLoaded.value = false
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('displayName')
    localStorage.removeItem('activeClubId')
    sessionStorage.removeItem('accessToken')
    sessionStorage.removeItem('refreshToken')
    sessionStorage.removeItem('displayName')
  }

  return {
    accessToken,
    refreshToken,
    role,
    displayName,
    isAuthenticated,
    isAdmin,
    isClub,
    userId,
    clubContexts,
    clubContextsLoaded,
    activeClubId,
    activeClubContext,
    activeClubRole,
    isMaster,
    isOwner,
    isSupervisor,
    isCashier,
    login,
    register,
    logout,
    refreshAccessToken,
    loadClubContexts,
    setActiveClub,
  }
})
