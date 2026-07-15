<template>
  <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-sm font-bold text-[#e8e4d9]">عملکرد گرداننده‌ها</h2>
      <div class="flex gap-2">
        <button
          @click="period = 'week'"
          class="text-xs px-3 py-1.5 rounded transition"
          :class="period === 'week' ? 'bg-[#c9b07a] text-[#141416] font-bold' : 'text-[rgba(232,228,217,0.4)] border border-[rgba(255,255,255,0.07)] hover:text-[#c9b07a]'"
        >
          هفته
        </button>
        <button
          @click="period = 'month'"
          class="text-xs px-3 py-1.5 rounded transition"
          :class="period === 'month' ? 'bg-[#c9b07a] text-[#141416] font-bold' : 'text-[rgba(232,228,217,0.4)] border border-[rgba(255,255,255,0.07)] hover:text-[#c9b07a]'"
        >
          ماه
        </button>
      </div>
    </div>
    <div v-if="loading" class="flex justify-center py-8">
      <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>
    <div v-else-if="items.length === 0" class="text-center py-8 text-sm text-[rgba(232,228,217,0.3)]">
      داده‌ای برای این بازه یافت نشد
    </div>
    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
            <th class="px-3 py-2 text-right">نام گرداننده</th>
            <th class="px-3 py-2 text-right">تعداد بازی</th>
            <th class="px-3 py-2 text-right">نفر-بازی</th>
            <th class="px-3 py-2 text-right">مهمان</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.masterId" class="border-b border-[rgba(255,255,255,0.04)] text-[#e8e4d9]">
            <td class="px-3 py-2.5 font-medium">{{ item.masterName }}</td>
            <td class="px-3 py-2.5">{{ item.playCount }}</td>
            <td class="px-3 py-2.5">{{ item.entryCount }}</td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.4)]">{{ item.guestEntryCount }}</td>
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
