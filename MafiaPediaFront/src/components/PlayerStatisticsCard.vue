<template>
  <div class="bg-surface rounded-[10px] border border-border p-4 text-center">
    <p class="text-sm text-muted mb-1">{{ label }}</p>
    <p class="text-xl font-bold text-fg">{{ value }}</p>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value: string
}>()
</script>
