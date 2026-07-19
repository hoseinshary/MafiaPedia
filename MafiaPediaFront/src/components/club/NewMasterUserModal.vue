<template>
  <Modal :is-open="isOpen" title="اطلاعات کاربر جدید" @close="$emit('close')">
    <div class="flex flex-col gap-3">
      <input
        v-model="form.username"
        type="text"
        placeholder="نام کاربری *"
        class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      />
      <input
        v-model="form.password"
        type="password"
        placeholder="رمز عبور *"
        class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      />
      <input
        v-model="form.mobile"
        type="text"
        placeholder="موبایل *"
        class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      />
      <input
        v-model="form.displayName"
        type="text"
        placeholder="نام نمایشی (اختیاری)"
        class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      />
      <p v-if="error" class="text-xs text-danger">{{ error }}</p>
    </div>
    <template #footer>
      <button
        @click="$emit('close')"
        class="px-4 py-2 border border-border text-muted hover:text-fg text-sm rounded font-medium transition"
      >
        انصراف
      </button>
      <button
        @click="submit"
        class="px-4 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-medium transition"
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
