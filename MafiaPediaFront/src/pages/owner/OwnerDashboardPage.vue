<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-gold-text">داشبورد مدیر کافه</h1>
      <p class="text-sm text-muted mt-1">{{ authStore.activeClubContext?.clubName }}</p>
    </div>

    <ClubTodaysPlaysCard :club-id="authStore.activeClubId!" />
    <ClubStatsCard :club-id="authStore.activeClubId!" />
    <MasterPerformanceTable :club-id="authStore.activeClubId!" />

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <router-link to="/owner/members" class="block bg-surface border border-border rounded-xl p-6 hover:border-gold/30 transition text-center">
        <div class="text-2xl mb-2">👥</div>
        <div class="text-sm font-bold text-gold-text">مدیریت اعضا</div>
        <div class="text-xs text-muted mt-1">مدیریت نقش‌های سوپروایزر و صندوق‌دار</div>
      </router-link>
      <router-link to="/owner/deleted-plays" class="block bg-surface border border-border rounded-xl p-6 hover:border-gold/30 transition text-center">
        <div class="text-2xl mb-2">🗑️</div>
        <div class="text-sm font-bold text-gold-text">بازی‌های حذف‌شده</div>
        <div class="text-xs text-muted mt-1">مشاهده بازی‌های حذف‌شده کافه</div>
      </router-link>
      <button @click="openClubSettings" class="block bg-surface border border-border rounded-xl p-6 hover:border-gold/30 transition text-center w-full">
        <div class="text-2xl mb-2">⚙️</div>
        <div class="text-sm font-bold text-gold-text">تنظیمات کافه</div>
        <div class="text-xs text-muted mt-1">ویرایش اطلاعات و مالیات کافه</div>
      </button>
    </div>

    <!-- Club Settings Modal -->
    <Modal :is-open="showClubSettings" title="تنظیمات کافه" max-width="md" @close="closeClubSettings">
      <div class="flex flex-col gap-3">
        <div>
          <label class="text-sm text-muted">نام کافه <span class="text-danger">*</span></label>
          <input v-model="clubForm.name" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div>
          <label class="text-sm text-muted">شهر</label>
          <input v-model="clubForm.city" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div>
          <label class="text-sm text-muted">آدرس</label>
          <input v-model="clubForm.address" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div>
          <label class="text-sm text-muted">تلفن</label>
          <input v-model="clubForm.phone" type="text" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
        </div>
        <div>
          <label class="text-sm text-muted">توضیحات</label>
          <textarea v-model="clubForm.description" rows="2" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition resize-none" />
        </div>
        <div>
          <label class="text-sm text-muted">لوگو</label>
          <input type="file" accept="image/jpeg,image/png,image/webp" class="w-full text-sm text-muted file:mr-2 file:py-1.5 file:px-3 file:rounded file:border-0 file:text-xs file:bg-gold/15 file:text-gold-text hover:file:bg-gold/25 transition cursor-pointer" @change="onClubLogoChange" />
          <img v-if="clubLogoPreview" :src="clubLogoPreview" class="mt-2 w-20 h-20 rounded object-cover border border-border" />
        </div>
        <div>
          <label class="text-sm text-muted">درصد مالیات بر ارزش افزوده</label>
          <input v-model.number="clubForm.vatPercent" type="number" min="0" max="100" step="0.01" class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition ltr" />
          <p class="text-xs text-muted mt-1">اگر خالی بماند، مالیات محاسبه نمی‌شود. (فقط قابل ویرایش توسط owner)</p>
        </div>
        <p v-if="clubSettingsError" class="text-xs text-danger">{{ clubSettingsError }}</p>
      </div>
      <template #footer>
        <button @click="closeClubSettings" class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition">انصراف</button>
        <button @click="saveClubSettings" :disabled="!clubForm.name.trim() || savingClub" class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center gap-2">
          <div v-if="savingClub" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ذخیره
        </button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useToast } from '@/composables/useToast'
import { ClubApi } from '@/api'
import Modal from '@/components/shared/Modal.vue'
import ClubTodaysPlaysCard from '@/components/club/ClubTodaysPlaysCard.vue'
import ClubStatsCard from '@/components/club/ClubStatsCard.vue'
import MasterPerformanceTable from '@/components/club/MasterPerformanceTable.vue'

const authStore = useAuthStore()
const { toastSuccess } = useToast()

const showClubSettings = ref(false)
const savingClub = ref(false)
const clubSettingsError = ref('')
const clubLogoFile = ref<File | null>(null)
const clubLogoPreview = ref('')

const clubForm = reactive({
  name: '',
  address: '',
  phone: '',
  city: '',
  description: '',
  vatPercent: null as number | null,
})

async function openClubSettings() {
  clubSettingsError.value = ''
  clubLogoFile.value = null
  clubLogoPreview.value = ''
  if (!authStore.activeClubId) return
  try {
    const res = await ClubApi.getClubDetail(authStore.activeClubId)
    const c = res.data
    clubForm.name = c.name
    clubForm.address = c.address || ''
    clubForm.phone = c.phone || ''
    clubForm.city = c.city || ''
    clubForm.description = c.description || ''
    clubForm.vatPercent = c.vatPercent
    showClubSettings.value = true
  } catch {
    clubSettingsError.value = 'خطا در دریافت اطلاعات کافه'
  }
}

function closeClubSettings() {
  showClubSettings.value = false
  if (clubLogoPreview.value) {
    URL.revokeObjectURL(clubLogoPreview.value)
    clubLogoPreview.value = ''
  }
}

function onClubLogoChange(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return
  clubLogoFile.value = file
  if (clubLogoPreview.value) URL.revokeObjectURL(clubLogoPreview.value)
  clubLogoPreview.value = URL.createObjectURL(file)
}

async function saveClubSettings() {
  if (!authStore.activeClubId || !clubForm.name.trim()) return
  savingClub.value = true
  clubSettingsError.value = ''
  try {
    const dto = {
      name: clubForm.name.trim(),
      address: clubForm.address || null,
      phone: clubForm.phone || null,
      city: clubForm.city || null,
      description: clubForm.description || null,
      vatPercent: clubForm.vatPercent,
    }
    await ClubApi.updateClub(authStore.activeClubId, dto, clubLogoFile.value ?? undefined)
    closeClubSettings()
    toastSuccess('اطلاعات کافه با موفقیت ویرایش شد')
  } catch {
    clubSettingsError.value = 'خطا در ویرایش کافه'
  } finally {
    savingClub.value = false
  }
}
</script>
