<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl md:text-3xl font-bold text-[#e8e4d9]">مدیریت کافه‌ها</h1>
      <router-link
        to="/admin/clubs/create"
        class="px-4 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center gap-2"
      >
        کافه جدید
      </router-link>
    </div>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm border" :class="notification.type === 'success' ? 'bg-[rgba(111,207,138,0.1)] border-[rgba(111,207,138,0.2)] text-[#6fcf8a]' : 'bg-[rgba(224,112,112,0.1)] border-[rgba(224,112,112,0.2)] text-[#e07070]'">
      {{ notification.message }}
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="clubs.length === 0" class="text-center py-20 text-[rgba(232,228,217,0.4)] text-lg">
      هیچ کافه‌ای ثبت نشده است.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e] text-[rgba(232,228,217,0.5)]">
            <th class="px-4 py-3 text-right">نام کافه</th>
            <th class="px-4 py-3 text-right">اتاق‌ها</th>
            <th class="px-4 py-3 text-right">گردانندگان</th>
            <th class="px-4 py-3 text-right">عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="club in clubs"
            :key="club.id"
            class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e] transition text-[#e8e4d9]"
          >
            <td class="px-4 py-3 font-medium">{{ club.name }}</td>
            <td class="px-4 py-3 text-[rgba(232,228,217,0.4)]">{{ club.roomCount }}</td>
            <td class="px-4 py-3 text-[rgba(232,228,217,0.4)]">{{ club.masterCount }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-2">
                <router-link
                  :to="`/admin/clubs/${club.id}`"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-[#c9b07a] text-[#0d0d0f] hover:bg-[#b8a16e] transition"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M10 12a2 2 0 100-4 2 2 0 000 4z"/>
                    <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd"/>
                  </svg>
                  مشاهده
                </router-link>
                <button
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-[#e07070] text-[#0d0d0f] hover:bg-[#d06060] transition"
                  @click="confirmDelete(club)"
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
    </div>

    <ConfirmModal
      :is-open="showDeleteModal"
      title="حذف کافه"
      :message="deleteMessage"
      confirm-text="بله، حذف کن"
      cancel-text="انصراف"
      :is-loading="isDeleting"
      @confirm="handleDelete"
      @cancel="closeDeleteModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ClubApi } from '@/api'
import { useToast } from '@/composables/useToast'
import ConfirmModal from '@/components/ConfirmModal.vue'

interface ClubRow {
  id: number
  name: string
  roomCount: number
  masterCount: number
}

const clubs = ref<ClubRow[]>([])
const loading = ref(true)
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)
const { toastWarning } = useToast()

const showDeleteModal = ref(false)
const clubToDelete = ref<ClubRow | null>(null)
const isDeleting = ref(false)

const deleteMessage = computed(() =>
  `آیا مطمئن هستید که می‌خواهید کافه «${clubToDelete.value?.name}» را حذف کنید؟`
)

function showNotification(type: 'success' | 'error', message: string) {
  notification.value = { type, message }
  setTimeout(() => { notification.value = null }, 4000)
}

async function fetchClubs() {
  loading.value = true
  try {
    const res = await ClubApi.getClubs()
    const items = res.data
    const details = await Promise.all(
      items.map(c => ClubApi.getClubDetail(c.id).then(r => r.data).catch(() => null))
    )
    clubs.value = items.map((c, i) => {
      const d = details[i]
      return {
        id: c.id,
        name: c.name,
        roomCount: d ? d.rooms.length : 0,
        masterCount: d ? d.masters.length : 0,
      }
    })
  } catch {
    clubs.value = []
  } finally {
    loading.value = false
  }
}

function confirmDelete(club: ClubRow) {
  clubToDelete.value = club
  showDeleteModal.value = true
}

function closeDeleteModal() {
  showDeleteModal.value = false
  clubToDelete.value = null
}

async function handleDelete() {
  if (!clubToDelete.value) return
  isDeleting.value = true
  try {
    await ClubApi.deleteClub(clubToDelete.value.id)
    showDeleteModal.value = false
    showNotification('success', 'کافه با موفقیت حذف شد')
    clubToDelete.value = null
    await fetchClubs()
  } catch (e: unknown) {
    showDeleteModal.value = false
    clubToDelete.value = null
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        toastWarning(err.response?.data?.message || 'این کافه دارای بازی ثبت‌شده است و قابل حذف نیست')
      } else {
        showNotification('error', err.response?.data?.message || 'خطا در حذف کافه')
      }
    } else {
      showNotification('error', 'خطا در برقراری ارتباط')
    }
  } finally {
    isDeleting.value = false
  }
}

onMounted(() => {
  fetchClubs()
})
</script>
