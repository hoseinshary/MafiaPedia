<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-[#c9b07a]">داشبورد مدیر کافه</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ authStore.activeClubContext?.clubName }}</p>
    </div>

    <ClubTodaysPlaysCard :club-id="authStore.activeClubId!" />
    <ClubStatsCard :club-id="authStore.activeClubId!" />
    <MasterPerformanceTable :club-id="authStore.activeClubId!" />

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <router-link to="/owner/members" class="block bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 hover:border-[rgba(201,176,122,0.3)] transition text-center">
        <div class="text-2xl mb-2">👥</div>
        <div class="text-sm font-bold text-[#c9b07a]">مدیریت اعضا</div>
        <div class="text-xs text-[rgba(232,228,217,0.3)] mt-1">مدیریت نقش‌های سوپروایزر و صندوق‌دار</div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore'
import ClubTodaysPlaysCard from '@/components/club/ClubTodaysPlaysCard.vue'
import ClubStatsCard from '@/components/club/ClubStatsCard.vue'
import MasterPerformanceTable from '@/components/club/MasterPerformanceTable.vue'

const authStore = useAuthStore()
</script>
