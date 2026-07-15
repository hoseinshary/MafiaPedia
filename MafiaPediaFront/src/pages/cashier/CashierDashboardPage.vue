<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-[#f472b6]">داشبورد صندوق‌دار</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ authStore.activeClubContext?.clubName }}</p>
    </div>

    <ClubTodaysPlaysCard :club-id="authStore.activeClubId!" />
    <ClubStatsCard :club-id="authStore.activeClubId!" />
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore'
import ClubTodaysPlaysCard from '@/components/club/ClubTodaysPlaysCard.vue'
import ClubStatsCard from '@/components/club/ClubStatsCard.vue'

const authStore = useAuthStore()
</script>
