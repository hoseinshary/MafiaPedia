<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <div v-if="error" class="text-center py-20 text-red-500 text-lg">
      {{ error }}
    </div>

    <template v-else-if="player">
      <section class="mb-8">
        <div class="flex items-center gap-6 bg-[#141416] rounded-[12px] border border-[rgba(255,255,255,0.07)] p-6">
          <img
            v-if="player.picture"
            :src="`http://localhost:5272/${player.picture}`"
            :alt="player.name"
            class="w-20 h-20 rounded-full object-cover shrink-0 border-2 border-[rgba(201,176,122,0.3)]"
          />
          <div v-else class="w-20 h-20 rounded-full bg-[#2a2820] flex items-center justify-center text-3xl text-[#c9b07a] shrink-0 border-[1.5px] border-[rgba(201,176,122,0.3)]">
            {{ player.name.charAt(0) }}
          </div>
          <div>
            <h1 class="text-xl font-bold text-[#e8e4d9]">{{ player.name }}</h1>
            <p v-if="player.birthday" class="text-sm mt-1 text-[rgba(232,228,217,0.35)]">{{ player.birthday }}</p>
          </div>
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">آمار کلی</h2>
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
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">رکوردها</h2>
        <div class="grid grid-cols-2 gap-4">
          <PlayerStatisticsCard label="🔥 Win Streak فعلی" :value="`${player.winStreak} بازی`" />
          <PlayerStatisticsCard label="🏆 بهترین رکورد برد پیاپی" :value="`${player.bestRun} بازی`" />
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">بهترین هم‌تیمی‌ها</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-4 text-center">
            <p class="text-sm text-[rgba(232,228,217,0.4)] mb-1">بهترین هم‌تیمی مافیایی</p>
            <template v-if="player.bestMafiaPartner">
              <router-link
                :to="`/player/${player.bestMafiaPartner.playerId}`"
                class="text-lg font-bold text-[#c9b07a] hover:underline"
              >
                {{ player.bestMafiaPartner.playerName }}
              </router-link>
              <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ player.bestMafiaPartner.sharedGames }} بازی مشترک</p>
              <p class="text-sm text-[rgba(232,228,217,0.4)]">{{ (player.bestMafiaPartner.winRate ).toFixed(2) }}% برد</p>
            </template>
            <p v-else class="text-[rgba(232,228,217,0.4)] mt-2">اطلاعات کافی نیست</p>
          </div>
          <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-4 text-center">
            <p class="text-sm text-[rgba(232,228,217,0.4)] mb-1">بهترین هم‌تیمی شهروندی</p>
            <template v-if="player.bestCitizenPartner">
              <router-link
                :to="`/player/${player.bestCitizenPartner.playerId}`"
                class="text-lg font-bold text-[#c9b07a] hover:underline"
              >
                {{ player.bestCitizenPartner.playerName }}
              </router-link>
              <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ player.bestCitizenPartner.sharedGames }} بازی مشترک</p>
              <p class="text-sm text-[rgba(232,228,217,0.4)]">{{ (player.bestCitizenPartner.winRate ).toFixed(2) }}% برد</p>
            </template>
            <p v-else class="text-[rgba(232,228,217,0.4)] mt-2">اطلاعات کافی نیست</p>
          </div>
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">روند برد</h2>
        <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-4">
          <template v-if="player.winRateTrend.length >= 5">
            <div style="height: 250px">
              <Line :data="chartData" :options="chartOptions" />
            </div>
          </template>
          <p v-else class="text-center text-[rgba(232,228,217,0.4)] py-10">برای نمایش نمودار حداقل 10 بازی لازم است</p>
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">پر تکرارترین نقش‌ها</h2>
        <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] overflow-hidden">
          <RolesTable :roles="player.mostPlayedRoles.slice(0, 5)" />
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">بهترین نقش‌ها</h2>
        <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] overflow-hidden">
          <RolesTable :roles="player.bestRoles.slice(0, 5)" :showWins="true" :showWinRate="true" />
        </div>
      </section>

      <section class="mb-8">
        <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">بازی‌های اخیر</h2>
        <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] overflow-hidden">
          <RecentGamesTable :games="player.recentGames" />
        </div>
      </section>

      <section class="mb-8">
        <PlayerComments entityType="player" :entityId="player.id" />
      </section>
    </template>

    <div v-else class="space-y-6 animate-pulse">
      <div class="flex items-center gap-6 bg-[#141416] rounded-[12px] border border-[rgba(255,255,255,0.07)] p-6">
        <div class="w-20 h-20 rounded-full bg-[#1e1e22]" />
        <div class="space-y-3 flex-1">
          <div class="h-6 w-48 bg-[#1e1e22] rounded" />
          <div class="h-4 w-32 bg-[#1e1e22] rounded" />
        </div>
      </div>
      <div class="grid grid-cols-2 sm:grid-cols-3 gap-4">
        <div v-for="i in 6" :key="i" class="h-24 bg-[#1e1e22] rounded-lg" />
      </div>
      <div class="h-48 bg-[#1e1e22] rounded-lg" />
      <div class="h-48 bg-[#1e1e22] rounded-lg" />
      <div class="h-48 bg-[#1e1e22] rounded-lg" />
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
      backgroundColor: '#141416',
      titleColor: '#e8e4d9',
      bodyColor: '#e8e4d9',
      borderColor: 'rgba(255,255,255,0.1)',
      borderWidth: 1,
      padding: 10,
      callbacks: {
        label: (ctx: { parsed: { y?: number | null } }) => `${ctx.parsed.y ?? 0}%`,
      }
    }
  },
  scales: {
    x: {
      title: { display: true, text: 'بازی', color: 'rgba(232,228,217,0.3)' },
      grid: { color: 'rgba(255,255,255,0.05)' },
      ticks: { color: 'rgba(232,228,217,0.35)' }
    },
    y: {
      min: 0,
      max: 100,
      title: { display: true, text: 'درصد برد', color: 'rgba(232,228,217,0.3)' },
      grid: { color: 'rgba(255,255,255,0.05)' },
      ticks: { color: 'rgba(232,228,217,0.35)', callback: (v: string | number) => `${v}%` }
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
