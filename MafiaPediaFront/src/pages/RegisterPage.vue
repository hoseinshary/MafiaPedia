<template>
  <div dir="rtl" class="min-h-screen bg-bg flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-fg">ثبت‌نام</h1>
        <p class="text-muted mt-2 text-sm">حساب کاربری جدید بسازید</p>
      </div>
      <form @submit.prevent="handleRegister" class="bg-surface rounded-[10px] border border-border p-6 space-y-4">
        <div v-if="error" class="bg-danger/20 border border-danger/40 text-danger text-sm rounded px-4 py-3">
          {{ error }}
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">شماره موبایل</label>
          <input
            v-model="mobile"
            type="text"
            dir="ltr"
            maxlength="11"
            class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            placeholder="09123456789"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">نام کاربری</label>
          <input
            v-model="username"
            type="text"
            dir="ltr"
            class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            placeholder="username"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">رمز عبور</label>
          <input
            v-model="password"
            type="password"
            dir="ltr"
            class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            placeholder="حداقل ۶ کاراکتر"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-muted">تکرار رمز عبور</label>
          <input
            v-model="confirmPassword"
            type="password"
            dir="ltr"
            class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            placeholder="رمز عبور را دوباره وارد کنید"
          />
        </div>
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded-[8px] font-medium transition"
        >
          <span v-if="loading" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            در حال ثبت‌نام...
          </span>
          <span v-else>ثبت‌نام</span>
        </button>
        <p class="text-center text-sm text-muted mt-4">
          قبلاً ثبت‌نام کردی؟
          <router-link to="/login" class="text-gold-text hover:opacity-80 transition">وارد شو</router-link>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

const mobile = ref('')
const username = ref('')
const password = ref('')
const confirmPassword = ref('')
const error = ref('')
const loading = ref(false)

function validateMobile(val: string): boolean {
  return /^09\d{9}$/.test(val)
}

async function handleRegister() {
  error.value = ''

  if (!mobile.value || !username.value || !password.value || !confirmPassword.value) {
    error.value = 'لطفاً همه فیلدها را پر کنید'
    return
  }

  if (!validateMobile(mobile.value)) {
    error.value = 'شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود'
    return
  }

  if (password.value.length < 6) {
    error.value = 'رمز عبور باید حداقل ۶ کاراکتر باشد'
    return
  }

  if (password.value !== confirmPassword.value) {
    error.value = 'رمز عبور و تکرار آن یکسان نیستند'
    return
  }

  loading.value = true
  try {
    await authStore.register(username.value, mobile.value, password.value)
    await router.push('/')
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ثبت‌نام'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}
</script>
