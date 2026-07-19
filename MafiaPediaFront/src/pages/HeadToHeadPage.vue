<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-fg">مقایسه دو بازیکن</h1>

    <div class="bg-surface rounded-[10px] border border-border p-4 mb-8">
      <div class="flex items-end gap-4 flex-wrap">
        <div class="flex-1 min-w-[200px]">
          <label class="text-sm text-muted mb-1 block">بازیکن اول</label>
          <PlayerSearchAutocomplete filter-mode @select="onSelectPlayer1" />
          <p v-if="selectedPlayer1" class="text-sm text-gold-text mt-1">{{ selectedPlayer1.name }}</p>
        </div>
        <div class="text-2xl text-muted pb-2">⚔️</div>
        <div class="flex-1 min-w-[200px]">
          <label class="text-sm text-muted mb-1 block">بازیکن دوم</label>
          <PlayerSearchAutocomplete filter-mode @select="onSelectPlayer2" />
          <p v-if="selectedPlayer2" class="text-sm text-gold-text mt-1">{{ selectedPlayer2.name }}</p>
        </div>
        <button
          :disabled="!selectedPlayer1 || !selectedPlayer2 || loading"
          class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded font-medium transition"
          @click="compare"
        >
          مقایسه
        </button>
      </div>
    </div>

    <div v-if="error" class="bg-danger/10 border border-danger/20 text-danger text-sm rounded px-4 py-3 mb-6">
      {{ error }}
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-if="result && !loading">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div class="bg-surface rounded-[10px] border border-border p-6 text-center">
          <template v-if="result.player1.picture">
            <img :src="getImageUrl(result.player1.picture)" :alt="result.player1.name" class="w-20 h-20 rounded-full object-cover mx-auto border-[1.5px] border-gold/30" />
          </template>
          <div v-else class="w-20 h-20 rounded-full bg-surface flex items-center justify-center mx-auto border-[1.5px] border-gold/30">
            <span class="text-2xl text-gold-text font-bold">{{ result.player1.name.charAt(0) }}</span>
          </div>
          <router-link :to="`/player/${result.player1.id}`" class="text-gold-text hover:underline font-medium mt-3 block">
            {{ result.player1.name }}
          </router-link>
          <p class="text-muted text-sm mt-1">{{ formatPercent(result.player1WinRate) }}</p>
        </div>

        <div class="bg-surface rounded-[10px] border border-border p-6 text-center flex flex-col items-center justify-center">
          <span class="text-3xl">⚔️</span>
          <span class="text-4xl font-bold text-fg mt-2">{{ result.totalSharedPlays }}</span>
          <span class="text-muted text-sm">بازی مشترک</span>
        </div>

        <div class="bg-surface rounded-[10px] border border-border p-6 text-center">
          <template v-if="result.player2.picture">
            <img :src="getImageUrl(result.player2.picture)" :alt="result.player2.name" class="w-20 h-20 rounded-full object-cover mx-auto border-[1.5px] border-gold/30" />
          </template>
          <div v-else class="w-20 h-20 rounded-full bg-surface flex items-center justify-center mx-auto border-[1.5px] border-gold/30">
            <span class="text-2xl text-gold-text font-bold">{{ result.player2.name.charAt(0) }}</span>
          </div>
          <router-link :to="`/player/${result.player2.id}`" class="text-gold-text hover:underline font-medium mt-3 block">
            {{ result.player2.name }}
          </router-link>
          <p class="text-muted text-sm mt-1">{{ formatPercent(result.player2WinRate) }}</p>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div class="bg-surface rounded-[10px] border border-border p-6">
          <h3 class="text-lg font-medium text-fg mb-3">ساید مخالف هم</h3>
          <p class="text-muted text-sm mb-2">{{ result.oppositeSides.count }} بازی</p>
          <div class="space-y-1 text-sm">
            <p><span class="text-success">{{ result.player1.name }}</span> <span class="text-muted">{{ result.oppositeSides.player1Wins }} برد</span></p>
            <p><span class="text-danger">{{ result.player2.name }}</span> <span class="text-muted">{{ result.oppositeSides.player2Wins }} برد</span></p>
            <p v-if="result.oppositeSides.draws" class="text-muted">{{ result.oppositeSides.draws }} مساوی</p>
          </div>
        </div>

        <div class="bg-surface rounded-[10px] border border-border p-6">
          <h3 class="text-lg font-medium text-fg mb-3">هر دو مافیا</h3>
          <p class="text-muted text-sm mb-2">{{ result.sameSideMafia.count }} بازی</p>
          <div class="space-y-1 text-sm">
            <p><span class="text-success">برد</span> <span class="text-muted">{{ result.sameSideMafia.wins }}</span></p>
            <p><span class="text-danger">باخت</span> <span class="text-muted">{{ result.sameSideMafia.losses }}</span></p>
          </div>
        </div>

        <div class="bg-surface rounded-[10px] border border-border p-6">
          <h3 class="text-lg font-medium text-fg mb-3">هر دو شهروند</h3>
          <p class="text-muted text-sm mb-2">{{ result.sameSideCitizen.count }} بازی</p>
          <div class="space-y-1 text-sm">
            <p><span class="text-success">برد</span> <span class="text-muted">{{ result.sameSideCitizen.wins }}</span></p>
            <p><span class="text-danger">باخت</span> <span class="text-muted">{{ result.sameSideCitizen.losses }}</span></p>
          </div>
        </div>
      </div>

      <div class="bg-surface rounded-[10px] border border-border overflow-hidden">
        <div v-if="result.sharedPlays.length === 0" class="text-center py-12 text-muted">
          بازی مشترکی یافت نشد
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-border bg-surface-hover">
                <th class="px-4 py-3 text-muted text-center whitespace-nowrap">ردیف</th>
                <th class="px-4 py-3 text-muted text-right whitespace-nowrap">عنوان بازی</th>
                <th class="px-4 py-3 text-muted text-center whitespace-nowrap">تاریخ</th>
                <th class="px-4 py-3 text-muted text-center whitespace-nowrap">ساید {{ result.player1.name }}</th>
                <th class="px-4 py-3 text-muted text-center whitespace-nowrap">ساید {{ result.player2.name }}</th>
                <th class="px-4 py-3 text-muted text-center whitespace-nowrap">برنده</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(play, index) in result.sharedPlays" :key="play.playId" class="border-b border-border hover:bg-surface-hover transition">
                <td class="px-4 py-3 text-center text-fg">{{ index + 1 }}</td>
                <td class="px-4 py-3">
                  <router-link :to="`/plays/${play.playId}`" class="text-gold-text hover:underline">
                    {{ play.title }}
                  </router-link>
                </td>
                <td class="px-4 py-3 text-center text-muted">{{ formatDate(play.dateTime) }}</td>
                <td class="px-4 py-3 text-center">
                  <span
                    class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium"
                    :class="play.player1Side === 'Mafia' ? 'bg-danger/20 text-danger' : 'bg-[rgba(50,120,180,0.2)] text-gold-text'"
                  >
                    <span v-if="play.player1Won" class="text-success text-sm">✅</span>
                    {{ play.player1Side === 'Mafia' ? 'مافیا' : 'شهروند' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-center">
                  <span
                    class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium"
                    :class="play.player2Side === 'Mafia' ? 'bg-danger/20 text-danger' : 'bg-[rgba(50,120,180,0.2)] text-gold-text'"
                  >
                    <span v-if="play.player2Won" class="text-success text-sm">✅</span>
                    {{ play.player2Side === 'Mafia' ? 'مافیا' : 'شهروند' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-center">
                  <span
                    v-if="play.winnerSide"
                    class="px-2 py-0.5 rounded text-xs font-medium"
                    :class="play.winnerSide === 'Mafia' ? 'text-danger' : 'text-gold-text'"
                  >
                    {{ play.winnerSide === 'Mafia' ? 'مافیا' : 'شهروند' }}
                  </span>
                  <span v-else class="text-muted">-</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { PlayerApi } from '@/api'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'
import type { PlayerSearchResult, HeadToHeadDto } from '@/types'

const router = useRouter()
const route = useRoute()

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api'

const selectedPlayer1 = ref<PlayerSearchResult | null>(null)
const selectedPlayer2 = ref<PlayerSearchResult | null>(null)
const result = ref<HeadToHeadDto | null>(null)
const loading = ref(false)
const error = ref('')

function onSelectPlayer1(player: PlayerSearchResult) {
  selectedPlayer1.value = player
  error.value = ''
}

function onSelectPlayer2(player: PlayerSearchResult) {
  selectedPlayer2.value = player
  error.value = ''
}

async function compare() {
  if (!selectedPlayer1.value || !selectedPlayer2.value) return
  router.replace({ query: { p1: selectedPlayer1.value.id, p2: selectedPlayer2.value.id } })
  await fetchComparison()
}

async function fetchComparison() {
  if (!selectedPlayer1.value || !selectedPlayer2.value) return
  loading.value = true
  error.value = ''
  result.value = null
  try {
    const res = await PlayerApi.getHeadToHead(selectedPlayer1.value.id, selectedPlayer2.value.id)
    result.value = res.data
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 404) {
        error.value = 'بازیکن مورد نظر یافت نشد'
      } else if (err.response?.status === 400) {
        error.value = err.response.data?.message || 'خطا در درخواست'
      } else {
        error.value = 'خطا در برقراری ارتباط'
      }
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}

function getImageUrl(picture: string | undefined): string {
  if (!picture) return ''
  if (picture.startsWith('http')) return picture
  const base = baseUrl.replace(/\/api$/, '')
  return `${base}${picture.startsWith('/') ? '' : '/'}${picture}`
}

function formatPercent(value: number): string {
  return `${(value * 100).toFixed(1)}%`
}

function formatDate(dateStr: string): string {
  const d = new Date(dateStr)
  return d.toLocaleDateString('fa-IR')
}

onMounted(async () => {
  const p1 = Number(route.query.p1)
  const p2 = Number(route.query.p2)
  if (p1 && p2) {
    selectedPlayer1.value = { id: p1, name: '', totalGames: 0, picture: null }
    selectedPlayer2.value = { id: p2, name: '', totalGames: 0, picture: null }
    await fetchComparison()
  }
})
</script>
