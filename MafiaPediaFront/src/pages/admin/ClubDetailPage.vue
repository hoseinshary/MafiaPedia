<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="club">
      <div class="flex items-start justify-between mb-6">
        <div class="flex items-start gap-4">
          <router-link to="/admin/clubs" class="text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] transition mt-1">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd"/>
            </svg>
          </router-link>
          <div>
            <div class="flex items-center gap-3">
              <img v-if="club.logo" :src="getImageUrl(club.logo)" class="w-12 h-12 rounded object-cover" />
              <h1 class="text-2xl md:text-3xl font-bold text-[#e8e4d9]">{{ club.name }}</h1>
              <button @click="openClubEditModal" class="text-[rgba(232,228,217,0.3)] hover:text-[#c9b07a] transition">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
                  <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z"/>
                </svg>
              </button>
            </div>
            <div v-if="club.address || club.phone || club.city" class="flex flex-wrap gap-3 mt-2 text-xs text-[rgba(232,228,217,0.4)]">
              <span v-if="club.city">{{ club.city }}</span>
              <span v-if="club.address">{{ club.address }}</span>
              <span v-if="club.phone" dir="ltr">{{ club.phone }}</span>
            </div>
            <p v-if="club.description" class="mt-2 text-sm text-[rgba(232,228,217,0.5)]">{{ club.description }}</p>
          </div>
        </div>
      </div>

      <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm border" :class="notification.type === 'success' ? 'bg-[rgba(111,207,138,0.1)] border-[rgba(111,207,138,0.2)] text-[#6fcf8a]' : 'bg-[rgba(224,112,112,0.1)] border-[rgba(224,112,112,0.2)] text-[#e07070]'">
        {{ notification.message }}
      </div>

      <!-- Rooms Section -->
      <section class="mb-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-[#e8e4d9]">اتاق‌ها</h2>
          <button
            @click="openRoomModal(null)"
            class="px-3 py-1.5 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-xs rounded font-medium transition"
          >
            افزودن اتاق
          </button>
        </div>
        <div class="space-y-2">
          <div
            v-for="r in club.rooms"
            :key="r.id"
            class="flex items-center justify-between bg-[rgba(255,255,255,0.03)] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5"
          >
            <div class="flex items-center gap-2">
              <span class="text-sm text-[#e8e4d9]">{{ r.name }}</span>
              <span v-if="!r.isActive" class="text-xs text-[#e07070]">(غیرفعال)</span>
            </div>
            <div class="flex items-center gap-2">
              <button @click="openRoomModal(r)" class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a] transition">ویرایش</button>
              <button @click="confirmDeleteRoom(r)" class="text-xs text-[#e07070] hover:underline">حذف</button>
            </div>
          </div>
          <p v-if="club.rooms.length === 0" class="text-sm text-[rgba(232,228,217,0.3)] text-center py-4">هیچ اتاقی ثبت نشده</p>
        </div>
      </section>

      <!-- Masters Section -->
      <section class="mb-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-[#e8e4d9]">گردانندگان</h2>
          <button
            @click="openMasterModal(null)"
            class="px-3 py-1.5 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-xs rounded font-medium transition"
          >
            افزودن گرداننده
          </button>
        </div>
        <div class="space-y-2">
          <div
            v-for="m in club.masters"
            :key="m.id"
            class="bg-[rgba(255,255,255,0.03)] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3 flex-1">
                <img v-if="m.photo" :src="getImageUrl(m.photo)" class="w-8 h-8 rounded-full object-cover" />
                <div>
                  <div class="flex items-center gap-2">
                    <span class="text-sm text-[#e8e4d9]">{{ m.name }}</span>
                    <span v-if="m.ratePerGame != null" class="text-xs text-[rgba(232,228,217,0.3)]">{{ m.ratePerGame.toLocaleString() }} تومان</span>
                    <span v-if="!m.userId" class="text-xs text-[rgba(232,228,217,0.25)]">(بدون اکانت)</span>
                  </div>
                  <div class="flex items-center gap-2 text-xs text-[rgba(232,228,217,0.4)]">
                    <span v-if="m.userDisplayName">{{ m.userDisplayName }}</span>
                    <span v-if="m.userMobile" dir="ltr">{{ m.userMobile }}</span>
                  </div>
                  <p v-if="m.bio" class="text-xs text-[rgba(232,228,217,0.3)] mt-0.5">{{ m.bio }}</p>
                </div>
              </div>
              <div class="flex items-center gap-2 shrink-0">
                <button @click="openMasterModal(m)" class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a] transition">ویرایش</button>
                <button @click="confirmDeleteMaster(m)" class="text-xs text-[#e07070] hover:underline">حذف</button>
              </div>
            </div>
          </div>
          <p v-if="club.masters.length === 0" class="text-sm text-[rgba(232,228,217,0.3)] text-center py-4">هیچ گرداننده‌ای ثبت نشده</p>
        </div>
      </section>

      <!-- Customers Section -->
      <section class="mb-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-[#e8e4d9]">مشتریان</h2>
          <button
            @click="openCustomerModal(null)"
            class="px-3 py-1.5 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-xs rounded font-medium transition"
          >
            افزودن مشتری
          </button>
        </div>

        <div class="mb-4">
          <input
            v-model="customersSearch"
            type="text"
            placeholder="جستجوی نام یا موبایل..."
            class="w-full md:w-1/2 bg-[#141416] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
          />
        </div>

        <div v-if="customersLoading" class="flex justify-center py-10">
          <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
        </div>

        <div v-else-if="customers.length === 0" class="text-sm text-[rgba(232,228,217,0.3)] text-center py-10">
          هیچ مشتری‌ای ثبت نشده
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm border-collapse">
            <thead>
              <tr class="border-b border-[rgba(255,255,255,0.07)] bg-[#1a1a1e] text-[rgba(232,228,217,0.5)]">
                <th class="px-4 py-3 text-right">عکس</th>
                <th class="px-4 py-3 text-right">نام</th>
                <th class="px-4 py-3 text-right">موبایل</th>
                <th class="px-4 py-3 text-right">کد</th>
                <th class="px-4 py-3 text-right">عملیات</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="c in customers"
                :key="c.id"
                class="border-b border-[rgba(255,255,255,0.04)] hover:bg-[#1a1a1e] transition text-[#e8e4d9]"
              >
                <td class="px-4 py-3">
                  <img
                    v-if="c.picture"
                    :src="getImageUrl(c.picture)"
                    class="w-9 h-9 rounded-full object-cover border border-[rgba(201,176,122,0.2)]"
                  />
                  <div
                    v-else
                    class="w-9 h-9 rounded-full flex items-center justify-center text-white text-sm font-bold select-none"
                    :style="{ backgroundColor: avatarColor(c.name) }"
                  >
                    {{ c.name.charAt(0) }}
                  </div>
                </td>
                <td class="px-4 py-3 font-medium">{{ c.name }}</td>
                <td class="px-4 py-3 ltr text-left text-[rgba(232,228,217,0.6)]">{{ c.mobile }}</td>
                <td class="px-4 py-3 text-[rgba(232,228,217,0.4)]">{{ c.code || '—' }}</td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-2">
                    <button @click="openCustomerModal(c)" class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a] transition">ویرایش</button>
                    <button @click="confirmDeleteCustomer(c)" class="text-xs text-[#e07070] hover:underline">حذف</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="flex items-center justify-between gap-4 mt-4 text-sm">
            <span class="text-[rgba(232,228,217,0.4)]">
              صفحه {{ customersPage }} از {{ customersTotalPages }}
            </span>
            <div class="flex gap-2">
              <button
                :disabled="customersPage <= 1"
                class="px-4 py-2 rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] disabled:opacity-40 disabled:cursor-not-allowed hover:bg-[#1a1a1e] transition"
                @click="customersPage = Math.max(1, customersPage - 1)"
              >
                قبلی
              </button>
              <button
                :disabled="customersPage >= customersTotalPages"
                class="px-4 py-2 rounded border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] disabled:opacity-40 disabled:cursor-not-allowed hover:bg-[#1a1a1e] transition"
                @click="customersPage = Math.min(customersTotalPages, customersPage + 1)"
              >
                بعدی
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Club Edit Modal -->
      <Modal :is-open="showClubEditModal" title="ویرایش کافه" max-width="md" @close="closeClubEditModal">
        <div class="flex flex-col gap-3">
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">نام کافه <span class="text-[#e07070]">*</span></label>
            <input
              v-model="clubEditForm.name"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            />
          </div>
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">شهر</label>
            <input
              v-model="clubEditForm.city"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            />
          </div>
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">آدرس</label>
            <input
              v-model="clubEditForm.address"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            />
          </div>
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">تلفن</label>
            <input
              v-model="clubEditForm.phone"
              type="text"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            />
          </div>
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">توضیحات</label>
            <textarea
              v-model="clubEditForm.description"
              rows="3"
              class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition resize-none"
            />
          </div>
          <div>
            <label class="text-sm text-[rgba(232,228,217,0.4)]">لوگو</label>
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp"
              class="w-full text-sm text-[rgba(232,228,217,0.4)] file:mr-2 file:py-1.5 file:px-3 file:rounded file:border-0 file:text-xs file:bg-[rgba(201,176,122,0.15)] file:text-[#c9b07a] hover:file:bg-[rgba(201,176,122,0.25)] transition cursor-pointer"
              @change="onClubLogoChange"
            />
            <img
              v-if="clubEditLogoPreview"
              :src="clubEditLogoPreview"
              class="mt-2 w-20 h-20 rounded object-cover border border-[rgba(255,255,255,0.07)]"
            />
          </div>
          <p v-if="clubEditError" class="text-xs text-[#e07070]">{{ clubEditError }}</p>
        </div>
        <template #footer>
          <button
            @click="closeClubEditModal"
            class="px-4 py-2 border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] text-sm rounded font-medium transition"
          >
            انصراف
          </button>
          <button
            @click="saveClub"
            :disabled="!clubEditForm.name.trim() || savingClub"
            class="px-4 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center gap-2"
          >
            <div v-if="savingClub" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            ذخیره
          </button>
        </template>
      </Modal>

      <!-- Room Modal -->
      <Modal
        :is-open="showRoomModal"
        :title="editingRoom ? 'ویرایش اتاق' : 'افزودن اتاق'"
        @close="closeRoomModal"
      >
        <RoomForm
          :club-id="club.id"
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
          :club-id="club.id"
          :master="editingMaster ?? undefined"
          @saved="onMasterModalSaved"
          @cancelled="closeMasterModal"
        />
      </Modal>

      <!-- Customer Modal -->
      <Modal
        :is-open="showCustomerModal"
        :title="editingCustomer ? 'ویرایش مشتری' : 'افزودن مشتری'"
        max-width="md"
        @close="closeCustomerModal"
      >
        <CustomerForm
          :club-id="club.id"
          :initial="editingCustomer"
          @saved="onCustomerModalSaved"
          @cancel="closeCustomerModal"
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

      <ConfirmModal
        :is-open="showDeleteCustomerModal"
        title="حذف مشتری از کافه"
        :message="`آیا مطمئن هستید که می‌خواهید «${customerToDelete?.name}» را از این کافه حذف کنید؟`"
        confirm-text="بله، حذف کن"
        cancel-text="انصراف"
        :is-loading="isDeletingCustomer"
        @confirm="handleDeleteCustomer"
        @cancel="showDeleteCustomerModal = false; customerToDelete = null"
      />
    </template>

    <div v-else class="text-center py-20 text-[rgba(232,228,217,0.4)] text-lg">
      کافه یافت نشد.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ClubApi, ClubPlayerApi } from '@/api'
