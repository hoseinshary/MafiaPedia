<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-fg">رنکینگ کلی</h1>

    <div class="flex flex-wrap gap-4 mb-6 items-end">
      <div class="flex flex-col gap-1">
        <label class="text-sm text-muted">کلاب</label>
        <select
          v-model="filters.clubId"
          class="bg-surface border border-border rounded px-3 py-2 text-sm min-w-[140px] text-fg focus:outline-none focus:border-gold"
        >
          <option :value="undefined">همه</option>
          <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm text-muted">ایونت</label>
        <select
          v-model="filters.eventId"
          class="bg-surface border border-border rounded px-3 py-2 text-sm min-w-[140px] text-fg focus:outline-none focus:border-gold"
        >
          <option :value="undefined">همه</option>
          <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm text-muted">سناریو</label>
        <select
          v-model="filters.senarioId"
          class="bg-surface border border-border rounded px-3 py-2 text-sm min-w-[140px] text-fg focus:outline-none focus:border-gold"
        >
          <option :value="undefined">همه</option>
          <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm text-muted">مینیموم تعداد بازی</label>
        <input
          v-model.number="filters.minimumGames"
          type="number"
          min="0"
          class="bg-surface border border-border rounded px-3 py-2 text-sm w-[120px] text-fg focus:outline-none focus:border-gold"
        />
      </div>

      <button
        class="px-4 py-2 text-sm rounded border border-border text-muted hover:bg-surface-hover transition"
        @click="resetFilters"
      >
        پاک کردن فیلترها
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="data.length === 0" class="text-center py-20 text-muted text-lg">
      No players found.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-border bg-surface-hover text-muted">
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
            v-for="(row, index) in paginated"
            :key="row.playerId"
            class="border-b border-border hover:bg-surface-hover transition text-fg"
          >
            <td class="px-4 py-3 text-center">
              <span v-if="showTrophies && (page - 1) * perPage + index < 3" class="ml-1">
                {{ ['🥇', '🥈', '🥉'][(page - 1) * perPage + index] }}
              </span>
              {{ (page - 1) * perPage + index + 1 }}
            </td>
            <td class="px-4 py-3">
              <router-link
                :to="`/player/${row.playerId}`"
                class="text-gold-text hover:underline font-medium"
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
        <span class="text-sm text-muted">
          Page {{ page }} of {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 text-sm rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
            @click="page = Math.max(1, page - 1)"
          >
            Previous
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 text-sm rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
            @click="page = Math.min(totalPages, page + 1)"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { RankingApi, LookupApi } from '@/api'
import type { OverallRankingEntry, Club, Event, Senario } from '@/types'

type SortKey = keyof OverallRankingEntry
type SortDir = 'asc' | 'desc'

interface ColumnDef {
  key: SortKey | 'rowNumber'
  label: string
}

const columns: ColumnDef[] = [
  { key: 'rowNumber', label: 'ردیف' },
  { key: 'playerName', label: 'نام بازیکن' },
  { key: 'totalGames', label: 'تعداد بازی' },
  { key: 'overallWinRate', label: 'آمار برد کل' },
  { key: 'citizenWinRate', label: 'آمار برد شهروندی' },
  { key: 'mafiaWinRate', label: 'آمار برد مافیایی' },
]

const data = ref<OverallRankingEntry[]>([])
const loading = ref(true)
const sortKey = ref<SortKey>('overallWinRate')
const sortDir = ref<SortDir>('desc')
const page = ref(1)
const perPage = 50
const showTrophies = ref(true)

watch([sortKey, sortDir], () => { showTrophies.value = false })

const clubs = ref<Club[]>([])
const events = ref<Event[]>([])
const scenarios = ref<Senario[]>([])

const route = useRoute()

const filters = reactive({
  clubId: (route.query.clubId ? Number(route.query.clubId) : undefined) as number | undefined,
  eventId: undefined as number | undefined,
  senarioId: undefined as number | undefined,
  minimumGames: undefined as number | undefined,
})

watch(() => filters.clubId, () => { showTrophies.value = false })
watch(() => filters.eventId, () => { showTrophies.value = false })
watch(() => filters.senarioId, () => { showTrophies.value = false })
watch(() => filters.minimumGames, () => { showTrophies.value = false })

const filteredEvents = computed(() =>
  filters.clubId
    ? events.value.filter((e) => e.clubId === filters.clubId)
    : events.value
)

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

function toggleSort(key: SortKey | 'rowNumber') {
  if (key === 'rowNumber') return
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

function resetFilters() {
  filters.clubId = undefined
  filters.eventId = undefined
  filters.senarioId = undefined
  filters.minimumGames = undefined
  page.value = 1
}

async function fetchRanking() {
  loading.value = true
  page.value = 1
  try {
    const res = await RankingApi.getOverallRanking({
      clubId: filters.clubId,
      eventId: filters.eventId,
      senarioId: filters.senarioId,
      minimumGames: filters.minimumGames,
    })
    data.value = res.data
  } finally {
    loading.value = false
  }
}

watch(filters, fetchRanking, { deep: true })

onMounted(async () => {
  const [clubRes, eventRes, scenarioRes] = await Promise.all([
    LookupApi.getClubs(),
    LookupApi.getEvents(),
    LookupApi.getScenarios(),
  ])
  clubs.value = clubRes.data
  events.value = eventRes.data
  scenarios.value = scenarioRes.data
  await fetchRanking()
})
</script>
