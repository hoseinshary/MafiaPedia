<template>
  <div dir="rtl" class="w-full md:w-3/4 mx-auto">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-white">ویرایش بازی</h1>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm" :class="notification.type === 'success' ? 'bg-green-900 text-green-300 border border-green-700' : 'bg-red-900 text-red-300 border border-red-700'">
      {{ notification.message }}
    </div>

    <div v-if="loadingPlay" class="flex justify-center py-20">
      <div class="w-10 h-10 border-4 border-gray-600 border-t-red-500 rounded-full animate-spin" />
    </div>

    <template v-else>
      <section class="bg-gray-800 rounded-lg border border-gray-700 p-6 mb-6">
        <h2 class="text-lg font-semibold mb-4 text-white">اطلاعات بازی</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">عنوان</label>
            <input v-model="form.title" type="text" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-red-500 transition" />
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">تاریخ</label>
            <input
              v-model="form.dateTime"
              type="date"
              class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition"
              dir="ltr"
            />
            <div v-if="!form.dateTime && touched" class="text-xs text-red-400 mt-1">
              الزامی
            </div>
          </div>
          <div class="flex flex-col gap-1 md:col-span-2">
            <label class="text-sm text-gray-400">توضیحات</label>
            <textarea v-model="form.desc" rows="3" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-red-500 transition resize-none"></textarea>
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">سناریو</label>
            <select v-model.number="form.senarioId" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition" @change="onScenarioChange">
              <option :value="0" disabled>انتخاب سناریو</option>
              <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
            </select>
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">کلاب</label>
            <select v-model.number="form.clubId" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition" @change="onClubChange">
              <option :value="0" disabled>انتخاب کلاب</option>
              <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
            </select>
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">ایونت</label>
            <select v-model.number="form.eventId" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition" :disabled="!form.clubId">
              <option :value="0" disabled>{{ form.clubId ? 'انتخاب ایونت' : 'ابتدا کلاب را انتخاب کنید' }}</option>
              <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
            </select>
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">برنده</label>
            <select v-model.number="form.winnersideId" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition">
              <option :value="0" disabled>انتخاب برنده</option>
              <option :value="1">شهروند</option>
              <option :value="2">مافیا</option>
            </select>
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">لینک</label>
            <input v-model="form.link" type="text" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-red-500 transition" />
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-gray-400">تعداد بازیکن</label>
            <input v-model.number="form.playersCount" type="number" min="1" max="50" class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition" @input="syncPlayerRows" />
          </div>
        </div>
      </section>

      <section class="bg-gray-800 rounded-lg border border-gray-700 p-6 mb-6">
        <h2 class="text-lg font-semibold mb-4 text-white">بازیکنان</h2>
        <div v-if="form.players.length === 0" class="text-center py-8 text-gray-500">
          تعداد بازیکن را مشخص کنید
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm border-collapse">
            <thead>
              <tr class="border-b border-gray-600 bg-gray-700 text-gray-300">
                <th class="px-4 py-3 text-right">#</th>
                <th class="px-4 py-3 text-right">بازیکن</th>
                <th class="px-4 py-3 text-right">نقش</th>
                <th class="px-4 py-3 text-right">اکشن</th>
                <th class="px-4 py-3 text-right">رنک</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(player, index) in form.players" :key="index" class="border-b border-gray-700">
                <td class="px-4 py-2 text-gray-400">{{ index + 1 }}</td>
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
                      class="bg-gray-700 border border-gray-600 rounded px-2 py-1 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-red-500 transition w-40"
                    />
                    <div
                      v-if="playerSuggestions[index]?.show && playerSuggestions[index]?.results.length"
                      class="absolute left-0 right-0 top-full mt-1 bg-gray-700 border border-gray-600 rounded shadow-lg z-50 overflow-hidden"
                    >
                      <div
                        v-for="(s, sIdx) in playerSuggestions[index].results"
                        :key="s.id"
                        @mousedown.prevent="selectPlayer(index, s)"
                        @mouseenter="playerSuggestions[index].highlightedIndex = sIdx"
                        class="px-3 py-2 cursor-pointer text-sm text-gray-200"
                        :class="sIdx === playerSuggestions[index].highlightedIndex ? 'bg-gray-600' : 'hover:bg-gray-600'"
                      >
                        {{ s.name }}
                        <span class="text-xs text-gray-400">({{ s.totalGames }} games)</span>
                      </div>
                    </div>
                    <div v-if="duplicateError(index)" class="text-xs text-red-400 mt-1">
                      بازیکن تکراری
                    </div>
                    <div v-if="!player.playerId && touched" class="text-xs text-red-400 mt-1">
                      الزامی
                    </div>
                  </div>
                </td>
                <td class="px-4 py-2">
                  <select v-model.number="player.roleId" class="bg-gray-700 border border-gray-600 rounded px-2 py-1 text-sm text-white focus:outline-none focus:border-red-500 transition w-32">
                    <option :value="0" disabled>انتخاب نقش</option>
                    <option v-for="r in filteredRoles" :key="r.id" :value="r.id">{{ r.name }}</option>
                  </select>
                  <div v-if="!player.roleId && touched" class="text-xs text-red-400 mt-1">
                    الزامی
                  </div>
                </td>
                <td class="px-4 py-2">
                  <input v-model.number="player.action" type="number" min="0" class="bg-gray-700 border border-gray-600 rounded px-2 py-1 text-sm text-white focus:outline-none focus:border-red-500 transition w-20" />
                </td>
                <td class="px-4 py-2">
                  <input v-model.number="player.rank" type="number" min="0" class="bg-gray-700 border border-gray-600 rounded px-2 py-1 text-sm text-white focus:outline-none focus:border-red-500 transition w-20" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div class="flex justify-end gap-3 mb-8">
        <router-link
          to="/plays"
          class="px-6 py-3 bg-gray-700 text-gray-300 rounded font-medium hover:bg-gray-600 transition text-center"
        >
          انصراف
        </router-link>
        <button
          @click="submitForm"
          :disabled="saving"
          class="px-6 py-3 bg-blue-600 text-white rounded font-medium hover:bg-blue-700 transition disabled:opacity-40 disabled:cursor-not-allowed"
        >
          <span v-if="saving" class="inline-flex items-center gap-2">
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            در حال ذخیره...
          </span>
          <span v-else>ذخیره تغییرات</span>
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { LookupApi, PlayerApi, PlaysApi } from '@/api'
import type { Senario, Event as EventItem, Role, Club, PlayerSearchResult } from '@/types'