import { useToast } from '@/composables/useToast'

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api'

function getImageUrl(path: string | null | undefined): string | undefined {
  if (!path) return undefined
  const base = baseUrl.replace(/\/api$/, '')
  return base + path
}

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
import ConfirmModal from '@/components/ConfirmModal.vue'
import Modal from '@/components/shared/Modal.vue'
import RoomForm from '@/components/club/RoomForm.vue'
import MasterForm from '@/components/club/MasterForm.vue'
import CustomerForm from '@/components/club/CustomerForm.vue'
import type { ClubDetailDto, RoomDto, MasterDto } from '@/types/club'
import type { ClubPlayerDto, ClubPlayerJoinResult } from '@/types/clubPlayer'

const route = useRoute()
const router = useRouter()
const { toastSuccess, toastWarning } = useToast()

const club = ref<ClubDetailDto | null>(null)
const loading = ref(true)
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

// Club edit modal
const showClubEditModal = ref(false)
const clubEditForm = reactive({ name: '', address: '', phone: '', city: '', description: '' })
const clubEditLogo = ref<File | null>(null)
const clubEditLogoPreview = ref('')
const savingClub = ref(false)
const clubEditError = ref('')

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

// Customer state
const customers = ref<ClubPlayerDto[]>([])
const customersTotal = ref(0)
const customersPage = ref(1)
const customersPageSize = 20
const customersTotalPages = ref(1)
const customersSearch = ref('')
const customersLoading = ref(false)
let customersSearchTimer: ReturnType<typeof setTimeout>

