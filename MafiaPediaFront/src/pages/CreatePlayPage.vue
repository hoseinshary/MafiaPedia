<template>
  <div dir="rtl" class="w-full md:w-3/4 mx-auto">
    <h1 class="text-2xl md:text-3xl font-bold mb-6">ساخت بازی جدید</h1>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm" :class="notification.type === 'success' ? 'bg-green-100 text-green-800 border border-green-300' : 'bg-red-100 text-red-800 border border-red-300'">
      {{ notification.message }}
    </div>

    <section class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
      <h2 class="text-lg font-semibold mb-4">اطلاعات بازی</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">عنوان</label>
          <input v-model="form.title" type="text" class="border border-gray-300 rounded px-3 py-2 text-sm" />
        </div>
        <div class="flex flex-col gap-1 relative" ref="datePickerRef">
          <label class="text-sm text-gray-600">تاریخ</label>
          <input
            type="text"
            :value="persianDateDisplay"
            @focus="calendarOpen = true"
            placeholder="مثال: ۱۴۰۳/۰۱/۱۵"
            class="border border-gray-300 rounded px-3 py-2 text-sm cursor-pointer"
            readonly
          />
          <div
            v-if="calendarOpen"
            class="absolute left-0 top-full mt-1 bg-white border border-gray-300 rounded shadow-lg z-50 p-3 w-72"
          >
            <div class="flex items-center justify-between mb-3">
              <button @click="prevMonth" class="text-sm px-2 py-1 hover:bg-gray-100 rounded">&lt;</button>
              <span class="text-sm font-semibold">{{ persianMonthName(calJy, calJm) }} {{ calJy }}</span>
              <button @click="nextMonth" class="text-sm px-2 py-1 hover:bg-gray-100 rounded">&gt;</button>
            </div>
            <div class="grid grid-cols-7 gap-1 text-center mb-1">
              <div v-for="d in ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج']" :key="d" class="text-xs text-gray-500 py-1">{{ d }}</div>
            </div>
            <div class="grid grid-cols-7 gap-1 text-center">
              <template v-for="(day, idx) in calendarDays" :key="idx">
                <div
                  v-if="day"
                  @click="selectDate(day)"
                  class="text-sm py-1 rounded cursor-pointer transition"
                  :class="dayClass(day)"
                >{{ day }}</div>
                <div v-else class="text-sm py-1"></div>
              </template>
            </div>
          </div>
          <div v-if="!form.dateTime && touched" class="text-xs text-red-500 mt-1">
            الزامی
          </div>
        </div>
        <div class="flex flex-col gap-1 md:col-span-2">
          <label class="text-sm text-gray-600">توضیحات</label>
          <textarea v-model="form.desc" rows="3" class="border border-gray-300 rounded px-3 py-2 text-sm resize-none"></textarea>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">سناریو</label>
          <select v-model.number="form.senarioId" class="border border-gray-300 rounded px-3 py-2 text-sm" @change="onScenarioChange">
            <option :value="0" disabled>انتخاب سناریو</option>
            <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">کلاب</label>
          <select v-model.number="form.clubId" class="border border-gray-300 rounded px-3 py-2 text-sm" @change="onClubChange">
            <option :value="0" disabled>انتخاب کلاب</option>
            <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">ایونت</label>
          <select v-model.number="form.eventId" class="border border-gray-300 rounded px-3 py-2 text-sm" :disabled="!form.clubId">
            <option :value="0" disabled>{{ form.clubId ? 'انتخاب ایونت' : 'ابتدا کلاب را انتخاب کنید' }}</option>
            <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">برنده</label>
          <select v-model.number="form.winnersideId" class="border border-gray-300 rounded px-3 py-2 text-sm">
            <option :value="0" disabled>انتخاب برنده</option>
            <option :value="1">شهروند</option>
            <option :value="2">مافیا</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">لینک</label>
          <input v-model="form.link" type="text" class="border border-gray-300 rounded px-3 py-2 text-sm" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-600">تعداد بازیکن</label>
          <input v-model.number="form.playersCount" type="number" min="1" max="50" class="border border-gray-300 rounded px-3 py-2 text-sm" @input="syncPlayerRows" />
        </div>
      </div>
    </section>

    <section class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
      <h2 class="text-lg font-semibold mb-4">بازیکنان</h2>
      <div v-if="form.players.length === 0" class="text-center py-8 text-gray-400">
        تعداد بازیکن را مشخص کنید
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm border-collapse">
          <thead>
            <tr class="border-b border-gray-300 bg-gray-100">
              <th class="px-4 py-3 text-right">#</th>
              <th class="px-4 py-3 text-right">بازیکن</th>
              <th class="px-4 py-3 text-right">نقش</th>
              <th class="px-4 py-3 text-right">اکشن</th>
              <th class="px-4 py-3 text-right">رنک</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(player, index) in form.players" :key="index" class="border-b border-gray-200">
              <td class="px-4 py-2 text-gray-500">{{ index + 1 }}</td>
              <td class="px-4 py-2">
                <div class="relative">
                  <input
                    type="text"
                    :value="playerSuggestions[index]?.isTyping ? playerSuggestions[index]?.query : getPlayerName(player.playerId)"
                    @input="onPlayerSearchInput($event, index)"
                    @focus="openPlayerSuggestions(index)"
                    @blur="onPlayerSearchBlur(index)"
                    @keydown.escape="closePlayerSuggestions(index)"
                    @keydown.prevent.up="onArrowUp(index)"
                    @keydown.prevent.down="onArrowDown(index)"
                    @keydown.prevent.enter="onEnter(index)"
                    placeholder="جستجوی بازیکن..."
                    class="border border-gray-300 rounded px-2 py-1 text-sm w-40"
                  />
                  <div
                    v-if="playerSuggestions[index]?.show && playerSuggestions[index]?.results.length"
                    class="absolute left-0 right-0 top-full mt-1 bg-white border border-gray-300 rounded shadow-lg z-50 overflow-hidden"
                  >
                    <div
                      v-for="(s, sIdx) in playerSuggestions[index].results"
                      :key="s.id"
                      @mousedown.prevent="selectPlayer(index, s)"
                      @mouseenter="playerSuggestions[index].highlightedIndex = sIdx"
                      class="px-3 py-2 cursor-pointer text-sm"
                      :class="sIdx === playerSuggestions[index].highlightedIndex ? 'bg-gray-200' : 'hover:bg-gray-100'"
                    >
                      {{ s.name }}
                      <span class="text-xs text-gray-400">({{ s.totalGames }} games)</span>
                    </div>
                  </div>
                  <div v-if="duplicateError(index)" class="text-xs text-red-500 mt-1">
                    بازیکن تکراری
                  </div>
                  <div v-if="!player.playerId && touched" class="text-xs text-red-500 mt-1">
                    الزامی
                  </div>
                </div>
              </td>
              <td class="px-4 py-2">
                <select v-model.number="player.roleId" class="border border-gray-300 rounded px-2 py-1 text-sm w-32">
                  <option :value="0" disabled>انتخاب نقش</option>
                  <option v-for="r in filteredRoles" :key="r.id" :value="r.id">{{ r.name }}</option>
                </select>
                <div v-if="!player.roleId && touched" class="text-xs text-red-500 mt-1">
                  الزامی
                </div>
              </td>
              <td class="px-4 py-2">
                <input v-model.number="player.action" type="number" min="0" class="border border-gray-300 rounded px-2 py-1 text-sm w-20" />
              </td>
              <td class="px-4 py-2">
                <input v-model.number="player.rank" type="number" min="0" class="border border-gray-300 rounded px-2 py-1 text-sm w-20" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div class="flex justify-end mb-8">
      <button
        @click="submitForm"
        :disabled="saving"
        class="px-6 py-3 bg-blue-600 text-white rounded font-medium hover:bg-blue-700 transition disabled:opacity-40 disabled:cursor-not-allowed"
      >
        <span v-if="saving" class="inline-flex items-center gap-2">
          <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
          در حال ذخیره...
        </span>
        <span v-else>ساخت بازی</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { LookupApi, PlayerApi, PlaysApi } from '@/api'
