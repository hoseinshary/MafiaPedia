import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { PlayerProfile, OverallRankingEntry } from '@/types'

export const usePlayerStore = defineStore('player', () => {
  const players = ref<OverallRankingEntry[]>([])
  const currentPlayer = ref<PlayerProfile | null>(null)
  const loading = ref(false)

  function setPlayers(data: OverallRankingEntry[]) {
    players.value = data
  }

  function setCurrentPlayer(player: PlayerProfile) {
    currentPlayer.value = player
  }

  return { players, currentPlayer, loading, setPlayers, setCurrentPlayer }
})
