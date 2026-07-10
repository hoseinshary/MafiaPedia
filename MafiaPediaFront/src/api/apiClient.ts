import axios, { type AxiosInstance, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { useToast } from '@/composables/useToast'

function getStoredToken(key: string): string | null {
  return localStorage.getItem(key) ?? sessionStorage.getItem(key)
}

function setStoredToken(key: string, value: string): void {
  if (localStorage.getItem(key) !== null) {
    localStorage.setItem(key, value)
  } else if (sessionStorage.getItem(key) !== null) {
    sessionStorage.setItem(key, value)
  } else {
    sessionStorage.setItem(key, value)
  }
}

function clearStoredToken(key: string): void {
  localStorage.removeItem(key)
  sessionStorage.removeItem(key)
}

const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api',
  timeout: 10000,
  headers: {
    //'Content-Type': 'application/json',
  },
})

function isTokenExpiringSoon(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const expiresAt = payload.exp * 1000
    const now = Date.now()
    const fiveMinutes = 5 * 60 * 1000
    return expiresAt - now < fiveMinutes
  } catch {
    return false
  }
}

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = getStoredToken('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (err: unknown) => void
}> = []

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token!)
    }
  })
  failedQueue = []
}

apiClient.interceptors.response.use(
  async (response: AxiosResponse) => {
    const token = getStoredToken('accessToken')
    if (token && isTokenExpiringSoon(token) && !isRefreshing) {
      try {
        isRefreshing = true
        const refreshToken = getStoredToken('refreshToken')
        if (refreshToken) {
          const res = await axios.post(
            `${apiClient.defaults.baseURL}/auth/refresh`,
            { refreshToken }
          )
          const { accessToken: newAccess, refreshToken: newRefresh } = res.data
          setStoredToken('accessToken', newAccess)
          setStoredToken('refreshToken', newRefresh)
        }
      } catch {
        // ignore – 401 interceptor handles it later
      } finally {
        isRefreshing = false
      }
    }
    return response
  },
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`
          return apiClient(originalRequest)
        })
      }
      originalRequest._retry = true
      isRefreshing = true
      try {
        const refreshToken = getStoredToken('refreshToken')
        if (!refreshToken) throw new Error('No refresh token')
        const res = await axios.post(
          `${apiClient.defaults.baseURL}/auth/refresh`,
          { refreshToken }
        )
        const { accessToken: newAccess, refreshToken: newRefresh } = res.data
        setStoredToken('accessToken', newAccess)
        setStoredToken('refreshToken', newRefresh)
        processQueue(null, newAccess)
        originalRequest.headers.Authorization = `Bearer ${newAccess}`
        return apiClient(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        clearStoredToken('accessToken')
        clearStoredToken('refreshToken')
        clearStoredToken('role')
        clearStoredToken('displayName')
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }
    const { toastError, toastWarning } = useToast()

    if (!error.response) {
      toastError('اتصال به سرور برقرار نیست')
    } else if (error.response.status === 403) {
      toastError('شما دسترسی به این بخش را ندارید')
    } else if (error.response.status === 409) {
      toastWarning(error.response?.data?.message ?? 'تعارض در عملیات')
    } else if (error.response.status === 500) {
      toastError('خطای سرور — لطفاً دوباره تلاش کنید')
    }
    return Promise.reject(error)
  }
)

export default apiClient