import type { Scenario, Event as EventItem, Role, Club, PlayerSearchResult } from '@/types'

const scenarios = ref<Scenario[]>([])
const clubs = ref<Club[]>([])
const events = ref<EventItem[]>([])
const roles = ref<Role[]>([])

const filteredEvents = computed(() =>
  events.value.filter(e => e.clubId === form.clubId)
)
const saving = ref(false)
const touched = ref(false)

const filteredRoles = computed(() =>
  roles.value.filter(r => r.senarioId === form.senarioId)
)

interface PlayerSuggestionState {
  show: boolean
  query: string
  results: PlayerSearchResult[]
  isTyping: boolean
  highlightedIndex: number
  debounceTimer?: ReturnType<typeof setTimeout>
}

const playerSuggestions = reactive<Record<number, PlayerSuggestionState>>({})
const playerCache = ref<Map<number, string>>(new Map())

const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

const form = reactive({
  title: '',
  dateTime: '',
  desc: '',
  senarioId: 0,
  winnersideId: 0,
  clubId: 0,
  eventId: 0,
  link: '',
  playersCount: 10,
  players: [] as { playerId: number; roleId: number; action: number; rank: number }[],
})

function syncPlayerRows() {
  const count = Math.max(0, form.playersCount)
  while (form.players.length < count) {
    form.players.push({ playerId: 0, roleId: 0, action: 0, rank: 0 })
  }
  if (form.players.length > count) {
    form.players.splice(count)
  }
}

