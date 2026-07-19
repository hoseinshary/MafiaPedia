<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link v-if="isAdmin" :to="`/admin/clubs/${clubId}`" class="text-xs text-muted hover:text-gold-text transition">&larr; بازگشت</router-link>
      <h1 class="text-xl font-bold text-gold-text">بازی‌های حذف‌شده</h1>
      <p class="text-sm text-muted mt-1">{{ clubName }}</p>
    </div>

    <div class="bg-surface border border-border rounded-xl overflow-hidden">
      <div v-if="loading" class="flex justify-center py-16">
        <div class="w-8 h-8 border-2 border-gold border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else-if="plays.length === 0" class="text-center py-16 text-sm text-muted">
        هیچ بازی حذف‌شده‌ای یافت نشد
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted bg-surface-hover">
              <th class="px-4 py-3 text-right">عنوان</th>
              <th class="px-4 py-3 text-right">گرداننده</th>
              <th class="px-4 py-3 text-right">تاریخ بازی</th>
              <th class="px-4 py-3 text-right">وضعیت</th>
              <th class="px-4 py-3 text-right">تاریخ حذف</th>
              <th class="px-4 py-3 text-right">حذف‌کننده</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="play in plays" :key="play.id" class="border-b border-border text-fg">
              <td class="px-4 py-3 font-medium">{{ play.title || 'بدون عنوان' }}</td>
              <td class="px-4 py-3 text-muted">{{ play.masterName }}</td>
              <td class="px-4 py-3 text-muted">{{ formatDate(play.dateTime) }}</td>
              <td class="px-4 py-3"><span class="status-badge" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span></td>
              <td class="px-4 py-3 text-muted">{{ formatDate(play.deletedAt) }}</td>
              <td class="px-4 py-3 text-muted">{{ play.deletedByDisplayName || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-6">
      <button @click="loadPage(page - 1)" :disabled="page <= 1" class="px-3 py-1.5 text-sm rounded border border-border text-muted hover:text-fg disabled:opacity-30 disabled:cursor-not-allowed transition">قبلی</button>
      <span class="text-sm text-muted">صفحه {{ page }} از {{ totalPages }}</span>
      <button @click="loadPage(page + 1)" :disabled="page >= totalPages" class="px-3 py-1.5 text-sm rounded border border-border text-muted hover:text-fg disabled:opacity-30 disabled:cursor-not-allowed transition">بعدی</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ClubPlayApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { ClubPlayDeletedListItemDto } from '@/types/clubPlay'

const route = useRoute()
const authStore = useAuthStore()

const isAdmin = authStore.isAdmin
const clubId = ref<number>(isAdmin ? Number(route.params.id) : authStore.activeClubId!)
const clubName = ref(isAdmin ? '' : authStore.activeClubContext?.clubName ?? '')
const plays = ref<ClubPlayDeletedListItemDto[]>([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)
const pageSize = 20

function formatDate(dt: string | null | undefined) {
  if (!dt) return '—'
  return new Date(dt).toLocaleDateString('fa-IR', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
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

async function loadPage(p: number) {
  if (!clubId.value) return
  loading.value = true
  page.value = p
  try {
    const res = await ClubPlayApi.getDeletedPlays(clubId.value, { page: p, pageSize })
    plays.value = res.data.items
    totalPages.value = Math.ceil(res.data.total / pageSize)
  } catch {
    // handled
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadPage(1)
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
