<template>
  <Teleport to="body">
    <div v-if="isOpen" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="absolute inset-0 bg-black/60" @click="$emit('cancel')" />
      <div class="relative bg-surface rounded-lg w-full max-w-md mx-4 p-6 shadow-xl border border-border">
        <h3 class="text-lg font-bold text-fg mb-2">{{ title }}</h3>
        <p class="text-sm text-fg mb-6 leading-relaxed">{{ message }}</p>
        <div class="flex gap-3 justify-end">
          <button
            :disabled="isLoading"
            class="px-5 py-2.5 rounded font-medium text-sm bg-input text-fg hover:bg-surface-hover transition disabled:opacity-50"
            @click="$emit('cancel')"
          >
            {{ cancelText }}
          </button>
          <button
            :disabled="isLoading"
            class="px-5 py-2.5 rounded font-medium text-sm bg-danger text-fg hover:opacity-80 transition disabled:opacity-50 disabled:cursor-not-allowed inline-flex items-center gap-2"
            @click="$emit('confirm')"
          >
            <div v-if="isLoading" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            {{ confirmText }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{
  isOpen: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  isLoading?: boolean
}>()

defineEmits<{
  confirm: []
  cancel: []
}>()
</script>