function getPlayerName(playerId: number): string {
  if (!playerId) return ''
  return playerCache.value.get(playerId) || ''
}

function openPlayerSuggestions(index: number) {
  if (!playerSuggestions[index]) {
    playerSuggestions[index] = { show: true, query: '', results: [], isTyping: false, highlightedIndex: -1 }
  }
  playerSuggestions[index].show = true
  playerSuggestions[index].highlightedIndex = -1
}

function closePlayerSuggestions(index: number) {
  if (playerSuggestions[index]) playerSuggestions[index].show = false
}

function onArrowDown(index: number) {
  const state = playerSuggestions[index]
  if (!state || state.results.length === 0) return
  state.highlightedIndex = (state.highlightedIndex + 1) % state.results.length
}

function onArrowUp(index: number) {
  const state = playerSuggestions[index]
  if (!state || state.results.length === 0) return
  state.highlightedIndex = state.highlightedIndex <= 0 ? state.results.length - 1 : state.highlightedIndex - 1
}

function onEnter(index: number) {
  const state = playerSuggestions[index]
  if (!state || state.highlightedIndex < 0 || state.highlightedIndex >= state.results.length) return
  selectPlayer(index, state.results[state.highlightedIndex])
}

function onPlayerSearchInput(e: { target: EventTarget | null }, index: number) {
  const target = e.target as HTMLInputElement
  const query = target.value

  if (!playerSuggestions[index]) {
    playerSuggestions[index] = { show: true, query: '', results: [], isTyping: true, highlightedIndex: -1 }
  }

  const state = playerSuggestions[index]
  state.query = query
  state.show = true
  state.isTyping = true

  // Clear selected player when user starts typing a new search
  if (form.players[index].playerId) {
    form.players[index].playerId = 0
  }

  state.highlightedIndex = -1

  if (state.debounceTimer) clearTimeout(state.debounceTimer)

  if (query.length < 2) {
    state.results = []
    return
  }

  state.debounceTimer = setTimeout(async () => {
    try {
      const res = await PlayerApi.searchPlayers(query)
      state.results = res.data.slice(0, 10)
    } catch {
      state.results = []
    }
  }, 200)
}

