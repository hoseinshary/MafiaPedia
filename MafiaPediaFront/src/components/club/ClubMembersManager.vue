<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-bold text-fg">اعضا</h2>
      <button
        @click="showAddForm = !showAddForm"
        class="px-3 py-1.5 bg-gold hover:opacity-80 text-[#0d0d0f] text-xs rounded font-bold transition"
      >
        {{ showAddForm ? 'بستن' : 'افزودن عضو' }}
      </button>
    </div>

    <div v-if="showAddForm" class="bg-surface border border-border rounded-xl p-4 mb-4">
      <ClubMemberForm
        :club-id="clubId"
        :allow-owner-role="allowOwnerRole"
        @saved="onFormSaved"
        @cancelled="showAddForm = false"
      />
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="members.length === 0" class="text-center py-12 text-muted text-sm">
      هیچ عضوی یافت نشد
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-right">
        <thead>
          <tr class="border-b border-border text-muted text-xs">
            <th class="px-4 py-3">نام / موبایل</th>
            <th class="px-4 py-3">نقش مدیریتی</th>
            <th class="px-4 py-3">گرداننده</th>
            <th class="px-4 py-3">عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in members" :key="m.id" class="border-b border-border hover:bg-surface-hover transition">
            <td class="px-4 py-3">
              <div class="text-sm text-fg">{{ m.userDisplayName || '—' }}</div>
              <div class="text-xs text-muted mt-0.5 ltr text-left">{{ m.userMobile || '' }}</div>
            </td>

            <!-- ستون نقش مدیریتی -->
            <td class="px-4 py-3">
              <div v-if="m.clubuserRole !== 'master'" class="flex items-center gap-1.5">
                <select
                  v-model="m.clubuserRole"
                  @change="onRoleChange(m, $event)"
                  class="bg-input border border-border rounded px-2 py-1 text-xs text-fg focus:outline-none focus:border-gold transition"
                >
                  <option v-for="r in getAvailableRoles()" :key="r" :value="r">{{ roleLabel(r) }}</option>
                </select>
                <button
                  @click="confirmRemoveRole(m)"
                  class="text-xs text-danger hover:underline whitespace-nowrap"
                >
                  حذف نقش
                </button>
              </div>
              <div v-else class="flex items-center gap-1.5">
                <span class="text-xs text-muted">—</span>
                <div class="relative">
                  <button
                    @click="toggleAddRoleDropdown(m.id)"
                    class="text-xs text-gold-text hover:underline"
                  >
                    افزودن نقش مدیریتی
                  </button>
                  <div
                    v-if="openAddRoleDropdownId === m.id"
                    class="absolute right-0 top-full mt-1 bg-surface border border-border rounded shadow-lg z-50 overflow-hidden min-w-[120px]"
                  >
                    <button
                      v-for="r in getAvailableRoles()"
                      :key="r"
                      @click="addManagementRole(m, r)"
                      class="block w-full text-right px-3 py-2 text-xs text-fg hover:bg-surface-hover transition"
                    >
                      {{ roleLabel(r) }}
                    </button>
                  </div>
                </div>
              </div>
            </td>

            <!-- ستون گرداننده -->
            <td class="px-4 py-3">
              <div v-if="m.masterId" class="flex items-center gap-1.5">
                <span class="text-xs px-2 py-0.5 rounded bg-success/12 text-success">هست</span>
                <button
                  @click="confirmUnlink(m)"
                  class="text-xs text-danger hover:underline"
                >
                  قطع ارتباط
                </button>
              </div>
              <div v-else>
                <button
                  @click="handleAddMaster(m)"
                  class="text-xs text-gold-text hover:underline"
                >
                  افزودن پروفایل گرداننده
                </button>
              </div>
            </td>

            <!-- ستون عملیات -->
            <td class="px-4 py-3">
              <span class="text-xs text-muted">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Confirm حذف نقش مدیریتی / حذف عضو -->
    <ConfirmModal
      :is-open="showDeleteConfirm"
      title="حذف نقش مدیریتی"
      :message="deleteMessage"
      confirm-text="تأیید"
      cancel-text="انصراف"
      :is-loading="isProcessing"
      @confirm="handleRemoveRole"
      @cancel="showDeleteConfirm = false"
    />

    <!-- Confirm قطع ارتباط گرداننده -->
    <ConfirmModal
      :is-open="showUnlinkConfirm"
      title="قطع ارتباط گرداننده"
      message="پروفایل گرداننده از این کاربر جدا می‌شود؛ تاریخچه بازی‌هایش حفظ می‌ماند ولی خودش دیگر دسترسی گرداننده نخواهد داشت"
      confirm-text="تأیید"
      cancel-text="انصراف"
      :is-loading="isProcessing"
      @confirm="handleUnlink"
      @cancel="showUnlinkConfirm = false"
    />

    <!-- Modal افزودن پروفایل گرداننده -->
    <AddMasterProfileModal
      :is-open="showMasterModal"
      :default-name="masterModalDefaultName"
      @saved="handleMasterSaved"
      @close="showMasterModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ClubUserApi, ClubApi } from '@/api'
