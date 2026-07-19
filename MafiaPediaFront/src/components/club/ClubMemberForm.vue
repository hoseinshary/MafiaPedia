<template>
  <div>
    <div class="flex items-center gap-2 flex-wrap">
      <div class="flex flex-col gap-1 flex-1">
        <label class="text-xs text-muted">نقش</label>
        <div class="flex gap-2 flex-wrap">
          <button
            v-for="r in availableRoles"
            :key="r"
            @click="selectedRole = r"
            class="px-3 py-1.5 text-xs rounded font-medium transition"
            :class="selectedRole === r ? 'bg-gold text-[#0d0d0f]' : 'bg-surface-hover text-muted hover:text-fg'"
          >
            {{ roleLabel(r) }}
          </button>
        </div>
      </div>
      <button
        @click="submit"
        :disabled="!selectedRole || loading"
        class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition whitespace-nowrap inline-flex items-center gap-2 self-end"
      >
        <div v-if="loading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
        {{ member ? 'ویرایش' : 'افزودن عضو' }}
      </button>
      <button
        v-if="member"
        @click="$emit('cancelled')"
        class="px-3 py-2 text-sm text-muted hover:text-fg transition self-end"
      >
        انصراف
      </button>
    </div>

    <div class="mt-3">
      <div class="flex gap-1 mb-2">
        <button
          @click="userTab = 'existing'"
          class="px-3 py-1.5 text-xs rounded font-medium transition"
          :class="userTab === 'existing' ? 'bg-gold text-[#0d0d0f]' : 'bg-surface-hover text-muted hover:text-fg'"
        >
          یوزر موجود
        </button>
        <button
          @click="userTab = 'new'"
          class="px-3 py-1.5 text-xs rounded font-medium transition"
          :class="userTab === 'new' ? 'bg-gold text-[#0d0d0f]' : 'bg-surface-hover text-muted hover:text-fg'"
        >
          یوزر جدید
        </button>
      </div>

      <div v-if="userTab === 'existing'" class="relative">
        <input
          v-model="userQuery"
          type="text"
          placeholder="جستجوی کاربر..."
          class="w-full bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          @input="onUserSearch"
          @focus="onUserFocus"
        />
        <div
          v-if="userResults.length > 0"
          class="absolute left-0 right-0 top-full mt-1 bg-surface border border-border rounded shadow-lg z-50 overflow-hidden"
        >
          <div
            v-for="u in userResults"
            :key="u.id"
            @click="selectUser(u)"
            class="px-3 py-2 text-sm text-fg hover:bg-surface-hover cursor-pointer transition"
          >
            {{ u.displayName || u.username }} — {{ u.mobile }}
          </div>
        </div>
        <div v-if="selectedUser" class="mt-1 flex items-center gap-2">
          <span class="text-xs text-muted">
            {{ selectedUser.displayName || selectedUser.username }}
          </span>
          <button @click="clearUser" class="text-xs text-danger hover:underline">حذف</button>
        </div>
      </div>

      <div v-if="userTab === 'new'">
        <div v-if="!newUserData" class="flex items-center gap-2">
          <button
            @click="showNewUserModal = true"
            class="px-3 py-1.5 bg-gold/15 hover:bg-gold/25 text-gold-text text-xs rounded font-medium transition"
          >
            ساخت یوزر جدید
          </button>
        </div>
        <div v-else class="flex items-center gap-2">
          <span class="text-xs text-muted">
            یوزر جدید: {{ newUserData.username }}
          </span>
          <button @click="editNewUser" class="text-xs text-gold-text hover:underline">ویرایش</button>
          <button @click="clearNewUser" class="text-xs text-danger hover:underline">حذف</button>
        </div>
      </div>
    </div>

    <p v-if="error" class="text-xs text-danger mt-1">{{ error }}</p>

    <NewMasterUserModal
      :is-open="showNewUserModal"
      :initial="editingNewUserData"
      @saved="onNewUserSaved"
      @close="showNewUserModal = false; editingNewUserData = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ClubUserApi, UserApi } from '@/api'
import { useToast } from '@/composables/useToast'
import NewMasterUserModal from '@/components/club/NewMasterUserModal.vue'
import type { ClubUserDto, CreateClubUserDto, UpdateClubUserRoleDto } from '@/types/club'
import type { UserDto } from '@/types'
import type { NewUserFormData } from '@/components/club/NewMasterUserModal.vue'

const props = defineProps<{
  clubId: number
  member?: ClubUserDto
  allowOwnerRole: boolean
}>()

const emit = defineEmits<{
  saved: [member: ClubUserDto]
  cancelled: []
}>()

const { toastSuccess } = useToast()

const availableRoles = computed(() => {
  return props.allowOwnerRole ? ['owner', 'supervisor', 'cashier'] : ['supervisor', 'cashier']
})

const selectedRole = ref(props.member?.clubuserRole && props.member.clubuserRole !== 'master' ? props.member.clubuserRole : '')
const loading = ref(false)
const error = ref('')

const userTab = ref<'existing' | 'new'>(
  props.member?.userId ? 'existing' : 'existing'
)

const userQuery = ref(props.member?.userDisplayName ?? '')
const userResults = ref<UserDto[]>([])
const selectedUser = ref<UserDto | null>(
  props.member?.userId
    ? { id: props.member.userId, displayName: props.member.userDisplayName ?? undefined, username: '', mobile: props.member.userMobile ?? '' } as UserDto
    : null
)

const showNewUserModal = ref(false)
const newUserData = ref<NewUserFormData | null>(null)
const editingNewUserData = ref<NewUserFormData | null>(null)
let debounceTimer: ReturnType<typeof setTimeout>

function roleLabel(role: string): string {
  const map: Record<string, string> = { owner: 'مدیر', supervisor: 'سوپروایزر', cashier: 'صندوق‌دار' }
  return map[role] || role
}

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
}

async function submit() {
  if (!selectedRole.value) return
  loading.value = true
  error.value = ''
  try {
    if (props.member) {
      const dto: UpdateClubUserRoleDto = { clubuserRole: selectedRole.value as 'owner' | 'supervisor' | 'cashier' }
      const res = await ClubUserApi.updateMemberRole(props.clubId, props.member.id, dto)
      emit('saved', res.data)
      toastSuccess('نقش عضو با موفقیت ویرایش شد')
    } else {
      const dto: CreateClubUserDto = { clubuserRole: selectedRole.value as 'owner' | 'supervisor' | 'cashier' }
      if (userTab.value === 'new' && newUserData.value) {
        dto.newUser = {
          username: newUserData.value.username,
          password: newUserData.value.password,
          mobile: newUserData.value.mobile,
          displayName: newUserData.value.displayName || null,
        }
      } else if (userTab.value === 'existing' && selectedUser.value) {
        dto.existingUserId = selectedUser.value.id
      } else {
        error.value = 'لطفاً یک کاربر انتخاب کنید'
        loading.value = false
        return
      }
      const res = await ClubUserApi.createMember(props.clubId, dto)
      emit('saved', res.data)
      selectedRole.value = ''
      selectedUser.value = null
      userQuery.value = ''
      newUserData.value = null
      toastSuccess('عضو با موفقیت اضافه شد')
    }
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در عملیات'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}
</script>
