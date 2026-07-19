import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthApi } from '@/api/AuthApi'
import { getMyAccount } from '@/api/AccountApi'
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

function hasSessionToken(): boolean {
  return !!(localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken'))
}

const AUTH_SYNC_CHANNEL = 'mafiapedia-auth-sync'
const RELAY_TIMEOUT_MS = 700
const RELAY_REQUEST_KEY = '__mafia_auth_relay_req'
const RELAY_RESPONSE_KEY = '__mafia_auth_relay_resp'

let authReadyResolve: () => void
const authReady: Promise<void> = new Promise(resolve => { authReadyResolve = resolve })

function initAuthSync(
  setTokens: (access: string, refresh: string, name: string) => void,
  onSynced: () => void,
) {
  if (hasSessionToken()) {
    authReadyResolve()
    return
  }

  if (typeof BroadcastChannel !== 'undefined') {
    let resolved = false
    const done = () => { if (!resolved) { resolved = true; authReadyResolve() } }

    const channel = new BroadcastChannel(AUTH_SYNC_CHANNEL)
    channel.onmessage = (e) => {
      if (e.data?.type === 'SESSION_RESPONSE') {
        setTokens(e.data.accessToken, e.data.refreshToken, e.data.displayName)
        onSynced()
        done()
      }
    }

    channel.postMessage({ type: 'REQUEST_SESSION' })
    setTimeout(done, RELAY_TIMEOUT_MS)
  } else {
    // localStorage fallback: write a request key (fires storage event in other tabs),
    // other tabs respond by writing sessionStorage (tab-scoped, so requesting tab reads directly).
    try { localStorage.setItem(RELAY_REQUEST_KEY, String(Date.now())) } catch { authReadyResolve(); return }

    const readResponse = () => {
      try {
        const raw = sessionStorage.getItem(RELAY_RESPONSE_KEY)
        if (raw) {
          const data = JSON.parse(raw)
          sessionStorage.removeItem(RELAY_RESPONSE_KEY)
          setTokens(data.accessToken, data.refreshToken, data.displayName)
          onSynced()
          return true
        }
      } catch { /* ignore */ }
      return false
    }

    if (readResponse()) { authReadyResolve(); return }

    const interval = setInterval(() => { if (readResponse()) { clearInterval(interval); authReadyResolve() } }, 50)
    setTimeout(() => { clearInterval(interval); try { localStorage.removeItem(RELAY_REQUEST_KEY) } catch {} authReadyResolve() }, RELAY_TIMEOUT_MS)
  }
}

function setupSessionProvider() {
  if (typeof BroadcastChannel !== 'undefined') {
    const channel = new BroadcastChannel(AUTH_SYNC_CHANNEL)
    channel.onmessage = (e) => {
      if (e.data?.type === 'REQUEST_SESSION' && hasSessionToken()) {
        channel.postMessage({
          type: 'SESSION_RESPONSE',
          accessToken: localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken') || '',
          refreshToken: localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken') || '',
          displayName: localStorage.getItem('displayName') || sessionStorage.getItem('displayName') || '',
        })
      }
    }
  } else {
    window.addEventListener('storage', (e) => {
      if (e.key === RELAY_REQUEST_KEY && e.newValue && hasSessionToken()) {
        try {
          sessionStorage.setItem(RELAY_RESPONSE_KEY, JSON.stringify({
            accessToken: localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken') || '',
            refreshToken: localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken') || '',
            displayName: localStorage.getItem('displayName') || sessionStorage.getItem('displayName') || '',
          }))
        } catch { /* ignore */ }
      }
    })
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

  async function fetchDisplayName() {
    if (!accessToken.value || displayName.value) return
    try {
      const account = await getMyAccount()
      displayName.value = account.displayName
      persist()
    } catch { /* ignore — displayName stays empty until next attempt */ }
  }

  function setTokens(access: string, refresh: string, name: string) {
    useLocalStorage = false
    accessToken.value = access
    refreshToken.value = refresh
    displayName.value = name
    persist()
  }

  initAuthSync(setTokens, loadClubContexts)
  setupSessionProvider()

  async function login(mobile: string, password: string, rememberMe = false) {
    const res = await AuthApi.login(mobile, password)
    const data = res.data
    useLocalStorage = rememberMe
    accessToken.value = data.accessToken
    refreshToken.value = data.refreshToken
    persist()
    fetchDisplayName()
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
      fetchDisplayName()
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
      fetchDisplayName()
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
    localStorage.removeItem(RELAY_REQUEST_KEY)
    sessionStorage.removeItem('accessToken')
    sessionStorage.removeItem('refreshToken')
    sessionStorage.removeItem('displayName')
    sessionStorage.removeItem(RELAY_RESPONSE_KEY)
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
    authReady,
  }
})
