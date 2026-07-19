<template>
  <div>
    <div v-if="existingCustomerInfo && isCreateMode" class="mb-4 px-4 py-3 rounded text-sm bg-gold/10 border border-gold/20 text-gold-text">
      این شماره قبلاً با نام «{{ existingCustomerInfo.name }}» ثبت شده — با ثبت، فقط این مشتری به کافه اضافه می‌شود و اطلاعاتش تغییر نمی‌کند.
    </div>

    <div class="flex flex-col gap-3">
      <div>
        <label class="text-sm text-muted">نام <span class="text-danger">*</span></label>
        <input
          v-model="form.name"
          type="text"
          :disabled="fieldsDisabled"
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition disabled:opacity-40"
          placeholder="نام مشتری"
        />
      </div>

      <div>
        <label class="text-sm text-muted">موبایل <span class="text-danger">*</span></label>
        <input
          v-model="form.mobile"
          type="text"
          :readonly="!isCreateMode"
          :disabled="!isCreateMode"
          maxlength="11"
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition disabled:opacity-40 ltr text-left"
          placeholder="09123456789"
          @blur="onMobileBlur"
        />
      </div>

      <div>
        <label class="text-sm text-muted">تاریخ تولد</label>
        <input
          v-model="form.birthday"
          type="date"
          :disabled="fieldsDisabled"
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition disabled:opacity-40"
        />
      </div>

      <div>
        <label class="text-sm text-muted">کد</label>
        <input
          v-model="form.code"
          type="text"
          :disabled="fieldsDisabled"
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition disabled:opacity-40"
          placeholder="کد مشتری"
        />
      </div>

      <div>
        <label class="text-sm text-muted">توضیحات</label>
        <textarea
          v-model="form.desc"
          :disabled="fieldsDisabled"
          rows="2"
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition resize-none disabled:opacity-40"
          placeholder="توضیحات"
        />
      </div>

      <div>
        <label class="text-sm text-muted">عکس</label>
        <input
          type="file"
          accept="image/jpeg,image/png,image/webp"
          :disabled="fieldsDisabled"
          class="w-full text-sm text-muted file:mr-2 file:py-1.5 file:px-3 file:rounded file:border-0 file:text-xs file:bg-gold/15 file:text-gold-text hover:file:bg-gold/25 transition cursor-pointer disabled:opacity-40"
          @change="onPictureChange"
        />
        <img
          v-if="picturePreview"
          :src="picturePreview"
          class="mt-2 w-20 h-20 rounded object-cover border border-border"
        />
      </div>

      <p v-if="error" class="text-xs text-danger">{{ error }}</p>
    </div>

    <div class="flex gap-3 justify-end mt-6 pt-4 border-t border-border">
      <button
        @click="$emit('cancel')"
        class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition"
      >
        انصراف
      </button>
      <button
        @click="submit"
        :disabled="!form.name.trim() || !form.mobile.trim() || loading"
        class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center gap-2"
      >
        <div v-if="loading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
        {{ initial ? 'ویرایش' : 'افزودن مشتری' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ClubPlayerApi } from '@/api'
import { useToast } from '@/composables/useToast'
import type { ClubPlayerDto, ClubPlayerJoinResult } from '@/types/clubPlayer'

const props = defineProps<{
  clubId: number
  initial?: ClubPlayerDto | null
}>()

const emit = defineEmits<{
  saved: [result: ClubPlayerJoinResult]
  cancel: []
}>()

const isCreateMode = computed(() => !props.initial)
const fieldsDisabled = computed(() => isCreateMode.value && existingCustomerInfo.value !== null)

const form = reactive({
  name: props.initial?.name ?? '',
  mobile: props.initial?.mobile ?? '',
  birthday: props.initial?.birthday ? props.initial.birthday.slice(0, 10) : '',
  code: props.initial?.code ?? '',
  desc: props.initial?.desc ?? '',
})

const pictureFile = ref<File | null>(null)
const picturePreview = ref('')
const loading = ref(false)
const error = ref('')
const existingCustomerInfo = ref<ClubPlayerDto | null>(null)
let mobileCheckTimer: ReturnType<typeof setTimeout>

const { toastSuccess } = useToast()

function onPictureChange(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return
  pictureFile.value = file
  if (picturePreview.value) URL.revokeObjectURL(picturePreview.value)
  picturePreview.value = URL.createObjectURL(file)
}

function onMobileBlur() {
  if (!isCreateMode.value) return
  clearTimeout(mobileCheckTimer)
  const mobile = form.mobile.trim()
  if (mobile.length !== 11) return
  mobileCheckTimer = setTimeout(async () => {
    try {
      const res = await ClubPlayerApi.searchByMobile(mobile)
      existingCustomerInfo.value = res.data
    } catch {
      existingCustomerInfo.value = null
    }
  }, 400)
}

function buildFormData(): FormData {
  const fd = new FormData()
  fd.append('name', form.name)
  fd.append('mobile', form.mobile)
  if (form.birthday) fd.append('birthday', form.birthday)
  if (form.code) fd.append('code', form.code)
  if (form.desc) fd.append('desc', form.desc)
  if (pictureFile.value) fd.append('picture', pictureFile.value)
  return fd
}

async function submit() {
  if (!form.name.trim() || !form.mobile.trim()) return
  loading.value = true
  error.value = ''
  try {
    if (props.initial) {
      const fd = buildFormData()
      const res = await ClubPlayerApi.updateClubPlayer(props.clubId, props.initial.id, fd)
      const result: ClubPlayerJoinResult = { clubPlayer: res.data, wasExistingCustomer: false }
      emit('saved', result)
      toastSuccess('مشتری با موفقیت ویرایش شد')
    } else {
      const fd = buildFormData()
      const res = await ClubPlayerApi.createOrJoin(props.clubId, fd)
      emit('saved', res.data)
      form.name = ''
      form.mobile = ''
      form.birthday = ''
      form.code = ''
      form.desc = ''
      pictureFile.value = null
      if (picturePreview.value) {
        URL.revokeObjectURL(picturePreview.value)
        picturePreview.value = ''
      }
      existingCustomerInfo.value = null
    }
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { status?: number; data?: { message?: string } } }
      if (err.response?.status === 409) {
        error.value = err.response?.data?.message || 'این مشتری قبلاً عضو این کافه است'
      } else {
        error.value = err.response?.data?.message || 'خطا در ثبت مشتری'
      }
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (props.initial?.picture) {
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5272/api'
    const base = baseUrl.replace(/\/api$/, '')
    picturePreview.value = base + props.initial.picture
  }
})
</script>
