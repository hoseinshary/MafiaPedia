<template>
  <div dir="rtl" class="min-h-screen bg-[#0d0d0f] flex items-start justify-center px-4 py-10">
    <div class="w-full max-w-lg">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-[#e8e4d9]">ثبت بازیکن جدید</h1>
        <p class="text-[rgba(232,228,217,0.4)] mt-2 text-sm">اطلاعات بازیکن را وارد کنید</p>
      </div>

      <form @submit.prevent="handleSubmit" class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-6 space-y-5">
        <div v-if="error" class="bg-[rgba(224,112,112,0.1)] border border-[rgba(224,112,112,0.2)] text-[#e07070] text-sm rounded px-4 py-3">
          {{ error }}
        </div>
        <div v-if="success" class="bg-[rgba(111,207,138,0.1)] border border-[rgba(111,207,138,0.2)] text-[#6fcf8a] text-sm rounded px-4 py-3">
          {{ success }}
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">نام <span class="text-[#e07070]">*</span></label>
          <input
            v-model="name"
            type="text"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="نام بازیکن"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">کد بازیکن</label>
          <input
            v-model="code"
            type="text"
            dir="ltr"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="اختیاری"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">موبایل</label>
          <input
            v-model="mobile"
            type="text"
            dir="ltr"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="09123456789"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">تاریخ تولد</label>
          <input
            v-model="birthday"
            type="text"
            dir="ltr"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
            placeholder="مثال: ۱۳۷۰/۰۱/۱۵"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">توضیحات</label>
          <textarea
            v-model="desc"
            rows="3"
            class="bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-2 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition resize-none"
            placeholder="توضیحات درباره بازیکن"
          />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm text-[rgba(232,228,217,0.4)]">عکس پروفایل</label>
          <input
            ref="fileInput"
            type="file"
            accept=".jpg,.jpeg,.png,.webp"
            @change="onFileChange"
            class="text-sm text-[rgba(232,228,217,0.4)] file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-medium file:bg-[#c9b07a] file:text-[#0d0d0f] hover:file:bg-[#b8a16e] file:cursor-pointer"
          />
          <div v-if="fileError" class="text-xs text-[#e07070] mt-1">{{ fileError }}</div>
          <div v-if="preview" class="mt-2">
            <img :src="preview" class="w-24 h-24 rounded-lg object-cover border border-[rgba(255,255,255,0.07)]" />
          </div>
        </div>

        <div class="flex gap-3 pt-2">
          <button
            type="submit"
            :disabled="submitting"
            class="flex-1 py-2.5 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] rounded font-medium transition"
          >
            <span v-if="submitting" class="inline-flex items-center gap-2">
              <div class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              در حال ثبت...
            </span>
            <span v-else>ثبت بازیکن</span>
          </button>
          <router-link
            to="/"
            class="flex-1 py-2.5 bg-[rgba(255,255,255,0.05)] hover:bg-[rgba(255,255,255,0.08)] text-[rgba(232,228,217,0.4)] rounded font-medium transition text-center"
          >
            انصراف
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { PlayerApi } from '@/api'

const fileInput = ref<HTMLInputElement | null>(null)

const name = ref('')
const code = ref('')
const mobile = ref('')
const birthday = ref('')
const desc = ref('')
const picture = ref<File | null>(null)
const preview = ref<string | null>(null)
const fileError = ref('')

const error = ref('')
const success = ref('')
const submitting = ref(false)

const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
const MAX_SIZE = 2 * 1024 * 1024

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
    fileError.value = 'حجم فایل باید کمتر از ۲ مگابایت باشد'
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

    await PlayerApi.createPlayer(fd)
    success.value = 'بازیکن با موفقیت ثبت شد'
    resetForm()
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ثبت بازیکن'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  name.value = ''
  code.value = ''
  mobile.value = ''
  birthday.value = ''
  desc.value = ''
  picture.value = null
  preview.value = null
  fileError.value = ''
  if (fileInput.value) fileInput.value.value = ''
}
</script>
