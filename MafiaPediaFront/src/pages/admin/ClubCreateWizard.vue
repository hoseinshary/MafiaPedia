<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <div class="flex items-center justify-center gap-2 mb-8">
      <div
        v-for="s in 4"
        :key="s"
        class="flex items-center gap-2"
      >
        <div
          class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold transition"
          :class="s === currentStep ? 'bg-gold text-[#0d0d0f]' : s < currentStep ? 'bg-success/30 text-success' : 'bg-border text-muted'"
        >
          {{ s < currentStep ? '✓' : s }}
        </div>
        <span v-if="s < 4" class="w-8 h-px" :class="s < currentStep ? 'bg-success' : 'bg-border'" />
      </div>
    </div>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm border" :class="notification.type === 'success' ? 'bg-success/20 border-success text-success' : 'bg-danger/20 border-danger text-danger'">
      {{ notification.message }}
    </div>

    <!-- Step 1: Club Info -->
    <div v-if="currentStep === 1" class="max-w-md mx-auto">
      <h2 class="text-xl font-bold text-fg mb-6">اطلاعات کافه</h2>
      <div class="flex flex-col gap-3 mb-6">
        <div>
          <label class="text-sm text-muted">نام کافه <span class="text-danger">*</span></label>
          <input
            v-model="clubName"
            type="text"
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>
        <div>
          <label class="text-sm text-muted">شهر</label>
          <input
            v-model="clubCity"
            type="text"
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>
        <div>
          <label class="text-sm text-muted">آدرس</label>
          <input
            v-model="clubAddress"
            type="text"
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>
        <div>
          <label class="text-sm text-muted">تلفن</label>
          <input
            v-model="clubPhone"
            type="text"
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>
        <div>
          <label class="text-sm text-muted">توضیحات</label>
          <textarea
            v-model="clubDescription"
            rows="3"
            class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition resize-none"
          />
        </div>
        <p v-if="step1Error" class="text-xs text-danger">{{ step1Error }}</p>
      </div>
      <div class="flex justify-end">
        <button
          @click="goStep2"
          :disabled="!clubName.trim() || creatingClub"
          class="px-6 py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center gap-2"
        >
          <div v-if="creatingClub" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          بعدی
        </button>
      </div>
    </div>

    <!-- Step 2: Rooms -->
    <div v-if="currentStep === 2">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-bold text-fg">اتاق‌ها</h2>
        <button
          @click="openRoomModal(null)"
          class="px-3 py-1.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-xs rounded font-medium transition"
        >
          افزودن اتاق
        </button>
      </div>
      <div v-if="rooms.length > 0" class="space-y-2 mb-6">
        <div
          v-for="r in rooms"
          :key="r.id"
          class="flex items-center justify-between bg-surface-hover border border-border rounded px-4 py-2.5"
        >
          <div class="flex items-center gap-2">
            <span class="text-sm text-fg">{{ r.name }}</span>
            <span v-if="!r.isActive" class="text-xs text-danger">(غیرفعال)</span>
          </div>
          <div class="flex items-center gap-2">
            <button @click="openRoomModal(r)" class="text-xs text-muted hover:text-gold-text transition">ویرایش</button>
            <button @click="confirmDeleteRoom(r)" class="text-xs text-danger hover:underline">حذف</button>
          </div>
        </div>
      </div>
      <div v-else class="text-center py-6 text-sm text-muted mb-6">
        هنوز اتاقی اضافه نشده (می‌توانید بعداً اضافه کنید)
      </div>
      <div class="flex justify-between gap-4">
        <button
          @click="currentStep = 1"
          class="px-6 py-2.5 border border-border text-muted hover:text-fg text-sm rounded font-medium transition"
        >
          قبلی
        </button>
        <button
          @click="currentStep = 3"
          class="px-6 py-2.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-medium transition"
        >
          بعدی
        </button>
      </div>
    </div>

    <!-- Step 3: Masters -->
    <div v-if="currentStep === 3">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-bold text-fg">گردانندگان</h2>
        <button
          @click="openMasterModal(null)"
          class="px-3 py-1.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-xs rounded font-medium transition"
        >
          افزودن گرداننده
        </button>
      </div>
      <div v-if="masters.length > 0" class="space-y-2 mb-6">
        <div
          v-for="m in masters"
          :key="m.id"
          class="flex items-center justify-between bg-surface-hover border border-border rounded px-4 py-2.5"
        >
          <div class="flex items-center gap-3">
            <span class="text-sm text-fg">{{ m.name }}</span>
            <span v-if="m.userDisplayName" class="text-xs text-muted">({{ m.userDisplayName }})</span>
          </div>
          <div class="flex items-center gap-2">
            <button @click="openMasterModal(m)" class="text-xs text-muted hover:text-gold-text transition">ویرایش</button>
            <button @click="confirmDeleteMaster(m)" class="text-xs text-danger hover:underline">حذف</button>
          </div>
        </div>
      </div>
      <div v-else class="text-center py-6 text-sm text-muted mb-6">
        هنوز گرداننده‌ای اضافه نشده (می‌توانید بعداً اضافه کنید)
      </div>

      <div class="flex justify-between gap-4">
        <button
          @click="currentStep = 2"
          class="px-6 py-2.5 border border-border text-muted hover:text-fg text-sm rounded font-medium transition"
        >
          قبلی
        </button>
        <button
          @click="currentStep = 4"
          class="px-6 py-2.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-medium transition"
        >
          بعدی
        </button>
      </div>
    </div>

    <!-- Step 4: Review -->
    <div v-if="currentStep === 4">
      <h2 class="text-xl font-bold text-fg mb-6">مرور اطلاعات</h2>
      <div class="space-y-4">
        <div class="bg-surface-hover border border-border rounded p-4">
          <p class="text-sm text-muted mb-1">نام کافه</p>
          <p class="text-fg font-medium">{{ clubName }}</p>
          <p v-if="clubCity" class="text-xs text-muted mt-1">{{ clubCity }}</p>
          <p v-if="clubAddress" class="text-xs text-muted">{{ clubAddress }}</p>
          <p v-if="clubPhone" class="text-xs text-muted" dir="ltr">{{ clubPhone }}</p>
          <p v-if="clubDescription" class="text-xs text-muted mt-1">{{ clubDescription }}</p>
        </div>
        <div class="bg-surface-hover border border-border rounded p-4">
          <p class="text-sm text-muted mb-2">اتاق‌ها ({{ rooms.length }})</p>
          <p v-if="rooms.length === 0" class="text-sm text-muted">هیچ اتاقی ثبت نشده</p>
          <ul v-else class="space-y-1">
            <li v-for="r in rooms" :key="r.id" class="text-sm text-fg">• {{ r.name }}</li>
          </ul>
        </div>
        <div class="bg-surface-hover border border-border rounded p-4">
          <p class="text-sm text-muted mb-2">گردانندگان ({{ masters.length }})</p>
          <p v-if="masters.length === 0" class="text-sm text-muted">هیچ گرداننده‌ای ثبت نشده</p>
          <ul v-else class="space-y-1">
            <li v-for="m in masters" :key="m.id" class="text-sm text-fg">
              • {{ m.name }}<span v-if="m.userDisplayName" class="text-muted"> ({{ m.userDisplayName }})</span>
            </li>
          </ul>
        </div>
      </div>
      <div class="flex justify-between gap-4 mt-8">
        <button
          @click="currentStep = 3"
          class="px-6 py-2.5 border border-border text-muted hover:text-fg text-sm rounded font-medium transition"
        >
          قبلی
        </button>
        <button
          @click="finish"
          class="px-6 py-2.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-medium transition"
        >
          پایان
        </button>
      </div>
    </div>

    <!-- Room Modal -->
    <Modal
      :is-open="showRoomModal"
      :title="editingRoom ? 'ویرایش اتاق' : 'افزودن اتاق'"
      @close="closeRoomModal"
    >
      <RoomForm
        :club-id="clubId"
        :room="editingRoom ?? undefined"
        @saved="onRoomModalSaved"
        @cancelled="closeRoomModal"
      />
    </Modal>

    <!-- Master Modal -->
    <Modal
      :is-open="showMasterModal"
      :title="editingMaster ? 'ویرایش گرداننده' : 'افزودن گرداننده'"
      max-width="lg"
      @close="closeMasterModal"
    >
      <MasterForm
        :club-id="clubId"
        :master="editingMaster ?? undefined"
        @saved="onMasterModalSaved"
        @cancelled="closeMasterModal"
      />
    </Modal>

    <ConfirmModal
      :is-open="showDeleteRoomModal"
      title="حذف اتاق"
      :message="`آیا مطمئن هستید که می‌خواهید اتاق «${roomToDelete?.name}» را حذف کنید؟`"
      confirm-text="بله، حذف کن"
      cancel-text="انصراف"
      :is-loading="isDeletingRoom"
      @confirm="handleDeleteRoom"
      @cancel="showDeleteRoomModal = false; roomToDelete = null"
    />

    <ConfirmModal
      :is-open="showDeleteMasterModal"
      title="حذف گرداننده"
      :message="`آیا مطمئن هستید که می‌خواهید گرداننده «${masterToDelete?.name}» را حذف کنید؟`"
      confirm-text="بله، حذف کن"
      cancel-text="انصراف"
      :is-loading="isDeletingMaster"
      @confirm="handleDeleteMaster"
      @cancel="showDeleteMasterModal = false; masterToDelete = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ClubApi } from '@/api'