const showCustomerModal = ref(false)
const editingCustomer = ref<ClubPlayerDto | null>(null)

const showDeleteCustomerModal = ref(false)
const customerToDelete = ref<ClubPlayerDto | null>(null)
const isDeletingCustomer = ref(false)

async function fetchCustomers() {
  if (!club.value) return
  customersLoading.value = true
  try {
    const res = await ClubPlayerApi.getClubPlayers(club.value.id, customersPage.value, customersPageSize, customersSearch.value || undefined)
    customers.value = res.data.items
    customersTotal.value = res.data.total
    customersTotalPages.value = res.data.totalPages || Math.max(1, Math.ceil(res.data.total / customersPageSize))
  } catch {
    customers.value = []
  } finally {
    customersLoading.value = false
  }
}

function openCustomerModal(customer: ClubPlayerDto | null) {
  editingCustomer.value = customer
  showCustomerModal.value = true
}

function closeCustomerModal() {
  showCustomerModal.value = false
  editingCustomer.value = null
}

function onCustomerModalSaved(result: ClubPlayerJoinResult) {
  if (editingCustomer.value) {
    const idx = customers.value.findIndex(c => c.id === result.clubPlayer.id)
    if (idx >= 0) customers.value[idx] = result.clubPlayer
  } else {
    customers.value.unshift(result.clubPlayer)
    customersTotal.value++
  }
  closeCustomerModal()
  if (result.wasExistingCustomer) {
    toastSuccess('مشتری موجود به کافه اضافه شد')
  } else if (!editingCustomer.value) {
    toastSuccess('مشتری با موفقیت اضافه شد')
  }
}

