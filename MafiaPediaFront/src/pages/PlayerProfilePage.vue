<template>
  <div dir="rtl" class="w-full md:w-3/4 mx-auto">
    <div v-if="error" class="text-center py-20 text-red-500 text-lg">
      {{ error }}
    </div>

    <template v-else-if="player">
      <section class="mb-8">
        <div class="flex items-center gap-6 bg-white rounded-lg border border-gray-200 shadow-sm p-6">
          <img
            v-if="player.picture"
            :src="`https://localhost:7097/${player.picture}`"
            :alt="player.name"
            class="w-20 h-20 rounded-full object-cover shrink-0"
          />
          <div v-else class="w-20 h-20 rounded-full bg-gray-200 flex items-center justify-center text-3xl text-gray-500 shrink-0">
            {{ player.name.charAt(0) }}
          </div>
          <div>
            <h1 class="text-2xl font-bold">{{ player.name }}</h1>
            <p v-if="player.birthday" class="text-gray-500 text-sm mt-1">{{ player.birthday }}</p>
          </div>
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">آمار کلی</h2>
        <div class="grid grid-cols-2 gap-4">
          <PlayerStatisticsCard label="تعداد کل بازی‌ها" :value="String(player.statistics.totalGames)" />
          <PlayerStatisticsCard label="آمار برد کل" :value="formatPercent(player.statistics.overallWinRate)" />
          <PlayerStatisticsCard label="تعداد بازی شهروندی" :value="String(player.statistics.citizenGames)" />
          <PlayerStatisticsCard label="آمار برد شهروندی" :value="formatPercent(player.statistics.citizenWinRate)" />
          <PlayerStatisticsCard label="تعداد بازی مافیایی" :value="String(player.statistics.mafiaGames)" />
          <PlayerStatisticsCard label="آمار برد مافیایی" :value="formatPercent(player.statistics.mafiaWinRate)" />
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">پر تکرارترین نقش‌ها</h2>
        <RolesTable :roles="player.mostPlayedRoles.slice(0, 5)" />
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">بهترین نقش‌ها</h2>
        <RolesTable :roles="player.bestRoles.slice(0, 5)" :showWins="true" :showWinRate="true" />
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">بازی‌های اخیر</h2>
        <RecentGamesTable :games="player.recentGames" />
      </section>
    </template>

    <div v-else class="space-y-6 animate-pulse">
      <div class="flex items-center gap-6 bg-white rounded-lg border border-gray-200 shadow-sm p-6">
        <div class="w-20 h-20 rounded-full bg-gray-200" />
        <div class="space-y-3 flex-1">
          <div class="h-6 w-48 bg-gray-200 rounded" />
          <div class="h-4 w-32 bg-gray-200 rounded" />
        </div>
      </div>
      <div class="grid grid-cols-2 sm:grid-cols-3 gap-4">
        <div v-for="i in 6" :key="i" class="h-24 bg-gray-200 rounded-lg" />
      </div>
      <div class="h-48 bg-gray-200 rounded-lg" />
      <div class="h-48 bg-gray-200 rounded-lg" />
      <div class="h-48 bg-gray-200 rounded-lg" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { PlayerApi } from '@/api'
import type { PlayerProfile } from '@/types'
import PlayerStatisticsCard from '@/components/PlayerStatisticsCard.vue'
import RolesTable from '@/components/RolesTable.vue'
import RecentGamesTable from '@/components/RecentGamesTable.vue'

const route = useRoute()
const player = ref<PlayerProfile | null>(null)
const error = ref<string | null>(null)

function formatPercent(value: number): string {
  return `${value.toFixed(2)}%`
}

onMounted(async () => {
  const playerId = Number(route.params.id)
  if (!playerId) {
    error.value = 'Invalid player ID.'
    return
  }
  try {
    const res = await PlayerApi.getPlayerProfile(playerId)
    player.value = res.data
  } catch {
    error.value = 'Failed to load player profile. Please try again.'
  }
})
</script>
