<template>
  <div class="bg-white rounded-lg border border-gray-200 shadow-sm overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-gray-200 bg-gray-50">
          <th class="px-4 py-3 text-right">نقش</th>
          <th class="px-4 py-3 text-right">تعداد بازی</th>
          <th v-if="showWins" class="px-4 py-3 text-right">تعداد برد</th>
          <th v-if="showWinRate" class="px-4 py-3 text-right">آمار برد</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="role in roles" :key="role.roleId" class="border-b border-gray-100 hover:bg-gray-50">
          <td class="px-4 py-3 font-medium">{{ role.roleName }}</td>
          <td class="px-4 py-3">{{ role.games }}</td>
          <td v-if="showWins" class="px-4 py-3">{{ role.wins ?? '-' }}</td>
          <td v-if="showWinRate" class="px-4 py-3">{{ role.winRate != null ? formatPercent(role.winRate) : '-' }}</td>
        </tr>
        <tr v-if="roles.length === 0">
          <td :colspan="colspan" class="px-4 py-8 text-center text-gray-500">No data available.</td>
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
