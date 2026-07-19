<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-4">
      <router-link to="/finance/today" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت</router-link>
      <h1 class="text-lg font-bold text-fg mt-1">تسویه حساب</h1>
    </div>

    <!-- Player search -->
    <div class="bg-surface border border-border rounded-xl p-4 mb-4">
      <label class="text-sm text-muted mb-2 block">انتخاب مشتری</label>
      <input v-model="query" type="text" placeholder="نام یا موبایل..." class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" @input="onSearchDebounce" />
      <div v-if="searchResults.length > 0 && query.trim().length >= 2" class="mt-1 bg-surface border border-border rounded shadow-lg overflow-hidden">
        <div v-for="player in searchResults" :key="player.id" @click="selectPlayer(player)" class="px-3 py-2.5 text-sm text-fg hover:bg-surface-hover cursor-pointer transition border-b border-border last:border-0">{{ player.name }} - {{ player.mobile }}</div>
      </div>
    </div>

          <!-- Already settled warning -->
      <div v-if="isAlreadySettled" class="bg-[rgba(255,165,0,0.08)] border border-[rgba(255,165,0,0.2)] rounded-lg px-4 py-3 text-sm text-[#ffa500] mb-4 flex items-center gap-3">
        <svg class="w-5 h-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" /></svg>
        <span>حساب امروز این مشتری قبلاً به‌طور کامل تسویه شده است. ثبت پرداخت مجدد باعث مغایرت مالی (بستانکار شدن مشتری) خواهد شد.</span>
      </div>
      
    <template v-if="balance">
      <!-- Player info -->
      <div class="flex items-center justify-between mb-4">
        <div>
          <h2 class="text-base font-bold text-fg">{{ balance.clubPlayerName }}</h2>
        </div>
        <div class="flex gap-4 text-center">
          <div v-if="balance.previousBalance !== 0" class="bg-input rounded-lg px-4 py-2">
            <p class="text-xs text-muted">{{ balance.previousBalance > 0 ? 'بدهی قبلی' : 'بستانکاری قبلی' }}</p>
            <p class="text-sm font-bold" :class="balance.previousBalance > 0 ? 'text-danger' : 'text-success'">{{ Math.abs(balance.previousBalance).toLocaleString() }} تومان</p>
          </div>
          <div class="bg-input rounded-lg px-4 py-2">
            <p class="text-xs text-muted">قابل پرداخت</p>
            <p class="text-sm font-bold text-danger">{{ balance.totalDue.toLocaleString() }} تومان</p>
          </div>
        </div>
      </div>

      <!-- Unified invoice list -->
      <div class="bg-surface border border-border rounded-xl p-4 mb-6">
        <!-- Header row -->
        <div class="grid grid-cols-4 gap-1 sm:gap-2 text-[10px] sm:text-xs text-muted font-medium mb-3 pb-2 border-b border-border">
          <span>عنوان</span>
          <span class="text-center">تعداد</span>
          <span class="text-center">قیمت واحد</span>
          <span class="text-left">مبلغ کل</span>
        </div>

        <!-- Games grouped by nerkh -->
        <template v-if="balance.todayGames.length > 0">
          <div v-for="(group, nerkhName) in groupedGames" :key="'ng-' + nerkhName" class="grid grid-cols-4 gap-1 sm:gap-2 text-xs sm:text-sm py-1.5 border-b border-border/40 last:border-0">
            <span class="text-fg truncate">{{ nerkhName || 'نامشخص' }}</span>
            <span class="text-center text-muted">{{ group.count }}</span>
            <span class="text-center text-muted">{{ group.unitPrice.toLocaleString() }}</span>
            <span class="text-left dark:text-gold text-gold-text font-medium">{{ group.total.toLocaleString() }} تومان</span>
          </div>
        </template>

        <!-- Separator between games and orders -->
        <div v-if="balance.todayGames.length > 0 && balance.todayOrders.length > 0" class="border-t border-border my-1"></div>

        <!-- Orders grouped by product -->
        <template v-if="balance.todayOrders.length > 0">
          <div v-for="(group, productName) in groupedOrders" :key="'op-' + productName" class="grid grid-cols-4 gap-1 sm:gap-2 text-xs sm:text-sm py-1.5 border-b border-border/40 last:border-0">
            <span class="text-fg truncate">{{ productName }}</span>
            <span class="text-center text-muted">{{ group.count }}</span>
            <span class="text-center text-muted">{{ group.unitPrice.toLocaleString() }}</span>
            <span class="text-left dark:text-gold text-gold-text font-medium">{{ group.total.toLocaleString() }} تومان</span>
          </div>
        </template>

        <!-- Empty state -->
        <div v-if="balance.todayGames.length === 0 && balance.todayOrders.length === 0" class="text-center py-6 text-xs text-muted">
          هیچ آیتمی برای امروز ثبت نشده
        </div>
      </div>

      <!-- Totals summary -->
      <div class="bg-surface border border-border rounded-xl p-4 mb-6 print-totals">
        <h3 class="text-sm font-bold text-fg mb-3">خلاصه</h3>
        <div class="space-y-1 text-sm">
          <div class="flex justify-between"><span class="text-muted">بازی‌های امروز</span><span class="text-fg">{{ balance.todayGamesTotal.toLocaleString() }} تومان</span></div>
          <div class="flex justify-between"><span class="text-muted">سفارشات امروز</span><span class="text-fg">{{ balance.todayOrdersTotal.toLocaleString() }} تومان</span></div>
          <div class="flex justify-between border-t border-border/50 pt-1"><span class="text-muted">جمع (بدون مالیات)</span><span class="text-fg">{{ balance.todaySubtotal.toLocaleString() }} تومان</span></div>
          <div v-if="balance.vatPercent && balance.vatPercent > 0" class="flex justify-between">
            <span class="text-muted">مالیات بر ارزش افزوده (٪{{ balance.vatPercent }})</span>
            <span class="text-fg">{{ balance.vatAmount.toLocaleString() }} تومان</span>
          </div>
          <div v-if="balance.previousBalance !== 0" class="flex justify-between"><span class="text-muted">{{ balance.previousBalance > 0 ? 'بدهی قبلی' : 'بستانکاری قبلی' }}</span><span :class="balance.previousBalance > 0 ? 'text-danger' : 'text-success'">{{ Math.abs(balance.previousBalance).toLocaleString() }} تومان</span></div>
          <div class="flex justify-between pt-2 border-t border-border"><span class="text-sm font-bold text-fg">قابل پرداخت</span><span class="text-sm font-bold text-danger">{{ balance.totalDue.toLocaleString() }} تومان</span></div>
        </div>
      </div>



      <!-- Payment section -->
      <div ref="paymentFormRef" class="bg-surface border border-border rounded-xl p-4 mb-6">
        <h3 class="text-sm font-bold text-fg mb-3">پرداخت</h3>

        <!-- Single settle today button -->
        <button v-if="balance.todayDue > 0" @click="settleToday" class="w-full px-3 py-2.5 bg-gold/10 text-gold-text text-sm rounded-lg font-medium hover:bg-gold/20 transition min-h-[44px] flex items-center justify-between mb-4">
          <span class="dark:text-gold">تسویه حساب امروز</span>
          <span class="text-xs text-muted">{{ balance.todayDue.toLocaleString() }} تومان</span>
        </button>

        <div class="space-y-3">
          <div>
            <label class="text-xs text-muted">مبلغ (تومان)</label>
            <input v-model.number="settlementAmount" type="number" min="0" placeholder="مبلغ" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition ltr mt-1" />
          </div>
          <div class="flex gap-2 flex-wrap">
            <button v-for="q in quickAmounts" :key="q" @click="settlementAmount = q" class="px-3 py-1.5 text-xs text-muted border border-border rounded hover:text-fg hover:border-gold/40 transition">{{ q.toLocaleString() }}</button>
            <button @click="settlementAmount = balance.totalDue" class="px-3 py-1.5 text-xs dark:text-gold text-gold-text border border-gold/30 rounded hover:bg-gold/10 transition">تسویه کامل</button>
          </div>
          <div>
            <label class="text-xs text-muted">روش پرداخت</label>
            <select v-model="paymentMethod" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition mt-1">
              <option value="pos">دستگاه POS</option>
              <option value="cash">نقدی</option>
              <option value="card">کارت</option>
            </select>
          </div>
          <div>
            <label class="text-xs text-muted">توضیحات (اختیاری)</label>
            <input v-model="settlementNote" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition mt-1" />
          </div>
          <p v-if="settlementError" class="text-sm text-danger">{{ settlementError }}</p>
          <button @click="handleGeneralSettle" :disabled="!settlementAmount || settlementAmount <= 0 || settleSubmitting" class="w-full px-4 py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition min-h-[44px] inline-flex items-center justify-center gap-2">
            <div v-if="settleSubmitting" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            ثبت تسویه
          </button>
        </div>
      </div>

      <!-- Ledger link -->
      <div class="text-center mb-6">
        <router-link :to="`/finance/ledger/${selectedPlayerBalanceId}`" class="dark:text-gold text-xs  text-gold-text hover:opacity-80 transition">مشاهده گردش حساب</router-link>
      </div>

      <!-- Recent settlements -->
      <div v-if="recentSettlements.length > 0" class="bg-surface border border-border rounded-xl p-4">
        <h3 class="text-sm font-bold text-fg mb-3">تسویه‌های امروز</h3>
        <div v-for="s in recentSettlements" :key="s.id" class="flex items-center justify-between py-1.5 text-sm border-b border-border last:border-0">
          <div class="flex items-center gap-2">
            <span class="text-xs px-2 py-0.5 rounded bg-success/20 text-success">{{ s.amount.toLocaleString() }}</span>
            <span class="text-xs text-muted">{{ paymentLabel(s.paymentMethod) }}</span>
            <span v-if="s.note" class="text-xs text-muted">- {{ s.note }}</span>
          </div>
          <span class="text-xs text-muted">{{ s.createdAt ? s.createdAt.slice(11, 19) : '' }}</span>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { FinanceApi, ClubPlayerApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import { useToast } from '@/composables/useToast'
import type { ClubPlayerBalanceDto, SettlementDto } from '@/types/finance'
import type { ClubPlayerDto } from '@/types/clubPlayer'
import { getBusinessDateStr } from '@/utils/businessDate'

const authStore = useAuthStore()
const route = useRoute()
const clubId = () => authStore.activeClubId!
const { toastSuccess } = useToast()

const query = ref('')
const searchResults = ref<ClubPlayerDto[]>([])
const balance = ref<ClubPlayerBalanceDto | null>(null)
const selectedPlayerBalanceId = ref<number | null>(null)
const recentSettlements = ref<SettlementDto[]>([])

const settlementAmount = ref(0)
const paymentMethod = ref('pos')
const settlementNote = ref('')
const settleSubmitting = ref(false)
const settlementError = ref('')
const paymentFormRef = ref<HTMLDivElement | null>(null)
const isAlreadySettled = ref(false)

const quickAmounts = [50000, 100000, 200000, 500000]

let searchDebounce: ReturnType<typeof setTimeout>

// Group today's games by nerkhName
const groupedGames = computed(() => {
  if (!balance.value) return {}
  const map: Record<string, { count: number; total: number; unitPrice: number }> = {}
  for (const g of balance.value.todayGames) {
    const key = g.nerkhName || 'نامشخص'
    if (!map[key]) map[key] = { count: 0, total: 0, unitPrice: g.price }
    map[key].count++
    map[key].total += g.price
  }
  return map
})

// Group today's orders by productName (across all orders)
const groupedOrders = computed(() => {
  if (!balance.value) return {}
  const map: Record<string, { count: number; total: number; unitPrice: number }> = {}
  for (const o of balance.value.todayOrders) {
    for (const item of o.items) {
      const key = item.productName
      if (!map[key]) map[key] = { count: 0, total: 0, unitPrice: item.unitPrice }
      map[key].count += item.quantity
      map[key].total += item.lineTotal
    }
  }
  return map
})

function paymentLabel(m: string): string {
  const labels: Record<string, string> = { cash: 'نقدی', card: 'کارت', pos: 'POS', online: 'آنلاین' }
  return labels[m] || m
}

onMounted(async () => {
  const playerId = route.params.clubPlayerId ? Number(route.params.clubPlayerId) : null
  if (playerId && authStore.activeClubId) {
    try {
      const p = (await ClubPlayerApi.getClubPlayerDetail(clubId(), playerId)).data
      await selectPlayer(p)
    } catch { /* player not found → keep search visible */ }
  }
})

function onSearchDebounce() {
  clearTimeout(searchDebounce)
  if (query.value.trim().length < 2) { searchResults.value = []; return }
  searchDebounce = setTimeout(async () => {
    try {
      const res = await ClubPlayerApi.searchAllCustomers(clubId(), query.value)
      searchResults.value = res.data.inClub
    } catch { searchResults.value = [] }
  }, 300)
}

async function selectPlayer(player: ClubPlayerDto) {
  selectedPlayerBalanceId.value = player.id
  query.value = ''
  searchResults.value = []
  settlementAmount.value = 0
  settlementError.value = ''
  recentSettlements.value = []
  isAlreadySettled.value = false
  try {
    const today = getBusinessDateStr()
    const [balRes, ledgerRes, overviewRes] = await Promise.all([
      FinanceApi.getBalance(clubId(), player.id),
      FinanceApi.getLedger(clubId(), player.id),
      FinanceApi.getTodayOverview(clubId(), 'all', today),
    ])
    balance.value = balRes.data
    settlementAmount.value = balRes.data.totalDue
    // Check if already settled today
    const overview = overviewRes.data.find(o => o.clubPlayerId === player.id)
    if (overview && overview.status === 'settled') {
      isAlreadySettled.value = true
    }
    // Extract today's settlements from ledger
    recentSettlements.value = ledgerRes.data.entries
      .filter(e => e.entryType === 'settlement' && e.businessDate === today)
      .map(e => ({
        id: e.relatedId || 0,
        clubPlayerId: player.id,
        amount: e.amount,
        paymentMethod: e.description?.split(' - ')[0] || 'cash',
        note: e.description || null,
        createdAt: e.dateTime,
        createdByUserId: 0,
        createdByDisplayName: null,
        orderId: null,
      } as SettlementDto))
  } catch { balance.value = null }
}

function settleToday() {
  if (!balance.value) return
  settlementAmount.value = balance.value.todayDue
  paymentFormRef.value?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

async function handleGeneralSettle() {
  if (!settlementAmount.value || settlementAmount.value <= 0 || !selectedPlayerBalanceId.value) return
  settleSubmitting.value = true
  settlementError.value = ''
  try {
    await FinanceApi.createSettlement(clubId(), {
      clubPlayerId: selectedPlayerBalanceId.value,
      amount: settlementAmount.value,
      paymentMethod: paymentMethod.value,
      note: settlementNote.value || undefined,
    })
    toastSuccess('تسویه ثبت شد')
    settlementAmount.value = 0
    settlementNote.value = ''
    await refreshBalance()
  } catch (err: any) {
    settlementError.value = err.response?.data?.message || 'خطا در ثبت تسویه'
  } finally {
    settleSubmitting.value = false
  }
}

async function refreshBalance() {
  if (!selectedPlayerBalanceId.value) return
  try {
    const res = await FinanceApi.getBalance(clubId(), selectedPlayerBalanceId.value)
    balance.value = res.data
  } catch { /* ignore */ }
}
</script>

<style scoped>
@media print {
  .print-totals { break-inside: avoid; }
}
</style>