const route = useRoute()
const router = useRouter()
const playId = Number(route.params.id)

const scenarios = ref<Senario[]>([])
const clubs = ref<Club[]>([])
const events = ref<EventItem[]>([])
const roles = ref<Role[]>([])

const filteredEvents = computed(() =>
  events.value.filter(e => e.clubId === form.clubId)
)
const saving = ref(false)
const touched = ref(false)
const loadingPlay = ref(true)

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
    await PlaysApi.updatePlay(playId, {
      title: form.title,
      dateTime: new Date(form.dateTime).toISOString(),
      playersCount: form.playersCount,
      desc: form.desc,
      senarioId: form.senarioId,
      winnersideId: form.winnersideId,
      eventId: form.eventId,
      roomId: 1,
      masterId: 1,
      guestCount: 0,
      link: form.link,
      players: form.players.map(p => ({
        playerId: p.playerId,
        roleId: p.roleId,
        action: p.action,
        rank: p.rank,
      })),
    })
    notification.value = { type: 'success', message: 'تغییرات با موفقیت ذخیره شد' }
    setTimeout(() => {
      router.push('/plays')
    }, 1500)
  } catch {
    notification.value = { type: 'error', message: 'خطا در ذخیره تغییرات' }
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  try {
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

    const playRes = await PlaysApi.getPlay(playId)
    const play = playRes.data

    form.title = play.title
    form.dateTime = play.dateTime ? play.dateTime.substring(0, 10) : ''
    form.desc = play.desc || ''
    form.senarioId = play.senarioId
    form.winnersideId = play.winnersideId
    form.eventId = play.eventId
    form.link = play.link || ''
    form.playersCount = play.playersCount
    form.clubId = play.eventId ? events.value.find(e => e.id === play.eventId)?.clubId || 0 : 0

    if (play.players && play.players.length > 0) {
      form.players = play.players.map((p: { playerId: number; roleId: number; action?: number; rank?: number }) => ({
        playerId: p.playerId,
        roleId: p.roleId,
        action: p.action ?? 0,
        rank: p.rank ?? 0,
      }))
    } else {
      syncPlayerRows()
    }

    for (const p of form.players) {
      if (p.playerId) {
        try {
          const playerRes = await PlayerApi.getPlayerProfile(p.playerId)
          playerCache.value.set(p.playerId, playerRes.data.name)
        } catch {
          // ignore
        }
      }
    }
  } catch {
    notification.value = { type: 'error', message: 'خطا در بارگذاری اطلاعات بازی' }
  } finally {
    loadingPlay.value = false
  }
})
</script>