<template>
  <div class="overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e]">
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">نقش</th>
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">تعداد بازی</th>
          <th v-if="showWins" class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">تعداد برد</th>
          <th v-if="showWinRate" class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">آمار برد</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="role in roles" :key="role.roleId" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e]">
          <td class="px-4 py-3 font-medium text-[#e8e4d9]">{{ role.roleName }}</td>
          <td class="px-4 py-3 text-[#e8e4d9]">{{ role.games }}</td>
          <td v-if="showWins" class="px-4 py-3 text-[#e8e4d9]">{{ role.wins ?? '-' }}</td>
          <td v-if="showWinRate" class="px-4 py-3 text-[#e8e4d9]">{{ role.winRate != null ? formatPercent(role.winRate) : '-' }}</td>
        </tr>
        <tr v-if="roles.length === 0">
          <td :colspan="colspan" class="px-4 py-8 text-center text-[rgba(232,228,217,0.4)]">No data available.</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { RoleStat } from '@/types'

const props = defineProps<{
  roles: RoleStat[]
  showWins?: boolean
  showWinRate?: boolean
}>()

const colspan = props.showWins || props.showWinRate ? 4 : 2

function formatPercent(value: number): string {
  return `${value.toFixed(2)}%`
}
</script>
