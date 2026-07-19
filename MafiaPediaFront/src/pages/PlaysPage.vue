<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-fg">بازی‌ها</h1>

    <div class="bg-surface rounded-[10px] border border-border p-4 mb-6">
      <div class="flex flex-wrap items-end gap-3">
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">جستجو</label>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="جستجوی بازی..."
            class="bg-input border border-border rounded px-3 py-1.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition w-44"
          />
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">کلاب</label>
          <select
            v-model.number="filters.clubId"
            class="bg-input border border-border rounded px-3 py-1.5 text-sm text-fg focus:outline-none focus:border-gold transition w-36"
            @change="onClubChange"
          >
            <option :value="0">همه کلاب‌ها</option>
            <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">رویداد</label>
          <select
            v-model.number="filters.eventId"
            class="bg-input border border-border rounded px-3 py-1.5 text-sm text-fg focus:outline-none focus:border-gold transition w-36"
            
          >
          <!-- :disabled="!filters.clubId" -->
            <option :value="0" >{{  'همه رویدادها' }}</option>
            <option v-for="e in allEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">سناریو</label>
          <select
            v-model.number="filters.senarioId"
            class="bg-input border border-border rounded px-3 py-1.5 text-sm text-fg focus:outline-none focus:border-gold transition w-36"
          >
            <option :value="0">همه سناریوها</option>
            <option v-for="s in senarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">برنده</label>
          <select
            v-model.number="filters.winnersideId"
            class="bg-input border border-border rounded px-3 py-1.5 text-sm text-fg focus:outline-none focus:border-gold transition w-28"
          >
            <option :value="0">همه</option>
            <option v-for="s in sides" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1 min-w-0">
          <label class="text-xs text-muted">بازیکن</label>
          <PlayerSearchAutocomplete
            filter-mode
            @select="onPlayerSelect"
          />
        </div>
        <div v-if="filters.playerId" class="flex items-center gap-1 pb-1">
          <span class="text-xs text-gold-text">بازیکن: {{ selectedPlayerName }}</span>
          <button class="text-muted hover:text-danger transition" @click="clearPlayerFilter" title="حذف فیلتر بازیکن">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
        <button
          class="px-3 py-1.5 rounded text-sm bg-surface text-muted hover:bg-surface-hover transition border border-border"
          @click="clearFilters"
        >
          پاک کردن فیلترها
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="plays.length === 0" class="text-center py-20 text-muted text-lg">
      هیچ بازی‌ای یافت نشد.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-border bg-surface-hover text-muted">
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
            class="border-b border-border hover:bg-surface-hover transition"
          >
            <td class="px-4 py-3 text-muted">{{ (page - 1) * pageSize + index + 1 }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-3">
                <PlayThumbnail :picture="play.picture" size="sm" />
                <router-link
                  :to="`/plays/${play.id}`"
                  class="text-gold-text hover:underline transition font-medium"
                >
                  {{ play.title }}
                </router-link>
              </div>
            </td>
            <td class="px-4 py-3 text-muted whitespace-nowrap" >{{ formatDate(play.dateTime) }}</td>
            <td class="px-4 py-3 text-muted">{{ play.senarioName }}</td>
            <td class="px-4 py-3 text-muted">
              <span v-if="play.clubName" class="text-muted">{{ play.clubName }}</span>
              <span v-if="play.clubName && play.eventName" class="text-muted mx-1">/</span>
              <span v-if="play.eventName" class="text-muted">{{ play.eventName }}</span>
            </td>
            <td class="px-4 py-3">
              <span
                class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                :class="play.winnersideName === 'شهروند' ? 'bg-success/15 text-success' : 'bg-danger/20 text-danger'"
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
                class="inline-flex items-center justify-center text-[rgba(201,176,122,0.5)] hover:text-gold-text transition"
                title="مشاهده در یوتیوب"
              >
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
                </svg>
              </a>
              <span v-else class="text-muted">—</span>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="flex items-center justify-between gap-4 mt-6 text-sm">
        <span class="text-muted">
          صفحه {{ page }} از {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
            @click="page = Math.max(1, page - 1)"
          >
            قبلی
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
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
import { ref, watch, onMounted, reactive } from 'vue'
import { PlaysApi, LookupApi } from '@/api'
import type { PlayDto, Club, Event as EventItem, Senario, DropdownSide, PlayerSearchResult } from '@/types'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'
import PlayThumbnail from '@/components/shared/PlayThumbnail.vue'

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

// const filteredEvents = computed(() =>

  
//   allEvents.value.filter(e => Number(e.clubId) === Number(filters.clubId))
  
// )

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
