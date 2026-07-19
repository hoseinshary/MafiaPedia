<template>
  <div
    class="relative overflow-hidden rounded-lg bg-surface border border-border hover:shadow-md transition"
    :class="sizeClasses[size]"
  >
    <div class="aspect-video relative">
      <img
        v-if="resolvedUrl"
        :src="resolvedUrl"
        class="w-full h-full object-cover"
        alt=""
      />
      <div
        v-else
        class="w-full h-full flex items-center justify-center bg-surface text-muted"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="w-8 h-8" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 10.5l4.72-4.72a.75.75 0 011.28.53v11.38a.75.75 0 01-1.28.53l-4.72-4.72M4.5 18.75h9a2.25 2.25 0 002.25-2.25v-9a2.25 2.25 0 00-2.25-2.25h-9A2.25 2.25 0 002.25 7.5v9a2.25 2.25 0 002.25 2.25z" />
        </svg>
      </div>
      <slot name="overlay" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { getPictureUrl } from '@/utils/picture'

const props = withDefaults(defineProps<{
  picture: string | null
  size?: 'sm' | 'md' | 'lg'
}>(), {
  size: 'md'
})

const resolvedUrl = computed(() => getPictureUrl(props.picture))

const sizeClasses: Record<string, string> = {
  sm: 'w-28 shrink-0',
  md: 'w-56 shrink-0',
  lg: 'w-full',
}
</script>
