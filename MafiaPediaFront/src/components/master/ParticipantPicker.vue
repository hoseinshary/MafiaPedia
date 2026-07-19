<template>
  <div>
    <div class="relative" ref="containerRef">
      <div class="flex items-center gap-2">
        <input
          type="text"
          v-model="query"
          :placeholder="replaceTargetId != null ? 'جستجوی بازیکن جایگزین...' : `جستجوی بازیکن با نام یا موبایل... (${selected.length} نفر)`"
          class="flex-1 bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition ltr text-left"
          @input="onInput"
          @focus="onFocus"
        />
        <button
          v-if="replaceTargetId != null"
          @click="cancelReplace"
          class="text-xs text-muted hover:text-fg whitespace-nowrap border border-border rounded px-3 py-2 transition"
        >
          انصراف
        </button>
      </div>
      <div
        v-if="loading"
        class="absolute left-0 right-0 top-full mt-1 bg-surface border border-border rounded shadow-lg z-50 flex items-center justify-center py-4"
      >
        <div class="w-5 h-5 border-2 border-gold border-t-transparent rounded-full animate-spin" />
      </div>
      <div
        v-else-if="showResults"
        class="absolute left-0 right-0 top-full mt-1 bg-surface border border-border rounded shadow-lg z-50 overflow-hidden"
        :style="{ maxHeight: '320px', overflowY: 'auto' }"
      >
        <div v-if="inClub.length > 0">
          <div class="px-3 py-1.5 text-xs text-gold-text/50 font-medium border-b border-border">بازیکنان این کافه</div>
          <div
            v-for="player in inClub"
            :key="'in-'+player.id"
            @click="replaceTargetId != null ? doReplace(player) : addParticipant(player)"
            class="flex items-center gap-3 px-3 py-2 cursor-pointer transition hover:bg-surface-hover"
            :class="{ 'opacity-40': isSelected(player.id) && replaceTargetId == null }"
          >
            <img :src="player.picture || defaultAvatar" alt="" class="w-8 h-8 rounded-full object-cover bg-surface-hover" />
            <div class="flex-1 min-w-0">
              <p class="text-sm text-fg truncate">{{ player.name }}</p>
              <p class="text-xs text-muted">{{ player.mobile }}</p>
            </div>
            <span v-if="isSelected(player.id)" class="text-xs text-gold-text">انتخاب شده</span>
          </div>
        </div>

        <div v-if="limitedGlobalOthers.length > 0">
          <div class="px-3 py-1.5 text-xs text-muted font-medium border-b border-border">در سیستم هست، ولی عضو این کافه نیست</div>
          <div
            v-for="player in limitedGlobalOthers"
            :key="'global-'+player.id"
            @click="replaceTargetId != null ? replaceWithGlobal(player) : addGlobalAndSelect(player)"
            class="flex items-center gap-3 px-3 py-2 cursor-pointer transition hover:bg-surface-hover border-r-2 border-r-gold"
            :class="{ 'opacity-40': isSelected(player.id) && replaceTargetId == null }"
          >
            <img :src="player.picture || defaultAvatar" alt="" class="w-8 h-8 rounded-full object-cover bg-surface-hover" />
            <div class="flex-1 min-w-0">
              <p class="text-sm text-fg truncate">{{ player.name }}</p>
              <p class="text-xs text-muted">{{ player.mobile }}</p>
            </div>
            <span v-if="isSelected(player.id)" class="text-xs text-gold-text">انتخاب شده</span>
          </div>
        </div>

        <div v-if="inClub.length === 0 && globalOthers.length === 0 && query.trim().length >= 2" class="px-3 py-3">
          <p class="text-sm text-muted mb-3">بازیکنی با این مشخصات یافت نشد</p>
          <div class="flex flex-col gap-2">
            <input
              v-model="newName"
              type="text"
              class="w-full bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold"
              placeholder="نام بازیکن جدید"
              @click.stop
            />
            <input
              v-model="newMobile"
              type="text"
              class="w-full bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold ltr"
              placeholder="09123456789"
              maxlength="11"
              @click.stop
            />
            <button
              @click.stop="createAndSelect"
              :disabled="!newName.trim() || newMobile.trim().length !== 11 || creating"
              class="px-3 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition"
            >
              <span v-if="creating" class="inline-block w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              <span v-else>افزودن و اضافه به بازی</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="selected.length > 0" class="mt-3 space-y-1">
      <div
        v-for="p in selected"
        :key="p.player.id"
        class="flex items-center justify-between px-3 py-2 rounded bg-gold/10 border border-gold/10"
      >
        <div class="flex items-center gap-3">
          <span class="text-sm text-fg">{{ p.player.name }}</span>
          <div v-if="props.playType === 'normal' && !p.isGuest" class="flex items-center gap-1">
            <button
              @click="changeEntryCount(p.player.id, -1)"
              class="w-6 h-6 flex items-center justify-center text-xs rounded border border-gold/30 text-gold-text hover:bg-gold/20 transition disabled:opacity-30"
              :disabled="p.entryCount <= 1"
            >−</button>
            <span class="w-7 text-center text-xs text-fg tabular-nums" dir="ltr">{{ p.entryCount }}</span>
            <button
              @click="changeEntryCount(p.player.id, 1)"
              class="w-6 h-6 flex items-center justify-center text-xs rounded border border-gold/30 text-gold-text hover:bg-gold/20 transition disabled:opacity-30"
              :disabled="p.entryCount >= 10"
            >+</button>
            <span class="text-xs text-muted mr-1">تعداد ورودی</span>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <label class="flex items-center gap-1 text-xs cursor-pointer text-muted hover:text-gold-text">
            <input type="checkbox" :checked="p.isGuest" @change="toggleGuest(p.player.id)" class="accent-gold" />
            مهمان
          </label>
          <button
            v-if="allowInPlaceReplace && playId"
            @click="startReplace(p.player.id)"
            class="text-gold-text/50 hover:text-gold-text text-sm transition"
            :title="'تعویض بازیکن'"
          >&#9998;</button>
          <span v-if="allowInPlaceReplace && playId" class="text-[rgba(255,255,255,0.1)] mx-1">|</span>
          <button @click="removeParticipant(p.player.id)" class="text-danger hover:opacity-80 text-sm">&times;</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { ClubPlayerApi, ClubPlayApi } from '@/api'
