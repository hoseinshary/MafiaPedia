<template>
  <div class="min-h-screen flex flex-col" :style="{ background: 'var(--color-bg)', color: 'var(--color-fg)', fontFamily: 'Vazirmatn, sans-serif' }">
    <header :style="{ background: 'var(--color-bg)', borderBottom: '0.5px solid var(--color-border)', position: 'sticky', top: 0, zIndex: 50 }">
      <div class="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between" dir="rtl">
        <div class="flex items-center gap-6">
                    <NavDrawer v-if="authStore.isAdmin" :links="adminLinks" title="مدیریت" />
          <NavDrawer v-if="authStore.isMaster" :links="masterLinks" title="پنل گرداننده" />
          <NavDrawer v-if="authStore.isOwner" :links="ownerLinks" title="پنل مدیر کافه" />
          <NavDrawer v-if="authStore.isSupervisor" :links="supervisorLinks" title="پنل سوپروایزر" />
          <NavDrawer v-if="authStore.isCashier" :links="cashierLinks" title="پنل صندوق‌دار" />
          <router-link to="/" :style="{ fontSize: '18px', fontWeight: 700, letterSpacing: '0.04em', color: 'var(--color-gold)', textDecoration: 'none' }">
            Mafia<span :style="{ color: 'var(--color-fg)' }">Tracker</span>
          </router-link>
          <button
            @click="mobileNavOpen = !mobileNavOpen"
            class="md:hidden p-1 hover:opacity-70 transition"
            :style="{ color: 'var(--color-muted)' }"
            :title="mobileNavOpen ? 'بستن منو' : 'باز کردن منو'"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="w-4 h-4 transition-transform"
              :style="{ transform: mobileNavOpen ? 'rotate(180deg)' : 'rotate(0deg)' }"
              viewBox="0 0 20 20"
              fill="currentColor"
            >
              <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
          <nav class="hidden md:flex gap-6 items-center" :style="{ fontSize: '13px', color: 'rgba(232,228,217,0.5)' }">
            <div class="relative" @mouseenter="rankingOpen = true" @mouseleave="rankingOpen = false">
              <span class="nav-link" :class="{ 'text-fg': isRankingActive }" style="cursor:pointer;">رتبه‌بندی</span>
              <div v-show="rankingOpen" class="dropdown-menu" :style="{ background: 'var(--color-surface)', border: '0.5px solid var(--color-border)', boxShadow: '0 4px 20px rgba(0,0,0,0.4)' }">
                <router-link to="/ranking/overall" class="dropdown-link" :class="{ active: route.path === '/ranking/overall' }">رنکینگ کل بازیکنان</router-link>
                <router-link to="/ranking/citizen" class="dropdown-link" :class="{ active: route.path === '/ranking/citizen' }">رنکینگ شهروندی بازیکنان</router-link>
                <router-link to="/ranking/mafia" class="dropdown-link" :class="{ active: route.path === '/ranking/mafia' }">رنکینگ مافیایی بازیکنان</router-link>
              </div>
            </div>
            <router-link to="/plays-public" class="nav-link">بازی‌ها</router-link>
            <router-link to="/statistics" class="nav-link">آمار</router-link>
            <router-link to="/head-to-head" class="nav-link" >Head To Head</router-link>

          </nav>

        </div>
        <div class="flex items-center gap-4">
          <div class="hidden md:block">
            <PlayerSearchAutocomplete />
          </div>
          <button @click="mobileSearchOpen = !mobileSearchOpen" class="md:hidden hover:opacity-70 transition p-1">
            <svg v-if="!mobileSearchOpen" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" style="color:var(--color-muted);" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd" />
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" style="color:var(--color-muted);" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
          <button
            @click="theme.toggleTheme()"
            class="p-2 rounded-lg text-muted hover:text-fg hover:bg-white/10 dark:hover:bg-white/10 transition-colors"
            :title="theme.isDark ? 'تم روشن' : 'تم تاریک'"
          >
            <svg v-if="theme.isDark" xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364-6.364l-.707.707M6.343 17.657l-.707.707M17.657 17.657l-.707-.707M6.343 6.343l-.707-.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"/>
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/>
            </svg>
          </button>
          <UserMenu v-if="authStore.isAuthenticated" @logout="handleLogout" />
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
      <div v-if="mobileSearchOpen" class="md:hidden px-4 py-3" :style="{ borderTop: '0.5px solid var(--color-border)' }">
        <PlayerSearchAutocomplete />
      </div>
      <div v-if="mobileNavOpen" class="md:hidden px-4 py-2 flex flex-col" :style="{ borderTop: '0.5px solid var(--color-border)' }">
        <router-link
          v-for="link in publicLinks"
          :key="link.to"
          :to="link.to"
          @click="mobileNavOpen = false"
          class="dropdown-link"
          :class="{ active: route.path === link.to }"
        >
          {{ link.label }}
        </router-link>
      </div>
    </header>

    <main class="flex-1 max-w-6xl mx-auto px-6 py-6" style="width:100%;">
      <router-view />
    </main>

    <footer :style="{ borderTop: '0.5px solid var(--color-border)', textAlign: 'center', padding: '1rem', fontSize: '12px', color: 'rgba(232,228,217,0.3)', background: 'var(--color-bg)' }">
      &copy; {{ currentYear }} MafiaPedia
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'
import NavDrawer from '@/components/shared/NavDrawer.vue'
import UserMenu from '@/components/shared/UserMenu.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const theme = useThemeStore()

