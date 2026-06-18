<template>
  <div class="bg-[#141416] rounded-[10px] border border-[rgba(255,255,255,0.07)] p-4 text-center">
    <p class="text-sm text-[rgba(232,228,217,0.4)] mb-1">{{ label }}</p>
    <p class="text-xl font-bold text-[#e8e4d9]">{{ value }}</p>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value: string
}>()
</script>