function onPlayerSearchBlur(index: number) {
  // If no player was selected, reset query display
  setTimeout(() => {
    if (playerSuggestions[index]) {
      playerSuggestions[index].show = false
      if (!form.players[index].playerId) {
        playerSuggestions[index].query = ''
      }
      playerSuggestions[index].isTyping = false
    }
  }, 200)
}


function selectPlayer(index: number, player: PlayerSearchResult) {
  form.players[index].playerId = player.id
  playerCache.value.set(player.id, player.name)
  if (playerSuggestions[index]) {
    playerSuggestions[index].show = false
    playerSuggestions[index].results = []
    playerSuggestions[index].query = ''
    playerSuggestions[index].isTyping = false

  }
}

function duplicateError(index: number): boolean {
  if (!form.players[index].playerId) return false
  return form.players.some((p, i) => i !== index && p.playerId === form.players[index].playerId)
}

function onClubChange() {
  form.eventId = 0
}

function onScenarioChange() {
  for (const p of form.players) {
    if (p.roleId && !roles.value.some(r => r.id === p.roleId && r.senarioId === form.senarioId)) {
      p.roleId = 0
    }
  }
}

function validate(): boolean {
  touched.value = true
  if (!form.title) return false
  if (!form.dateTime) return false
  if (!form.senarioId) return false
  if (!form.eventId) return false
  if (!form.winnersideId) return false
  for (const p of form.players) {
    if (!p.playerId) return false
    if (!p.roleId) return false
  }
  for (let i = 0; i < form.players.length; i++) {
    if (duplicateError(i)) return false
  }
  return true
}

async function submitForm() {
  if (!validate()) return
  saving.value = true
  notification.value = null
  try {
    await PlaysApi.createPlay({
      title: form.title,
      dateTime: new Date(form.dateTime).toISOString(),
      playersCount: form.playersCount,
      desc: form.desc,
      senarioId: form.senarioId,
      winnersideId: form.winnersideId,
      eventId: form.eventId,
      roomId: 1,
      masterId: 1,
      userId: 1,
      guestCount: 0,
      link: form.link,
      players: form.players.map(p => ({
        playerId: p.playerId,
        roleId: p.roleId,
        action: p.action,
        rank: p.rank,
      })),
    })
    notification.value = { type: 'success', message: 'بازی با موفقیت ساخته شد' }
    form.title = ''
    form.dateTime = ''
    form.desc = ''
    form.senarioId = 0
    form.winnersideId = 0
    form.eventId = 0
    form.link = ''
    form.playersCount = 10
    form.players = []
    touched.value = false
    syncPlayerRows()
  } catch {
    notification.value = { type: 'error', message: 'خطا در ساخت بازی' }
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  const [scenariosRes, eventsRes, rolesRes, clubsRes] = await Promise.all([
    LookupApi.getScenarios(),
    LookupApi.getEvents(),
    LookupApi.getRoles(),
    LookupApi.getClubs()
  ])
  scenarios.value = scenariosRes.data
  events.value = eventsRes.data
  roles.value = rolesRes.data
  clubs.value = clubsRes.data
  syncPlayerRows()
})

function gregorianToJalali(gy: number, gm: number, gd: number): [number, number, number] {
  const gdm = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334]
  const gy2 = gm > 2 ? gy + 1 : gy
  let days = 355666 + 365 * gy + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + Math.floor((gy2 + 399) / 400) + gd + gdm[gm - 1]
  let jy = -1595 + 33 * Math.floor(days / 12053)
  days %= 12053
  jy += 4 * Math.floor(days / 1461)
  days %= 1461
  if (days > 365) {
    jy += Math.floor((days - 1) / 365)
    days = (days - 1) % 365
  }
  const jm = days < 186 ? 1 + Math.floor(days / 31) : 7 + Math.floor((days - 186) / 30)
  const jd = 1 + (days < 186 ? days % 31 : (days - 186) % 30)
  return [jy, jm, jd]
}

