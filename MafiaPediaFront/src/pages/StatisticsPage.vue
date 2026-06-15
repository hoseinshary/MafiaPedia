<template>
  <div dir="rtl" class="w-full md:w-3/4 mx-auto">
    <h1 class="text-2xl md:text-3xl font-bold mb-6">آمار</h1>

    <div class="flex flex-wrap gap-4 mb-6 items-end">
      <div class="flex flex-col gap-1">
        <label class="text-sm text-gray-600">کلاب</label>
        <select
          v-model="filters.clubId"
          class="border border-gray-300 rounded px-3 py-2 text-sm min-w-[140px]"
        >
          <option :value="undefined">همه</option>
          <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm text-gray-600">ایونت</label>
        <select
          v-model="filters.eventId"
          class="border border-gray-300 rounded px-3 py-2 text-sm min-w-[140px]"
        >
          <option :value="undefined">همه</option>
          <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm text-gray-600">سناریو</label>
        <select
          v-model="filters.scenarioId"
          class="border border-gray-300 rounded px-3 py-2 text-sm min-w-[140px]"
        >
          <option :value="undefined">همه</option>
          <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>
      </div>

      <button
        class="px-4 py-2 text-sm rounded border border-gray-300 hover:bg-gray-100 transition"
        @click="clearFilters"
      >
        پاک کردن فیلترها
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-4 border-gray-300 border-t-blue-600 rounded-full animate-spin" />
    </div>

    <template v-else-if="data">
      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">آمار کلی</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="bg-white rounded-lg border border-gray-200 p-6 text-center shadow-sm">
            <p class="text-sm text-gray-500 mb-1">کل بازی‌ها</p>
            <p class="text-3xl font-bold text-gray-900">{{ data.totalGames }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-6 text-center shadow-sm">
            <p class="text-sm text-gray-500 mb-1">کل بازیکنان</p>
            <p class="text-3xl font-bold text-gray-900">{{ data.totalPlayers }}</p>
          </div>
        </div>
      </section>

    

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">نرخ برد بر اساس کلاب</h2>
        <WinRateTable :rows="data.winRateByClub" />
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">نرخ برد بر اساس رویداد</h2>
        <WinRateTable :rows="data.winRateByEvent" />
      </section>

      <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">نرخ برد بر اساس سناریو</h2>
        <WinRateTable :rows="data.winRateByScenario" />
      </section>

        <section class="mb-8">
        <h2 class="text-lg font-semibold mb-4">روند زمانی برد</h2>
        <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
          <template v-if="data.winRateTrend.length > 0">
            <div style="height: 280px">
              <Line :data="trendChartData" :options="trendChartOptions" />
            </div>
          </template>
          <p v-else class="text-center text-gray-400 py-10">داده‌ای برای نمایش وجود ندارد</p>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch, onMounted } from 'vue'
import { LookupApi } from '@/api'
import { getStatistics, type StatisticsDto, type StatisticsFilterDto } from '@/api/StatisticsApi'
import type { Club, Event, Senario } from '@/types'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip as ChartTooltip,
  Legend,
  Filler
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, ChartTooltip, Legend, Filler)

import WinRateTable from '@/components/WinRateTable.vue'

const persianMonths = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند']

const data = ref<StatisticsDto | null>(null)
const loading = ref(true)

const clubs = ref<Club[]>([])
const events = ref<Event[]>([])
const scenarios = ref<Senario[]>([])

const filters = reactive<StatisticsFilterDto>({
  clubId: undefined,
  eventId: undefined,
  scenarioId: undefined,
})

const filteredEvents = computed(() =>
  filters.clubId
    ? events.value.filter((e) => e.clubId === filters.clubId)
    : events.value
)

function clearFilters() {
  filters.clubId = undefined
  filters.eventId = undefined
  filters.scenarioId = undefined
}

async function fetchStatistics() {
  loading.value = true
  try {
    const res = await getStatistics({
      clubId: filters.clubId,
      eventId: filters.eventId,
      scenarioId: filters.scenarioId,
    })
    data.value = res.data
  } finally {
    loading.value = false
  }
}

watch(filters, fetchStatistics, { deep: true })

onMounted(async () => {
  const [clubRes, eventRes, scenarioRes] = await Promise.all([
    LookupApi.getClubs(),
    LookupApi.getEvents(),
    LookupApi.getScenarios(),
  ])
  clubs.value = clubRes.data
  events.value = eventRes.data
  scenarios.value = scenarioRes.data
  await fetchStatistics()
})

const trendChartData = computed(() => {
  const trend = data.value?.winRateTrend ?? []
  return {
    labels: trend.map((p) => `${persianMonths[p.month - 1]} ${p.year}`),
    datasets: [
      {
        label: 'مافیا',
        data: trend.map((p) => +(p.mafiaWinRate ).toFixed(1)),
        borderColor: '#e94560',
        backgroundColor: 'rgba(233, 69, 96, 0.1)',
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#e94560',
        pointRadius: 3,
        pointHoverRadius: 5,
      },
      {
        label: 'شهروند',
        data: trend.map((p) => +(p.citizenWinRate ).toFixed(1)),
        borderColor: '#3b82f6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#3b82f6',
        pointRadius: 3,
        pointHoverRadius: 5,
      },
    ],
  }
})

const trendChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top' as const,
      labels: {
        color: '#666',
      },
    },
    tooltip: {
      backgroundColor: '#1a1a2e',
      titleColor: '#e0e0e0',
      bodyColor: '#e0e0e0',
      borderColor: '#16213e',
      borderWidth: 1,
      padding: 10,
      callbacks: {
        label: (ctx: { dataset: { label?: string }; parsed: { y?: number | null } }) =>
          `${ctx.dataset.label ?? ''}: ${ctx.parsed.y ?? 0}%`,
      },
    },
  },
  scales: {
    x: {
      title: { display: true, text: 'ماه', color: '#666' },
      grid: { color: 'rgba(0,0,0,0.05)' },
      ticks: { color: '#666' },
    },
    y: {
      min: 0,
      max: 100,
      title: { display: true, text: 'درصد برد', color: '#666' },
      grid: { color: 'rgba(0,0,0,0.05)' },
      ticks: { color: '#666', callback: (v: string | number) => `${v}%` },
    },
  },
}
</script>
