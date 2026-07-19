<template>
  <div dir="rtl" class="max-w-6xl mx-auto w-full">
    <div class="flex items-center justify-between mb-4 flex-wrap gap-2">
      <h1 class="text-lg font-bold text-fg">حساب‌های روز</h1>
      <div class="flex items-center gap-3">
        <button @click="$router.push('/finance/order')" class="px-3 py-2 bg-gold text-[#0d0d0f] text-xs rounded-lg font-bold hover:opacity-80 transition min-h-[36px]">+ ثبت سفارش جدید</button>
      </div>
    </div>

    <!-- Date navigation -->
    <div class="bg-surface border border-border rounded-xl p-3 mb-4 flex items-center justify-between flex-wrap gap-3">
      <div class="flex items-center gap-2">
        <button @click="prevDay" class="px-3 py-1.5 bg-input border border-border text-muted hover:text-fg text-sm rounded transition">&larr; روز قبل</button>
        <span class="text-sm font-bold text-fg min-w-[140px] text-center">{{ persianDate }}</span>
        <button @click="nextDay" :disabled="isNextDayDisabled" class="px-3 py-1.5 bg-input border border-border text-muted hover:text-fg disabled:opacity-30 disabled:cursor-not-allowed text-sm rounded transition">روز بعد &rarr;</button>
      </div>
      <div class="flex items-center gap-2">
        <label class="text-xs text-muted">انتخاب تاریخ:</label>
        <Vue3PersianDatetimePicker
          v-model="selectedDate"
          format="YYYY-MM-DD"
          inputFormat="YYYY-MM-DD"
          displayFormat="jYYYY/jMM/jDD"
          type="date"
          :clearable="false"
          :color="'#c9b07a'"
          placeholder="انتخاب تاریخ"
          @update:model-value="onDatePicked"
        />
      </div>
    </div>

    <div class="flex flex-wrap gap-2 mb-4">
      <button v-for="tab in tabs" :key="tab.value" @click="selectedTab = tab.value" class="px-4 py-2 text-sm rounded-lg font-medium transition min-h-[36px]" :class="selectedTab === tab.value ? 'bg-gold text-[#0d0d0f]' : 'bg-input text-muted hover:text-fg border border-border'">{{ tab.label }}</button>
    </div>

    <div class="bg-surface border border-border rounded-xl overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted text-xs">
              <th class="text-right px-4 py-3 font-medium">نام</th>
              <th class="text-right px-4 py-3 font-medium">موبایل</th>
              <th class="text-center px-4 py-3 font-medium">بازی‌ها</th>
              <th class="text-center px-4 py-3 font-medium">امروز</th>
              <th class="text-center px-4 py-3 font-medium">بدهی قبلی</th>
              <th class="text-center px-4 py-3 font-medium">پرداخت امروز</th>
              <th class="text-center px-4 py-3 font-medium">وضعیت</th>
              <th class="text-center px-4 py-3 font-medium">عملیات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in filteredRows" :key="row.clubPlayerId" class="border-b border-border last:border-0 hover:bg-surface-hover/50 transition">
              <td class="px-4 py-3 text-fg font-medium">{{ row.name }}</td>
              <td class="px-4 py-3 text-muted text-xs">{{ row.mobile }}</td>
              <td class="px-4 py-3 text-center text-fg">{{ row.gamesCountToday }}</td>
              <td class="px-4 py-3 text-center text-fg">{{ row.todayDue.toLocaleString() }}</td>
              <td class="px-4 py-3 text-center">
                <span v-if="row.previousBalance > 0" class="text-danger">{{ row.previousBalance.toLocaleString() }}</span>
                <span v-else class="text-muted">0</span>
              </td>
              <td class="px-4 py-3 text-center text-success">{{ row.paidToday.toLocaleString() }}</td>
              <td class="px-4 py-3 text-center">
                <span class="text-xs px-2 py-0.5 rounded" :class="statusBadge(row.status)">{{ statusDisplay(row.status) }}</span>
              </td>
              <td class="px-4 py-3 text-center">
                <div class="flex items-center justify-center gap-1">
                  <button @click="goOrder(row.clubPlayerId)" class="px-2.5 py-1.5 bg-gold/10 dark:text-gold text-gold-text text-xs rounded font-medium hover:bg-gold/20 transition">سفارش</button>
                  <button @click="goSettlement(row.clubPlayerId)" class="px-2.5 py-1.5 bg-success/10 text-success text-xs rounded font-medium hover:bg-success/20 transition">تسویه</button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredRows.length === 0">
              <td colspan="8" class="text-center py-8 text-muted text-xs">هیچ موردی یافت نشد</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import Vue3PersianDatetimePicker from 'vue3-persian-datetime-picker'