import { useToast } from '@/composables/useToast'
import type { ClubPlayerDto } from '@/types/clubPlayer'

const props = defineProps<{
  clubId: number
  initialSelected?: PickerParticipant[]
  allowInPlaceReplace?: boolean
  playId?: number
  playType?: string
}>()

export interface PickerParticipant {
  id?: number
  player: ClubPlayerDto
  isGuest: boolean
  entryCount: number
}

const emit = defineEmits<{
  change: [selected: PickerParticipant[]]
}>()

const query = ref('')
const inClub = ref<ClubPlayerDto[]>([])
const globalOthers = ref<ClubPlayerDto[]>([])
const loading = ref(false)
const selected = ref<PickerParticipant[]>([])
const containerRef = ref<HTMLElement | null>(null)
const showDropdown = ref(false)
const newName = ref('')
const newMobile = ref('')
const creating = ref(false)
const replaceTargetId = ref<number | null>(null)
const replacing = ref(false)

const { toastSuccess } = useToast()

const defaultAvatar = 'data:image/svg+xml,' + encodeURIComponent('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" fill="%236b7280"><rect width="100" height="100" rx="50"/><text x="50" y="58" text-anchor="middle" font-size="40" fill="%23d1d5db" font-family="Arial">?</text></svg>')

let debounceTimer: ReturnType<typeof setTimeout>

const showResults = computed(() => showDropdown.value && (inClub.value.length > 0 || globalOthers.value.length > 0 || query.value.trim().length >= 2))

const limitedGlobalOthers = computed(() => globalOthers.value.slice(0, 5))

function isSelected(id: number): boolean {
  return selected.value.some(p => p.player.id === id)
}

function onInput() {
  clearTimeout(debounceTimer)
  if (query.value.trim().length < 2) {
    inClub.value = []
    globalOthers.value = []
    showDropdown.value = false
    return
  }
  debounceTimer = setTimeout(async () => {
    loading.value = true
    showDropdown.value = true
    try {
      const res = await ClubPlayerApi.searchAllCustomers(props.clubId, query.value)
      inClub.value = res.data.inClub
      globalOthers.value = res.data.globalOthers
    } catch {
      inClub.value = []
      globalOthers.value = []
    } finally {
      loading.value = false
    }
  }, 300)
}

function onFocus() {
  if (inClub.value.length > 0 || globalOthers.value.length > 0 || query.value.trim().length >= 2) {
    showDropdown.value = true
  }
}

function changeEntryCount(id: number, delta: number) {
  const entry = selected.value.find(p => p.player.id === id)
  if (!entry) return
  const newVal = entry.entryCount + delta
  if (newVal < 1 || newVal > 10) return
  entry.entryCount = newVal
  emit('change', selected.value)
}

watch(() => props.playType, (newType) => {
  if (newType && newType !== 'normal') {
    let changed = false
    selected.value.forEach(p => {
      if (p.entryCount > 1) {
        p.entryCount = 1
        changed = true
      }
    })
    if (changed) emit('change', selected.value)
  }
})

