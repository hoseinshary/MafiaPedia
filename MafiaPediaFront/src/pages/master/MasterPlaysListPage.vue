<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full" v-if="ctx">
    <div class="mb-6">
      <h1 class="text-xl font-bold text-[#c9b07a]">لیست بازی‌ها</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ ctx.clubName }}</p>
    </div>

    <!-- Filters -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-4 mb-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div>
          <label class="text-xs text-[rgba(232,228,217,0.4)] block mb-1">از تاریخ</label>
          <input v-model="filters.dateFrom" type="date" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" />
        </div>
        <div>
          <label class="text-xs text-[rgba(232,228,217,0.4)] block mb-1">تا تاریخ</label>
          <input v-model="filters.dateTo" type="date" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" />
        </div>
        <div>
          <label class="text-xs text-[rgba(232,228,217,0.4)] block mb-1">وضعیت</label>
          <select v-model="filters.status" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition">
            <option value="">همه</option>
            <option value="pending">در حال پخش</option>
            <option value="notwinside">ثبت برنده</option>
            <option value="notrank">ثبت رتبه</option>
            <option value="done">کامل شد</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="loadPlays(1)" class="w-full px-4 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition">اعمال فیلتر</button>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl overflow-hidden">
      <div v-if="loading" class="flex justify-center py-16">
        <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else-if="plays.length === 0" class="text-center py-16 text-sm text-[rgba(232,228,217,0.3)]">
        هیچ بازی‌ای یافت نشد
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)] bg-[#1a1a1e]">
              <th class="px-4 py-3 text-right">عنوان</th>
              <th class="px-4 py-3 text-right">تاریخ</th>
              <th class="px-4 py-3 text-right">ساعت</th>
              <th class="px-4 py-3 text-right">سالن</th>
              <th class="px-4 py-3 text-right">سناریو</th>
              <th class="px-4 py-3 text-right">نوع بازی</th>
              <th class="px-4 py-3 text-right">تعداد</th>
              <th class="px-4 py-3 text-right">وضعیت</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="play in plays" :key="play.id" @click="goToPlay(play.id)" class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[rgba(255,255,255,0.02)] transition cursor-pointer text-[#e8e4d9]">
              <td class="px-4 py-3 font-medium">{{ play.title || 'بدون عنوان' }}</td>
              <td class="px-4 py-3 text-[rgba(232,228,217,0.5)]">{{ formatDate(play.dateTime) }}</td>
              <td class="px-4 py-3 text-[rgba(232,228,217,0.5)]">{{ formatTime(play.dateTime) }}</td>
              <td class="px-4 py-3 text-[rgba(232,228,217,0.5)]">{{ play.roomName }}</td>
              <td class="px-4 py-3 text-[rgba(232,228,217,0.5)]">{{ play.senarioName }}</td>
              <td class="px-4 py-3"><span class="status-badge" :class="playTypeClass(play.playType)">{{ playTypeLabel(play.playType) }}</span></td>
              <td class="px-4 py-3 text-[rgba(232,228,217,0.5)]">{{ play.playersCount }}</td>
              <td class="px-4 py-3"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-6">
      <button @click="loadPlays(page - 1)" :disabled="page <= 1" class="px-3 py-1.5 text-sm rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)] hover:text-[#e8e4d9] disabled:opacity-30 disabled:cursor-not-allowed transition">قبلی</button>
      <span class="text-sm text-[rgba(232,228,217,0.4)]">صفحه {{ page }} از {{ totalPages }}</span>
      <button @click="loadPlays(page + 1)" :disabled="page >= totalPages" class="px-3 py-1.5 text-sm rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)] hover:text-[#e8e4d9] disabled:opacity-30 disabled:cursor-not-allowed transition">بعدی</button>
    </div>
  </div>
  <div v-else class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { MasterApi, ClubPlayApi } from '@/api'
import type { MasterContextDto, ClubPlayListItemDto } from '@/types/clubPlay'

const router = useRouter()

const ctx = ref<MasterContextDto | null>(null)
const plays = ref<ClubPlayListItemDto[]>([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)
const pageSize = 20

const filters = reactive({
  dateFrom: '',
  dateTo: '',
  status: '',
})

function formatDate(dt: string) {
  const d = new Date(dt)
  return d.toLocaleDateString('fa-IR', { month: 'short', day: 'numeric' })
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

async function loadPlays(p: number) {
  if (!ctx.value) return
  loading.value = true
  page.value = p
  try {
    const params: Record<string, string | number> = { page: p, pageSize }
    if (filters.dateFrom) params.dateFrom = filters.dateFrom
    if (filters.dateTo) params.dateTo = filters.dateTo
    if (filters.status) params.status = filters.status
    const res = await ClubPlayApi.getMyPlays(ctx.value.clubId, params)
    plays.value = res.data.items
    totalPages.value = Math.ceil(res.data.total / pageSize)
  } catch {
    // handled
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  try {
    const ctxRes = await MasterApi.getMasterContext()
    ctx.value = ctxRes.data
    await loadPlays(1)
  } catch {
    ctx.value = null
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