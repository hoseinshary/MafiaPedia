<template>
  <Teleport to="body">
    <div
      dir="rtl"
      class="fixed bottom-4 left-4 z-[9999] flex flex-col gap-3 pointer-events-none"
    >
      <TransitionGroup name="toast">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          class="pointer-events-auto flex items-center gap-3 rounded-[10px] px-4 py-3 shadow-lg"
          :class="borderClass(toast.type)"
          style="
            min-width: 260px;
            max-width: 360px;
            background: var(--color-surface);
            border: 0.5px solid var(--color-border);
            font-family: Vazirmatn, system-ui, sans-serif;
            font-size: 14px;
            color: var(--color-fg);
          "
        >
          <span class="flex-shrink-0 text-base leading-none">{{ icon(toast.type) }}</span>
          <span class="flex-1 leading-relaxed">{{ toast.message }}</span>
          <button
            class="flex-shrink-0 text-base leading-none opacity-50 hover:opacity-100 transition-opacity bg-transparent border-none cursor-pointer p-0"
            @click="removeToast(toast.id)"
          >
            &times;
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useToast, type ToastType } from '../composables/useToast'

const { toasts, removeToast } = useToast()

function icon(type: ToastType): string {
  if (type === 'error') return '\u2715'
  if (type === 'success') return '\u2713'
  return '\u26A0'
}

function borderClass(type: ToastType): string {
  if (type === 'error') return 'border-r-[4px] border-r-danger'
  if (type === 'success') return 'border-r-[4px] border-r-success'
  return 'border-r-[4px] border-r-gold'
}
</script>

<style scoped>
.toast-enter-active {
  transition: all 0.3s ease-out;
}
.toast-leave-active {
  transition: all 0.25s ease-in;
}
.toast-enter-from {
  opacity: 0;
  transform: translateY(12px);
}
.toast-leave-to {
  opacity: 0;
  transform: translateY(12px);
}
</style>
