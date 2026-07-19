<template>
  <div dir="rtl" class="max-w-6xl mx-auto w-full">
    <div class="mb-4 flex items-center justify-between">
      <div>
        <router-link to="/finance/today" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت</router-link>
        <h1 class="text-lg font-bold text-fg mt-1">ثبت سفارش</h1>
      </div>
      <div v-if="selectedPlayer" class="flex items-center gap-2">
        <span class="text-sm text-fg font-medium">{{ selectedPlayer.name }}</span>
        <span class="text-xs text-muted">{{ selectedPlayer.mobile }}</span>
      </div>
    </div>

    <!-- Player search (if none selected via route) -->
    <div v-if="!selectedPlayer" class="bg-surface border border-border rounded-xl p-4 mb-6">
      <button @click="showCustomerSearch = true" class="w-full px-4 py-2.5 bg-input border border-border rounded text-sm text-muted hover:text-fg hover:border-gold/40 transition text-left flex items-center gap-2">
        <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        نام یا موبایل مشتری...
      </button>
    </div>

    <!-- Existing open order warning -->
    <div v-if="selectedPlayer && openOrders.length > 0 && !editingOrderId && !orderChoiceResolved" class="bg-[rgba(255,165,0,0.08)] border border-[rgba(255,165,0,0.2)] rounded-lg px-4 py-3 text-sm text-[#ffa500] mb-4 flex items-center gap-3">
      <span class="flex-1">این مشتری {{ openOrders.length }} فاکتور باز دارد (جمع {{ openOrdersTotal.toLocaleString() }} تومان).</span>
      <button @click="chooseExistingOrder" class="px-3 py-1.5 bg-gold/20 text-gold-text rounded text-xs font-medium hover:opacity-80 transition">استفاده از همین</button>
      <button @click="chooseNewOrder" class="px-3 py-1.5 border border-border text-muted rounded text-xs hover:text-fg transition">فاکتور جدید</button>
    </div>

    <!-- Past date warning — no new orders allowed -->
    <div v-if="isPastDateWithoutOrder" class="bg-[rgba(201,176,122,0.06)] border border-gold/30 rounded-lg px-4 py-3 text-sm text-gold-text mb-4 flex items-center gap-3">
      <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
      </svg>
      <span class="flex-1">این روز گذشته است — امکان ثبت سفارش جدید روی این تاریخ وجود ندارد</span>
    </div>



    <template v-if="selectedPlayer">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <!-- Product categories and products (left two thirds) -->
        <div class="lg:col-span-2 space-y-4">
          <!-- Category buttons -->
          <div class="bg-surface border border-border rounded-xl p-3">
            <div class="flex flex-wrap gap-2">
              <button @click="selectedCategory = null" class="px-4 py-2.5 text-sm rounded-lg font-medium transition min-h-[44px]" :class="selectedCategory === null ? 'bg-gold text-[#0d0d0f]' : 'bg-input text-muted hover:text-fg border border-border'">همه</button>
              <button v-for="cat in categories" :key="cat.id" @click="selectedCategory = cat.id" class="px-4 py-2.5 text-sm rounded-lg font-medium transition min-h-[44px]" :class="selectedCategory === cat.id ? 'bg-gold text-[#0d0d0f]' : 'bg-input text-muted hover:text-fg border border-border'">{{ cat.name }}</button>
            </div>
          </div>

          <!-- Product buttons grid -->
          <div class="bg-surface border border-border rounded-xl p-3">
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2">
              <button v-for="product in filteredProducts" :key="product.id" @click="addItem(product)" :disabled="saving || isPastDateWithoutOrder" class="bg-input border border-border hover:border-gold/40 rounded-xl px-3 py-3 text-center transition min-h-[60px] flex flex-col items-center justify-center gap-1 disabled:opacity-40">
                <span class="text-sm text-fg font-medium leading-tight">{{ product.name }}</span>
                <span class="text-xs dark:text-gold text-gold-text">{{ product.price.toLocaleString() }}</span>
              </button>
            </div>
            <div v-if="filteredProducts.length === 0" class="text-center py-8 text-muted text-xs">هیچ محصولی در این دسته فعال نیست</div>
          </div>
        </div>

        <!-- Order items (right one third) -->
        <div class="bg-surface border border-border rounded-xl p-4 flex flex-col">
          <div class="flex items-center justify-between mb-3 pb-2 border-b border-border">
            <span class="text-sm font-bold text-fg">فاکتور</span>
            <span v-if="activeOrder?.status" class="text-xs px-2 py-0.5 rounded" :class="statusBadge(activeOrder.status)">{{ statusLabel(activeOrder.status) }}</span>
          </div>

          <div v-if="draftItems.length === 0 && (!balance?.todayGames || balance.todayGames.length === 0)" class="flex-1 flex items-center justify-center text-xs text-muted py-12">
            {{ isPastDateWithoutOrder ? 'نمایش اطلاعات این روز — امکان ثبت سفارش جدید وجود ندارد' : 'روی محصولات کلیک کنید تا به فاکتور اضافه شوند' }}
          </div>

          <div v-else class="flex-1 space-y-2 overflow-y-auto max-h-[400px]">
            <!-- Today's games (read-only) -->
            <div v-for="g in balance?.todayGames ?? []" :key="'g-'+g.clubPlayId" class="bg-[rgba(201,176,122,0.06)] rounded-lg p-2.5 border-r-2 border-gold/40">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <span class="text-sm text-fg">{{ g.title || 'بازی' }}<template v-if="g.roomName"> — {{ g.roomName }}</template></span>
                </div>
                <span class="text-xs text-muted">{{ g.nerkhName }}</span>
                <span class="text-sm dark:text-gold text-gold-text font-bold">{{ g.price.toLocaleString() }} تومان</span>
              </div>
            </div>
            <!-- Order items (local draft) -->
            <div v-for="(item, idx) in draftItems" :key="'draft-'+idx" class="bg-input rounded-lg p-2.5">
              <div class="flex items-center justify-between mb-1">
                <span class="text-sm text-fg font-medium">{{ item.productName }}</span>
                <span class="text-xs text-muted">{{ item.unitPrice.toLocaleString() }} تومان</span>
              </div>
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-1">
                  <button @click="decreaseQty(idx)" :disabled="saving || isPastDateWithoutOrder" class="w-7 h-7 flex items-center justify-center rounded bg-surface text-muted hover:text-fg text-sm transition disabled:opacity-40">&minus;</button>
                  <span class="w-8 text-center text-sm text-fg">{{ item.quantity }}</span>
                  <button @click="increaseQty(idx)" :disabled="saving || isPastDateWithoutOrder" class="w-7 h-7 flex items-center justify-center rounded bg-surface text-muted hover:text-fg text-sm transition disabled:opacity-40">+</button>
                </div>
                <div class="flex items-center gap-2">
                  <span class="text-sm dark:text-gold text-gold-text font-bold">{{ (item.quantity * item.unitPrice).toLocaleString() }}</span>
                  <button @click="removeItem(idx)" :disabled="saving || isPastDateWithoutOrder" class="text-xs text-danger hover:opacity-80 disabled:opacity-40">&times;</button>
                </div>
              </div>
            </div>
          </div>

          <div class="mt-3 pt-3 border-t border-border space-y-2">
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">جمع بازی‌ها و سفارشات</span>
              <span class="text-sm font-bold text-fg">{{ (balance?.todaySubtotal ?? draftTotal).toLocaleString() }} تومان</span>
            </div>
            <div v-if="balance && balance.vatPercent && balance.vatPercent > 0" class="flex items-center justify-between text-sm">
              <span class="text-muted">مالیات بر ارزش افزوده (٪{{ balance.vatPercent }})</span>
              <span class="text-sm text-fg">{{ balance.vatAmount.toLocaleString() }} تومان</span>
            </div>
            <div v-if="balance && balance.previousBalance > 0" class="flex items-center justify-between text-sm">
              <span class="text-muted">بدهی قبلی</span>
              <span class="text-sm text-danger">{{ balance.previousBalance.toLocaleString() }} تومان</span>
            </div>
            <div v-if="balance && balance.previousBalance < 0" class="flex items-center justify-between text-sm">
              <span class="text-muted">بستانکاری قبلی</span>
              <span class="text-sm text-success">{{ Math.abs(balance.previousBalance).toLocaleString() }} تومان</span>
            </div>
            <div v-if="balance && (balance.vatPercent || balance.previousBalance !== 0)" class="border-t border-border/50 my-1"></div>
            <div class="flex items-center justify-between">
              <span class="text-sm  font-bold text-fg">مبلغ نهایی قابل پرداخت</span>
              <span class="text-lg dark:text-gold font-bold text-gold-text">{{ (balance ? balance.previousBalance + balance.todayDue : draftTotal).toLocaleString() }} تومان</span>
            </div>
            <div v-if="saveError" class="text-xs text-danger text-center">{{ saveError }}</div>
            <div class="flex gap-2">
              <button @click="handleSaveAndGoBack" :disabled="saving || draftItems.length === 0 || isPastDateWithoutOrder" class="flex-1 px-3 py-2.5 bg-gold text-[#0d0d0f] text-sm rounded-lg font-bold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed transition text-center min-h-[44px]">
                <span v-if="saving && clickedButton === 'save'" class="inline-block w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin"></span>
                <span v-else>ثبت</span>
              </button>
              <button @click="handleSaveAndGoSettlement" :disabled="saving || draftItems.length === 0 || isPastDateWithoutOrder" class="flex-1 px-3 py-2.5 border border-gold/40 text-gold-text text-sm rounded-lg font-medium hover:bg-gold/10 disabled:opacity-40 disabled:cursor-not-allowed transition text-center min-h-[44px]">
                <span v-if="saving && clickedButton === 'settlement'" class="inline-block w-4 h-4 border-2 border-gold border-t-transparent rounded-full animate-spin"></span>
                <span v-else class="dark:text-gold">برو به تسویه</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>

    <CustomerSearchOrCreateModal
      :is-open="showCustomerSearch"
      :club-id="clubId()"
      @selected="onCustomerSelected"
      @close="showCustomerSearch = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { FinanceApi, ClubPlayerApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import { useToast } from '@/composables/useToast'
