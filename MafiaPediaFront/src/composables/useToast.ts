import { reactive, readonly } from 'vue'

export type ToastType = 'error' | 'success' | 'warning'

export interface Toast {
  id: number
  message: string
  type: ToastType
  duration: number
}

const toasts = reactive<Toast[]>([])
let nextId = 1

export function useToast() {
  function showToast(message: string, type: ToastType = 'error', duration: number = 4000) {
    const id = nextId++
    toasts.push({ id, message, type, duration })
    if (duration > 0) {
      setTimeout(() => removeToast(id), duration)
    }
  }

  function removeToast(id: number) {
    const idx = toasts.findIndex((t) => t.id === id)
    if (idx !== -1) toasts.splice(idx, 1)
  }

  function toastError(message: string) {
    showToast(message, 'error')
  }

  function toastSuccess(message: string) {
    showToast(message, 'success')
  }

  function toastWarning(message: string) {
    showToast(message, 'warning')
  }

  return {
    toasts: readonly(toasts),
    showToast,
    removeToast,
    toastError,
    toastSuccess,
    toastWarning,
  }
}
