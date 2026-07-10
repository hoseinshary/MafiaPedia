<template>
  <button
    @click="open = true"
    class="p-2 rounded text-[#c9b07a] hover:bg-white/10 transition-colors"
    :title="title || 'منو'"
  >
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
    </svg>
  </button>

  <Teleport to="body">
    <div v-if="open" dir="rtl" class="fixed inset-0 z-50">
      <div class="absolute inset-0 bg-black/60" @click="open = false" />
      <div class="absolute top-0 right-0 h-full w-64 bg-[var(--color-card)] border-l border-[var(--color-border)] shadow-xl overflow-y-auto">
        <div class="px-4 py-4 border-b border-[var(--color-border)] flex items-center justify-between">
          <h3 class="text-sm font-semibold text-gray-400 tracking-wide">{{ title }}</h3>
          <button @click="open = false" class="text-gray-400 hover:text-white text-lg leading-none p-1">&times;</button>
        </div>
        <nav class="mt-2 space-y-1 px-2">
          <router-link
            v-for="link in links"
            :key="link.to"
            :to="link.to"
            @click="open = false"
            class="block px-3 py-2.5 rounded text-sm font-medium transition"
            :class="isActive(link.to) ? 'bg-gray-700 text-white' : 'text-gray-400 hover:text-white hover:bg-gray-800'"
          >
            {{ link.label }}
          </router-link>
        </nav>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'

defineProps<{
  links: { label: string; to: string }[]
  title?: string
}>()

const route = useRoute()
const open = ref(false)

function isActive(path: string): boolean {
  return route.path === path
}
</script>
