<template>
  <Teleport to="body">
    <div v-if="isOpen" dir="rtl" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="absolute inset-0 bg-black/60" @click="$emit('close')" />
      <div class="relative bg-surface rounded-lg w-[90%] sm:w-full p-6 shadow-xl border border-border max-h-[90vh] overflow-y-auto" :class="maxWidthClass">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-bold text-fg">{{ title }}</h3>
          <button @click="$emit('close')" class="text-muted hover:text-fg transition">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <slot />
        <div v-if="$slots.footer" class="flex gap-3 justify-end mt-6 pt-4 border-t border-border">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  isOpen: boolean
  title: string
  maxWidth?: 'sm' | 'md' | 'lg'
}>(), {
  maxWidth: 'md',
})

defineEmits<{
  close: []
}>()

const maxWidthClass = computed(() => {
  const map: Record<string, string> = { sm: 'max-w-sm', md: 'max-w-md', lg: 'max-w-lg' }
  return map[props.maxWidth]
})
</script>
