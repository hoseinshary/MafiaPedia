<template>
  <div class="relative" ref="containerRef">
    <input
      type="text"
      placeholder="Search players..."
      v-model="query"
      @input="onInput"
      @focus="onFocus"
      @keydown.prevent.up="onArrowUp"
      @keydown.prevent.down="onArrowDown"
      @keydown.prevent.enter="onEnter"
      @keydown.prevent.escape="onEscape"
      class="bg-gray-800 text-white rounded px-3 py-1 text-sm w-60 placeholder-gray-400 outline-none focus:ring-1 focus:ring-gray-500"
    />
    <div
      v-if="loading"
      class="absolute left-0 right-0 top-full mt-1 bg-gray-800 border border-gray-700 rounded shadow-lg z-50 flex items-center justify-center py-3"
    >
      <div class="w-5 h-5 border-2 border-gray-400 border-t-white rounded-full animate-spin" />
    </div>
    <div
      v-else-if="query.length >= 2 && results.length > 0"
      class="absolute left-0 right-0 top-full mt-1 bg-gray-800 border border-gray-700 rounded shadow-lg z-50 overflow-hidden"
    >
      <div
        v-for="(player, index) in results"
        :key="player.id"
        @click="selectPlayer(player)"
        @mouseenter="highlightedIndex = index"
        class="flex items-center gap-3 px-3 py-2 cursor-pointer transition"
        :class="index === highlightedIndex ? 'bg-gray-700' : 'hover:bg-gray-700'"
      >
        <img
          :src="player.picture || defaultAvatar"
          alt=""
          class="w-8 h-8 rounded-full object-cover bg-gray-600"
        />
        <div class="flex-1 min-w-0">
          <p class="text-sm text-white truncate">{{ player.name }}</p>
          <p class="text-xs text-gray-400">{{ player.totalGames }} games</p>
        </div>
      </div>
    </div>
    <div
      v-else-if="query.length >= 2 && !loading && results.length === 0"
      class="absolute left-0 right-0 top-full mt-1 bg-gray-800 border border-gray-700 rounded shadow-lg z-50 flex items-center justify-center py-3"
    >
      <span class="text-sm text-gray-400">No players found</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { PlayerApi } from '@/api'
import type { PlayerSearchResult } from '@/types'

const props = withDefaults(defineProps<{
  filterMode?: boolean
}>(), {
  filterMode: false,
})

const emit = defineEmits<{
  select: [player: PlayerSearchResult]
}>()

const router = useRouter()

const query = ref('')
const results = ref<PlayerSearchResult[]>([])
const loading = ref(false)
const highlightedIndex = ref(-1)
const containerRef = ref<HTMLElement | null>(null)
const defaultAvatar = 'data:image/svg+xml,' + encodeURIComponent('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" fill="%236b7280"><rect width="100" height="100" rx="50"/><text x="50" y="58" text-anchor="middle" font-size="40" fill="%23d1d5db" font-family="Arial">?</text></svg>')

let debounceTimer: ReturnType<typeof setTimeout>

function onInput() {
  clearTimeout(debounceTimer)
  highlightedIndex.value = -1
  if (query.value.length < 2) {
    results.value = []
    return
  }
  debounceTimer = setTimeout(async () => {
    loading.value = true
    try {
      const res = await PlayerApi.searchPlayers(query.value)
      results.value = res.data.slice(0, 10)
    } catch {
      results.value = []
    } finally {
      loading.value = false
    }
  }, 300)
}

function onFocus() {
  if (results.value.length > 0) return
  if (query.value.length >= 2) onInput()
}

function selectPlayer(player: PlayerSearchResult) {
  results.value = []
  query.value = ''
  if (props.filterMode) {
    emit('select', player)
    return
  }
  router.push(`/player/${player.id}`)
}

function onArrowDown() {
  if (results.value.length === 0) return
  highlightedIndex.value = (highlightedIndex.value + 1) % results.value.length
}

function onArrowUp() {
  if (results.value.length === 0) return
  highlightedIndex.value = highlightedIndex.value <= 0 ? results.value.length - 1 : highlightedIndex.value - 1
}

function onEnter() {
  if (highlightedIndex.value >= 0 && highlightedIndex.value < results.value.length) {
    selectPlayer(results.value[highlightedIndex.value])
  }
}

function onEscape() {
  results.value = []
  query.value = ''
}

function handleClickOutside(e: MouseEvent) {
  if (containerRef.value && !containerRef.value.contains(e.target as Node)) {
    results.value = []
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  clearTimeout(debounceTimer)
})
</script>
