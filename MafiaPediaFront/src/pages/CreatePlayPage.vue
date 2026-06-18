<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-[#e8e4d9]">ساخت بازی جدید</h1>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm border" :class="notification.type === 'success' ? 'bg-[rgba(111,207,138,0.1)] border-[rgba(111,207,138,0.2)] text-[#6fcf8a]' : 'bg-[rgba(224,112,112,0.1)] border-[rgba(224,112,112,0.2)] text-[#e07070]'">
      {{ notification.message }}
    </div>

    <section class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-6 mb-6">
      <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">اطلاعات بازی</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">عنوان</label>
          <input v-model="form.title" type="text" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">تاریخ</label>
          <input
            v-model="form.dateTime"
            type="date"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            dir="ltr"
          />
          <div v-if="!form.dateTime && touched" class="text-xs text-[#e07070] mt-1">
            الزامی
          </div>
        </div>
        <div class="flex flex-col gap-1 md:col-span-2">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">توضیحات</label>
          <textarea v-model="form.desc" rows="3" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition resize-none"></textarea>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">سناریو</label>
          <select v-model.number="form.senarioId" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" @change="onSenarioChange">
            <option :value="0" disabled>انتخاب سناریو</option>
            <option v-for="s in senarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">کلاب</label>
          <select v-model.number="form.clubId" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" @change="onClubChange">
            <option :value="0" disabled>انتخاب کلاب</option>
            <option v-for="c in clubs" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">ایونت</label>
          <select v-model.number="form.eventId" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" :disabled="!form.clubId">
            <option :value="0" disabled>{{ form.clubId ? 'انتخاب ایونت' : 'ابتدا کلاب را انتخاب کنید' }}</option>
            <option v-for="e in filteredEvents" :key="e.id" :value="e.id">{{ e.name }}</option>
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">برنده</label>

          
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">لینک</label>
          <input v-model="form.link" type="text" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">تعداد بازیکن</label>
          <input v-model.number="form.playersCount" type="number" min="1" max="50" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" @input="syncPlayerRows" />
        </div>
      </div>
    </section>

    <section class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-6 mb-6">
      <h2 class="text-xs uppercase tracking-widest text-[rgba(232,228,217,0.45)] mb-4">بازیکنان</h2>
      <div v-if="form.players.length === 0" class="text-center py-8 text-[rgba(232,228,217,0.4)]">
        تعداد بازیکن را مشخص کنید
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm border-collapse">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e] text-[rgba(232,228,217,0.5)]">
              <th class="px-4 py-3 text-right">#</th>
              <th class="px-4 py-3 text-right">بازیکن</th>
              <th class="px-4 py-3 text-right">نقش</th>
              <th class="px-4 py-3 text-right">اکشن</th>
              <th class="px-4 py-3 text-right">رنک</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(player, index) in form.players" :key="index" class="border-b border-[rgba(255,255,255,0.04)] text-[#e8e4d9]">
              <td class="px-4 py-2 text-[rgba(232,228,217,0.4)]">{{ index + 1 }}</td>
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
                    class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-2 py-1 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition w-40"
                  />
                  <div
                    v-if="playerSuggestions[index]?.show && playerSuggestions[index]?.results.length"
                    class="absolute left-0 right-0 top-full mt-1 bg-[#141416] border border-[rgba(255,255,255,0.07)] rounded shadow-lg z-50 overflow-hidden"
                  >
                    <div
                      v-for="(s, sIdx) in playerSuggestions[index].results"
                      :key="s.id"
                      @mousedown.prevent="selectPlayer(index, s)"
                      @mouseenter="playerSuggestions[index].highlightedIndex = sIdx"
                      class="px-3 py-2 cursor-pointer text-sm text-[#e8e4d9]"
                      :class="sIdx === playerSuggestions[index].highlightedIndex ? 'bg-[rgba(255,255,255,0.05)]' : 'hover:bg-[rgba(255,255,255,0.03)]'"
                    >
                      {{ s.name }}
                      <span class="text-xs text-[rgba(232,228,217,0.35)]">({{ s.totalGames }} games)</span>
                    </div>
                  </div>
                  <div v-if="duplicateError(index)" class="text-xs text-[#e07070] mt-1">
                    بازیکن تکراری
                  </div>
                  <div v-if="!player.playerId && touched" class="text-xs text-[#e07070] mt-1">
                    الزامی
                  </div>
                </div>
              </td>
              <td class="px-4 py-2">
                <select v-model.number="player.roleId" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-2 py-1 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition w-32">
                  <option :value="0" disabled>انتخاب نقش</option>
                  <option v-for="r in filteredRoles" :key="r.id" :value="r.id">{{ r.name }}</option>
                </select>
                <div v-if="!player.roleId && touched" class="text-xs text-[#e07070] mt-1">
                  الزامی
                </div>
              </td>
              <td class="px-4 py-2">
                <input v-model.number="player.action" type="number" min="0" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-2 py-1 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition w-20" />
              </td>
              <td class="px-4 py-2">
                <input v-model.number="player.rank" type="number" min="0" class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-2 py-1 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition w-20" />
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
        class="px-6 py-3 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] rounded font-medium transition disabled:opacity-40 disabled:cursor-not-allowed"
      >
        <span v-if="saving" class="inline-flex items-center gap-2">
          <div class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          در حال ذخیره...
        </span>
        <span v-else>ساخت بازی</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { LookupApi, PlayerApi, PlaysApi } from '@/api'
import type { Senario, Event as EventItem, Role, Club, PlayerSearchResult } from '@/types'

const senarios = ref<Senario[]>([])
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

function onSenarioChange() {
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
  const [senariosRes, eventsRes, rolesRes, clubsRes] = await Promise.all([
    LookupApi.getScenarios(),
    LookupApi.getEvents(),
    LookupApi.getRoles(),
    LookupApi.getClubs()
  ])
  senarios.value = senariosRes.data
  events.value = eventsRes.data
  roles.value = rolesRes.data
  clubs.value = clubsRes.data
  syncPlayerRows()
})
</script>
