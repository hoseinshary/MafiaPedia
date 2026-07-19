<template>
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
    <div class="bg-surface border border-border rounded-xl p-6">
      <h3 class="text-xs text-muted mb-3">آمار هفتگی کافه</h3>
      <div v-if="weeklyLoading" class="flex justify-center py-4">
        <div class="w-5 h-5 border-2 border-gold border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else class="grid grid-cols-3 gap-2">
        <div>
          <div class="text-2xl font-bold text-fg">{{ weekStats?.totalPlays ?? '-' }}</div>
          <div class="text-xs text-muted">تعداد بازی</div>
        </div>
        <div>
          <div class="text-2xl font-bold text-fg">{{ weekStats?.totalEntries ?? '-' }}</div>
          <div class="text-xs text-muted">نفر-بازی</div>
        </div>
        <div>
          <div class="text-2xl font-bold text-muted">{{ weekStats?.totalGuestEntries ?? '-' }}</div>
          <div class="text-xs text-muted">مهمان</div>
        </div>
      </div>
    </div>
    <div class="bg-surface border border-border rounded-xl p-6">
      <h3 class="text-xs text-muted mb-3">آمار ماهانه کافه</h3>
      <div v-if="monthlyLoading" class="flex justify-center py-4">
        <div class="w-5 h-5 border-2 border-gold border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else class="grid grid-cols-3 gap-2">
        <div>
          <div class="text-2xl font-bold text-fg">{{ monthStats?.totalPlays ?? '-' }}</div>
          <div class="text-xs text-muted">تعداد بازی</div>
        </div>
        <div>
          <div class="text-2xl font-bold text-fg">{{ monthStats?.totalEntries ?? '-' }}</div>
          <div class="text-xs text-muted">نفر-بازی</div>
        </div>
        <div>
          <div class="text-2xl font-bold text-muted">{{ monthStats?.totalGuestEntries ?? '-' }}</div>
          <div class="text-xs text-muted">مهمان</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ClubPlayApi } from '@/api'
import type { MasterStatsDto } from '@/types/clubPlay'

const props = defineProps<{
  clubId: number
}>()

const weekStats = ref<MasterStatsDto | null>(null)
const weeklyLoading = ref(true)
const monthStats = ref<MasterStatsDto | null>(null)
const monthlyLoading = ref(true)

onMounted(async () => {
  try {
    const [weekRes, monthRes] = await Promise.all([
      ClubPlayApi.getClubStats(props.clubId, 'week'),
      ClubPlayApi.getClubStats(props.clubId, 'month'),
    ])
    weekStats.value = weekRes.data
    monthStats.value = monthRes.data
  } catch {
    // handled
  } finally {
    weeklyLoading.value = false
    monthlyLoading.value = false
  }
})
</script>
