<template>
  <div dir="rtl" class="w-full md:w-4/5 mx-auto">
    <h1 class="text-2xl md:text-3xl font-bold mb-6">بازی‌ها</h1>

    <div class="bg-gray-800 rounded-lg border border-gray-700 p-4 mb-6">
      <div class="flex flex-wrap items-end gap-3">
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">جستجو</label>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="جستجوی بازی..."
            class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-red-500 transition w-44"
          />
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">کلاب</label>
          <select
            v-model.number="filters.clubId"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-red-500 transition w-36"
            @change="onClubChange"
          >
            <option :value="0">همه کلاب‌ها</option>
            <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">رویداد</label>
          <select
            v-model.number="filters.eventId"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-red-500 transition w-36"
            :disabled="!filters.clubId"
          >
            <option :value="0" >{{ filters.clubId ? 'همه رویدادها' : 'ابتدا کلاب' }}</option>
            <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">سناریو</label>
          <select
            v-model.number="filters.senarioId"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-red-500 transition w-36"
          >
            <option :value="0">همه سناریوها</option>
            <option v-for="s in senarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">برنده</label>
          <select
            v-model.number="filters.winnersideId"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-red-500 transition w-28"
          >
            <option :value="0">همه</option>
            <option v-for="s in sides" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-gray-400">بازیکن</label>
          <PlayerSearchAutocomplete
            filter-mode
            @select="onPlayerSelect"
          />
        </div>
        <div v-if="filters.playerId" class="flex items-center gap-1 pb-1">
          <span class="text-xs text-blue-400">بازیکن: {{ selectedPlayerName }}</span>
          <button class="text-gray-500 hover:text-red-400 transition" @click="clearPlayerFilter" title="حذف فیلتر بازیکن">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
        <button
          class="px-3 py-1.5 rounded text-sm bg-gray-700 text-gray-300 hover:bg-gray-600 transition border border-gray-600"
          @click="clearFilters"
        >
          پاک کردن فیلترها
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-4 border-gray-600 border-t-red-500 rounded-full animate-spin" />
    </div>

    <div v-else-if="plays.length === 0" class="text-center py-20 text-gray-500 text-lg">
      هیچ بازی‌ای یافت نشد.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-gray-700 bg-gray-800 text-gray-300">
            <th class="px-4 py-3 text-right">ردیف</th>
            <th class="px-4 py-3 text-right">عنوان</th>
            <th class="px-4 py-3 text-right">تاریخ</th>
            <th class="px-4 py-3 text-right">سناریو</th>
            <th class="px-4 py-3 text-right">کلاب / رویداد</th>
            <th class="px-4 py-3 text-right">برنده</th>
            <th class="px-4 py-3 text-right">لینک</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(play, index) in plays"
            :key="play.id"
            class="border-b border-gray-700 hover:bg-gray-750 transition"
          >
            <td class="px-4 py-3 text-gray-500">{{ (page - 1) * pageSize + index + 1 }}</td>
            <td class="px-4 py-3">
              <router-link
                :to="`/plays/${play.id}`"
                class="text-blue-400 hover:text-blue-300 transition font-medium"
              >
                {{ play.title }}
              </router-link>
            </td>
            <td class="px-4 py-3 text-gray-500 whitespace-nowrap" >{{ formatDate(play.dateTime) }}</td>
            <td class="px-4 py-3 text-gray-500">{{ play.senarioName }}</td>
            <td class="px-4 py-3 text-gray-500">
              <span v-if="play.clubName" class="text-gray-600">{{ play.clubName }}</span>
              <span v-if="play.clubName && play.eventName" class="text-gray-600 mx-1">/</span>
              <span v-if="play.eventName" class="text-gray-600">{{ play.eventName }}</span>
            </td>
            <td class="px-4 py-3">
              <span
                class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                :class="play.winnersideName === 'شهروند' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
              >
                {{ play.winnersideName }}
              </span>
            </td>
            <td class="px-4 py-3">
              <a
                v-if="play.link"
                :href="play.link"
                target="_blank"
                rel="noopener noreferrer"
                class="inline-flex items-center justify-center text-red-500 hover:text-red-400 transition"
                title="مشاهده در یوتیوب"
              >
               
              </a>
              <span v-else class="text-gray-600">—</span>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="flex items-center justify-between gap-4 mt-6 text-sm">
        <span class="text-gray-400">
          صفحه {{ page }} از {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 rounded border border-gray-600 text-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-700 transition"
            @click="page = Math.max(1, page - 1)"
          >
            قبلی
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 rounded border border-gray-600 text-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-700 transition"
            @click="page = Math.min(totalPages, page + 1)"
          >
            بعدی
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, reactive } from 'vue'
import { PlaysApi, LookupApi } from '@/api'
import type { PlayDto, Club, Event as EventItem, Senario, DropdownSide, PlayerSearchResult } from '@/types'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'

