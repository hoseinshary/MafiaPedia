<template>
  <div dir="rtl" class="min-h-screen bg-gray-900 flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-white">ورود</h1>
        <p class="text-gray-400 mt-2 text-sm">به حساب کاربری خود وارد شوید</p>
      </div>
      <form @submit.prevent="handleLogin" class="bg-gray-800 rounded-lg p-6 space-y-4">
        <div v-if="error" class="bg-red-900/50 border border-red-700 text-red-300 text-sm rounded px-4 py-3">
          {{ error }}
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
            placeholder="••••••••"
          />
        </div>
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded font-medium transition"
        >
          <span v-if="loading" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            در حال ورود...
          </span>
          <span v-else>ورود</span>
        </button>
        <p class="text-center text-sm text-gray-400 mt-4">
          حساب کاربری نداری؟
          <router-link to="/register" class="text-blue-400 hover:text-blue-300 transition">ثبت‌نام کن</router-link>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  error.value = ''
  if (!username.value || !password.value) {
    error.value = 'لطفاً نام کاربری و رمز عبور را وارد کنید'
    return
  }
  loading.value = true
  try {
    await authStore.login(username.value, password.value)
    const redirect = (route.query.redirect as string) || '/'
    await router.push(redirect)
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ورود'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}
</script>
