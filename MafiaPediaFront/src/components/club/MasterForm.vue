<template>
  <div>
    <div class="flex items-center gap-2 flex-wrap">
      <input
        v-model="name"
        type="text"
        placeholder="نام گرداننده"
        class="flex-1 min-w-[120px] bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
        @keydown.enter.prevent="submit"
      />
      <input
        v-model.number="ratePerGame"
        type="number"
        step="0.1"
        min="0"
        placeholder="دستمزد هر بازی"
        class="w-28 bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
      <label class="cursor-pointer">
        <input
          type="file"
          accept="image/jpeg,image/png,image/webp"
          class="hidden"
          @change="onPhotoChange"
        />
        <span class="text-xs text-[rgba(201,176,122,0.6)] hover:text-[#c9b07a] transition">
          {{ photoFile ? 'عکس選択 شد' : 'عکس' }}
        </span>
      </label>
      <button
        @click="submit"
        :disabled="!name.trim() || loading"
        class="px-4 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition whitespace-nowrap inline-flex items-center gap-2"
      >
        <div v-if="loading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
        {{ master ? 'ویرایش' : 'افزودن گرداننده' }}
      </button>
      <button
        v-if="master"
        @click="$emit('cancelled')"
        class="px-3 py-2 text-sm text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] transition"
      >
        انصراف
      </button>
    </div>

    <div class="mt-3">
      <div class="flex gap-1 mb-2">
        <button
          @click="userTab = 'none'"
          class="px-3 py-1.5 text-xs rounded font-medium transition"
          :class="userTab === 'none' ? 'bg-[#c9b07a] text-[#0d0d0f]' : 'bg-[rgba(255,255,255,0.05)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9]'"
        >
          بدون اکانت
        </button>
        <button
          @click="userTab = 'existing'"
          class="px-3 py-1.5 text-xs rounded font-medium transition"
          :class="userTab === 'existing' ? 'bg-[#c9b07a] text-[#0d0d0f]' : 'bg-[rgba(255,255,255,0.05)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9]'"
        >
          یوزر موجود
        </button>
        <button
          @click="userTab = 'new'"
          class="px-3 py-1.5 text-xs rounded font-medium transition"
          :class="userTab === 'new' ? 'bg-[#c9b07a] text-[#0d0d0f]' : 'bg-[rgba(255,255,255,0.05)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9]'"
        >
          یوزر جدید
        </button>
      </div>

      <div v-if="userTab === 'none'" class="text-xs text-[rgba(232,228,217,0.3)] px-1">
        این گرداننده به هیچ کاربری لینک نیست
      </div>

      <div v-if="userTab === 'existing'" class="relative">
        <input
          v-model="userQuery"
          type="text"
          placeholder="جستجوی کاربر..."
          class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
          @input="onUserSearch"
          @focus="onUserFocus"
        />
        <div
          v-if="userResults.length > 0"
          class="absolute left-0 right-0 top-full mt-1 bg-[#141416] border border-[rgba(255,255,255,0.07)] rounded shadow-lg z-50 overflow-hidden"
        >
          <div
            v-for="u in userResults"
            :key="u.id"
            @click="selectUser(u)"
            class="px-3 py-2 text-sm text-[#e8e4d9] hover:bg-[rgba(255,255,255,0.05)] cursor-pointer transition"
          >
            {{ u.displayName || u.username }} — {{ u.mobile }}
          </div>
        </div>
        <div v-if="selectedUser" class="mt-1 flex items-center gap-2">
          <span class="text-xs text-[rgba(232,228,217,0.5)]">
            {{ selectedUser.displayName || selectedUser.username }}
          </span>
          <button @click="clearUser" class="text-xs text-[#e07070] hover:underline">حذف</button>
        </div>
      </div>

      <div v-if="userTab === 'new'">
        <div v-if="!newUserData" class="flex items-center gap-2">
          <button
            @click="showNewUserModal = true"
            class="px-3 py-1.5 bg-[rgba(201,176,122,0.15)] hover:bg-[rgba(201,176,122,0.25)] text-[#c9b07a] text-xs rounded font-medium transition"
          >
            ساخت یوزر جدید
          </button>
        </div>
        <div v-else class="flex items-center gap-2">
          <span class="text-xs text-[rgba(232,228,217,0.5)]">
            یوزر جدید: {{ newUserData.username }}
          </span>
          <button
            @click="editNewUser"
            class="text-xs text-[#c9b07a] hover:underline"
          >
            ویرایش
          </button>
          <button
            @click="clearNewUser"
            class="text-xs text-[#e07070] hover:underline"
          >
            حذف
          </button>
        </div>
      </div>
    </div>

    <div class="mt-2">
      <textarea
        v-model="bio"
        placeholder="بیوگرافی"
        rows="2"
        class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition resize-none"
      />
    </div>
    <p v-if="error" class="text-xs text-[#e07070] mt-1">{{ error }}</p>

    <NewMasterUserModal
      :is-open="showNewUserModal"
      :initial="editingNewUserData"
      @saved="onNewUserSaved"
      @close="showNewUserModal = false; editingNewUserData = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ClubApi, UserApi } from '@/api'
