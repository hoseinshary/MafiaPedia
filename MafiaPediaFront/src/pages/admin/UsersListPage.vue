<template>
  <div dir="rtl" class="max-w-4xl mx-auto px-6 w-full">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl md:text-3xl font-bold text-fg">مدیریت کاربران</h1>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-medium transition"
      >
        کاربر جدید
      </button>
    </div>

    <div class="mb-6">
      <input
        v-model="searchQuery"
        type="text"
        placeholder="جستجوی کاربر..."
        class="w-full md:w-1/2 bg-surface border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      />
    </div>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm border" :class="notification.type === 'success' ? 'bg-success/20 border-success text-success' : 'bg-danger/20 border-danger text-danger'">
      {{ notification.message }}
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="users.length === 0" class="text-center py-20 text-muted text-lg">
      هیچ کاربری یافت نشد.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-border bg-surface-hover text-muted">
            <th class="px-4 py-3 text-right">نام نمایشی</th>
            <th class="px-4 py-3 text-right">نام کاربری</th>
            <th class="px-4 py-3 text-right">موبایل</th>
            <th class="px-4 py-3 text-right">نقش</th>
            <th class="px-4 py-3 text-right">وضعیت</th>
            <th class="px-4 py-3 text-right">آخرین ورود</th>
            <th class="px-4 py-3 text-right">عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="user in users"
            :key="user.id"
            class="border-b border-border hover:bg-surface-hover transition text-fg"
          >
            <td class="px-4 py-3 font-medium">{{ user.displayName || '—' }}</td>
            <td class="px-4 py-3">{{ user.username }}</td>
            <td class="px-4 py-3 text-muted">{{ user.mobile || '—' }}</td>
            <td class="px-4 py-3">
              <span class="inline-block px-2 py-0.5 rounded text-xs font-medium" :class="roleBadgeClass(user.role)">
                {{ roleLabel(user.role) }}
              </span>
            </td>
            <td class="px-4 py-3">
              <span class="inline-block px-2 py-0.5 rounded text-xs font-medium" :class="user.isActive ? 'bg-success/15 text-success' : 'bg-danger/15 text-danger'">
                {{ user.isActive ? 'فعال' : 'غیرفعال' }}
              </span>
            </td>
            <td class="px-4 py-3 text-muted whitespace-nowrap">{{ user.lastLogin ? formatDate(user.lastLogin) : '—' }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-2">
                <router-link
                  :to="`/admin/users/${user.id}/edit`"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-gold text-[#0d0d0f] hover:opacity-80 transition"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z"/>
                  </svg>
                  ویرایش
                </router-link>
                <button
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded text-xs font-medium bg-danger text-[#0d0d0f] hover:opacity-80 transition"
                  @click="confirmDelete(user)"
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
        <span class="text-muted">
          صفحه {{ page }} از {{ totalPages }}
        </span>
        <div class="flex gap-2">
          <button
            :disabled="page <= 1"
            class="px-4 py-2 rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
            @click="page = Math.max(1, page - 1)"
          >
            قبلی
          </button>
          <button
            :disabled="page >= totalPages"
            class="px-4 py-2 rounded border border-border text-muted disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-hover transition"
            @click="page = Math.min(totalPages, page + 1)"
          >
            بعدی
          </button>
        </div>
      </div>
    </div>

    <!-- Create User Modal -->
    <Teleport to="body">
      <div v-if="showCreateModal" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center">
        <div class="absolute inset-0 bg-black/60" @click="showCreateModal = false" />
        <div class="relative bg-surface rounded-[10px] w-full max-w-md mx-4 p-6 shadow-xl border border-border max-h-[90vh] overflow-y-auto">
          <h3 class="text-lg font-bold text-fg mb-4">ایجاد کاربر جدید</h3>

          <div v-if="createError" class="mb-4 px-4 py-2 rounded text-sm bg-danger/10 border border-danger/20 text-danger">
            {{ createError }}
          </div>

          <div class="space-y-4">
            <div class="flex flex-col gap-1">
              <label class="text-sm text-muted">نام کاربری <span class="text-danger">*</span></label>
              <input v-model="createForm.username" type="text" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-sm text-muted">رمز عبور <span class="text-danger">*</span></label>
              <input v-model="createForm.password" type="password" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
              <div v-if="createTouched && createForm.password && createForm.password.length < 6" class="text-xs text-danger mt-1">حداقل ۶ کاراکتر</div>
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-sm text-muted">نام نمایشی</label>
              <input v-model="createForm.displayName" type="text" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-sm text-muted">موبایل</label>
              <input v-model="createForm.mobile" type="text" dir="ltr" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition" />
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-sm text-muted">نقش</label>
              <select v-model="createForm.role" class="bg-input border border-border rounded px-3 py-2 text-sm text-fg focus:outline-none focus:border-gold transition">
                <option value="user">بازیکن</option>
                <option value="admin">ادمین</option>
                <option value="master">گرداننده</option>
                <option value="cafe_owner">صاحب کافه</option>
              </select>
            </div>
          </div>

          <div class="flex gap-3 mt-6">
            <button
              @click="submitCreate"
              :disabled="creating"
              class="flex-1 py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded font-medium transition inline-flex items-center justify-center gap-2"
            >
              <div v-if="creating" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              ایجاد کاربر
            </button>
            <button
              @click="showCreateModal = false"
              class="flex-1 py-2.5 bg-surface-hover hover:bg-surface text-muted rounded font-medium transition"
            >
              انصراف
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <ConfirmModal
      :is-open="showDeleteModal"
      title="حذف کاربر"
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
import { ref, computed, watch, onMounted } from 'vue'
import { UserApi } from '@/api'
import type { UserDto } from '@/types'
import ConfirmModal from '@/components/ConfirmModal.vue'

const users = ref<UserDto[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = 20
const totalPages = ref(1)
const searchQuery = ref('')
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

const showCreateModal = ref(false)
const creating = ref(false)
const createError = ref('')
const createTouched = ref(false)
const createForm = ref({ username: '', password: '', displayName: '', mobile: '', role: 'user' })

const showDeleteModal = ref(false)
const userToDelete = ref<{ id: number; username: string } | null>(null)
const isDeleting = ref(false)

const deleteMessage = computed(() =>
  `آیا مطمئن هستید که می‌خواهید کاربر «${userToDelete.value?.username}» را حذف کنید؟`
)

let debounceTimer: ReturnType<typeof setTimeout>

const roleBadgeClass = (role: string): string => {
  const map: Record<string, string> = {
    admin: 'bg-danger/15 text-danger',
    user: 'bg-gold/15 text-gold-text',
    master: 'bg-gold/15 text-gold-text',
    cafe_owner: 'bg-success/15 text-success',
  }
  return map[role] || 'bg-border text-muted'
}

const roleLabel = (role: string): string => {
  const map: Record<string, string> = {
    admin: 'ادمین',
    user: 'بازیکن',
    master: 'گرداننده',
    cafe_owner: 'صاحب کافه',
  }
  return map[role] || role
}

function formatDate(dateStr: string): string {
  try {
    return new Intl.DateTimeFormat('fa-IR', { year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

async function fetchUsers() {
  loading.value = true
  try {
    const res = await UserApi.getUsers(page.value, pageSize, searchQuery.value || undefined)
    users.value = res.data.items
    totalPages.value = res.data.totalPages || Math.max(1, Math.ceil(res.data.totalItems / pageSize))
  } catch {
    users.value = []
  } finally {
    loading.value = false
  }
}

function showNotification(type: 'success' | 'error', message: string) {
  notification.value = { type, message }
  setTimeout(() => { notification.value = null }, 4000)
}

async function submitCreate() {
  createTouched.value = true
  createError.value = ''

  if (!createForm.value.username.trim()) {
    createError.value = 'نام کاربری اجباری است'
    return
  }
  if (!createForm.value.password || createForm.value.password.length < 6) {
    createError.value = 'رمز عبور باید حداقل ۶ کاراکتر باشد'
    return
  }

  creating.value = true
  try {
    await UserApi.createUser({
      username: createForm.value.username.trim(),
      password: createForm.value.password,
      displayName: createForm.value.displayName.trim() || undefined,
      mobile: createForm.value.mobile.trim() || undefined,
      role: createForm.value.role,
    })
    showCreateModal.value = false
    createForm.value = { username: '', password: '', displayName: '', mobile: '', role: 'user' }
    createTouched.value = false
    showNotification('success', 'کاربر با موفقیت ایجاد شد')
    await fetchUsers()
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      const msg = err.response?.data?.message || ''
      if (msg.toLowerCase().includes('duplicate') || msg.toLowerCase().includes('تکراری')) {
        createError.value = 'این نام کاربری قبلاً ثبت شده است'
      } else {
        createError.value = msg || 'خطا در ایجاد کاربر'
      }
    } else {
      createError.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    creating.value = false
  }
}

function confirmDelete(user: UserDto) {
  userToDelete.value = { id: user.id, username: user.username }
  showDeleteModal.value = true
}

function closeDeleteModal() {
  showDeleteModal.value = false
  userToDelete.value = null
}

async function handleDelete() {
  if (!userToDelete.value) return
  isDeleting.value = true
  try {
    await UserApi.deleteUser(userToDelete.value.id)
    showDeleteModal.value = false
    userToDelete.value = null
    showNotification('success', 'کاربر با موفقیت حذف شد')
    await fetchUsers()
  } catch (e: unknown) {
    showDeleteModal.value = false
    userToDelete.value = null
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 400) {
        showNotification('error', 'نمی‌توانید حساب کاربری خود را حذف کنید')
      } else if (err.response?.status === 409) {
        showNotification('error', err.response?.data?.message || 'این کاربر دارای کامنت است و قابل حذف نیست')
      } else {
        showNotification('error', err.response?.data?.message || 'خطا در حذف کاربر')
      }
    } else {
      showNotification('error', 'خطا در برقراری ارتباط')
    }
  } finally {
    isDeleting.value = false
  }
}

watch(searchQuery, () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    page.value = 1
    fetchUsers()
  }, 400)
})

watch(page, () => {
  fetchUsers()
})

onMounted(() => {
  fetchUsers()
})
</script>