function jalaliToGregorian(jy: number, jm: number, jd: number): [number, number, number] {
  let ep = jy - 979
  let ey = -61 + 33 * Math.floor(ep / 33)
  ep %= 33
  if (ep >= 0) ey += 4 * Math.floor(ep / 4) - 2 * Math.floor((ep % 4) / 3)
  else ey += 4 * Math.ceil(ep / 4) - 2 * Math.ceil(((-ep - 1) % 4) / 3)
  const em = jm < 7 ? (jm - 1) * 31 : (jm - 7) * 30 + 186
  const ed = jd + em + 365 * ey
  const gy = ey + 621
  const gdm = [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
  const isLeap = (gy % 4 === 0 && gy % 100 !== 0) || gy % 400 === 0
  if (isLeap) gdm[2] = 29
  let remaining = ed
  let gm = 1
  while (gm <= 12 && remaining > gdm[gm]) {
    remaining -= gdm[gm]
    gm++
  }
  return [gy, gm, remaining]
}

const persianMonthNames = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند']

function persianMonthName(_jy: number, jm: number): string {
  return persianMonthNames[jm - 1] || ''
}

function daysInJalaliMonth(_jy: number, jm: number): number {
  if (jm <= 6) return 31
  if (jm <= 11) return 30
  const isLeap = (_jy + 1) % 4 === 0 && ((_jy + 1) % 100 !== 0 || (_jy + 1) % 400 === 0)
  return isLeap ? 30 : 29
}

function todayJalali(): [number, number, number] {
  const now = new Date()
  return gregorianToJalali(now.getFullYear(), now.getMonth() + 1, now.getDate())
}

const calendarOpen = ref(false)
const datePickerRef = ref<HTMLElement | null>(null)
const calJy = ref(todayJalali()[0])
const calJm = ref(todayJalali()[1])
const selectedJy = ref(0)
const selectedJm = ref(0)
const selectedJd = ref(0)

const persianDateDisplay = computed(() => {
  if (!form.dateTime) return ''
  const [jy, jm, jd] = gregorianToJalali(
    new Date(form.dateTime).getFullYear(),
    new Date(form.dateTime).getMonth() + 1,
    new Date(form.dateTime).getDate()
  )
  selectedJy.value = jy
  selectedJm.value = jm
  selectedJd.value = jd
  return `${jy}/${String(jm).padStart(2, '0')}/${String(jd).padStart(2, '0')}`
})

const calendarDays = computed(() => {
  const firstDayOfMonth = new Date(
    ...jalaliToGregorian(calJy.value, calJm.value, 1)
  ).getDay()
  const daysInMonth = daysInJalaliMonth(calJy.value, calJm.value)
  const days: (number | null)[] = []
  for (let i = 0; i < (firstDayOfMonth + 1) % 7; i++) {
    days.push(null)
  }
  for (let d = 1; d <= daysInMonth; d++) {
    days.push(d)
  }
  return days
})

function prevMonth() {
  if (calJm.value === 1) {
    calJm.value = 12
    calJy.value--
  } else {
    calJm.value--
  }
}

function nextMonth() {
  if (calJm.value === 12) {
    calJm.value = 1
    calJy.value++
  } else {
    calJm.value++
  }
}

function dayClass(day: number): string {
  const isSelected = selectedJy.value === calJy.value && selectedJm.value === calJm.value && selectedJd.value === day
  const isToday = (() => {
    const [ty, tm, td] = todayJalali()
    return ty === calJy.value && tm === calJm.value && td === day
  })()
  if (isSelected) return 'bg-blue-600 text-white'
  if (isToday) return 'bg-blue-100 text-blue-700 font-semibold'
  return 'hover:bg-gray-100'
}

function selectDate(day: number) {
  const [gy, gm, gd] = jalaliToGregorian(calJy.value, calJm.value, day)
  const d = new Date(gy, gm - 1, gd)
  form.dateTime = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T00:00:00`
  selectedJy.value = calJy.value
  selectedJm.value = calJm.value
  selectedJd.value = day
  calendarOpen.value = false
}

function handleClickOutside(e: MouseEvent) {
  if (datePickerRef.value && !datePickerRef.value.contains(e.target as Node)) {
    calendarOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