function confirmDeleteCustomer(customer: ClubPlayerDto) {
  customerToDelete.value = customer
  showDeleteCustomerModal.value = true
}

async function handleDeleteCustomer() {
  if (!customerToDelete.value || !club.value) return
  isDeletingCustomer.value = true
  try {
    await ClubPlayerApi.removeFromClub(club.value.id, customerToDelete.value.id)
    customers.value = customers.value.filter(c => c.id !== customerToDelete.value!.id)
    customersTotal.value--
    showDeleteCustomerModal.value = false
    customerToDelete.value = null
    toastSuccess('مشتری با موفقیت از کافه حذف شد')
  } catch (e: unknown) {
    showDeleteCustomerModal.value = false
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        toastWarning(err.response?.data?.message || 'این مشتری در بازی‌های این کافه ثبت شده است و قابل حذف نیست')
      }
    }
    customerToDelete.value = null
  } finally {
    isDeletingCustomer.value = false
  }
}

watch(customersSearch, () => {
  clearTimeout(customersSearchTimer)
  customersSearchTimer = setTimeout(() => {
    customersPage.value = 1
    fetchCustomers()
  }, 400)
})

watch(customersPage, () => {
  fetchCustomers()
})

async function fetchClub() {
  const id = Number(route.params.id)
  if (!id) {
    router.push('/admin/clubs')
    return
  }
  loading.value = true
  try {
    const res = await ClubApi.getClubDetail(id)
    club.value = res.data
  } catch {
    club.value = null
  } finally {
    loading.value = false
  }
}