import CustomerSearchOrCreateModal from '@/components/club/CustomerSearchOrCreateModal.vue'
import type { ClubPlayerDto } from '@/types/clubPlayer'
import type { ProductDto, ProductCategoryDto, ClubOrderDto, ClubPlayerBalanceDto } from '@/types/finance'
import { getBusinessDateStr } from '@/utils/businessDate'


interface DraftOrderItem {
  productId: number
  productName: string
  unitPrice: number
  quantity: number
  serverItemId?: number
}

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
useToast()
const clubId = () => authStore.activeClubId!

const selectedPlayer = ref<ClubPlayerDto | null>(null)
const showCustomerSearch = ref(false)

const categories = ref<ProductCategoryDto[]>([])
const products = ref<ProductDto[]>([])
const selectedCategory = ref<number | null>(null)

const orderChoiceResolved = ref(false)
const openOrders = ref<ClubOrderDto[]>([])
const useExistingOrders = ref(true)
const editingOrderId = ref<number | null>(null)
const activeOrder = ref<ClubOrderDto | null>(null)
const balance = ref<ClubPlayerBalanceDto | null>(null)
const saving = ref(false)
const clickedButton = ref<'save' | 'settlement' | null>(null)
const saveError = ref('')

const draftItems = ref<DraftOrderItem[]>([])
const originalSnapshot = ref<string>('')

