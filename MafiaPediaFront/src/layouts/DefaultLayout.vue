<template>
  <div class="min-h-screen flex flex-col" style="background:#0d0d0f;color:#e8e4d9;font-family:'Vazirmatn',sans-serif;">
    <header style="background:#0d0d0f;border-bottom:0.5px solid rgba(255,255,255,0.08);position:sticky;top:0;z-index:50;">
      <div class="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between" dir="rtl">
        <div class="flex items-center gap-6">
          <router-link to="/" style="font-size:18px;font-weight:700;letter-spacing:0.04em;color:#c9b07a;text-decoration:none;">
            Mafia<span style="color:#e8e4d9;">Pedia</span>
          </router-link>
          <nav class="hidden md:flex gap-6" style="font-size:13px;color:rgba(232,228,217,0.5);">
            <router-link to="/ranking/overall" class="nav-link">رتبه‌بندی</router-link>
            <router-link to="/plays-public" class="nav-link">بازی‌ها</router-link>
            <router-link to="/statistics" class="nav-link">آمار</router-link>
            <router-link to="/head-to-head" class="nav-link" style="font-size:13px;color:rgba(232,228,217,0.8);">Head To Head</router-link>
          </nav>
        </div>
        <div class="flex items-center gap-4">
          <div class="hidden md:block">
            <PlayerSearchAutocomplete />
          </div>
          <button @click="mobileSearchOpen = !mobileSearchOpen" class="md:hidden hover:opacity-70 transition p-1">
            <svg v-if="!mobileSearchOpen" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" style="color:rgba(232,228,217,0.5);" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd" />
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" style="color:rgba(232,228,217,0.5);" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
          <button class="hover:opacity-70 transition opacity-60 cursor-not-allowed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" style="color:rgba(232,228,217,0.5);" viewBox="0 0 20 20" fill="currentColor">
              <path d="M10 2a6 6 0 00-6 6v3.586l-.707.707A1 1 0 004 14h12a1 1 0 00.707-1.707L16 11.586V8a6 6 0 00-6-6zM10 18a3 3 0 01-3-3h6a3 3 0 01-3 3z" />
            </svg>
          </button>
          <template v-if="authStore.isAuthenticated">
            <span class="text-sm hidden md:inline" style="color:rgba(232,228,217,0.7);">{{ authStore.displayName }}</span>
            <button @click="handleLogout" class="text-sm" style="color:#e07070;background:none;border:none;cursor:pointer;font-family:inherit;">
              خروج
            </button>
          </template>
          <template v-else>
            <router-link to="/login" style="text-decoration:none;">
              <button class="btn-ghost">ورود</button>
            </router-link>
            <router-link to="/register" style="text-decoration:none;">
              <button class="btn-gold">ثبت‌نام</button>
            </router-link>
          </template>
        </div>
      </div>
      <div v-if="mobileSearchOpen" class="md:hidden px-4 py-3" style="border-top:0.5px solid rgba(255,255,255,0.06);">
        <PlayerSearchAutocomplete />
      </div>
    </header>

    <AdminSidebar v-if="authStore.isAdmin" />

    <main class="flex-1 max-w-6xl mx-auto px-6 py-6" style="width:100%;" :class="authStore.isAdmin ? 'mr-48' : ''">
      <router-view />
    </main>

    <footer style="border-top:0.5px solid rgba(255,255,255,0.06);text-align:center;padding:1rem;font-size:12px;color:rgba(232,228,217,0.3);background:#0d0d0f;">
      &copy; {{ currentYear }} MafiaPedia
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'
import AdminSidebar from '@/components/AdminSidebar.vue'

const router = useRouter()
const authStore = useAuthStore()

const mobileSearchOpen = ref(false)
const currentYear = new Date().getFullYear()

function handleLogout() {
  authStore.logout()
  router.push('/')
}
</script>

<style scoped>
.nav-link {
  color: rgba(232, 228, 217, 0.5);
  text-decoration: none;
  transition: color 0.15s;
}
.nav-link:hover {
  color: #e8e4d9;
}
.btn-ghost {
  font-size: 12px;
  padding: 6px 14px;
  border: 0.5px solid rgba(255,255,255,0.2);
  border-radius: 6px;
  background: transparent;
  color: rgba(232,228,217,0.7);
  cursor: pointer;
  font-family: inherit;
  transition: color 0.15s;
}
.btn-ghost:hover {
  color: #e8e4d9;
}
.btn-gold {
  font-size: 12px;
  padding: 6px 14px;
  border: none;
  border-radius: 6px;
  background: #c9b07a;
  color: #0d0d0f;
  cursor: pointer;
  font-weight: 700;
  font-family: inherit;
  transition: opacity 0.15s;
}
.btn-gold:hover {
  opacity: 0.88;
}
</style>