import type { TodayOverviewDto } from '@/types/finance'
import { getBusinessDateStr } from '@/utils/businessDate'

function toLocalDateStr(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const clubId = () => authStore.activeClubId!
const rows = ref<TodayOverviewDto[]>([])
const selectedTab = ref('all')
const selectedDate = ref('')
const todayDateStr = ref('')

const tabs = [
  { value: 'all', label: 'همه' },
  { value: 'unpaid', label: 'پرداخت نشده' },
  { value: 'partial', label: 'تسویه جزئی' },
  { value: 'settled', label: 'تسویه شده' },
]

const isNextDayDisabled = computed(() => {
  return selectedDate.value >= todayDateStr.value
})

function toPersianDate(iso: string): string {
  const d = new Date(iso + 'T00:00:00')
  return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' })
}

const persianDate = computed(() => {
  if (!selectedDate.value) return ''
  return toPersianDate(selectedDate.value)
})

const filteredRows = computed(() => {
  if (selectedTab.value === 'all') return rows.value
  return rows.value.filter(r => r.status === selectedTab.value)
})

function statusBadge(status: string): string {
  if (status === 'settled') return 'bg-success/20 text-success'
  if (status === 'partial') return 'bg-[rgba(255,165,0,0.2)] text-[#ffa500]'
  return 'bg-danger/20 text-danger'
}
function statusDisplay(status: string): string {
  if (status === 'settled') return 'تسویه شده'
  if (status === 'partial') return 'تسویه جزئی'
  return 'پرداخت نشده'
}

function goOrder(playerId: number) {
  router.push(`/finance/order/${playerId}`)
}
function goSettlement(playerId: number) {
  router.push(`/finance/settlement/${playerId}`)
}

function prevDay() {
  if (!selectedDate.value) return
  const d = new Date(selectedDate.value + 'T00:00:00')
  d.setDate(d.getDate() - 1)
  const iso = toLocalDateStr(d)
  router.push({ query: { ...route.query, date: iso } })
}

function nextDay() {
  if (!selectedDate.value || isNextDayDisabled.value) return
  const d = new Date(selectedDate.value + 'T00:00:00')
  d.setDate(d.getDate() + 1)
  const iso = toLocalDateStr(d)
  router.push({ query: { ...route.query, date: iso } })
}

function onDatePicked(val: string) {
  if (val && val !== route.query.date) router.push({ query: { ...route.query, date: val } })
}

async function load(dateStr: string) {
  if (!authStore.activeClubId) return
  try {
    const res = await FinanceApi.getTodayOverview(clubId(), 'all', dateStr)
    rows.value = res.data
  } catch { /* toast handled */ }
}

onMounted(() => {
  todayDateStr.value = getBusinessDateStr()
  selectedDate.value = (route.query.date as string) || todayDateStr.value
  selectedTab.value = (route.query.status as string) || 'all'
  load(selectedDate.value)
})

watch(() => route.query.date, (newDate) => {
  if (newDate && newDate !== selectedDate.value) {
    selectedDate.value = newDate as string
    load(selectedDate.value)
  }
})

watch(() => route.query.status, (newStatus) => {
  if (newStatus && newStatus !== selectedTab.value) {
    selectedTab.value = newStatus as string
  }
})
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