const filteredProducts = computed(() => {
  let list = products.value.filter(p => p.isActive !== false)
  if (selectedCategory.value) list = list.filter(p => p.categoryId === selectedCategory.value)
  return list
})

const draftTotal = computed(() =>
  draftItems.value.reduce((s, i) => s + i.quantity * i.unitPrice, 0)
)

const openOrdersTotal = computed(() => openOrders.value.reduce((s, o) => s + o.total, 0))

const viewingDate = computed(() => activeOrder.value?.businessDate || (route.query.date as string | undefined) || undefined)
const isPastDateWithoutOrder = computed(() => {
  if (!viewingDate.value || activeOrder.value) return false
  return viewingDate.value < getBusinessDateStr()
})

function statusBadge(status: string): string {
  if (status === 'settled') return 'bg-success/20 text-success'
  if (status === 'partial') return 'bg-[rgba(255,165,0,0.2)] text-[#ffa500]'
  return 'bg-gold/10 text-gold-text'
}
function statusLabel(status: string): string {
  if (status === 'settled') return 'تسویه شده'
  if (status === 'partial') return 'تسویه جزئی'
  return 'باز'
}

function snapshotKey(items: DraftOrderItem[]): string {
  return items.map(i => `${i.productId}:${i.quantity}`).join(',')
}

