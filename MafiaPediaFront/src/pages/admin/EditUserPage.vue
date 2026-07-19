<template>
  <div dir="rtl" class="min-h-screen bg-bg flex items-start justify-center px-4 py-10">
    <div class="w-full max-w-lg">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-fg">ویرایش کاربر</h1>
        <p class="text-muted mt-2 text-sm">اطلاعات کاربر را ویرایش کنید</p>
      </div>

      <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm" :class="notification.type === 'success' ? 'bg-green-900/50 border border-green-700 text-green-300' : 'bg-red-900/50 border border-red-700 text-red-300'">
        {{ notification.message }}
      </div>

      <div v-if="loadingDetail" class="flex justify-center py-20">
        <div class="w-10 h-10 border-4 border-border border-t-danger rounded-full animate-spin" />
      </div>

      <form v-else @submit.prevent="handleSubmit" class="bg-surface rounded-lg p-6 space-y-5">
        <div v-if="error" class="bg-red-900/50 border border-red-700 text-red-300 text-sm rounded px-4 py-3">
          {{ error }}
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-fg">نام نمایشی</label>
          <input
            v-model="displayName"
            type="text"
            class="bg-surface border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-fg">موبایل</label>
          <input
            v-model="mobile"
            type="text"
            dir="ltr"
            class="bg-surface border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-fg">نقش</label>
          <select
            v-model="role"
            class="bg-surface border border-border rounded px-3 py-2 text-sm text-fg focus:outline-none focus:border-gold transition"
          >
            <option value="player">بازیکن</option>
            <option value="admin">ادمین</option>
            <option value="master">گرداننده</option>
            <option value="cafe_owner">صاحب کافه</option>
          </select>
        </div>

        <div class="flex items-center gap-3">
          <label class="text-sm text-fg">وضعیت</label>
          <button
            type="button"
            @click="isActive = !isActive"
            class="relative w-11 h-6 rounded-full transition"
            :class="isActive ? 'bg-success' : 'bg-surface-hover'"
          >
            <span class="absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition" :class="isActive ? 'left-0.5' : 'right-0.5'" />
          </button>
          <span class="text-sm" :class="isActive ? 'text-success' : 'text-muted'">{{ isActive ? 'فعال' : 'غیرفعال' }}</span>
        </div>

        <div class="border-t border-border pt-4">
          <div class="flex flex-col gap-1">
            <label class="text-sm text-fg">رمز عبور جدید</label>
            <input
              v-model="newPassword"
              type="password"
              dir="ltr"
              class="bg-surface border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
              placeholder="اختیاری — فقط در صورت تغییر پر کنید"
            />
          </div>
          <div class="flex flex-col gap-1 mt-3">
            <label class="text-sm text-fg">تکرار رمز عبور جدید</label>
            <input
              v-model="confirmPassword"
              type="password"
              dir="ltr"
              class="bg-surface border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
              placeholder="تکرار رمز عبور"
            />
            <div v-if="passwordError" class="text-xs text-red-400 mt-1">{{ passwordError }}</div>
          </div>
        </div>

        <div class="flex gap-3 pt-2">
          <button
            type="submit"
            :disabled="submitting"
            class="flex-1 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-fg rounded font-medium transition inline-flex items-center justify-center gap-2"
          >
            <div v-if="submitting" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ذخیره تغییرات
          </button>
          <router-link
            to="/admin/users"
            class="flex-1 py-2.5 bg-surface hover:bg-surface-hover text-fg rounded font-medium transition text-center"
          >
            انصراف
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { UserApi } from '@/api'

const route = useRoute()
const router = useRouter()
const userId = Number(route.params.id)

const displayName = ref('')
const mobile = ref('')
const role = ref('player')
const isActive = ref(true)
const newPassword = ref('')
const confirmPassword = ref('')

const error = ref('')
const submitting = ref(false)
const loadingDetail = ref(true)
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

const passwordError = computed(() => {
  if (!newPassword.value) return ''
  if (newPassword.value.length < 6) return 'رمز عبور باید حداقل ۶ کاراکتر باشد'
  if (newPassword.value !== confirmPassword.value) return 'رمز عبور و تکرار آن یکسان نیست'
  return ''
})

async function handleSubmit() {
  error.value = ''
  notification.value = null

  if (newPassword.value && passwordError.value) return

  submitting.value = true
  try {
    const dto: { displayName?: string; mobile?: string; role?: string; isActive?: boolean; password?: string } = {}
    if (displayName.value) dto.displayName = displayName.value.trim()
    if (mobile.value) dto.mobile = mobile.value.trim()
    dto.role = role.value
    dto.isActive = isActive.value
    if (newPassword.value) dto.password = newPassword.value

    await UserApi.updateUser(userId, dto)
    notification.value = { type: 'success', message: 'تغییرات با موفقیت ذخیره شد' }
    setTimeout(() => {
      router.push('/admin/users')
    }, 1500)
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ذخیره تغییرات'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  try {
    const res = await UserApi.getUserDetail(userId)
    const data = res.data
    displayName.value = data.displayName || ''
    mobile.value = data.mobile || ''
    role.value = data.role
    isActive.value = data.isActive
  } catch {
    error.value = 'خطا در بارگذاری اطلاعات کاربر'
  } finally {
    loadingDetail.value = false
  }
})
</script>