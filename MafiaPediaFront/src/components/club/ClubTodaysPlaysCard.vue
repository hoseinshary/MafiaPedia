<template>
  <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
    <div class="flex items-center justify-between mb-4">
      <div class="flex items-center gap-3">
        <button
          @click="navigateDate(-1)"
          class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#c9b07a] transition px-2 py-1 border border-[rgba(255,255,255,0.07)] rounded"
        >
          ▶ روز قبل
        </button>
        <h2 class="text-sm font-bold text-[#e8e4d9]">
          بازی‌های {{ formatHeadingDate(viewedDate) }}
          <span class="text-[rgba(232,228,217,0.8)] font-normal">— {{ totalEntries }} نفر-بازی</span>
        </h2>
        <button
          @click="navigateDate(1)"
          :disabled="isToday"
          class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#c9b07a] disabled:opacity-30 disabled:cursor-not-allowed transition px-2 py-1 border border-[rgba(255,255,255,0.07)] rounded"
        >
          روز بعد ◀
        </button>
      </div>
    </div>
    <div v-if="loading" class="flex justify-center py-8">
      <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>
    <div v-else-if="plays.length === 0" class="text-center py-8 text-sm text-[rgba(232,228,217,0.3)]">
      این روز هنوز بازی‌ای ثبت نشده
    </div>
    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
            <th class="px-3 py-2 text-right">عنوان</th>
            <th class="px-3 py-2 text-right">ساعت</th>
            <th class="px-3 py-2 text-right">سالن</th>
            <th class="px-3 py-2 text-right">سناریو</th>
            <th class="px-3 py-2 text-right">نوع بازی</th>
            <th class="px-3 py-2 text-right">تعداد</th>
            <th class="px-3 py-2 text-right">گرداننده</th>
            <th class="px-3 py-2 text-right">وضعیت</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="play in plays"
            :key="play.id"
            @click="goToPlay(play.id)"
            class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[rgba(255,255,255,0.02)] transition cursor-pointer text-[#e8e4d9]"
          >
            <td class="px-3 py-2.5 font-medium">{{ play.title || 'بدون عنوان' }}</td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ formatTime(play.dateTime) }}</td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.roomName }}</td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.senarioName }}</td>
            <td class="px-3 py-2.5"><span class="status-badge" :class="playTypeClass(play.playType)">{{ playTypeLabel(play.playType) }}</span></td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.playersCount }}</td>
            <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.masterName || '—' }}</td>
            <td class="px-3 py-2.5"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ClubPlayApi } from '@/api'
import type { ClubPlayListItemDto } from '@/types/clubPlay'

const router = useRouter()

const props = defineProps<{
  clubId: number
}>()

function toDateString(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const todayStr = toDateString(new Date())
const viewedDate = ref(todayStr)
const isToday = computed(() => viewedDate.value === todayStr)
const plays = ref<ClubPlayListItemDto[]>([])
const loading = ref(true)

const totalEntries = computed(() =>
  plays.value.reduce((sum, p) => sum + (p.playersCount - p.guestCount), 0)
)

function navigateDate(delta: number) {
  const d = new Date(viewedDate.value + 'T12:00:00')
  d.setDate(d.getDate() + delta)
  viewedDate.value = toDateString(d)
  fetchPlays()
}

function formatHeadingDate(dateStr: string) {
  const d = new Date(dateStr + 'T12:00:00')
  return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' })
}

function formatTime(dt: string) {
  const d = new Date(dt)
  return d.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })
}

function statusClass(status: string) {
  if (status === 'done') return 'status-done'
  if (status === 'pending') return 'status-pending'
  if (status === 'notwinside') return 'status-notwinside'
  if (status === 'notrank') return 'status-notrank'
  return ''
}

function statusLabel(status: string) {
  const map: Record<string, string> = {
    pending: 'در حال پخش',
    notwinside: 'ثبت برنده',
    notrank: 'ثبت رتبه',
    done: 'کامل شد',
  }
  return map[status] || status
}

function playTypeLabel(pt: string) {
  const map: Record<string, string> = {
    normal: 'عادی',
    rank: 'رنک',
    superrank: 'سوپر رنک',
    etc: 'سایر',
  }
  return map[pt] || pt
}

function playTypeClass(pt: string) {
  return 'playtype-' + pt
}

function goToPlay(id: number) {
  router.push({ name: 'MasterPlayDetail', params: { id } })
}

async function fetchPlays() {
  loading.value = true
  try {
    const res = await ClubPlayApi.getClubPlaysByBusinessDate(props.clubId, viewedDate.value)
    plays.value = res.data
  } catch {
    plays.value = []
  } finally {
    loading.value = false
  }
}

onMounted(fetchPlays)
</script>

<style scoped>
.status-badge {
  display: inline-block;
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 500;
}
.status-pending {
  background: rgba(128, 128, 128, 0.15);
  color: #a0a0a0;
}
.status-notwinside {
  background: rgba(255, 165, 0, 0.15);
  color: #ffa500;
}
.status-notrank {
  background: rgba(255, 255, 0, 0.12);
  color: #d4d43a;
}
.status-done {
  background: rgba(74, 222, 128, 0.12);
  color: #4ade80;
}
.playtype-normal {
  background: rgba(128, 128, 128, 0.15);
  color: #a0a0a0;
}
.playtype-rank {
  background: rgba(201, 176, 122, 0.15);
  color: #c9b07a;
}
.playtype-superrank {
  background: rgba(255, 165, 0, 0.15);
  color: #ffa500;
}
.playtype-etc {
  background: rgba(100, 180, 255, 0.12);
  color: #64b4ff;
}
</style>