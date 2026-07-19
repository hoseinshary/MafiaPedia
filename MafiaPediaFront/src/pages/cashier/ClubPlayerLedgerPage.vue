<template>
  <div dir="rtl" class="max-w-5xl mx-auto w-full">
    <div class="mb-4">
      <router-link to="/finance/today" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت</router-link>
      <h1 class="text-lg font-bold text-fg mt-1">گردش حساب</h1>
    </div>

    <!-- Player search -->
    <div v-if="!selectedPlayer" class="bg-surface border border-border rounded-xl p-4 mb-4">
      <label class="text-sm text-muted mb-2 block">انتخاب مشتری</label>
      <input v-model="searchQuery" type="text" placeholder="نام یا موبایل..." class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" @input="onSearchDebounce" />
      <div v-if="searchResults.length > 0 && searchQuery.trim().length >= 2" class="mt-1 bg-surface border border-border rounded shadow-lg overflow-hidden">
        <div v-for="p in searchResults" :key="p.id" @click="selectCustomer(p)" class="px-3 py-2.5 text-sm text-fg hover:bg-surface-hover cursor-pointer transition border-b border-border last:border-0 flex items-center gap-3">
          <span class="text-fg">{{ p.name }}</span>
          <span class="text-xs text-muted">{{ p.mobile }}</span>
        </div>
      </div>
    </div>

    <!-- Player info header (when selected) -->
    <div v-if="selectedPlayer" class="flex items-center justify-between mb-4 flex-wrap gap-2">
      <div>
        <span class="text-sm font-bold text-fg">{{ selectedPlayer.name }}</span>
        <span class="text-xs text-muted mr-2">{{ selectedPlayer.mobile }}</span>
      </div>
      <button @click="clearSelection" class="text-xs text-muted hover:text-fg transition">تغییر مشتری</button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="ledgerData">
      <!-- Summary header -->
      <div class="grid grid-cols-3 gap-3 mb-6">
        <div class="bg-surface border border-border rounded-xl p-4 text-center">
          <p class="text-xs text-muted mb-1">مجموع بدهکاری</p>
          <p class="text-base font-bold text-danger">{{ ledgerData.totalDebit.toLocaleString() }}</p>
        </div>
        <div class="bg-surface border border-border rounded-xl p-4 text-center">
          <p class="text-xs text-muted mb-1">مجموع پرداختی</p>
          <p class="text-base font-bold text-success">{{ ledgerData.totalCredit.toLocaleString() }}</p>
        </div>
        <div class="bg-surface border border-border rounded-xl p-4 text-center">
          <p class="text-xs text-muted mb-1">مانده نهایی</p>
          <p class="text-base font-bold" :class="ledgerData.balance > 0 ? 'text-danger' : 'text-success'">
            {{ Math.abs(ledgerData.balance).toLocaleString() }}
            <span class="text-xs mr-1">{{ ledgerData.balance > 0 ? 'بدهکار' : ledgerData.balance < 0 ? 'بستانکار' : 'تسویه' }}</span>
          </p>
        </div>
      </div>

      <!-- Two-column T-account layout -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Debit column (right) -->
        <div>
          <div class="flex items-center justify-between mb-2 px-1">
            <h3 class="text-sm font-bold text-fg">بدهکاری</h3>
            <span class="text-xs text-danger">{{ debitEntries.length }} مورد</span>
          </div>
          <div class="space-y-2">
            <div v-for="entry in debitEntries" :key="'d-' + entry.relatedId + '-' + entry.dateTime" class="bg-surface border border-border rounded-xl p-3">
              <div class="flex items-center justify-between mb-1">
                <span class="text-xs text-muted">{{ entryTypeLabel(entry.entryType) }}</span>
                <span class="text-sm font-bold text-danger">{{ entry.amount.toLocaleString() }}</span>
              </div>
              <p v-if="entry.description" class="text-sm text-fg">{{ entry.description }}</p>
              <p class="text-xs text-muted mt-0.5">{{ toJalali(entry.businessDate) || toJalali(entry.dateTime) }}</p>
            </div>
            <div v-if="debitEntries.length === 0" class="text-center py-8 text-muted text-xs">هیچ بدهکاری ثبت نشده</div>
          </div>
        </div>

        <!-- Credit column (left) -->
        <div>
          <div class="flex items-center justify-between mb-2 px-1">
            <h3 class="text-sm font-bold text-fg">بستانکاری</h3>
            <span class="text-xs text-success">{{ creditEntries.length }} مورد</span>
          </div>
          <div class="space-y-2">
            <div v-for="entry in creditEntries" :key="'c-' + entry.relatedId + '-' + entry.dateTime" class="bg-surface border border-border rounded-xl p-3">
              <div class="flex items-center justify-between mb-1">
                <span class="text-xs text-muted">{{ entryTypeLabel(entry.entryType) }}</span>
                <span class="text-sm font-bold text-success">{{ entry.amount.toLocaleString() }}</span>
              </div>
              <p v-if="entry.description" class="text-sm text-fg">{{ entry.description }}</p>
              <p class="text-xs text-muted mt-0.5">{{ toJalali(entry.dateTime) }}</p>
            </div>
            <div v-if="creditEntries.length === 0" class="text-center py-8 text-muted text-xs">هیچ پرداختی ثبت نشده</div>
          </div>
        </div>
      </div>
    </template>

    <div v-else-if="selectedPlayer && !loading" class="text-center py-12 text-muted text-sm">هیچ ورودی برای این مشتری یافت نشد</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { FinanceApi, ClubPlayerApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import { toJalali } from '@/utils/jalaliDate'
import type { ClubPlayerDto } from '@/types/clubPlayer'
import type { LedgerResponseDto } from '@/types/finance'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const clubId = () => authStore.activeClubId!

const searchQuery = ref('')
const searchResults = ref<ClubPlayerDto[]>([])
const selectedPlayer = ref<ClubPlayerDto | null>(null)
const ledgerData = ref<LedgerResponseDto | null>(null)
const loading = ref(false)

let searchDebounce: ReturnType<typeof setTimeout>

const debitEntries = computed(() =>
  ledgerData.value?.entries.filter(e => e.entryType !== 'settlement') ?? []
)
const creditEntries = computed(() =>
  ledgerData.value?.entries.filter(e => e.entryType === 'settlement') ?? []
)

function entryTypeLabel(type: string): string {
  const map: Record<string, string> = { game: 'بازی', order: 'سفارش', settlement: 'پرداخت' }
  return map[type] || type
}

onMounted(async () => {
  const playerId = route.params.playerId ? Number(route.params.playerId) : null
  if (playerId && authStore.activeClubId) {
    try {
      const p = (await ClubPlayerApi.getClubPlayerDetail(clubId(), playerId)).data
      selectedPlayer.value = p
      await loadLedger()
    } catch { /* keep search visible */ }
  }
})

function onSearchDebounce() {
  clearTimeout(searchDebounce)
  if (searchQuery.value.trim().length < 2) { searchResults.value = []; return }
  searchDebounce = setTimeout(async () => {
    try {
      const res = await ClubPlayerApi.searchAllCustomers(clubId(), searchQuery.value)
      searchResults.value = res.data.inClub
    } catch { searchResults.value = [] }
  }, 300)
}

async function selectCustomer(player: ClubPlayerDto) {
  selectedPlayer.value = player
  searchQuery.value = ''
  searchResults.value = []
  router.push({ query: { ...route.query, playerId: player.id } })
  await loadLedger()
}

function clearSelection() {
  selectedPlayer.value = null
  ledgerData.value = null
  router.push({ query: {} })
}

async function loadLedger() {
  if (!selectedPlayer.value) return
  loading.value = true
  try {
    const res = await FinanceApi.getLedger(clubId(), selectedPlayer.value.id)
    ledgerData.value = res.data
  } catch { ledgerData.value = null }
  finally { loading.value = false }
}
</script>
