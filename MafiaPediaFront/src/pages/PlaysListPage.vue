<template>
  <div dir="rtl" class="w-full md:w-4/5 mx-auto">
    <h1 class="text-2xl md:text-3xl font-bold mb-6">لیست بازی‌ها</h1>

    <div class="mb-6">
      <input
        v-model="searchQuery"
        type="text"
        placeholder="جستجوی بازی..."
        class="w-full md:w-1/2 border border-gray-300 rounded px-4 py-2.5 text-sm focus:outline-none focus:border-red-500 transition"
      />
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-4 border-gray-300 border-t-red-500 rounded-full animate-spin" />
    </div>

    <div v-else-if="plays.length === 0" class="text-center py-20 text-gray-400 text-lg">
      هیچ بازی‌ای یافت نشد.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-gray-300 bg-gray-100">
            <th class="px-4 py-3 text-right">ردیف</th>
            <th class="px-4 py-3 text-right">عنوان</th>
            <th class="px-4 py-3 text-right">تاریخ</th>
            <th class="px-4 py-3 text-right">سناریو</th>
            <th class="px-4 py-3 text-right">برنده</th>
            <th class="px-4 py-3 text-right">رویداد</th>
            <th class="px-4 py-3 text-right">لینک</th>
            <th v-if="isAdmin" class="px-4 py-3 text-right">عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(play, index) in plays"
            :key="play.id"
            class="border-b border-gray-200 hover:bg-gray-50 transition"
          >
            <td class="px-4 py-3 text-gray-500">{{ (page - 1) * pageSize + index + 1 }}</td>
            <td class="px-4 py-3 font-medium">{{ play.title }}</td>
            <td class="px-4 py-3 text-gray-500 whitespace-nowrap" dir="ltr">{{ formatDate(play.dateTime) }}</td>
            <td class="px-4 py-3">{{ play.senarioName }}</td>
            <td class="px-4 py-3">
              <span
                class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                :class="play.winnersideId === 1 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
              >
                {{ play.winnersideName }}
              </span>
            </td>
            <td class="px-4 py-3 text-gray-500">{{ play.eventName }}</td>
            <td class="px-4 py-3">
              <a
                v-if="play.link"
                :href="play.link"
                target="_blank"
                rel="noopener noreferrer"
                class="inline-flex items-center justify-center text-red-600 hover:text-red-500 transition"
                title="مشاهده در یوتیوب"
              >
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
                </svg>
              </a>
              <span v-else class="text-gray-400">—</span>
            </td>
            <td v-if="isAdmin" class="px-4 py-3">
              <div class="flex items-center gap-2">
                <router-link
                  :to="`/plays/${play.id}/edit`"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-blue-600 text-white hover:bg-blue-700 transition"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z"/>
                  </svg>
                  ویرایش
                </router-link>
                <button
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-red-600 text-white hover:bg-red-700 transition"
                  @click="confirmDelete(play)"
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
        <span class="text-gray-500">
          صفحه {{ page }} از {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 rounded border border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition"
            @click="page = Math.max(1, page - 1)"
          >
            قبلی
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 rounded border border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition"
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
    title="حذف بازی"
    :message="deleteMessage"
    confirm-text="بله، حذف کن"
    cancel-text="انصراف"
    :is-loading="isDeleting"
    @confirm="handleDelete"
    @cancel="closeDeleteModal"
  />
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { PlaysApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import type { PlayDto } from '@/types'
import ConfirmModal from '@/components/ConfirmModal.vue'

const authStore = useAuthStore()
const isAdmin = authStore.isAdmin

const plays = ref<PlayDto[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = 20
const totalPages = ref(1)
const totalItems = ref(0)
const searchQuery = ref('')

const showDeleteModal = ref(false)
const playToDelete = ref<{ id: number; title: string } | null>(null)
const isDeleting = ref(false)

const deleteMessage = computed(() =>
  `آیا مطمئن هستید که می‌خواهید بازی «${playToDelete.value?.title}» را حذف کنید؟ تمام اطلاعات بازیکنان این بازی نیز حذف خواهد شد.`
)

let debounceTimer: ReturnType<typeof setTimeout>

async function fetchPlays() {
  loading.value = true
  try {
    const res = await PlaysApi.getPlays(page.value, pageSize, searchQuery.value || undefined)
    plays.value = res.data.items
    totalItems.value = res.data.totalItems
    totalPages.value = res.data.totalPages || Math.max(1, Math.ceil(res.data.totalItems / pageSize))
  } catch {
    plays.value = []
  } finally {
    loading.value = false
  }
}

function formatDate(dateStr: string): string {
  try {
    return new Intl.DateTimeFormat('fa-IR', { year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

function confirmDelete(play: PlayDto) {
  playToDelete.value = { id: play.id, title: play.title }
  showDeleteModal.value = true
}

function closeDeleteModal() {
  showDeleteModal.value = false
  playToDelete.value = null
}

async function handleDelete() {
  if (!playToDelete.value) return
  isDeleting.value = true
  try {
    await PlaysApi.deletePlay(playToDelete.value.id)
    showDeleteModal.value = false
    playToDelete.value = null
    await fetchPlays()
  } catch {
    //
  } finally {
    isDeleting.value = false
  }
}

watch(searchQuery, () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    page.value = 1
    fetchPlays()
  }, 400)
})

watch(page, () => {
  fetchPlays()
})

onMounted(() => {
  fetchPlays()
})
</script>