import { useToast } from '@/composables/useToast'
import ConfirmModal from '@/components/ConfirmModal.vue'
import Modal from '@/components/shared/Modal.vue'
import RoomForm from '@/components/club/RoomForm.vue'
import MasterForm from '@/components/club/MasterForm.vue'
import type { RoomDto, MasterDto } from '@/types/club'

const router = useRouter()
const { toastSuccess, toastWarning } = useToast()

const currentStep = ref(1)
const clubId = ref(0)
const clubName = ref('')
const clubCity = ref('')
const clubAddress = ref('')
const clubPhone = ref('')
const clubDescription = ref('')
const creatingClub = ref(false)
const step1Error = ref('')
const rooms = ref<RoomDto[]>([])
const masters = ref<MasterDto[]>([])
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

// Room modal
const showRoomModal = ref(false)
const editingRoom = ref<RoomDto | null>(null)

// Master modal
const showMasterModal = ref(false)
const editingMaster = ref<MasterDto | null>(null)

// Delete modals
const showDeleteRoomModal = ref(false)
const roomToDelete = ref<RoomDto | null>(null)
const isDeletingRoom = ref(false)

const showDeleteMasterModal = ref(false)
const masterToDelete = ref<MasterDto | null>(null)
const isDeletingMaster = ref(false)

