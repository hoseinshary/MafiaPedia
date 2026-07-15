<template>
  <div dir="rtl" class="min-h-screen bg-[#0d0d0f] flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <h1 class="text-2xl font-bold text-[#e8e4d9]">انتخاب باشگاه</h1>
        <p class="text-[rgba(232,228,217,0.4)] mt-2 text-sm">باشگاه مورد نظر خود را انتخاب کنید</p>
      </div>
      <div class="space-y-3">
        <div
          v-for="ctx in authStore.clubContexts"
          :key="ctx.clubId"
          @click="select(ctx)"
          class="bg-[#141416] border border-[rgba(255,255,255,0.07)] rounded-xl p-5 cursor-pointer hover:border-[rgba(201,176,122,0.3)] transition text-right"
        >
          <div class="font-bold text-[#e8e4d9]">{{ ctx.clubName }}</div>
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
    master: 'text-[#4ade80]',
    owner: 'text-[#c9b07a]',
    supervisor: 'text-[#60a5fa]',
    cashier: 'text-[#f472b6]',
  }
  return map[role] || 'text-[rgba(232,228,217,0.4)]'
}
</script>
