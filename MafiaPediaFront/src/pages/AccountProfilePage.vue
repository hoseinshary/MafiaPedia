<template>
  <div dir="rtl" class="max-w-lg mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-[#c9b07a]">پروفایل</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">تنظیمات حساب کاربری</p>
    </div>

    <div v-if="notification" class="mb-4 px-4 py-3 rounded text-sm" :class="notification.type === 'success' ? 'bg-green-900/50 border border-green-700 text-green-300' : 'bg-red-900/50 border border-red-700 text-red-300'">
      {{ notification.message }}
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else>
      <!-- Form section -->
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-6">
        <div class="flex flex-col gap-4">
          <div class="flex flex-col gap-1">
            <label class="text-sm text-[rgba(232,228,217,0.5)]">نام کاربری</label>
            <input v-model="form.username" type="text" class="bg-[var(--color-bg)] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.2)] focus:outline-none focus:border-[#c9b07a] transition" />
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-[rgba(232,228,217,0.5)]">نام نمایشی</label>
            <input v-model="form.displayName" type="text" class="bg-[var(--color-bg)] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.2)] focus:outline-none focus:border-[#c9b07a] transition" />
          </div>
          <div class="flex flex-col gap-1">
            <label class="text-sm text-[rgba(232,228,217,0.5)]">شماره موبایل</label>
            <input :value="account?.mobile" type="text" dir="ltr" disabled class="bg-[var(--color-bg)] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[rgba(232,228,217,0.3)] opacity-60 cursor-not-allowed" />
            <span class="text-xs text-[rgba(232,228,217,0.3)] mt-1">شماره موبایل قابل ویرایش نیست</span>
          </div>
          <button @click="handleSubmit" :disabled="submitting" class="self-start px-5 py-2 bg-[#c9b07a] text-[#141416] text-sm font-bold rounded hover:opacity-90 transition disabled:opacity-50 disabled:cursor-not-allowed inline-flex items-center gap-2">
            <div v-if="submitting" class="w-4 h-4 border-2 border-[#141416] border-t-transparent rounded-full animate-spin" />
            ذخیره تغییرات
          </button>
        </div>
      </div>

      <!-- Linked-entity picture sections -->
      <div v-if="hasAnyLinked" class="space-y-4">
        <div v-if="account?.player" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
          <h3 class="text-sm font-bold text-[#e8e4d9] mb-4">پروفایل عمومی بازیکن (فاز ۱)</h3>
          <LinkedPictureSection :name="account.player.name" :picture="account.player.picture" @upload="handleUpload('player', $event)" />
        </div>
        <div v-if="account?.clubplayer" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
          <h3 class="text-sm font-bold text-[#e8e4d9] mb-4">پروفایل مشتری کافه</h3>
          <LinkedPictureSection :name="account.clubplayer.name" :picture="account.clubplayer.picture" @upload="handleUpload('clubplayer', $event)" />
        </div>
        <div v-if="account?.master" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6">
          <h3 class="text-sm font-bold text-[#e8e4d9] mb-4">پروفایل گرداننده</h3>
          <LinkedPictureSection :name="account.master.name" :picture="account.master.photo" @upload="handleUpload('master', $event)" />
        </div>
      </div>
      <div v-else class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 text-center">
        <p class="text-sm text-[rgba(232,228,217,0.3)]">هیچ رکورد لینک‌شده‌ای یافت نشد</p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { getMyAccount, updateMyAccount, uploadLinkedPicture } from '@/api/AccountApi'
import type { AccountDto } from '@/api/AccountApi'
import LinkedPictureSection from '@/components/shared/LinkedPictureSection.vue'

const account = ref<AccountDto | null>(null)
const form = reactive({ username: '', displayName: '' })
const loading = ref(true)
const submitting = ref(false)
const notification = ref<{ type: 'success' | 'error'; message: string } | null>(null)

const hasAnyLinked = computed(() =>
  account.value?.player || account.value?.clubplayer || account.value?.master
)

onMounted(async () => {
  try {
    const data = await getMyAccount()
    account.value = data
    form.username = data.username
    form.displayName = data.displayName
  } catch {
    notification.value = { type: 'error', message: 'خطا در بارگذاری اطلاعات' }
  } finally {
    loading.value = false
  }
})

async function handleSubmit() {
  notification.value = null
  submitting.value = true
  try {
    const updated = await updateMyAccount({
      username: form.username || undefined,
      displayName: form.displayName || undefined,
    })
    account.value = updated
    notification.value = { type: 'success', message: 'تغییرات با موفقیت ذخیره شد' }
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        notification.value = { type: 'error', message: 'این نام کاربری قبلاً استفاده شده است' }
      } else {
        notification.value = { type: 'error', message: err.response?.data?.message || 'خطا در ذخیره تغییرات' }
      }
    } else {
      notification.value = { type: 'error', message: 'خطا در برقراری ارتباط' }
    }
  } finally {
    submitting.value = false
  }
}

async function handleUpload(target: 'player' | 'clubplayer' | 'master', file: File) {
  const allowed = ['.jpg', '.jpeg', '.png', '.webp']
  const ext = '.' + file.name.split('.').pop()?.toLowerCase()
  if (!allowed.includes(ext)) {
    notification.value = { type: 'error', message: 'فقط فایل‌های jpg، jpeg، png و webp مجاز هستند' }
    return
  }
  if (file.size > 5 * 1024 * 1024) {
    notification.value = { type: 'error', message: 'حجم فایل باید کمتر از ۵ مگابایت باشد' }
    return
  }

  notification.value = null
  try {
    const result = await uploadLinkedPicture(target, file)
    // Update local state with new path
    if (account.value) {
      if (target === 'player' && account.value.player) account.value.player.picture = result.path
      if (target === 'clubplayer' && account.value.clubplayer) account.value.clubplayer.picture = result.path
      if (target === 'master' && account.value.master) account.value.master.photo = result.path
    }
    notification.value = { type: 'success', message: 'عکس با موفقیت آپلود شد' }
  } catch {
    notification.value = { type: 'error', message: 'خطا در آپلود عکس' }
  }
}
</script>
