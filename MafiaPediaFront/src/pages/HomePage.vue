<template>
  <div class="justify-center" dir="rtl">
    <div class="relative w-full overflow-hidden md:w-4/5 mx-auto" dir="ltr">
      <div
        class="flex transition-transform duration-500"
        :style="{ transform: `translateX(-${currentSlide * 100}%)` }"
      >
        <div class="min-w-full h-96 flex items-center justify-center bg-gradient-to-l from-gray-900 via-gray-800 to-gray-900 text-white" >
          <div class="text-center" >
            <h1 class="text-5xl md:text-6xl font-bold">مافیا <span class="text-red-500">پدیا</span></h1>
            <h2 class="text-xl md:text-2xl mt-4 text-gray-300">رنکینگ رسمی مافیای حرفه ای</h2>
            <p class="text-base md:text-lg mt-2 text-gray-400">آمار جمع آوری شده از بازی های پخش شده در یوتوب</p>
          </div>
        </div>
        <div class="min-w-full h-96 bg-cover bg-center flex items-center justify-center" :style="{ backgroundImage: `url(${donImage})` }">
          <span class="text-white text-2xl font-bold bg-black/40 px-6 py-3 rounded">مشاهده آمار دن کلاب</span>
                      

        </div>
        <div class="min-w-full h-96 bg-cover bg-center flex items-center justify-center" :style="{ backgroundImage: `url(${legendaryImage})` }">
          <span class="text-white text-2xl font-bold bg-black/40 px-6 py-3 rounded">مشاهده آمارمافیا لجندری</span>


        </div>
      </div>
      <button
        class="absolute left-2 top-1/2 -translate-y-1/2 bg-transparent text-white hover:text-gray-300 transition p-2"
        @click="currentSlide = currentSlide > 0 ? currentSlide - 1 : 2"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <button
        class="absolute right-2 top-1/2 -translate-y-1/2 bg-transparent text-white hover:text-gray-300 transition p-2"
        @click="currentSlide = (currentSlide + 1) % 3"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
        </svg>
      </button>
      <div class="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
        <button
          v-for="(_, i) in 3"
          :key="i"
          @click="currentSlide = i"
          class="w-3 h-3 rounded-full transition"
          :class="currentSlide === i ? 'bg-white' : 'bg-white/50'"
        />
      </div>
    </div>

    <div class="w-full md:w-3/4 mx-auto mt-8">
      <h1 class="text-2xl md:text-3xl font-bold mb-6">Overall Ranking</h1>

      <div v-if="loading" class="flex justify-center py-20">
        <div class="w-10 h-10 border-4 border-gray-300 border-t-blue-600 rounded-full animate-spin" />
      </div>

      <div v-else-if="data.length === 0" class="text-center py-20 text-gray-500 text-lg">
        No players found.
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm border-collapse">
          <thead>
            <tr class="border-b border-gray-300 bg-gray-100">
              <th
                v-for="col in columns"
                :key="col.key"
                class="px-4 py-3 cursor-pointer select-none whitespace-nowrap"
                @click="toggleSort(col.key)"
              >
                <span class="inline-flex items-center gap-1">
                  {{ col.label }}
                  <span v-if="sortKey === col.key" class="text-xs">
                    {{ sortDir === 'asc' ? '\u25B2' : '\u25BC' }}
                  </span>
                </span>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in paginated"
              :key="row.playerId"
              class="border-b border-gray-200 hover:bg-gray-50 transition"
            >
              <td class="px-4 py-3">
                <router-link
                  :to="`/player/${row.playerId}`"
                  class="text-blue-600 hover:underline font-medium"
                >
                  {{ row.playerName }}
                </router-link>
              </td>
              <td class="px-4 py-3">{{ row.totalGames }}</td>
              <td class="px-4 py-3">{{ formatPercent(row.overallWinRate) }}</td>
              <td class="px-4 py-3">{{ formatPercent(row.citizenWinRate) }}</td>
              <td class="px-4 py-3">{{ formatPercent(row.mafiaWinRate) }}</td>
            </tr>
          </tbody>
        </table>

        <div class="flex flex-col sm:flex-row items-center justify-between gap-4 mt-6">
          <span class="text-sm text-gray-500">
            Page {{ page }} of {{ totalPages }}
          </span>
          <div class="flex gap-2">
            <button
              :disabled="page <= 1"
              class="px-4 py-2 text-sm rounded border border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition"
              @click="page = Math.max(1, page - 1)"
            >
              Previous
            </button>
            <button
              :disabled="page >= totalPages"
              class="px-4 py-2 text-sm rounded border border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition"
              @click="page = Math.min(totalPages, page + 1)"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RankingApi } from '@/api'
import type { OverallRankingEntry } from '@/types'
import donImage from '/src/assets/images/slider/don.jpeg'
import legendaryImage from '/src/assets/images/slider/legendary.jpeg'

type SortKey = keyof OverallRankingEntry
type SortDir = 'asc' | 'desc'

interface ColumnDef {
  key: SortKey
  label: string
}

const columns: ColumnDef[] = [
  { key: 'playerName', label: 'نام بازیکن' },
  { key: 'totalGames', label: 'تعداد بازی' },
  { key: 'overallWinRate', label: 'آمار برد کل' },
  { key: 'citizenWinRate', label: 'آمار برد شهروندی' },
  { key: 'mafiaWinRate', label: 'آمار برد مافیایی' },
]

const currentSlide = ref(0)
let slideTimer: ReturnType<typeof setInterval>

const data = ref<OverallRankingEntry[]>([])
const loading = ref(true)
const sortKey = ref<SortKey>('overallWinRate')
const sortDir = ref<SortDir>('desc')
const page = ref(1)
const perPage = 50

const sorted = computed(() =>
  [...data.value].sort((a, b) => {
    const aVal = a[sortKey.value]
    const bVal = b[sortKey.value]
    if (aVal == null) return 1
    if (bVal == null) return -1
    if (aVal < bVal) return sortDir.value === 'asc' ? -1 : 1
    if (aVal > bVal) return sortDir.value === 'asc' ? 1 : -1
    return 0
  })
)

const totalPages = computed(() => Math.max(1, Math.ceil(sorted.value.length / perPage)))

const paginated = computed(() => {
  const start = (page.value - 1) * perPage
  return sorted.value.slice(start, start + perPage)
})

function toggleSort(key: SortKey) {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortDir.value = 'desc'
  }
  page.value = 1
}

function formatPercent(value: number): string {
  return `${value.toFixed(2)}%`
}

onMounted(async () => {
  slideTimer = setInterval(() => {
    currentSlide.value = (currentSlide.value + 1) % 3
  }, 4000)
  try {
    const res = await RankingApi.getOverallRanking()
    data.value = res.data
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  clearInterval(slideTimer)
})


</script>