// Club edit
function openClubEditModal() {
  if (!club.value) return
  clubEditForm.name = club.value.name
  clubEditForm.address = club.value.address || ''
  clubEditForm.phone = club.value.phone || ''
  clubEditForm.city = club.value.city || ''
  clubEditForm.description = club.value.description || ''
  clubEditLogo.value = null
  clubEditLogoPreview.value = ''
  clubEditError.value = ''
  showClubEditModal.value = true
}

function closeClubEditModal() {
  showClubEditModal.value = false
  if (clubEditLogoPreview.value) {
    URL.revokeObjectURL(clubEditLogoPreview.value)
    clubEditLogoPreview.value = ''
  }
}

function onClubLogoChange(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return
  clubEditLogo.value = file
  if (clubEditLogoPreview.value) URL.revokeObjectURL(clubEditLogoPreview.value)
  clubEditLogoPreview.value = URL.createObjectURL(file)
}

async function saveClub() {
  if (!club.value || !clubEditForm.name.trim()) return
  savingClub.value = true
  clubEditError.value = ''
  try {
    const res = await ClubApi.updateClub(
      club.value.id,
      {
        name: clubEditForm.name.trim(),
        address: clubEditForm.address || null,
        phone: clubEditForm.phone || null,
        city: clubEditForm.city || null,
        description: clubEditForm.description || null,
      },
      clubEditLogo.value ?? undefined
    )
    club.value.name = res.data.name
    club.value.address = res.data.address
    club.value.phone = res.data.phone
    club.value.city = res.data.city
    club.value.description = res.data.description
    if (res.data.logo) club.value.logo = res.data.logo
    closeClubEditModal()
    toastSuccess('کافه با موفقیت ویرایش شد')
  } catch {
    clubEditError.value = 'خطا در ویرایش کافه'
  } finally {
    savingClub.value = false
  }
}

// Room modal
function openRoomModal(room: RoomDto | null) {
  editingRoom.value = room
  showRoomModal.value = true
}

function closeRoomModal() {
  showRoomModal.value = false
  editingRoom.value = null
}

function onRoomModalSaved(room: RoomDto) {
  if (!club.value) return
  if (editingRoom.value) {
    const idx = club.value.rooms.findIndex(r => r.id === room.id)
    if (idx >= 0) club.value.rooms[idx] = room
  } else {
    club.value.rooms.push(room)
  }
  closeRoomModal()
}

// Master modal
function openMasterModal(master: MasterDto | null) {
  editingMaster.value = master
  showMasterModal.value = true
}

function closeMasterModal() {
  showMasterModal.value = false
  editingMaster.value = null
}

function onMasterModalSaved(master: MasterDto) {
  if (!club.value) return
  if (editingMaster.value) {
    const idx = club.value.masters.findIndex(m => m.id === master.id)
    if (idx >= 0) club.value.masters[idx] = master
  } else {
    club.value.masters.push(master)
  }
  closeMasterModal()
}

// Delete room
function confirmDeleteRoom(room: RoomDto) {
  roomToDelete.value = room
  showDeleteRoomModal.value = true
}

async function handleDeleteRoom() {
  if (!roomToDelete.value || !club.value) return
  isDeletingRoom.value = true
  try {
    await ClubApi.deleteRoom(club.value.id, roomToDelete.value.id)
    club.value.rooms = club.value.rooms.filter(r => r.id !== roomToDelete.value!.id)
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

// Delete master
function confirmDeleteMaster(master: MasterDto) {
  masterToDelete.value = master
  showDeleteMasterModal.value = true
}

async function handleDeleteMaster() {
  if (!masterToDelete.value || !club.value) return
  isDeletingMaster.value = true
  try {
    await ClubApi.deleteMaster(club.value.id, masterToDelete.value.id)
    club.value.masters = club.value.masters.filter(m => m.id !== masterToDelete.value!.id)
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

onMounted(() => {
  fetchClub()
  fetchCustomers()
})
</script>