async function goStep2() {
  if (!clubName.value.trim()) return
  creatingClub.value = true
  step1Error.value = ''
  try {
    const res = await ClubApi.createClub({
      name: clubName.value.trim(),
      address: clubAddress.value || null,
      phone: clubPhone.value || null,
      city: clubCity.value || null,
      description: clubDescription.value || null,
    })
    clubId.value = res.data.id
    clubName.value = res.data.name
    toastSuccess('کافه با موفقیت ایجاد شد')
    currentStep.value = 2
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      step1Error.value = err.response?.data?.message || 'خطا در ایجاد کافه'
    } else {
      step1Error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    creatingClub.value = false
  }
}

// Room modal handlers
function openRoomModal(room: RoomDto | null) {
  editingRoom.value = room
  showRoomModal.value = true
}

function closeRoomModal() {
  showRoomModal.value = false
  editingRoom.value = null
}

function onRoomModalSaved(room: RoomDto) {
  if (editingRoom.value) {
    const idx = rooms.value.findIndex(r => r.id === room.id)
    if (idx >= 0) rooms.value[idx] = room
  } else {
    rooms.value.push(room)
  }
  closeRoomModal()
}

function confirmDeleteRoom(room: RoomDto) {
  roomToDelete.value = room
  showDeleteRoomModal.value = true
}

async function handleDeleteRoom() {
  if (!roomToDelete.value) return
  isDeletingRoom.value = true
  try {
    await ClubApi.deleteRoom(clubId.value, roomToDelete.value.id)
    rooms.value = rooms.value.filter(r => r.id !== roomToDelete.value!.id)
    showDeleteRoomModal.value = false
    roomToDelete.value = null
    toastSuccess('اتاق با موفقیت حذف شد')
  } catch (e: unknown) {
    showDeleteRoomModal.value = false
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        toastWarning(err.response?.data?.message || 'این اتاق دارای بازی ثبت‌شده است و قابل حذف نیست')
      }
    }
    roomToDelete.value = null
  } finally {
    isDeletingRoom.value = false
  }
}

// Master modal handlers
function openMasterModal(master: MasterDto | null) {
  editingMaster.value = master
  showMasterModal.value = true
}

function closeMasterModal() {
  showMasterModal.value = false
  editingMaster.value = null
}

function onMasterModalSaved(master: MasterDto) {
  if (editingMaster.value) {
    const idx = masters.value.findIndex(m => m.id === master.id)
    if (idx >= 0) masters.value[idx] = master
  } else {
    masters.value.push(master)
  }
  closeMasterModal()
}

function confirmDeleteMaster(master: MasterDto) {
  masterToDelete.value = master
  showDeleteMasterModal.value = true
}

async function handleDeleteMaster() {
  if (!masterToDelete.value) return
  isDeletingMaster.value = true
  try {
    await ClubApi.deleteMaster(clubId.value, masterToDelete.value.id)
    masters.value = masters.value.filter(m => m.id !== masterToDelete.value!.id)
    showDeleteMasterModal.value = false
    masterToDelete.value = null
    toastSuccess('گرداننده با موفقیت حذف شد')
  } catch (e: unknown) {
    showDeleteMasterModal.value = false
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        toastWarning(err.response?.data?.message || 'این گرداننده دارای بازی ثبت‌شده است و قابل حذف نیست')
      }
    }
    masterToDelete.value = null
  } finally {
    isDeletingMaster.value = false
  }
}

function finish() {
  router.push(`/admin/clubs/${clubId.value}`)
}
</script>
