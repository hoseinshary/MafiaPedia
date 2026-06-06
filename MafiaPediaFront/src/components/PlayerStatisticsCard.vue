<template>
  <div class="bg-white rounded-lg border border-gray-200 p-4 text-center shadow-sm">
    <p class="text-sm text-gray-500 mb-1">{{ label }}</p>
    <p class="text-xl font-bold text-gray-900">{{ value }}</p>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value: string
}>()
</script>
