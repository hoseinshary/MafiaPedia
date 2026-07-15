<template>
  <div>
    <div class="relative" ref="containerRef">
      <div class="flex items-center gap-2">
        <input
          type="text"
          v-model="query"
          :placeholder="replaceTargetId != null ? 'جستجوی بازیکن جایگزین...' : `جستجوی بازیکن با نام یا موبایل... (${selected.length} نفر)`"
          class="flex-1 bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition ltr text-left"
          @input="onInput"
          @focus="onFocus"
        />
        <button
          v-if="replaceTargetId != null"
          @click="cancelReplace"
          class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] whitespace-nowrap border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 transition"
        >
          انصراف
        </button>
      </div>
      <div
        v-if="loading"
        class="absolute left-0 right-0 top-full mt-1 bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded shadow-lg z-50 flex items-center justify-center py-4"
      >
        <div class="w-5 h-5 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div
        v-else-if="showResults"
        class="absolute left-0 right-0 top-full mt-1 bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded shadow-lg z-50 overflow-hidden"
        :style="{ maxHeight: '320px', overflowY: 'auto' }"
      >
        <div v-if="inClub.length > 0">
          <div class="px-3 py-1.5 text-xs text-[rgba(201,176,122,0.5)] font-medium border-b border-[rgba(255,255,255,0.05)]">بازیکنان این کافه</div>
          <div
            v-for="player in inClub"
            :key="'in-'+player.id"
            @click="replaceTargetId != null ? doReplace(player) : addParticipant(player)"
            class="flex items-center gap-3 px-3 py-2 cursor-pointer transition hover:bg-[rgba(255,255,255,0.03)]"
            :class="{ 'opacity-40': isSelected(player.id) && replaceTargetId == null }"
          >
            <img :src="player.picture || defaultAvatar" alt="" class="w-8 h-8 rounded-full object-cover bg-gray-600" />
            <div class="flex-1 min-w-0">
              <p class="text-sm text-[#e8e4d9] truncate">{{ player.name }}</p>
              <p class="text-xs text-[rgba(232,228,217,0.3)]">{{ player.mobile }}</p>
            </div>
            <span v-if="isSelected(player.id)" class="text-xs text-[#c9b07a]">انتخاب شده</span>
          </div>
        </div>

        <div v-if="limitedGlobalOthers.length > 0">
          <div class="px-3 py-1.5 text-xs text-[rgba(232,228,217,0.25)] font-medium border-b border-[rgba(255,255,255,0.05)]">در سیستم هست، ولی عضو این کافه نیست</div>
          <div
            v-for="player in limitedGlobalOthers"
            :key="'global-'+player.id"
            @click="replaceTargetId != null ? replaceWithGlobal(player) : addGlobalAndSelect(player)"
            class="flex items-center gap-3 px-3 py-2 cursor-pointer transition hover:bg-[rgba(255,255,255,0.03)] border-r-2 border-r-[rgba(201,176,122,0.3)]"
            :class="{ 'opacity-40': isSelected(player.id) && replaceTargetId == null }"
          >
            <img :src="player.picture || defaultAvatar" alt="" class="w-8 h-8 rounded-full object-cover bg-gray-600" />
            <div class="flex-1 min-w-0">
              <p class="text-sm text-[#e8e4d9] truncate">{{ player.name }}</p>
              <p class="text-xs text-[rgba(232,228,217,0.3)]">{{ player.mobile }}</p>
            </div>
            <span v-if="isSelected(player.id)" class="text-xs text-[#c9b07a]">انتخاب شده</span>
          </div>
        </div>

        <div v-if="inClub.length === 0 && globalOthers.length === 0 && query.trim().length >= 2" class="px-3 py-3">
          <p class="text-sm text-[rgba(232,228,217,0.4)] mb-3">بازیکنی با این مشخصات یافت نشد</p>
          <div class="flex flex-col gap-2">
            <input
              v-model="newName"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)]"
              placeholder="نام بازیکن جدید"
              @click.stop
            />
            <input
              v-model="newMobile"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] ltr"
              placeholder="09123456789"
              maxlength="11"
              @click.stop
            />
            <button
              @click.stop="createAndSelect"
              :disabled="!newName.trim() || newMobile.trim().length !== 11 || creating"
              class="px-3 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition"
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
        class="flex items-center justify-between px-3 py-2 rounded bg-[rgba(201,176,122,0.06)] border border-[rgba(201,176,122,0.1)]"
      >
        <span class="text-sm text-[#e8e4d9]">{{ p.player.name }}</span>
        <div class="flex items-center gap-2">
          <label class="flex items-center gap-1 text-xs cursor-pointer text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a]">
            <input type="checkbox" :checked="p.isGuest" @change="toggleGuest(p.player.id)" class="accent-[#c9b07a]" />
            مهمان
          </label>
          <button
            v-if="allowInPlaceReplace && playId"
            @click="startReplace(p.player.id)"
            class="text-[rgba(201,176,122,0.5)] hover:text-[#c9b07a] text-sm transition"
            :title="'تعویض بازیکن'"
          >&#9998;</button>
          <span v-if="allowInPlaceReplace && playId" class="text-[rgba(255,255,255,0.1)] mx-1">|</span>
          <button @click="removeParticipant(p.player.id)" class="text-[#e07070] hover:text-[#d06060] text-sm">&times;</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ClubPlayerApi, ClubPlayApi } from '@/api'