onMounted(init)

async function init() {
  const playerId = route.params.clubPlayerId ? Number(route.params.clubPlayerId) : null
  const orderId = route.params.orderId ? Number(route.params.orderId) : null
  const dateFromQuery = route.query.date as string | undefined

  if (authStore.activeClubId) {
    try {
      const [catRes, prodRes] = await Promise.all([
        FinanceApi.getProductCategories(clubId()),
        FinanceApi.getProducts(clubId()),
      ])
      categories.value = catRes.data
      products.value = prodRes.data
    } catch { /* toast handled */ }
  }

  if (orderId) {
    editingOrderId.value = orderId
  }

  if (playerId) {
    try {
      const p = (await ClubPlayerApi.getClubPlayerDetail(clubId(), playerId)).data
      selectedPlayer.value = p
      await loadOpenOrders()
      const balanceDate = activeOrder.value?.businessDate || dateFromQuery || undefined
      await loadBalance(balanceDate)
    } catch { /* player not found → keep search visible */ }
  }
}

watch(() => route.params.clubPlayerId, () => {
  if (route.params.clubPlayerId) init()
})

async function onCustomerSelected(player: ClubPlayerDto) {
  selectedPlayer.value = player
  showCustomerSearch.value = false
  orderChoiceResolved.value = false
  draftItems.value = []
  await Promise.all([loadOpenOrders(), loadBalance()])
}

function populateDraftFromOrder(order: ClubOrderDto) {
  draftItems.value = order.items.map(i => ({
    productId: i.productId,
    productName: i.productName,
    unitPrice: i.unitPrice,
    quantity: i.quantity,
    serverItemId: i.id,
  }))
  originalSnapshot.value = snapshotKey(draftItems.value)
}

async function loadOpenOrders() {
  if (!selectedPlayer.value) return
  if (editingOrderId.value) {
    try {
      const res = await FinanceApi.getOrderById(clubId(), editingOrderId.value)
      const order = res.data
      activeOrder.value = order
      populateDraftFromOrder(order)
    } catch { /* ignore */ }
    return
  }

  try {
    const res = await FinanceApi.getOpenOrdersForCustomer(clubId(), selectedPlayer.value.id)
    openOrders.value = res.data

    if (res.data.length > 0 && useExistingOrders.value) {
      activeOrder.value = res.data[0]
      populateDraftFromOrder(res.data[0])
      openOrders.value = res.data.slice(1)
    }
  } catch { openOrders.value = [] }
}

async function loadBalance(dateStr?: string) {
  if (!selectedPlayer.value || !authStore.activeClubId) return
  try {
    const res = await FinanceApi.getBalance(clubId(), selectedPlayer.value.id, dateStr)
    balance.value = res.data
  } catch { balance.value = null }
}

function chooseExistingOrder() {
  useExistingOrders.value = true
  orderChoiceResolved.value = true
  if (openOrders.value.length > 0) {
    activeOrder.value = openOrders.value[0]
    populateDraftFromOrder(openOrders.value[0])
  }
}

function chooseNewOrder() {
  useExistingOrders.value = false
  orderChoiceResolved.value = true
  activeOrder.value = null
  draftItems.value = []
  originalSnapshot.value = ''
}

