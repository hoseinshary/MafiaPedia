<template>
  <div dir="rtl" class="min-h-screen bg-bg flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <h1 class="text-2xl font-bold text-fg">انتخاب باشگاه</h1>
        <p class="text-muted mt-2 text-sm">باشگاه مورد نظر خود را انتخاب کنید</p>
      </div>
      <div class="space-y-3">
        <div
          v-for="ctx in authStore.clubContexts"
          :key="ctx.clubId"
          @click="select(ctx)"
          class="bg-surface border border-border rounded-xl p-5 cursor-pointer hover:border-gold/30 transition text-right"
        >
          <div class="font-bold text-fg">{{ ctx.clubName }}</div>
          <div class="text-xs mt-1" :class="roleColor(ctx.clubuserRole)">{{ roleLabel(ctx.clubuserRole) }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

const roleRoutes: Record<string, string> = {
  master: '/master',
  owner: '/owner',
  supervisor: '/supervisor',
  cashier: '/cashier',
}

function select(ctx: { clubId: number; clubuserRole: string }) {
  authStore.setActiveClub(ctx.clubId)
  const route = roleRoutes[ctx.clubuserRole]
  if (route) {
    router.push(route)
  } else {
    router.push('/')
  }
}

function roleLabel(role: string): string {
  const map: Record<string, string> = {
    master: 'گرداننده',
    owner: 'مدیر کافه',
    supervisor: 'سوپروایزر',
    cashier: 'صندوق‌دار',
  }
  return map[role] || role
}

function roleColor(role: string): string {
  const map: Record<string, string> = {
    master: 'text-success',
    owner: 'text-gold-text',
    supervisor: 'text-blue-400',
    cashier: 'text-pink-400',
  }
  return map[role] || 'text-muted'
}
</script>