import ClubMemberForm from '@/components/club/ClubMemberForm.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import AddMasterProfileModal from '@/components/club/AddMasterProfileModal.vue'
import type { ClubUserDto } from '@/types/club'

const props = defineProps<{
  clubId: number
  allowOwnerRole: boolean
}>()

const members = ref<ClubUserDto[]>([])
const loading = ref(true)
const showAddForm = ref(false)
const isProcessing = ref(false)

// Delete/remove role state
const showDeleteConfirm = ref(false)
const deleteTarget = ref<ClubUserDto | null>(null)
const deleteMessage = ref('')

// Unlink master state
const showUnlinkConfirm = ref(false)
const unlinkTarget = ref<ClubUserDto | null>(null)

// Add master profile modal state
const showMasterModal = ref(false)
const masterModalTarget = ref<ClubUserDto | null>(null)
const masterModalDefaultName = ref('')

// Add role dropdown
const openAddRoleDropdownId = ref<number | null>(null)

function getAvailableRoles(): string[] {
  return props.allowOwnerRole ? ['owner', 'supervisor', 'cashier'] : ['supervisor', 'cashier']
}

function roleLabel(role: string): string {
  const map: Record<string, string> = { owner: 'مدیر', supervisor: 'سوپروایزر', cashier: 'صندوق‌دار' }
  return map[role] || role
}

async function reload() {
  loading.value = true
  try {
    const res = await ClubUserApi.getMembers(props.clubId)
    members.value = res.data
  } catch {
    members.value = []
  } finally {
    loading.value = false
  }
}

function onFormSaved() {
  showAddForm.value = false
  reload()
}

// ── مدیریت نقش مدیریتی ──

function onRoleChange(m: ClubUserDto, event: Event) {
  const newRole = (event.target as HTMLSelectElement).value as 'owner' | 'supervisor' | 'cashier'
  updateMemberRole(m, newRole)
}

async function updateMemberRole(m: ClubUserDto, newRole: 'owner' | 'supervisor' | 'cashier') {
  try {
    await ClubUserApi.updateMemberRole(props.clubId, m.id, { clubuserRole: newRole })
    await reload()
  } catch {
    await reload()
  }
}

function confirmRemoveRole(m: ClubUserDto) {
  deleteTarget.value = m
  if (m.masterId !== null) {
    deleteMessage.value = 'این کاربر پروفایل گرداننده دارد؛ فقط نقش مدیریتی حذف می‌شود و عضو در سیستم باقی می‌ماند'
  } else {
    deleteMessage.value = 'این کاربر کاملاً از اعضای این باشگاه حذف می‌شود'
  }
  showDeleteConfirm.value = true
}

async function handleRemoveRole() {
  const m = deleteTarget.value
  if (!m) return
  isProcessing.value = true
  try {
    if (m.masterId !== null) {
      await ClubUserApi.updateMemberRole(props.clubId, m.id, { clubuserRole: 'master' })
    } else {
      await ClubUserApi.deleteMember(props.clubId, m.id)
    }
    await reload()
  } catch {
    await reload()
  } finally {
    isProcessing.value = false
    showDeleteConfirm.value = false
    deleteTarget.value = null
  }
}

function toggleAddRoleDropdown(id: number) {
  openAddRoleDropdownId.value = openAddRoleDropdownId.value === id ? null : id
}

async function addManagementRole(m: ClubUserDto, role: string) {
  openAddRoleDropdownId.value = null
  await updateMemberRole(m, role as 'owner' | 'supervisor' | 'cashier')
}

// ── مدیریت پروفایل گرداننده ──

function handleAddMaster(m: ClubUserDto) {
  masterModalTarget.value = m
  masterModalDefaultName.value = m.userDisplayName || ''
  showMasterModal.value = true
}

async function handleMasterSaved(name: string) {
  const m = masterModalTarget.value
  if (!m) return
  showMasterModal.value = false
  masterModalTarget.value = null
  try {
    await ClubApi.createMaster(props.clubId, { name, existingUserId: m.userId })
    await reload()
  } catch {
    await reload()
  }
}

function confirmUnlink(m: ClubUserDto) {
  unlinkTarget.value = m
  showUnlinkConfirm.value = true
}

async function handleUnlink() {
  const m = unlinkTarget.value
  if (!m || !m.masterId) return
  isProcessing.value = true
  try {
    await ClubApi.updateMaster(props.clubId, m.masterId, { unlinkUser: true })
    await reload()
  } catch {
    await reload()
  } finally {
    isProcessing.value = false
    showUnlinkConfirm.value = false
    unlinkTarget.value = null
  }
}

onMounted(reload)
</script>
