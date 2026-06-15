import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthApi } from '@/api/AuthApi'

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

  const userId = computed(() => accessToken.value ? extractUserId(accessToken.value) : null)

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

  function logout() {
    accessToken.value = ''
    refreshToken.value = ''
    displayName.value = ''
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('displayName')
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
    userId,
    login,
    register,
    logout,
    refreshAccessToken,
  }
})
