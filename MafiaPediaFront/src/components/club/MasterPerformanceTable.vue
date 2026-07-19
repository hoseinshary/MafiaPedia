<template>
  <div class="bg-surface border border-border rounded-xl p-6 mb-8">
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-sm font-bold text-fg">عملکرد گرداننده‌ها</h2>
      <div class="flex gap-2">
        <button
          @click="period = 'week'"
          class="text-xs px-3 py-1.5 rounded transition"
          :class="period === 'week' ? 'bg-gold text-[#141416] font-bold' : 'text-muted border border-border hover:text-gold-text'"
        >
          هفته
        </button>
        <button
          @click="period = 'month'"
          class="text-xs px-3 py-1.5 rounded transition"
          :class="period === 'month' ? 'bg-gold text-[#141416] font-bold' : 'text-muted border border-border hover:text-gold-text'"
        >
          ماه
        </button>
      </div>
    </div>
    <div v-if="loading" class="flex justify-center py-8">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>
    <div v-else-if="items.length === 0" class="text-center py-8 text-sm text-muted">
      داده‌ای برای این بازه یافت نشد
    </div>
    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-border text-muted">
            <th class="px-3 py-2 text-right">نام گرداننده</th>
            <th class="px-3 py-2 text-right">تعداد بازی</th>
            <th class="px-3 py-2 text-right">نفر-بازی</th>
            <th class="px-3 py-2 text-right">مهمان</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.masterId" class="border-b border-border text-fg">
            <td class="px-3 py-2.5 font-medium">{{ item.masterName }}</td>
            <td class="px-3 py-2.5">{{ item.playCount }}</td>
            <td class="px-3 py-2.5">{{ item.entryCount }}</td>
            <td class="px-3 py-2.5 text-muted">{{ item.guestEntryCount }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { ClubPlayApi } from '@/api'
import type { MasterPerformanceDto } from '@/types/clubPlay'

const props = defineProps<{
  clubId: number
}>()

const period = ref<'week' | 'month'>('week')
const items = ref<MasterPerformanceDto[]>([])
const loading = ref(true)

async function fetchData() {
  loading.value = true
  try {
    const res = await ClubPlayApi.getMasterPerformance(props.clubId, period.value)
    items.value = res.data
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

watch(period, fetchData)
onMounted(fetchData)
</script>
