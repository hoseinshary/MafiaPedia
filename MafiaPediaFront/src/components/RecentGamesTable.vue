<template>
  <div class="overflow-x-auto">
    <table class="w-full text-sm border-collapse">
      <thead>
        <tr class="border-b border-border bg-surface-hover">
          <th class="px-4 py-3 text-right text-muted">نام بازی</th>
          <th class="px-4 py-3 text-right text-muted">نقش</th>
          <th class="px-4 py-3 text-right text-muted">نتیجه</th>
          <th class="px-4 py-3 text-right text-muted">لینک</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="game in games" :key="game.playId" class="border-b border-border hover:bg-surface-hover">
          <td class="px-4 py-3 font-medium text-fg">{{ game.playTitle }}</td>
          <td class="px-4 py-3 text-fg">{{ game.roleName }}</td>
          <td class="px-4 py-3">
            <span
              class="inline-block px-2 py-0.5 rounded text-xs font-semibold"
              :class="game.result === 'Win' ? 'bg-success/20 text-success' : 'bg-danger/20 text-danger'"
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
              class="text-gold-text hover:underline text-sm"
            >
              Watch Game
            </a>
            <span v-else class="text-muted">-</span>
          </td>
        </tr>
        <tr v-if="games.length === 0">
          <td colspan="4" class="px-4 py-8 text-center text-muted">No recent games.</td>
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
