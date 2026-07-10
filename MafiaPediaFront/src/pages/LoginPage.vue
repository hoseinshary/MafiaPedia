<template>
  <div dir="rtl" class="min-h-screen bg-[#0d0d0f] flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-[#e8e4d9]">ورود</h1>
        <p class="text-[rgba(232,228,217,0.4)] mt-2 text-sm">به حساب کاربری خود وارد شوید</p>
      </div>
      <form @submit.prevent="handleLogin" class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-6 space-y-4">
        <div v-if="error" class="bg-[rgba(224,112,112,0.1)] border border-[rgba(224,112,112,0.2)] text-[#e07070] text-sm rounded px-4 py-3">
          {{ error }}
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">نام کاربری</label>
          <input
            v-model="username"
            type="text"
            dir="ltr"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="username"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">رمز عبور</label>
          <input
            v-model="password"
            type="password"
            dir="ltr"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="••••••••"
          />
        </div>
        <label class="flex items-center gap-2 cursor-pointer">
          <input
            v-model="rememberMe"
            type="checkbox"
            class="w-4 h-4 rounded border-[rgba(255,255,255,0.15)] bg-[#0d0d0f] text-[#c9b07a] focus:ring-[#c9b07a] focus:ring-2 transition"
          />
          <span class="text-sm text-[rgba(232,228,217,0.4)]">مرا به خاطر بسپار</span>
        </label>
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded-[8px] font-medium transition"
        >
          <span v-if="loading" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            در حال ورود...
          </span>
          <span v-else>ورود</span>
        </button>
        <p class="text-center text-sm text-[rgba(232,228,217,0.4)] mt-4">
          حساب کاربری نداری؟
          <router-link to="/register" class="text-[#c9b07a] hover:text-[#b8a16e] transition">ثبت‌نام کن</router-link>
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
const rememberMe = ref(false)
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
    await authStore.login(username.value, password.value, rememberMe.value)
    if (authStore.isMaster) {
      await router.push('/master')
      return
    }
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
