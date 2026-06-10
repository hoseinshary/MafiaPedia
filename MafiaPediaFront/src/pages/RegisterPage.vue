<template>
  <div dir="rtl" class="min-h-screen bg-gray-900 flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-white">ثبت‌نام</h1>
        <p class="text-gray-400 mt-2 text-sm">حساب کاربری جدید بسازید</p>
      </div>
      <form @submit.prevent="handleRegister" class="bg-gray-800 rounded-lg p-6 space-y-4">
        <div v-if="error" class="bg-red-900/50 border border-red-700 text-red-300 text-sm rounded px-4 py-3">
          {{ error }}
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">شماره موبایل</label>
          <input
            v-model="mobile"
            type="text"
            dir="ltr"
            maxlength="11"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="09123456789"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">نام کاربری</label>
          <input
            v-model="username"
            type="text"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="username"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">رمز عبور</label>
          <input
            v-model="password"
            type="password"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="حداقل ۶ کاراکتر"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">تکرار رمز عبور</label>
          <input
            v-model="confirmPassword"
            type="password"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="رمز عبور را دوباره وارد کنید"
          />
        </div>
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded font-medium transition"
        >
          <span v-if="loading" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            در حال ثبت‌نام...
          </span>
          <span v-else>ثبت‌نام</span>
        </button>
        <p class="text-center text-sm text-gray-400 mt-4">
          قبلاً ثبت‌نام کردی؟
          <router-link to="/login" class="text-blue-400 hover:text-blue-300 transition">وارد شو</router-link>
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
