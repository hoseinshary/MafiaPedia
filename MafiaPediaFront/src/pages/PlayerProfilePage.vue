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
        <h2 class="text-lg font-semibold mb-4">رکوردها</h2>
        <div class="grid grid-cols-2 gap-4">
          <PlayerStatisticsCard label="🔥 Win Streak فعلی" :value="`${player.winStreak} بازی`" />
          <PlayerStatisticsCard label="🏆 بهترین رکورد برد پیاپی" :value="`${player.bestRun} بازی`" />
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">بهترین هم‌تیمی‌ها</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="bg-white rounded-lg border border-gray-200 p-4 text-center shadow-sm">
            <p class="text-sm text-gray-500 mb-1">بهترین هم‌تیمی مافیایی</p>
            <template v-if="player.bestMafiaPartner">
              <router-link
                :to="`/player/${player.bestMafiaPartner.playerId}`"
                class="text-lg font-bold text-blue-600 hover:underline"
              >
                {{ player.bestMafiaPartner.playerName }}
              </router-link>
              <p class="text-sm text-gray-500 mt-1">{{ player.bestMafiaPartner.sharedGames }} بازی مشترک</p>
              <p class="text-sm text-gray-500">{{ (player.bestMafiaPartner.winRate ).toFixed(2) }}% برد</p>
            </template>
            <p v-else class="text-gray-400 mt-2">اطلاعات کافی نیست</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4 text-center shadow-sm">
            <p class="text-sm text-gray-500 mb-1">بهترین هم‌تیمی شهروندی</p>
            <template v-if="player.bestCitizenPartner">
              <router-link
                :to="`/player/${player.bestCitizenPartner.playerId}`"
                class="text-lg font-bold text-blue-600 hover:underline"
              >
                {{ player.bestCitizenPartner.playerName }}
              </router-link>
              <p class="text-sm text-gray-500 mt-1">{{ player.bestCitizenPartner.sharedGames }} بازی مشترک</p>
              <p class="text-sm text-gray-500">{{ (player.bestCitizenPartner.winRate ).toFixed(2) }}% برد</p>
            </template>
            <p v-else class="text-gray-400 mt-2">اطلاعات کافی نیست</p>
          </div>
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">روند برد</h2>
        <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
          <template v-if="player.winRateTrend.length >= 5">
            <div style="height: 250px">
              <Line :data="chartData" :options="chartOptions" />
            </div>
          </template>
          <p v-else class="text-center text-gray-400 py-10">برای نمایش نمودار حداقل 10 بازی لازم است</p>
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

      <section class="mb-8">
        <PlayerComments entityType="player" :entityId="player.id" />
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
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { PlayerApi } from '@/api'
import { watch } from 'vue'
import type { PlayerProfile } from '@/types'
import PlayerStatisticsCard from '@/components/PlayerStatisticsCard.vue'
import RolesTable from '@/components/RolesTable.vue'
import RecentGamesTable from '@/components/RecentGamesTable.vue'
import PlayerComments from '@/components/PlayerComments.vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip as ChartTooltip,
  Filler
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, ChartTooltip, Filler)

const route = useRoute()
const player = ref<PlayerProfile | null>(null)
const error = ref<string | null>(null)

function formatPercent(value: number): string {
  return `${value.toFixed(2)}%`
}

// این تابع رو اضافه کن
async function fetchPlayer(id: number) {
  player.value = null  // reset برای نمایش skeleton
  error.value = null
  try {
    const res = await PlayerApi.getPlayerProfile(id)
    player.value = res.data
  } catch {
    error.value = 'Failed to load player profile. Please try again.'
  }
}

const chartData = computed(() => ({
  labels: player.value?.winRateTrend.map((_, i) => i + 1) ?? [],
  datasets: [{
    label: 'درصد برد',
    data: player.value?.winRateTrend.map(p => +(p.winRate ).toFixed(1)) ?? [],
    borderColor: '#e94560',
    backgroundColor: 'rgba(233, 69, 96, 0.1)',
    fill: true,
    tension: 0.4,
    pointBackgroundColor: '#e94560',
    pointRadius: 3,
    pointHoverRadius: 5,
  }]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    tooltip: {
      backgroundColor: '#1a1a2e',
      titleColor: '#e0e0e0',
      bodyColor: '#e0e0e0',
      borderColor: '#16213e',
      borderWidth: 1,
      padding: 10,
      callbacks: {
        label: (ctx: { parsed: { y?: number | null } }) => `${ctx.parsed.y ?? 0}%`,
      }
    }
  },
  scales: {
    x: {
      title: { display: true, text: 'بازی', color: '#666' },
      grid: { color: 'rgba(0,0,0,0.05)' },
      ticks: { color: '#666' }
    },
    y: {
      min: 0,
      max: 100,
      title: { display: true, text: 'درصد برد', color: '#666' },
      grid: { color: 'rgba(0,0,0,0.05)' },
      ticks: { color: '#666', callback: (v: string | number) => `${v}%` }
    }
  }
}

watch(
  () => Number(route.params.id),
  (newId) => {
    if (!newId) {
      error.value = 'Invalid player ID.'
      return
    }
    fetchPlayer(newId)
  },
  { immediate: true }  // اجرا میشه از اول هم مثل onMounted
)
</script>
