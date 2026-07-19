<template>
  <div dir="rtl" class="max-w-6xl mx-auto w-full">
    <div class="flex items-center justify-between mb-4 flex-wrap gap-2">
      <div>
        <router-link to="/finance/today" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت</router-link>
        <h1 class="text-lg font-bold text-fg mt-1">آرشیو فاکتورها</h1>
      </div>
    </div>

    <div class="bg-surface border border-border rounded-xl p-4 mb-4 space-y-4">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1">
          <label class="text-xs text-muted block mb-1">جستجوی مشتری</label>
          <input
            v-model="query"
            type="text"
            placeholder="نام یا موبایل..."
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            @input="onSearchInput"
          />
        </div>
        <div>
          <label class="text-xs text-muted block mb-1">از تاریخ</label>
          <Vue3PersianDatetimePicker
            v-model="fromDate"
            format="YYYY-MM-DD"
            inputFormat="YYYY-MM-DD"
            displayFormat="jYYYY/jMM/jDD"
            type="date"
            :clearable="true"
            :color="'#c9b07a'"
            placeholder="از تاریخ"
            @update:model-value="onFilterChange"
          />
        </div>
        <div>
          <label class="text-xs text-muted block mb-1">تا تاریخ</label>
          <Vue3PersianDatetimePicker
            v-model="toDate"
            format="YYYY-MM-DD"
            inputFormat="YYYY-MM-DD"
            displayFormat="jYYYY/jMM/jDD"
            type="date"
            :clearable="true"
            :color="'#c9b07a'"
            placeholder="تا تاریخ"
            @update:model-value="onFilterChange"
          />
        </div>
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center py-12">
      <span class="inline-block w-5 h-5 border-2 border-gold border-t-transparent rounded-full animate-spin"></span>
    </div>

    <div v-else class="bg-surface border border-border rounded-xl overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted text-xs">
              <th class="text-right px-4 py-3 font-medium">نام</th>
              <th class="text-right px-4 py-3 font-medium">موبایل</th>
              <th class="text-center px-4 py-3 font-medium">تاریخ</th>
              <th class="text-center px-4 py-3 font-medium">آیتم‌ها</th>
              <th class="text-center px-4 py-3 font-medium">جمع</th>
              <th class="text-center px-4 py-3 font-medium">وضعیت</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in results"
              :key="row.orderId ?? `${row.clubPlayerId}-${row.businessDate}`"
              @click="goToOrder(row)"
              class="border-b border-border last:border-0 hover:bg-surface-hover/50 transition cursor-pointer"
            >
              <td class="px-4 py-3 text-fg font-medium">{{ row.clubPlayerName }}</td>
              <td class="px-4 py-3 text-muted text-xs">{{ row.clubPlayerMobile }}</td>
              <td class="px-4 py-3 text-center text-fg">{{ toJalali(row.businessDate) }}</td>
              <td class="px-4 py-3 text-center text-muted">{{ row.itemCount }}</td>
              <td class="px-4 py-3 text-center dark:text-gold text-gold-text font-bold">{{ row.total.toLocaleString() }}</td>
              <td class="px-4 py-3 text-center">
                <span class="text-xs px-2 py-0.5 rounded" :class="statusBadge(row.status)">{{ statusLabel(row.status) }}</span>
              </td>
            </tr>
            <tr v-if="results.length === 0">
              <td colspan="6" class="text-center py-12 text-muted text-xs">هیچ فاکتوری یافت نشد</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import Vue3PersianDatetimePicker from 'vue3-persian-datetime-picker'
import type { ClubOrderListItemDto } from '@/types/finance'

const router = useRouter()
const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!

const query = ref('')
const fromDate = ref('')
const toDate = ref('')
const results = ref<ClubOrderListItemDto[]>([])
const loading = ref(false)

let searchDebounce: ReturnType<typeof setTimeout>

function onSearchInput() {
  clearTimeout(searchDebounce)
  searchDebounce = setTimeout(loadResults, 300)
}

function onFilterChange() {
  loadResults()
}

function toJalali(iso: string): string {
  if (!iso) return ''
  const d = new Date(iso + 'T00:00:00')
  return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'short', day: 'numeric' })
}

function statusBadge(status: string): string {
  if (status === 'settled') return 'bg-success/20 text-success'
  if (status === 'partial') return 'bg-[rgba(255,165,0,0.2)] text-[#ffa500]'
  if (status === 'game_only') return 'bg-info/10 text-info'
  return 'bg-gold/10 text-gold-text'
}