import { useToast } from '@/composables/useToast'
import type { ClubPlayerDto } from '@/types/clubPlayer'

const props = defineProps<{
  clubId: number
  initialSelected?: PickerParticipant[]
  allowInPlaceReplace?: boolean
  playId?: number
}>()

export interface PickerParticipant {
  player: ClubPlayerDto
  isGuest: boolean
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

const { toastSuccess, toastError } = useToast()

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

function addParticipant(player: ClubPlayerDto) {
  if (isSelected(player.id)) return
  if (replaceTargetId.value != null) return // handled by doReplace
  selected.value.push({ player, isGuest: false })
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
  selected.value.push({ player, isGuest: false })
  query.value = ''
  inClub.value = []
  globalOthers.value = []
  showDropdown.value = false
  emit('change', selected.value)
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

    if (replaceTargetId.value != null && props.playId) {
      // Replace mode
      const currentEntry = selected.value.find(p => p.player.id === replaceTargetId.value)
      await ClubPlayApi.replaceParticipant(props.clubId, props.playId, replaceTargetId.value, {
        newClubPlayerId: newPlayer.id,
        isGuest: currentEntry?.isGuest ?? false,
      })
      const idx = selected.value.findIndex(p => p.player.id === replaceTargetId.value)
      if (idx !== -1) {
        selected.value[idx] = { player: newPlayer, isGuest: currentEntry?.isGuest ?? false }
        emit('change', [...selected.value])
      }
      toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
      cancelReplace()
    } else {
      selected.value.push({ player: newPlayer, isGuest: false })
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
  if (replaceTargetId.value == null || !props.playId) return
  replacing.value = true
  try {
    const currentEntry = selected.value.find(p => p.player.id === replaceTargetId.value)
    await ClubPlayApi.replaceParticipant(props.clubId, props.playId, replaceTargetId.value, {
      newClubPlayerId: newPlayer.id,
      isGuest: currentEntry?.isGuest ?? false,
    })
    const idx = selected.value.findIndex(p => p.player.id === replaceTargetId.value)
    if (idx !== -1) {
      selected.value[idx] = { player: newPlayer, isGuest: currentEntry?.isGuest ?? false }
      emit('change', [...selected.value])
    }
    toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
    cancelReplace()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در تعویض بازیکن')
  } finally {
    replacing.value = false
  }
}

async function replaceWithGlobal(player: ClubPlayerDto) {
  if (replaceTargetId.value == null || !props.playId) return
  replacing.value = true
  try {
    const fd = new FormData()
    fd.append('name', player.name)
    fd.append('mobile', player.mobile)
    await ClubPlayerApi.createOrJoin(props.clubId, fd)

    const currentEntry = selected.value.find(p => p.player.id === replaceTargetId.value)
    await ClubPlayApi.replaceParticipant(props.clubId, props.playId, replaceTargetId.value, {
      newClubPlayerId: player.id,
      isGuest: currentEntry?.isGuest ?? false,
    })
    const idx = selected.value.findIndex(p => p.player.id === replaceTargetId.value)
    if (idx !== -1) {
      selected.value[idx] = { player, isGuest: currentEntry?.isGuest ?? false }
      emit('change', [...selected.value])
    }
    toastSuccess('بازیکن با موفقیت تعویض شد. نقش‌ها تغییری نکرد.')
    cancelReplace()
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در تعویض بازیکن')
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
    selected.value = [...props.initialSelected]
    emit('change', selected.value)
  }
})
onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  clearTimeout(debounceTimer)
})
</script>
