<template>
  <div dir="rtl" class="max-w-lg mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-gold-text">تغییر رمز عبور</h1>
      <p class="text-sm text-muted mt-1">رمز عبور حساب خود را تغییر دهید</p>
    </div>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm" :class="notification.type === 'success' ? 'bg-success/20 border border-success text-success' : 'bg-danger/20 border border-danger text-danger'">
      {{ notification.message }}
    </div>

    <div class="bg-surface border border-border rounded-xl p-6">
      <div class="flex flex-col gap-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">رمز عبور فعلی</label>
          <input v-model="oldPassword" type="password" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">رمز عبور جدید</label>
          <input v-model="newPassword" type="password" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">تکرار رمز عبور جدید</label>
          <input v-model="confirmPassword" type="password" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
          <div v-if="passwordError" class="text-xs text-danger mt-1">{{ passwordError }}</div>
        </div>
        <button @click="handleSubmit" :disabled="submitting || !!passwordError" class="self-start px-5 py-2 bg-gold text-[#141416] text-sm font-bold rounded hover:opacity-90 transition disabled:opacity-50 disabled:cursor-not-allowed inline-flex items-center gap-2">
          <div v-if="submitting" class="w-4 h-4 border-2 border-[#141416] border-t-transparent rounded-full animate-spin" />
          تغییر رمز عبور
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { changePassword } from '@/api/AccountApi'

const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const submitting = ref(false)
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

const passwordError = computed(() => {
  if (!newPassword.value) return ''
  if (newPassword.value.length < 6) return 'رمز عبور باید حداقل ۶ کاراکتر باشد'
  if (newPassword.value !== confirmPassword.value) return 'رمز عبور و تکرار آن یکسان نیست'
  return ''
})

async function handleSubmit() {
  notification.value = null

  if (!oldPassword.value || !newPassword.value || !confirmPassword.value) {
    notification.value = { type: 'error', message: 'لطفاً همه فیلدها را پر کنید' }
    return
  }
  if (passwordError.value) return

  submitting.value = true
  try {
    await changePassword({ oldPassword: oldPassword.value, newPassword: newPassword.value })
    notification.value = { type: 'success', message: 'رمز عبور با موفقیت تغییر کرد' }
    oldPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      notification.value = { type: 'error', message: err.response?.data?.message || 'خطا در تغییر رمز عبور' }
    } else {
      notification.value = { type: 'error', message: 'خطا در برقراری ارتباط' }
    }
  } finally {
    submitting.value = false
  }
}
</script>
