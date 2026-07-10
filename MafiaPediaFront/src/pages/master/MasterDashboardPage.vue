<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full" v-if="ctx">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-[#c9b07a]">داشبورد</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ ctx.clubName }}</p>
    </div>

    <!-- Quick links -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
      <router-link to="/master/plays/create" class="block bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 hover:border-[rgba(201,176,122,0.3)] transition text-center">
        <div class="text-2xl mb-2">🎯</div>
        <div class="text-sm font-bold text-[#c9b07a]">ثبت بازی جدید</div>
        <div class="text-xs text-[rgba(232,228,217,0.3)] mt-1">ایجاد یک بازی جدید در کافه</div>
      </router-link>
      <router-link to="/master/plays/practice" class="block bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 hover:border-[rgba(201,176,122,0.3)] transition text-center">
        <div class="text-2xl mb-2">🎲</div>
        <div class="text-sm font-bold text-[#c9b07a]">حالت تمرین</div>
        <div class="text-xs text-[rgba(232,228,217,0.3)] mt-1">پیش‌نمایش پخش نقش بدون ثبت بازی</div>
      </router-link>
    </div>

    <!-- Today's plays -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-3">
          <button @click="navigateDate(-1)" class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#c9b07a] transition px-2 py-1 border border-[rgba(255,255,255,0.07)] rounded">◀ روز قبل</button>
          <h2 class="text-sm font-bold text-[#e8e4d9]">بازی‌های {{ formatHeadingDate(viewedDate) }} <span class="text-[rgba(232,228,217,0.4)] font-normal">— {{ totalEntries }} نفر-بازی</span></h2>
          <button @click="navigateDate(1)" :disabled="isToday" class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#c9b07a] disabled:opacity-30 disabled:cursor-not-allowed transition px-2 py-1 border border-[rgba(255,255,255,0.07)] rounded">روز بعد ▶</button>
        </div>
      </div>
      <div v-if="todaysLoading" class="flex justify-center py-8">
        <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else-if="todaysPlays.length === 0" class="text-center py-8 text-sm text-[rgba(232,228,217,0.3)]">
        امروز هنوز بازی‌ای ثبت نشده
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
              <th class="px-3 py-2 text-right">عنوان</th>
              <th class="px-3 py-2 text-right">ساعت</th>
              <th class="px-3 py-2 text-right">سالن</th>
              <th class="px-3 py-2 text-right">سناریو</th>
              <th class="px-3 py-2 text-right">تعداد</th>
              <th class="px-3 py-2 text-right">وضعیت</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="play in todaysPlays" :key="play.id" @click="goToPlay(play.id)" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[rgba(255,255,255,0.02)] transition cursor-pointer text-[#e8e4d9]">
              <td class="px-3 py-2.5 font-medium">{{ play.title || 'بدون عنوان' }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ formatTime(play.dateTime) }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.roomName }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.senarioName }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.playersCount }}</td>
              <td class="px-3 py-2.5"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
        <h3 class="text-xs text-[rgba(232,228,217,0.4)] mb-3">آمار هفتگی</h3>
        <div v-if="weeklyLoading" class="flex justify-center py-4">
          <div class="w-5 h-5 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
        </div>
        <div v-else class="grid grid-cols-3 gap-2">
          <div>
            <div class="text-2xl font-bold text-[#e8e4d9]">{{ weekStats?.totalPlays ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">تعداد بازی</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-[#e8e4d9]">{{ weekStats?.totalEntries ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">نفر-بازی</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-[rgba(232,228,217,0.4)]">{{ weekStats?.totalGuestEntries ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">مهمان</div>
          </div>
        </div>
      </div>
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
        <h3 class="text-xs text-[rgba(232,228,217,0.4)] mb-3">آمار ماهانه</h3>
        <div v-if="monthlyLoading" class="flex justify-center py-4">
          <div class="w-5 h-5 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
        </div>
        <div v-else class="grid grid-cols-3 gap-2">
          <div>
            <div class="text-2xl font-bold text-[#e8e4d9]">{{ monthStats?.totalPlays ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">تعداد بازی</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-[#e8e4d9]">{{ monthStats?.totalEntries ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">نفر-بازی</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-[rgba(232,228,217,0.4)]">{{ monthStats?.totalGuestEntries ?? '-' }}</div>
            <div class="text-xs text-[rgba(232,228,217,0.4)]">مهمان</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Open plays -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-sm font-bold text-[#e8e4d9]">کارهای باقی‌مانده</h2>
      </div>
      <div v-if="openLoading" class="flex justify-center py-8">
        <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else-if="openPlays.length === 0" class="text-center py-8 text-sm text-[rgba(232,228,217,0.3)]">
        هیچ بازی ناقصی وجود ندارد
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
              <th class="px-3 py-2 text-right">عنوان</th>
              <th class="px-3 py-2 text-right">تاریخ</th>
              <th class="px-3 py-2 text-right">سالن</th>
              <th class="px-3 py-2 text-right">سناریو</th>
              <th class="px-3 py-2 text-right">وضعیت</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="play in openPlays" :key="play.id" @click="goToPlay(play.id)" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[rgba(255,255,255,0.02)] transition cursor-pointer text-[#e8e4d9]">
              <td class="px-3 py-2.5 font-medium">{{ play.title || 'بدون عنوان' }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ formatDate(play.dateTime) }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.roomName }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.senarioName }}</td>
              <td class="px-3 py-2.5"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Recent plays preview -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-sm font-bold text-[#e8e4d9]">همه‌ی بازی‌ها</h2>
        <router-link to="/master/plays" class="text-xs text-[#c9b07a] hover:underline">مشاهده همه</router-link>
      </div>
      <div v-if="recentLoading" class="flex justify-center py-8">
        <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else-if="recentPlays.length === 0" class="text-center py-8 text-sm text-[rgba(232,228,217,0.3)]">
        هنوز بازی‌ای ثبت نشده
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
              <th class="px-3 py-2 text-right">عنوان</th>
              <th class="px-3 py-2 text-right">تاریخ</th>
              <th class="px-3 py-2 text-right">سالن</th>
              <th class="px-3 py-2 text-right">سناریو</th>
              <th class="px-3 py-2 text-right">تعداد</th>
              <th class="px-3 py-2 text-right">وضعیت</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="play in recentPlays" :key="play.id" @click="goToPlay(play.id)" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[rgba(255,255,255,0.02)] transition cursor-pointer text-[#e8e4d9]">
              <td class="px-3 py-2.5 font-medium">{{ play.title || 'بدون عنوان' }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ formatDate(play.dateTime) }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.roomName }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.senarioName }}</td>
              <td class="px-3 py-2.5 text-[rgba(232,228,217,0.5)]">{{ play.playersCount }}</td>
              <td class="px-3 py-2.5"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <div v-else class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { MasterApi, ClubPlayApi } from '@/api'
import type { MasterContextDto, ClubPlayListItemDto, MasterStatsDto } from '@/types/clubPlay'

const router = useRouter()

function toDateString(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const todayStr = toDateString(new Date())
const viewedDate = ref(todayStr)
const isToday = computed(() => viewedDate.value === todayStr)
const totalEntries = computed(() =>
  todaysPlays.value.reduce((sum, p) => sum + (p.playersCount - p.guestCount), 0)
)

const ctx = ref<MasterContextDto | null>(null)
const todaysPlays = ref<ClubPlayListItemDto[]>([])
const todaysLoading = ref(true)
const weekStats = ref<MasterStatsDto | null>(null)
const weeklyLoading = ref(true)
const monthStats = ref<MasterStatsDto | null>(null)
const monthlyLoading = ref(true)
const recentPlays = ref<ClubPlayListItemDto[]>([])
const recentLoading = ref(true)
const openPlays = ref<ClubPlayListItemDto[]>([])
const openLoading = ref(true)

function navigateDate(delta: number) {
  const d = new Date(viewedDate.value + 'T12:00:00')
  d.setDate(d.getDate() + delta)
  viewedDate.value = toDateString(d)
  fetchTodaysPlays()
}

function formatHeadingDate(dateStr: string) {
  const d = new Date(dateStr + 'T12:00:00')
  return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' })
}

function formatTime(dt: string) {
  const d = new Date(dt)
  return d.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })
}

function formatDate(dt: string) {
  const d = new Date(dt)
  return d.toLocaleDateString('fa-IR', { month: 'short', day: 'numeric' })
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

function goToPlay(id: number) {
  router.push({ name: 'MasterPlayDetail', params: { id } })
}

let clubIdCached = 0

async function fetchTodaysPlays() {
  if (!clubIdCached) return
  todaysLoading.value = true
  try {
    const res = await ClubPlayApi.getPlaysByBusinessDate(clubIdCached, viewedDate.value)
    todaysPlays.value = res.data
  } catch {
    todaysPlays.value = []
  } finally {
    todaysLoading.value = false
  }
}

onMounted(async () => {
  try {
    const ctxRes = await MasterApi.getMasterContext()
    ctx.value = ctxRes.data
    clubIdCached = ctxRes.data.clubId

    const [todaysRes, weekRes, monthRes, recentRes, openRes] = await Promise.all([
      ClubPlayApi.getPlaysByBusinessDate(clubIdCached, viewedDate.value),
      ClubPlayApi.getMyStats(clubIdCached, 'week'),
      ClubPlayApi.getMyStats(clubIdCached, 'month'),
      ClubPlayApi.getMyPlays(clubIdCached, { page: 1, pageSize: 5 }),
      ClubPlayApi.getOpenPlays(clubIdCached),
    ])
    todaysPlays.value = todaysRes.data
    weekStats.value = weekRes.data
    monthStats.value = monthRes.data
    recentPlays.value = recentRes.data.items
    openPlays.value = openRes.data
  } catch {
    // handled
  } finally {
    todaysLoading.value = false
    weeklyLoading.value = false
    monthlyLoading.value = false
    recentLoading.value = false
    openLoading.value = false
  }
})
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
</style>