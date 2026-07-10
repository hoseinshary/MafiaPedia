<template>
  <Modal :is-open="isOpen" title="اطلاعات کاربر جدید" @close="$emit('close')">
    <div class="flex flex-col gap-3">
      <input
        v-model="form.username"
        type="text"
        placeholder="نام کاربری *"
        class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
      <input
        v-model="form.password"
        type="password"
        placeholder="رمز عبور *"
        class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
      <input
        v-model="form.mobile"
        type="text"
        placeholder="موبایل *"
        class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
      <input
        v-model="form.displayName"
        type="text"
        placeholder="نام نمایشی (اختیاری)"
        class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
      />
      <p v-if="error" class="text-xs text-[#e07070]">{{ error }}</p>
    </div>
    <template #footer>
      <button
        @click="$emit('close')"
        class="px-4 py-2 border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] text-sm rounded font-medium transition"
      >
        انصراف
      </button>
      <button
        @click="submit"
        class="px-4 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-medium transition"
      >
        تأیید
      </button>
    </template>
  </Modal>
</template>

<script lang="ts">
export interface NewUserFormData {
  username: string
  password: string
  mobile: string
  displayName: string | null
}
</script>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import Modal from '@/components/shared/Modal.vue'

const props = defineProps<{
  isOpen: boolean
  initial?: NewUserFormData | null
}>()

const emit = defineEmits<{
  saved: [data: NewUserFormData]
  close: []
}>()

const error = ref('')
const form = reactive<NewUserFormData>({
  username: '',
  password: '',
  mobile: '',
  displayName: null,
})

watch(() => props.isOpen, (open) => {
  if (open && props.initial) {
    form.username = props.initial.username
    form.password = props.initial.password
    form.mobile = props.initial.mobile
    form.displayName = props.initial.displayName
  } else if (open && !props.initial) {
    form.username = ''
    form.password = ''
    form.mobile = ''
    form.displayName = null
  }
})

function submit() {
  error.value = ''
  if (!form.username.trim()) { error.value = 'نام کاربری الزامی است'; return }
  if (!form.password.trim()) { error.value = 'رمز عبور الزامی است'; return }
  if (!form.mobile.trim()) { error.value = 'موبایل الزامی است'; return }
  emit('saved', {
    username: form.username.trim(),
    password: form.password,
    mobile: form.mobile.trim(),
    displayName: form.displayName?.trim() || null,
  })
}
</script>
