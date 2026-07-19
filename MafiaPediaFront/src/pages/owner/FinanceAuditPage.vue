<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link to="/owner" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت به داشبورد</router-link>
      <h1 class="text-xl font-bold text-fg mt-2">گزارش حذف‌شدگان مالی</h1>
    </div>

    <div class="bg-surface border border-border rounded-xl p-6 mb-6">
      <label class="text-sm text-muted">نوع</label>
      <select v-model="selectedType" @change="loadDeleted" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition mt-1">
        <option value="">انتخاب کنید</option>
        <option value="nerkh">نرخ‌ها</option>
        <option value="product">محصولات</option>
        <option value="order">سفارشات</option>
        <option value="settlement">تسویه‌ها</option>
      </select>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="records.length > 0">
      <div v-for="r in records" :key="r.id" class="bg-surface border border-border rounded-xl p-4 mb-3 flex items-center justify-between">
        <div>
          <p class="text-sm text-fg">{{ r.name }}</p>
          <p class="text-xs text-muted mt-0.5">حذف شده در {{ r.deletedAt }} توسط {{ r.deletedBy }}</p>
        </div>
      </div>
    </div>

    <div v-else-if="selectedType && !loading" class="text-center py-12 text-muted text-sm">هیچ رکورد حذف‌شده‌ای یافت نشد</div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { FinanceDeletedRecordDto } from '@/types/finance'

const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!

const selectedType = ref('')
const records = ref<FinanceDeletedRecordDto[]>([])
const loading = ref(false)

async function loadDeleted() {
  if (!selectedType.value) return
  loading.value = true
  records.value = []
  try {
    const res = await FinanceApi.getDeletedFinanceRecords(clubId(), selectedType.value)
    records.value = res.data
  } catch { /* toast handled by interceptor */ }
  finally { loading.value = false }
}
</script>
