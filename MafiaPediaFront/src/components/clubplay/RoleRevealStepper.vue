<template>
  <div class="w-full max-w-sm">
    <div v-if="currentIndex < items.length" class="bg-surface border border-border rounded-2xl p-8 text-center">
      <div class="mb-2 text-sm text-muted">
        {{ currentIndex + 1 }} از {{ items.length }}
      </div>
      <div class="text-lg font-bold text-fg mb-6 flex items-center justify-center gap-2">
        {{ items[currentIndex]?.label }}
        <span v-if="items[currentIndex]?.isGuest" class="text-sm font-normal text-muted">(مهمان)</span>
        <span v-if="(items[currentIndex]?.entryCount ?? 0) > 1"
              class="inline-flex items-center px-2 py-0.5 text-xs rounded-full bg-gold/20 text-gold-text border border-gold/30 leading-none">
          ×{{ toPersianDigits(items[currentIndex]!.entryCount ?? 0) }}
        </span>
      </div>

      <div v-if="!revealed" @click="revealed = true" class="cursor-pointer select-none">
        <div class="max-w-[320px] w-full mx-auto mb-6 rounded-2xl overflow-hidden border-2 border-gold/30 flex items-center justify-center bg-bg" style="aspect-ratio: 1">
          <img src="/assets/images/bigmask.webp" alt="ماسک مافیا" class="w-full h-full object-contain" />
        </div>
        <p class="text-sm text-gold-text">لمس کن</p>
      </div>

      <div v-else class="animate-fadeIn">
        <div
          class="max-w-[320px] w-full mx-auto mb-3 overflow-hidden rounded-2xl border-2"
          style="aspect-ratio: 1"
          :class="items[currentIndex].sideId === 1 ? 'border-[rgba(220,80,80,0.5)] bg-[rgba(220,80,80,0.1)]' : 'border-[rgba(80,150,220,0.5)] bg-[rgba(80,150,220,0.1)]'"
        >
          <img v-if="items[currentIndex].rolePhoto" :src="getPictureUrl(items[currentIndex].rolePhoto)" alt="" class="w-full h-full object-contain p-4" />
          <div v-else class="flex flex-col items-center justify-center w-full h-full">
            <div class="text-lg font-bold" :class="items[currentIndex].sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">
              {{ items[currentIndex].sideId === 1 ? 'مافیا' : 'شهروند' }}
            </div>
            <div class="text-4xl text-fg mt-1">{{ items[currentIndex].roleName }}</div>
          </div>
        </div>
        <div v-if="items[currentIndex].rolePhoto" class="text-center mb-4">
          <div class="text-lg font-bold" :class="items[currentIndex].sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">
            {{ items[currentIndex].sideId === 1 ? 'مافیا' : 'شهروند' }}
          </div>
          <div class="text-4xl text-fg mt-1">{{ items[currentIndex].roleName }}</div>
        </div>
        <button
          @click="nextItem"
          class="px-6 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-bold transition"
        >
          متوجه شدم
        </button>
      </div>

      <div class="mt-6 pt-4 border-t border-border">
        <slot name="reset-btn" />
      </div>
    </div>

    <div v-else class="text-center mt-8">
      <div class="bg-surface border border-border rounded-2xl p-10 max-w-sm mx-auto">
        <div class="text-4xl mb-4">✅</div>
        <h2 class="text-xl font-bold text-gold-text mb-2">پخش نقش کامل شد</h2>
        <p class="text-sm text-muted mb-6">همه {{ items.length }} نفر نقش خود را دیدند</p>
        <slot name="completion-actions">
          <button
            @click="$emit('done')"
            class="px-6 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-bold transition"
          >
            {{ doneLabel || 'پایان' }}
          </button>
        </slot>
        <div class="mt-4">
          <slot name="reset-btn" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { getPictureUrl } from '@/utils/picture'

function toPersianDigits(n: number): string {
  return n.toString().replace(/\d/g, d => '۰۱۲۳۴۵۶۷۸۹'[parseInt(d)])
}

export interface RevealItem {
  label: string
  roleName: string
  rolePhoto: string | null
  sideId: number
  isGuest?: boolean
  entryCount?: number
}

const props = defineProps<{
  items: RevealItem[]
  doneLabel?: string
}>()

defineEmits<{
  (e: 'done'): void
}>()

const currentIndex = ref(0)
const revealed = ref(false)

function nextItem() {
  if (currentIndex.value < props.items.length - 1) {
    currentIndex.value++
    revealed.value = false
  } else {
    currentIndex.value++
  }
}

function reset() {
  currentIndex.value = 0
  revealed.value = false
}

defineExpose({ reset })
</script>

<style scoped>
.animate-fadeIn {
  animation: fadeIn 0.3s ease-in;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>