const rankingOpen = ref(false)
const isRankingActive = computed(() => route.path.startsWith('/ranking/'))

const mobileNavOpen = ref(false)
const mobileSearchOpen = ref(false)
const currentYear = new Date().getFullYear()

const publicLinks = [
  { label: 'رنکینگ کل بازیکنان', to: '/ranking/overall' },
  { label: 'رنکینگ شهروندی بازیکنان', to: '/ranking/citizen' },
  { label: 'رنکینگ مافیایی بازیکنان', to: '/ranking/mafia' },
  { label: 'بازی‌ها', to: '/plays-public' },
  { label: 'آمار', to: '/statistics' },
  { label: 'Head To Head', to: '/head-to-head' },
]

const adminLinks = [
  { label: 'مدیریت کافه‌ها', to: '/admin/clubs' },
  { label: 'مدیریت کاربران', to: '/admin/users' },
  { label: 'مدیریت بازی‌ها', to: '/plays' },
  { label: 'مدیریت بازیکنان', to: '/players/list' },
  { label: 'ثبت بازیکن جدید', to: '/admin/players/create' },
  { label: 'ثبت بازی جدید', to: '/plays/create' },
]

const masterLinks = [
  { label: 'داشبورد', to: '/master' },
  { label: 'ثبت بازی جدید', to: '/master/plays/create' },
  { label: 'لیست بازی‌ها', to: '/master/plays' },
  { label: 'حالت تمرین', to: '/master/plays/practice' },
]

const ownerLinks = [
  { label: 'داشبورد', to: '/owner' },
  { label: 'ثبت بازی جدید', to: '/master/plays/create' },
  { label: 'اعضا و نقش‌ها', to: '/owner/members' },
  { label: 'نرخ‌ها', to: '/owner/nerkh' },
  { label: 'محصولات', to: '/owner/products' },
  { label: 'حساب‌های امروز', to: '/finance/today' },
  { label: 'ثبت سفارش', to: '/finance/order' },
  { label: 'تسویه حساب', to: '/finance/settlement' },
  { label: 'بدهکاران', to: '/finance/debtors' },
  { label: 'آرشیو فاکتورها', to: '/finance/invoices' },
  { label: 'گزارش حذف‌شدگان', to: '/owner/finance-audit' },
]

const supervisorLinks = [
  { label: 'داشبورد', to: '/supervisor' },
  { label: 'ثبت بازی جدید', to: '/master/plays/create' },
  { label: 'حساب‌های امروز', to: '/finance/today' },
  { label: 'ثبت سفارش', to: '/finance/order' },
]

const cashierLinks = [
  { label: 'داشبورد', to: '/cashier' },
  { label: 'ثبت بازی جدید', to: '/master/plays/create' },
  { label: 'حساب‌های امروز', to: '/finance/today' },
  { label: 'ثبت سفارش', to: '/finance/order' },
  { label: 'تسویه حساب', to: '/finance/settlement' },
  { label: 'بدهکاران', to: '/finance/debtors' },
  { label: 'گردش حساب', to: '/finance/ledger' },
  { label: 'آرشیو فاکتورها', to: '/finance/invoices' },
]

function handleLogout() {
  authStore.logout()
  router.push('/')
}
</script>

<style scoped>
.nav-link {
  color: var(--color-muted);
  text-decoration: none;
  transition: color 0.15s;
}
.nav-link:hover {
  color: var(--color-fg);
}
:root:not(.dark) .nav-link {
  color: var(--color-muted);
}
:root:not(.dark) .nav-link:hover {
  color: var(--color-fg);
}
.btn-ghost {
  font-size: 12px;
  padding: 6px 14px;
  border: 0.5px solid var(--color-border);
  border-radius: 6px;
  background: transparent;
  color: var(--color-muted);
  cursor: pointer;
  font-family: inherit;
  transition: color 0.15s;
}
.btn-ghost:hover {
  color: var(--color-fg);
}
:root:not(.dark) .btn-ghost {
  color: var(--color-muted);
}
:root:not(.dark) .btn-ghost:hover {
  color: var(--color-fg);
}
.btn-gold {
  font-size: 12px;
  padding: 6px 14px;
  border: none;
  border-radius: 6px;
  background: var(--color-gold);
  color: var(--color-gold-text);
  cursor: pointer;
  font-weight: 700;
  font-family: inherit;
  transition: opacity 0.15s;
}
.btn-gold:hover {
  opacity: 0.88;
}
.dropdown-menu {
  position: absolute;
  right: 0;
  top: 100%;
  min-width: 190px;
  border-radius: 8px;
  padding: 4px 0;
  z-index: 50;
}

.dropdown-link {
  display: block;
  padding: 7px 16px;
  font-size: 13px;
  color: var(--color-muted);
  text-decoration: none;
  transition: all 0.12s;
}
.dropdown-link:hover {
  color: var(--color-gold);
  background: var(--color-surface-hover);
}
.dropdown-link.active {
  color: var(--color-gold);
}
:root:not(.dark) .dropdown-link {
  color: var(--color-muted);
}
:root:not(.dark) .dropdown-link:hover {
  background: var(--color-surface-hover);
}
</style>