function statusLabel(status: string): string {
  if (status === 'settled') return 'تسویه شده'
  if (status === 'partial') return 'تسویه جزئی'
  if (status === 'game_only') return 'فقط بازی — بدون سفارش'
  return 'باز'
}

function goToOrder(row: ClubOrderListItemDto) {
  if (row.orderId) {
    router.push(`/finance/order/${row.clubPlayerId}/${row.orderId}`)
  } else {
    router.push({ path: `/finance/order/${row.clubPlayerId}`, query: { date: row.businessDate } })
  }
}

async function loadResults() {
  if (!clubId()) return
  loading.value = true
  try {
    const params: { query?: string; fromDate?: string; toDate?: string } = {}
    if (query.value.trim()) params.query = query.value.trim()
    if (fromDate.value) params.fromDate = fromDate.value
    if (toDate.value) params.toDate = toDate.value
    const res = await FinanceApi.searchOrders(clubId(), params)
    results.value = res.data
  } catch {
    results.value = []
  } finally {
    loading.value = false
  }
}

loadResults()
</script>

<style scoped>
:deep(.vpd-main) {
  --vpd-bg: var(--color-surface);
  --vpd-header-bg: #c9b07a;
  --vpd-header-color: #0d0d0f;
  --vpd-cell-hover: rgba(201, 176, 122, 0.15);
  --vpd-selected-bg: #c9b07a;
  --vpd-selected-color: #0d0d0f;
  --vpd-today-border: #c9b07a;
  --vpd-text-color: var(--color-fg);
  --vpd-muted-color: var(--color-muted);
  --vpd-border-color: var(--color-border);
  --vpd-nav-color: var(--color-fg);
}

:deep(.vpd-input-group) {
  direction: ltr;
}

:deep(.vpd-input-group input) {
  background: var(--color-input) !important;
  border: 0.5px solid var(--color-border) !important;
  border-radius: 6px !important;
  padding: 6px 10px !important;
  font-size: 13px !important;
  color: var(--color-fg) !important;
  font-family: inherit !important;
  width: 160px !important;
}

:deep(.vpd-input-group input:focus) {
  border-color: #c9b07a !important;
  outline: none !important;
}

:deep(.vpd-icon-btn) {
  background: transparent !important;
  color: var(--color-muted) !important;
}

:deep(.vpd-icon-btn svg) {
  fill: var(--color-muted);
  width: 16px;
  height: 16px;
}

:deep(.vpd-wrapper) {
  z-index: 100;
}

:deep(.vpd-container) {
  background: var(--color-surface) !important;
  border: 0.5px solid var(--color-border) !important;
  border-radius: 10px !important;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5) !important;
}

:deep(.vpd-header) {
  background: #c9b07a !important;
  color: #0d0d0f !important;
  border-radius: 10px 10px 0 0 !important;
}

:deep(.vpd-year-label),
:deep(.vpd-date-label) {
  color: #0d0d0f !important;
}

:deep(.vpd-cell) {
  color: var(--color-fg) !important;
  border-radius: 6px !important;
}

:deep(.vpd-cell:hover) {
  background: rgba(201, 176, 122, 0.15) !important;
}

:deep(.vpd-selected) {
  background: #c9b07a !important;
  color: #0d0d0f !important;
  font-weight: 700 !important;
}

:deep(.vpd-today) {
  border: 1px solid #c9b07a !important;
}

:deep(.vpd-cell-weekend) {
  color: var(--color-muted) !important;
}

:deep(.vpd-cell-week-name) {
  color: var(--color-muted) !important;
  font-size: 11px !important;
}

:deep(.vpd-nav-btn) {
  color: var(--color-fg) !important;
}

:deep(.vpd-nav-btn:hover) {
  background: rgba(201, 176, 122, 0.15) !important;
  border-radius: 50% !important;
}

:deep(.vpd-footer) {
  border-top: 0.5px solid var(--color-border) !important;
  padding: 8px !important;
}

:deep(.vpd-footer button) {
  background: #c9b07a !important;
  color: #0d0d0f !important;
  border-radius: 6px !important;
  padding: 4px 16px !important;
  font-size: 12px !important;
  font-weight: 700 !important;
  border: none !important;
  cursor: pointer !important;
}

:deep(.vpd-footer button:hover) {
  opacity: 0.88 !important;
}

:deep(.vpd-clear-btn) {
  color: var(--color-muted) !important;
}

:deep(.vpd-disabled) {
  opacity: 0.4 !important;
}
</style>
