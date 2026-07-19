<template>
  <Modal :is-open="isOpen" title="افزودن پروفایل گرداننده" @close="$emit('close')">
    <div class="flex flex-col gap-3">
      <input
        v-model="name"
        type="text"
        placeholder="نام گرداننده *"
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

<script setup lang="ts">
import { ref, watch } from 'vue'
import Modal from '@/components/shared/Modal.vue'

const props = defineProps<{
  isOpen: boolean
  defaultName: string
}>()

const emit = defineEmits<{
  saved: [name: string]
  close: []
}>()

const name = ref('')
const error = ref('')

watch(() => props.isOpen, (open) => {
  if (open) {
    name.value = props.defaultName || ''
    error.value = ''
  }
})

function submit() {
  const trimmed = name.value.trim()
  if (!trimmed) {
    error.value = 'نام گرداننده الزامی است'
    return
  }
  emit('saved', trimmed)
}
</script>
