<template>
  <div class="overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e]">
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">نام بازی</th>
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">نقش</th>
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">نتیجه</th>
          <th class="px-4 py-3 text-right text-[rgba(232,228,217,0.5)]">لینک</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="game in games" :key="game.playId" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e]">
          <td class="px-4 py-3 font-medium text-[#e8e4d9]">{{ game.playTitle }}</td>
          <td class="px-4 py-3 text-[#e8e4d9]">{{ game.roleName }}</td>
          <td class="px-4 py-3">
            <span
              class="inline-block px-2 py-0.5 rounded text-xs font-semibold"
              :class="game.result === 'Win' ? 'bg-[rgba(111,207,138,0.15)] text-[#6fcf8a]' : 'bg-[rgba(224,112,112,0.15)] text-[#e07070]'"
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
              class="text-[#c9b07a] hover:underline text-sm"
            >
              Watch Game
            </a>
            <span v-else class="text-[rgba(232,228,217,0.35)]">-</span>
          </td>
        </tr>
        <tr v-if="games.length === 0">
          <td colspan="4" class="px-4 py-8 text-center text-[rgba(232,228,217,0.4)]">No recent games.</td>
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
