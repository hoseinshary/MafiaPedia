<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link to="/cashier" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت به داشبورد</router-link>
      <h1 class="text-xl font-bold text-fg mt-2">ثبت سفارش</h1>
    </div>

    <div class="bg-surface border border-border rounded-xl p-6 space-y-4">
      <div>
        <label class="text-sm text-muted">بازیکن</label>
        <input v-model="query" type="text" placeholder="جستجوی بازیکن..." class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" @input="onSearchInput" />
        <div v-if="searchResults.length > 0 && query.trim().length >= 2" class="mt-1 bg-surface border border-border rounded shadow-lg overflow-hidden">
          <div v-for="player in searchResults" :key="player.id" @click="selectPlayer(player)" class="px-3 py-2 text-sm text-fg hover:bg-surface-hover cursor-pointer transition">{{ player.name }} - {{ player.mobile }}</div>
        </div>
      </div>

      <div v-if="selectedPlayer">
        <div class="flex items-center justify-between bg-gold/10 border border-gold/10 rounded-lg px-4 py-2">
          <span class="text-sm text-fg font-medium">{{ selectedPlayer.name }}</span>
          <button @click="selectedPlayer = null; orderItems = []; query = ''" class="text-xs text-danger">&times;</button>
        </div>

        <div class="mt-4">
          <label class="text-sm text-muted">محصولات</label>
          <div v-for="item in orderItems" :key="item.productId" class="flex items-center gap-2 mt-2">
            <select v-model="item.productId" @change="onItemProductChange(item)" class="flex-1 bg-input border border-border rounded px-4 py-2 text-sm text-fg focus:outline-none focus:border-gold transition">
              <option value="" disabled>انتخاب محصول</option>
              <option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }} - {{ p.price.toLocaleString() }} تومان</option>
            </select>
            <input v-model.number="item.quantity" type="number" min="1" class="w-16 bg-input border border-border rounded px-2 py-2 text-sm text-fg focus:outline-none focus:border-gold transition ltr text-center" />
            <button @click="removeItem(item)" class="text-xs text-danger hover:opacity-80">&times;</button>
          </div>
          <button @click="addItem" class="mt-2 text-xs text-gold-text hover:opacity-80 transition">+ افزودن محصول</button>
        </div>

        <p v-if="orderError" class="text-sm text-danger mt-3">{{ orderError }}</p>

        <div class="flex gap-3 justify-end mt-6 pt-4 border-t border-border">
          <button @click="handleSubmitOrder" :disabled="!canSubmitOrder || orderSubmitting" class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2">
            <div v-if="orderSubmitting" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            ثبت سفارش
          </button>
        </div>
      </div>
    </div>

    <div v-if="recentOrders.length > 0" class="mt-8">
      <h2 class="text-lg font-bold text-fg mb-4">سفارشات امروز</h2>
      <div v-for="order in recentOrders" :key="order.id" class="bg-surface border border-border rounded-xl p-4 mb-3">
        <div class="flex items-center justify-between mb-2">
          <span class="text-sm text-fg font-medium">{{ order.clubPlayerName }}</span>
          <span class="text-xs text-muted">{{ order.itemCount }} قلم - {{ order.total.toLocaleString() }} تومان</span>
          <button @click="handleDeleteOrder(order.id)" class="text-xs text-danger hover:opacity-80">حذف</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { FinanceApi, ClubPlayerApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { ProductDto, CreateOrderItemDto } from '@/types/finance'
import type { ClubPlayerDto } from '@/types/clubPlayer'

const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!

const query = ref('')
const searchResults = ref<ClubPlayerDto[]>([])
const selectedPlayer = ref<ClubPlayerDto | null>(null)
const products = ref<ProductDto[]>([])
const orderItems = ref<Array<{ productId: number | ''; quantity: number }>>([])
const orderSubmitting = ref(false)
const orderError = ref('')
const recentOrders = ref<{ id: number; clubPlayerName: string; itemCount: number; total: number }[]>([])

interface OrderItemVM { productId: number | ''; quantity: number }

let searchDebounce: ReturnType<typeof setTimeout>

const canSubmitOrder = computed(() => selectedPlayer.value && orderItems.value.length > 0 && orderItems.value.every(i => i.productId !== ''))

onMounted(async () => {
  try {
    const res = await FinanceApi.getProducts(clubId())
    products.value = res.data.filter(p => p.isActive !== false)
  } catch { /* toast handled by interceptor */ }
})

function onSearchInput() {
  clearTimeout(searchDebounce)
  if (query.value.trim().length < 2) { searchResults.value = []; return }
  searchDebounce = setTimeout(async () => {
    try {
      const res = await ClubPlayerApi.searchAllCustomers(clubId(), query.value)
      searchResults.value = res.data.inClub
    } catch { searchResults.value = [] }
  }, 300)
}

function selectPlayer(player: ClubPlayerDto) {
  selectedPlayer.value = player
  query.value = ''
  searchResults.value = []
  orderItems.value = []
}

function addItem() {
  orderItems.value.push({ productId: '', quantity: 1 })
}

function removeItem(item: OrderItemVM) {
  orderItems.value = orderItems.value.filter(i => i !== item)
}

function onItemProductChange(_item: OrderItemVM) {
  // just triggers reactivity for canSubmitOrder
}

async function handleSubmitOrder() {
  if (!canSubmitOrder.value || !selectedPlayer.value) return
  orderSubmitting.value = true
  orderError.value = ''
  try {
    const items: CreateOrderItemDto[] = orderItems.value.map(i => ({ productId: i.productId as number, quantity: i.quantity }))
    await FinanceApi.createOrder(clubId(), { clubPlayerId: selectedPlayer.value.id, items })
    orderItems.value = []
    selectedPlayer.value = null
    query.value = ''
    await loadTodayOrders()
  } catch (err: any) {
    orderError.value = err.response?.data?.message || 'خطا در ثبت سفارش'
  } finally {
    orderSubmitting.value = false
  }
}

async function loadTodayOrders() {
  // This would ideally use a "get orders by date" endpoint; for now skip refresh
}

async function handleDeleteOrder(id: number) {
  if (!confirm('آیا از حذف این سفارش اطمینان دارید؟')) return
  try {
    await FinanceApi.deleteOrder(clubId(), id)
    recentOrders.value = recentOrders.value.filter(o => o.id !== id)
  } catch { /* toast handled by interceptor */ }
}
</script>
