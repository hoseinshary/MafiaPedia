<template>
  <div v-if="rows.length === 0" class="text-center py-10 text-[rgba(232,228,217,0.4)]">
    داده‌ای یافت نشد
  </div>
  <div v-else class="overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e] text-[rgba(232,228,217,0.5)]">
          <th class="px-4 py-3 text-right">نام</th>
          <th class="px-4 py-3 text-center">تعداد بازی</th>
          <th class="px-4 py-3 text-center">برد مافیا</th>
          <th class="px-4 py-3 text-center">برد شهروند</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e] transition text-[#e8e4d9]"
        >
          <td class="px-4 py-3 font-medium">{{ row.name }}</td>
          <td class="px-4 py-3 text-center">{{ row.totalGames }}</td>
          <td class="px-4 py-3 text-center text-[#e07070]">{{ (row.mafiaWinRate ).toFixed(2) }}%</td>
          <td class="px-4 py-3 text-center text-[#70a8e0]">{{ (row.citizenWinRate ).toFixed(2) }}%</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { SideWinRateDto } from '@/api/StatisticsApi'

defineProps<{
  rows: SideWinRateDto[]
}>()
</script>