const plays = ref<PlayDto[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = 20
const totalPages = ref(1)
const searchQuery = ref('')

const clubs = ref<Club[]>([])
const senarios = ref<Senario[]>([])
const allEvents = ref<EventItem[]>([])
const sides = ref<DropdownSide[]>([])
const selectedPlayerName = ref('')

const filters = reactive({
  clubId: 0,
  eventId: 0,
  senarioId: 0,
  winnersideId: 0,
  playerId: 0,
})

const filteredEvents = computed(() =>

  allEvents.value.filter(e => e.clubId === filters.clubId)
)

let debounceTimer: ReturnType<typeof setTimeout>

function buildFilterParams() {
  const params: Record<string, number | string | undefined> = {}
  if (filters.clubId) params.clubId = filters.clubId
  if (filters.eventId) params.eventId = filters.eventId
  if (filters.senarioId) params.senarioId = filters.senarioId
  if (filters.winnersideId) params.winnersideId = filters.winnersideId
  if (filters.playerId) params.playerId = filters.playerId
  if (searchQuery.value) params.search = searchQuery.value
  params.page = page.value
  params.pageSize = pageSize
  return params
}

async function fetchPlays() {
  loading.value = true
  try {
    const res = await PlaysApi.getPlaysPublic(buildFilterParams())
    plays.value = res.data.items
    totalPages.value = res.data.totalPages || Math.max(1, Math.ceil(res.data.totalItems / pageSize))
  } catch {
    plays.value = []
  } finally {
    loading.value = false
  }
}

function triggerFetch() {
  page.value = 1
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(fetchPlays, 400)
}

function onClubChange() {
  filters.eventId = 0
  triggerFetch()
}

function onPlayerSelect(player: PlayerSearchResult) {
  filters.playerId = player.id
  selectedPlayerName.value = player.name
  triggerFetch()
}

function clearPlayerFilter() {
  filters.playerId = 0
  selectedPlayerName.value = ''
  triggerFetch()
}

function clearFilters() {
  filters.clubId = 0
  filters.eventId = 0
  filters.senarioId = 0
  filters.winnersideId = 0
  filters.playerId = 0
  searchQuery.value = ''
  selectedPlayerName.value = ''
  triggerFetch()
}

function formatDate(dateStr: string): string {
  try {
    return new Intl.DateTimeFormat('fa-IR', { year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

watch(searchQuery, () => { triggerFetch() })

watch(page, () => { fetchPlays() })

watch([() => filters.senarioId, () => filters.winnersideId, () => filters.eventId], () => { triggerFetch() })

onMounted(async () => {
  try {
    const res = await LookupApi.getDropdown()
    const dd = res.data
    clubs.value = dd.clubs
    senarios.value = dd.senarios
    allEvents.value = dd.events
    sides.value = dd.sides
  } catch {
    //
  }
  fetchPlays()
})
</script>
