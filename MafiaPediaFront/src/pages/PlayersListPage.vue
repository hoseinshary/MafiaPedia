<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <h1 class="text-2xl md:text-3xl font-bold mb-6 text-[#e8e4d9]">لیست بازیکنان</h1>

    <div class="mb-6">
      <input
        v-model="searchQuery"
        type="text"
        placeholder="جستجوی بازیکن..."
        class="w-full md:w-1/2 bg-[#141416] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="players.length === 0" class="text-center py-20 text-[rgba(232,228,217,0.4)] text-lg">
      هیچ بازیکنی یافت نشد.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e] text-[rgba(232,228,217,0.5)]">
            <th class="px-4 py-3 text-right">عکس</th>
            <th class="px-4 py-3 text-right">نام</th>
            <th class="px-4 py-3 text-right">کد</th>
            <th class="px-4 py-3 text-right">تعداد بازی</th>
            <th v-if="isAdmin" class="px-4 py-3 text-right">عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="player in players"
            :key="player.id"
            class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e] transition text-[#e8e4d9]"
          >
            <td class="px-4 py-3">
              <img
                v-if="player.picture"
                :src="getPictureUrl(player.picture)"
                class="w-9 h-9 rounded-full object-cover border border-[rgba(201,176,122,0.2)]"
                :alt="player.name"
              />
              <div
                v-else
                class="w-9 h-9 rounded-full flex items-center justify-center text-white text-sm font-bold select-none"
                :style="{ backgroundColor: avatarColor(player.name) }"
              >
                {{ player.name.charAt(0) }}
              </div>
            </td>
            <td class="px-4 py-3 font-medium">{{ player.name }}</td>
            <td class="px-4 py-3 text-[rgba(232,228,217,0.4)]">{{ player.code || '—' }}</td>
            <td class="px-4 py-3">{{ player.totalGames }}</td>
            <td v-if="isAdmin" class="px-4 py-3">
              <div class="flex items-center gap-2">
                <router-link
                  :to="`/admin/players/${player.id}/edit`"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-[#c9b07a] text-[#0d0d0f] hover:bg-[#b8a16e] transition"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z"/>
                  </svg>
                  ویرایش
                </router-link>
                <button
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-[#e07070] text-[#0d0d0f] hover:bg-[#d06060] transition"
                  @click="confirmDelete(player)"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd"/>
                  </svg>
                  حذف
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="flex items-center justify-between gap-4 mt-6 text-sm">
        <span class="text-[rgba(232,228,217,0.4)]">
          صفحه {{ page }} از {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] disabled:opacity-40 disabled:cursor-not-allowed hover:bg-[#1a1a1e] transition"
            @click="page = Math.max(1, page - 1)"
          >
            قبلی
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] disabled:opacity-40 disabled:cursor-not-allowed hover:bg-[#1a1a1e] transition"
            @click="page = Math.min(totalPages, page + 1)"
          >
            بعدی
          </button>
        </div>
      </div>
    </div>
  </div>

  <ConfirmModal
    :is-open="showDeleteModal"
    title="حذف بازیکن"
    :message="deleteMessage"
    confirm-text="بله، حذف کن"
    cancel-text="انصراف"
    :is-loading="isDeleting"
    @confirm="handleDelete"
    @cancel="closeDeleteModal"
  />
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { PlayerApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { PlayerListItem } from '@/types'
import ConfirmModal from '@/components/ConfirmModal.vue'

const authStore = useAuthStore()
const isAdmin = authStore.isAdmin
const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api'

const route = useRoute()

const players = ref<PlayerListItem[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = 20
const totalPages = ref(1)
const searchQuery = ref((route.query.search as string) || '')

const showDeleteModal = ref(false)
const playerToDelete = ref<{ id: number; name: string } | null>(null)
const isDeleting = ref(false)
const deleteError = ref('')

const deleteMessage = computed(() => {
  if (deleteError.value) return deleteError.value
  return `آیا مطمئن هستید که می‌خواهید «${playerToDelete.value?.name}» را حذف کنید؟ این عملیات قابل بازگشت نیست.`
})

let debounceTimer: ReturnType<typeof setTimeout>

const avatarColors = [
  '#ef4444', '#f97316', '#eab308', '#22c55e', '#3b82f6',
  '#6366f1', '#a855f7', '#ec4899', '#14b8a6', '#f43f5e',
]

function avatarColor(name: string): string {
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function getPictureUrl(picture: string): string {
  if (picture.startsWith('http')) return picture
  const base = baseUrl.replace(/\/api$/, '')
  return `${base}${picture.startsWith('/') ? '' : '/'}${picture}`
}

async function fetchPlayers() {
  loading.value = true
  try {
    const res = await PlayerApi.getPlayers(page.value, pageSize, searchQuery.value || undefined)
    players.value = res.data.items
    totalPages.value = res.data.totalPages || Math.max(1, Math.ceil(res.data.totalItems / pageSize))
  } catch {
    players.value = []
  } finally {
    loading.value = false
  }
}

function confirmDelete(player: PlayerListItem) {
  playerToDelete.value = { id: player.id, name: player.name }
  deleteError.value = ''
  showDeleteModal.value = true
}

function closeDeleteModal() {
  showDeleteModal.value = false
  playerToDelete.value = null
  deleteError.value = ''
}

async function handleDelete() {
  if (!playerToDelete.value) return
  isDeleting.value = true
  deleteError.value = ''
  try {
    await PlayerApi.deletePlayer(playerToDelete.value.id)
    showDeleteModal.value = false
    playerToDelete.value = null
    await fetchPlayers()
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        deleteError.value = err.response?.data?.message || 'این بازیکن دارای بازی است و قابل حذف نیست'
      } else {
        deleteError.value = err.response?.data?.message || 'خطا در حذف بازیکن'
      }
    } else {
      deleteError.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    isDeleting.value = false
  }
}

watch(searchQuery, () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    page.value = 1
    fetchPlayers()
  }, 400)
})

watch(page, () => {
  fetchPlayers()
})

onMounted(() => {
  fetchPlayers()
})
</script>
