<template>
  <div dir="rtl" class="min-h-screen bg-bg flex items-center justify-center px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-fg">ورود</h1>
        <p class="text-muted mt-2 text-sm">به حساب کاربری خود وارد شوید</p>
      </div>
      <form @submit.prevent="handleLogin" class="bg-surface rounded-[10px] border border-border p-6 space-y-4">
        <div v-if="error" class="bg-danger/20 border border-danger/40 text-danger text-sm rounded px-4 py-3">
          {{ error }}
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
            placeholder="••••••••"
          />
        </div>
        <label class="flex items-center gap-2 cursor-pointer">
          <input
            v-model="rememberMe"
            type="checkbox"
            class="w-4 h-4 rounded border-border bg-input text-gold focus:ring-gold focus:ring-2 transition"
          />
          <span class="text-sm text-muted">مرا به خاطر بسپار</span>
        </label>
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded-[8px] font-medium transition"
        >
          <span v-if="loading" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            در حال ورود...
          </span>
          <span v-else>ورود</span>
        </button>
        <p class="text-center text-sm text-muted mt-4">
          حساب کاربری نداری؟
          <router-link to="/register" class="text-gold-text hover:opacity-80 transition">ثبت‌نام کن</router-link>
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
    await authStore.loadClubContexts()

    if (authStore.clubContexts.length > 1 && !authStore.activeClubContext) {
      await router.push('/select-club')
      return
    }

    const roleRoutes: Record<string, string> = {
      master: '/master',
      owner: '/owner',
      supervisor: '/supervisor',
      cashier: '/cashier',
    }
    if (authStore.activeClubRole && roleRoutes[authStore.activeClubRole]) {
      await router.push(roleRoutes[authStore.activeClubRole])
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