import { useToast } from '@/composables/useToast'
import NewMasterUserModal from '@/components/club/NewMasterUserModal.vue'
import type { MasterDto } from '@/types/club'
import type { UserDto } from '@/types'
import type { NewUserFormData } from '@/components/club/NewMasterUserModal.vue'

const props = defineProps<{
  clubId: number
  master?: MasterDto
}>()

const emit = defineEmits<{
  saved: [master: MasterDto]
  cancelled: []
}>()

const name = ref(props.master?.name ?? '')
const ratePerGame = ref<number | null>(props.master?.ratePerGame ?? null)
const bio = ref(props.master?.bio ?? '')
const loading = ref(false)
const error = ref('')
const photoFile = ref<File | null>(null)
const { toastSuccess } = useToast()

const userTab = ref<'none' | 'existing' | 'new'>(
  props.master?.userId ? 'existing' : 'none'
)

const userQuery = ref(props.master?.userDisplayName ?? '')
const userResults = ref<UserDto[]>([])
const selectedUser = ref<UserDto | null>(
  props.master?.userId
    ? { id: props.master.userId, displayName: props.master.userDisplayName ?? undefined, username: '', mobile: props.master.userMobile ?? '' } as UserDto
    : null
)

const showNewUserModal = ref(false)
const newUserData = ref<NewUserFormData | null>(null)
const editingNewUserData = ref<NewUserFormData | null>(null)
let debounceTimer: ReturnType<typeof setTimeout>

function onUserSearch() {
  clearTimeout(debounceTimer)
  if (userQuery.value.length < 2) {
    userResults.value = []
    return
  }
  debounceTimer = setTimeout(async () => {
    try {
      const res = await UserApi.searchUsers(userQuery.value)
      userResults.value = res.data.items.slice(0, 10)
    } catch {
      userResults.value = []
    }
  }, 300)
}

function onUserFocus() {
  if (userQuery.value.length >= 2) onUserSearch()
}

function selectUser(u: UserDto) {
  selectedUser.value = u
  userQuery.value = u.displayName || u.username
  userResults.value = []
}

function clearUser() {
  selectedUser.value = null
  userQuery.value = ''
  userResults.value = []
}

function onPhotoChange(e: Event) {
  const target = e.target as HTMLInputElement
  photoFile.value = target.files?.[0] ?? null
}

function onNewUserSaved(data: NewUserFormData) {
  newUserData.value = data
  editingNewUserData.value = null
  showNewUserModal.value = false
}

function editNewUser() {
  if (!newUserData.value) return
  editingNewUserData.value = { ...newUserData.value }
  showNewUserModal.value = true
}

function clearNewUser() {
  newUserData.value = null
  userTab.value = 'none'
}

async function submit() {
  const trimmed = name.value.trim()
  if (!trimmed) return
  loading.value = true
  error.value = ''
  try {
    if (props.master) {
      const dto: Record<string, unknown> = { name: trimmed }
      if (ratePerGame.value !== null) dto.ratePerGame = ratePerGame.value
      if (userTab.value === 'new' && newUserData.value) {
        dto.unlinkUser = true
      } else if (userTab.value === 'existing' && selectedUser.value && selectedUser.value.id !== props.master.userId) {
        dto.existingUserId = selectedUser.value.id
      } else if (userTab.value === 'none' && props.master.userId) {
        dto.unlinkUser = true
      }
      const res = await ClubApi.updateMaster(props.clubId, props.master.id, dto as never, photoFile.value ?? undefined)
      emit('saved', res.data)
      toastSuccess('گرداننده با موفقیت ویرایش شد')
    } else {
      const dto: Record<string, unknown> = { name: trimmed }
      if (ratePerGame.value !== null) dto.ratePerGame = ratePerGame.value
      if (userTab.value === 'new' && newUserData.value) {
        dto.newUser = {
          username: newUserData.value.username,
          password: newUserData.value.password,
          mobile: newUserData.value.mobile,
          displayName: newUserData.value.displayName || null,
        }
      } else if (userTab.value === 'existing' && selectedUser.value) {
        dto.existingUserId = selectedUser.value.id
      }
      const res = await ClubApi.createMaster(props.clubId, dto as never, photoFile.value ?? undefined)
      emit('saved', res.data)
      name.value = ''
      ratePerGame.value = null
      bio.value = ''
      photoFile.value = null
      userTab.value = 'none'
      selectedUser.value = null
      userQuery.value = ''
      newUserData.value = null
      toastSuccess('گرداننده با موفقیت اضافه شد')
    }
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ثبت گرداننده'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}
</script>