function addItem(product: ProductDto) {
  const existing = draftItems.value.find(i => i.productId === product.id)
  if (existing) {
    existing.quantity++
  } else {
    draftItems.value.push({
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: 1,
    })
  }
}

function increaseQty(idx: number) {
  if (idx >= 0 && idx < draftItems.value.length) {
    draftItems.value[idx].quantity++
  }
}

function decreaseQty(idx: number) {
  if (idx >= 0 && idx < draftItems.value.length) {
    if (draftItems.value[idx].quantity <= 1) {
      draftItems.value.splice(idx, 1)
    } else {
      draftItems.value[idx].quantity--
    }
  }
}

function removeItem(idx: number) {
  if (idx >= 0 && idx < draftItems.value.length) {
    draftItems.value.splice(idx, 1)
  }
}

async function saveOrder() {
  if (!selectedPlayer.value || draftItems.value.length === 0) return
  saveError.value = ''

  if (activeOrder.value?.id) {
    await saveExistingOrder()
  } else {
    await saveNewOrder()
  }
}

async function saveNewOrder() {
  if (!selectedPlayer.value) return
  const res = await FinanceApi.createOrder(clubId(), {
    clubPlayerId: selectedPlayer.value.id,
    items: draftItems.value.map(i => ({ productId: i.productId, quantity: i.quantity })),
  })
  activeOrder.value = res.data
  populateDraftFromOrder(res.data)
}

async function saveExistingOrder() {
  if (!activeOrder.value?.id || !selectedPlayer.value) return

  const currentKey = snapshotKey(draftItems.value)
  if (currentKey === originalSnapshot.value) return

  const cId = clubId()
  const pId = selectedPlayer.value.id
  const oId = activeOrder.value.id

  const origMap = new Map<string, DraftOrderItem>()
  const origItems = activeOrder.value.items
  for (const i of origItems) {
    origMap.set(`${i.productId}`, { productId: i.productId, productName: i.productName, unitPrice: i.unitPrice, quantity: i.quantity, serverItemId: i.id })
  }

  const hasSettledOrder = activeOrder.value.status === 'settled' || activeOrder.value.status === 'partial'

  for (const draft of draftItems.value) {
    const orig = origMap.get(`${draft.productId}`)
    if (!orig) {
      await FinanceApi.addItem(cId, { productId: draft.productId, clubPlayerId: pId, quantity: draft.quantity, orderId: oId })
    } else if (orig.quantity !== draft.quantity && draft.serverItemId) {
      await FinanceApi.updateItemQuantity(cId, draft.serverItemId, { newQuantity: draft.quantity })
    }
    origMap.delete(`${draft.productId}`)
  }

  for (const removed of origMap.values()) {
    if (removed.serverItemId) {
      await FinanceApi.removeItem(cId, removed.serverItemId)
    }
  }

  await refreshAfterSave(oId)
  if (hasSettledOrder) {
    saveError.value = 'این فاکتور قبلاً تسویه شده بود؛ تغییرات باعث مغایرت می‌شود.'
  }
}

async function refreshAfterSave(orderId: number) {
  if (!selectedPlayer.value) return
  try {
    const res = await FinanceApi.getOrderById(clubId(), orderId)
    activeOrder.value = res.data
    populateDraftFromOrder(res.data)
  } catch { /* ignore */ }
}

async function handleSaveAndGoBack() {
  clickedButton.value = 'save'
  saving.value = true
  saveError.value = ''
  try {
    await saveOrder()
    router.push('/finance/today')
  } catch (err: any) {
    saveError.value = err?.response?.data?.message || err?.message || 'خطا در ذخیره سفارش'
  } finally {
    saving.value = false
    clickedButton.value = null
  }
}

async function handleSaveAndGoSettlement() {
  clickedButton.value = 'settlement'
  saving.value = true
  saveError.value = ''
  try {
    await saveOrder()
    if (selectedPlayer.value) router.push(`/finance/settlement/${selectedPlayer.value.id}`)
  } catch (err: any) {
    saveError.value = err?.response?.data?.message || err?.message || 'خطا در ذخیره سفارش'
  } finally {
    saving.value = false
    clickedButton.value = null
  }
}
</script>
