<template>
  <div class="bg-white rounded-lg border border-gray-200 shadow-sm overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-gray-200 bg-gray-50">
          <th class="px-4 py-3 text-right">نام بازی</th>
          <th class="px-4 py-3 text-right">نقش</th>
          <th class="px-4 py-3 text-right">نتیجه</th>
          <th class="px-4 py-3 text-right">لینک</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="game in games" :key="game.playId" class="border-b border-gray-100 hover:bg-gray-50">
          <td class="px-4 py-3 font-medium">{{ game.playTitle }}</td>
          <td class="px-4 py-3">{{ game.roleName }}</td>
          <td class="px-4 py-3">
            <span
              class="inline-block px-2 py-0.5 rounded text-xs font-semibold"
              :class="game.result === 'Win' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'"
            >
              {{ game.result === 'Win' ? 'برد' : 'باخت' }}
            </span>
          </td>
          <td class="px-4 py-3">
            <a
              v-if="game.link"
              :href="game.link"
              target="_blank"
              rel="noopener noreferrer"
              class="text-blue-600 hover:underline text-sm"
            >
              Watch Game
            </a>
            <span v-else class="text-gray-400">-</span>
          </td>
        </tr>
        <tr v-if="games.length === 0">
          <td colspan="4" class="px-4 py-8 text-center text-gray-500">No recent games.</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { RecentGame } from '@/types'

defineProps<{
  games: RecentGame[]
}>()
</script>
