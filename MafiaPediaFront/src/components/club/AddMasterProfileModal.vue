<template>
  <Modal :is-open="isOpen" title="افزودن پروفایل گرداننده" @close="$emit('close')">
    <div class="flex flex-col gap-3">
      <input
        v-model="name"
        type="text"
        placeholder="نام گرداننده *"
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