function addParticipant(player: ClubPlayerDto) {
  if (isSelected(player.id)) return
  if (replaceTargetId.value != null) return // handled by doReplace
  selected.value.push({ player, isGuest: false, entryCount: 1 })
  query.value = ''
  inClub.value = []
  globalOthers.value = []
  showDropdown.value = false
  emit('change', selected.value)
}

async function addGlobalAndSelect(player: ClubPlayerDto) {
  if (isSelected(player.id)) return
  if (replaceTargetId.value != null) return // handled by replaceWithGlobal
  try {
    const fd = new FormData()
    fd.append('name', player.name)
    fd.append('mobile', player.mobile)
    await ClubPlayerApi.createOrJoin(props.clubId, fd)
  } catch {
    // already a member or other error — still allow adding to game
  }
  selected.value.push({ player, isGuest: false, entryCount: 1 })
  query.value = ''
  inClub.value = []
  globalOthers.value = []
  showDropdown.value = false
  emit('change', selected.value)
}

async function replaceAllEntries(oldPlayerId: number, newPlayer: ClubPlayerDto) {
  const matchingEntries = selected.value.filter(p => p.player.id === oldPlayerId)
  for (const entry of matchingEntries) {
    if (entry.id != null && props.playId) {
      await ClubPlayApi.replaceParticipant(props.clubId, props.playId, entry.id, {
        newClubPlayerId: newPlayer.id,
        isGuest: entry.isGuest,
        entryCount: 1,
      })
    }
  }
  const newEntries = matchingEntries.map(e => ({
    ...e,
    player: newPlayer,
  }))
  selected.value = selected.value.filter(p => p.player.id !== oldPlayerId)
  selected.value.push(...newEntries)
  emit('change', [...selected.value])
}

async function createAndSelect() {
  if (!newName.value.trim() || newMobile.value.trim().length !== 11) return
  creating.value = true
  try {
    const fd = new FormData()
    fd.append('name', newName.value.trim())
    fd.append('mobile', newMobile.value.trim())
    const res = await ClubPlayerApi.createOrJoin(props.clubId, fd)
    const newPlayer = res.data.clubPlayer

    if (replaceTargetId.value != null) {
      await replaceAllEntries(replaceTargetId.value, newPlayer)
      toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
      cancelReplace()
    } else {
      selected.value.push({ player: newPlayer, isGuest: false, entryCount: 1 })
      emit('change', selected.value)
    }
    newName.value = ''
    newMobile.value = ''
    query.value = ''
    inClub.value = []
    globalOthers.value = []
    showDropdown.value = false
  } catch {
    // ignore
  } finally {
    creating.value = false
  }
}

function removeParticipant(id: number) {
  selected.value = selected.value.filter(p => p.player.id !== id)
  emit('change', selected.value)
}

function toggleGuest(id: number) {
  const entry = selected.value.find(p => p.player.id === id)
  if (entry) {
    entry.isGuest = !entry.isGuest
    if (entry.isGuest) {
      entry.entryCount = 1
    }
    emit('change', selected.value)
  }
}

function startReplace(playerId: number) {
  replaceTargetId.value = playerId
  query.value = ''
  inClub.value = []
  globalOthers.value = []
  showDropdown.value = false
  setTimeout(() => containerRef.value?.querySelector('input')?.focus(), 50)
}

function cancelReplace() {
  replaceTargetId.value = null
  query.value = ''
  inClub.value = []
  globalOthers.value = []
  showDropdown.value = false
}

async function doReplace(newPlayer: ClubPlayerDto) {
  if (replaceTargetId.value == null) return
  replacing.value = true
  try {
    await replaceAllEntries(replaceTargetId.value, newPlayer)
    toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
    cancelReplace()
  } catch {
    // interceptor already shows toast
  } finally {
    replacing.value = false
  }
}

async function replaceWithGlobal(player: ClubPlayerDto) {
  if (replaceTargetId.value == null) return
  replacing.value = true
  try {
    const fd = new FormData()
    fd.append('name', player.name)
    fd.append('mobile', player.mobile)
    await ClubPlayerApi.createOrJoin(props.clubId, fd)

    await replaceAllEntries(replaceTargetId.value, player)
    toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
    cancelReplace()
  } catch {
    // interceptor already shows toast
  } finally {
    replacing.value = false
  }
}

function handleClickOutside(e: MouseEvent) {
  if (containerRef.value && !containerRef.value.contains(e.target as Node)) {
    showDropdown.value = false
  }
}

import { onMounted, onUnmounted } from 'vue'
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  if (props.initialSelected?.length) {
    selected.value = props.initialSelected.map(p => ({ ...p, entryCount: p.entryCount ?? 1 }))
    emit('change', selected.value)
  }
})
onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  clearTimeout(debounceTimer)
})
</script>
