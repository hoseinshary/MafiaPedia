<template>
  <div dir="rtl" class="min-h-screen bg-gray-900 flex items-start justify-center px-4 py-10">
    <div class="w-full max-w-lg">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-white">ویرایش بازیکن</h1>
        <p class="text-gray-400 mt-2 text-sm">اطلاعات بازیکن را ویرایش کنید</p>
      </div>

      <div v-if="loadingDetail" class="flex justify-center py-20">
        <div class="w-10 h-10 border-4 border-gray-600 border-t-red-500 rounded-full animate-spin" />
      </div>

      <form v-else @submit.prevent="handleSubmit" class="bg-gray-800 rounded-lg p-6 space-y-5">
        <div v-if="error" class="bg-red-900/50 border border-red-700 text-red-300 text-sm rounded px-4 py-3">
          {{ error }}
        </div>
        <div v-if="success" class="bg-green-900/50 border border-green-700 text-green-300 text-sm rounded px-4 py-3">
          {{ success }}
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">نام <span class="text-red-500">*</span></label>
          <input
            v-model="name"
            type="text"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="نام بازیکن"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">کد بازیکن</label>
          <input
            v-model="code"
            type="text"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="اختیاری"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">موبایل</label>
          <input
            v-model="mobile"
            type="text"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="09123456789"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">تاریخ تولد</label>
          <input
            v-model="birthday"
            type="text"
            dir="ltr"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition"
            placeholder="مثال: ۱۳۷۰/۰۱/۱۵"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">توضیحات</label>
          <textarea
            v-model="desc"
            rows="3"
            class="bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition resize-none"
            placeholder="توضیحات درباره بازیکن"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-gray-300">عکس پروفایل</label>
          <div v-if="currentPicture && !preview" class="mb-2">
            <img :src="currentPicture" class="w-24 h-24 rounded-lg object-cover border border-gray-600" />
            <p class="text-xs text-gray-500 mt-1">عکس فعلی</p>
          </div>
          <input
            ref="fileInput"
            type="file"
            accept=".jpg,.jpeg,.png,.webp"
            @change="onFileChange"
            class="text-sm text-gray-400 file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-medium file:bg-blue-600 file:text-white hover:file:bg-blue-700 file:cursor-pointer"
          />
          <div v-if="fileError" class="text-xs text-red-400 mt-1">{{ fileError }}</div>
          <div v-if="preview" class="mt-2">
            <img :src="preview" class="w-24 h-24 rounded-lg object-cover border border-gray-600" />
            <p class="text-xs text-gray-500 mt-1">عکس جدید</p>
          </div>
        </div>

        <div class="flex gap-3 pt-2">
          <button
            type="submit"
            :disabled="submitting"
            class="flex-1 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded font-medium transition"
          >
            <span v-if="submitting" class="inline-flex items-center gap-2">
              <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              در حال ذخیره...
            </span>
            <span v-else>ذخیره تغییرات</span>
          </button>
          <router-link
            to="/players/list"
            class="flex-1 py-2.5 bg-gray-700 hover:bg-gray-600 text-gray-300 rounded font-medium transition text-center"
          >
            انصراف
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PlayerApi } from '@/api'

const route = useRoute()
const router = useRouter()
const playerId = Number(route.params.id)

const fileInput = ref<HTMLInputElement | null>(null)

const name = ref('')
const code = ref('')
const mobile = ref('')
const birthday = ref('')
const desc = ref('')
const picture = ref<File | null>(null)
const preview = ref<string | null>(null)
const currentPicture = ref<string | null>(null)
const fileError = ref('')

const error = ref('')
const success = ref('')
const submitting = ref(false)
const loadingDetail = ref(true)

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api'

const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
const MAX_SIZE = 3 * 1024 * 1024

function getFullUrl(picture: string): string {
  if (picture.startsWith('http')) return picture
  const base = baseUrl.replace(/\/api$/, '')
  return `${base}${picture.startsWith('/') ? '' : '/'}${picture}`
}

function onFileChange(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  fileError.value = ''
  preview.value = null
  picture.value = null

  if (!file) return

  if (!ALLOWED_TYPES.includes(file.type)) {
    fileError.value = 'فقط فرمت‌های jpg، jpeg، png و webp مجاز هستند'
    return
  }

  if (file.size > MAX_SIZE) {
    fileError.value = 'حجم فایل باید کمتر از 3 مگابایت باشد'
    return
  }

  picture.value = file
  const reader = new FileReader()
  reader.onload = () => preview.value = reader.result as string
  reader.readAsDataURL(file)
}

async function handleSubmit() {
  error.value = ''
  success.value = ''

  if (!name.value.trim()) {
    error.value = 'فیلد نام اجباری است'
    return
  }

  submitting.value = true
  try {
    const fd = new FormData()
    fd.append('Name', name.value.trim())
    if (code.value) fd.append('Code', code.value.trim())
    if (mobile.value) fd.append('Mobile', mobile.value.trim())
    if (birthday.value) fd.append('Birthday', birthday.value.trim())
    if (desc.value) fd.append('Desc', desc.value.trim())
    if (picture.value) fd.append('Picture', picture.value)

    await PlayerApi.updatePlayer(playerId, fd)
    success.value = 'تغییرات با موفقیت ذخیره شد'
    setTimeout(() => {
      router.push('/players/list')
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
    const res = await PlayerApi.getPlayerDetail(playerId)
    const data = res.data
    name.value = data.name
    code.value = data.code || ''
    mobile.value = data.mobile || ''
    birthday.value = data.birthday || ''
    desc.value = data.desc || ''
    if (data.picture) {
      currentPicture.value = getFullUrl(data.picture)
    }
  } catch {
    error.value = 'خطا در بارگذاری اطلاعات بازیکن'
  } finally {
    loadingDetail.value = false
  }
})
</script>