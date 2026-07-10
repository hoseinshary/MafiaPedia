<template>
  <div class="w-full max-w-sm">
    <div v-if="currentIndex < items.length" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-2xl p-8 text-center">
      <div class="mb-2 text-sm text-[rgba(232,228,217,0.4)]">
        {{ currentIndex + 1 }} از {{ items.length }}
      </div>
      <div class="text-lg font-bold text-[#e8e4d9] mb-6">
        {{ items[currentIndex].label }}
        <span v-if="items[currentIndex].isGuest" class="text-sm font-normal text-[rgba(232,228,217,0.4)]">(مهمان)</span>
      </div>

      <div v-if="!revealed" @click="revealed = true" class="cursor-pointer select-none">
        <div class="max-w-[320px] w-[80vw] mx-auto mb-6 rounded-2xl overflow-hidden border-2 border-[rgba(201,176,122,0.3)] flex items-center justify-center bg-[#0d0d0f]" style="aspect-ratio: 1">
          <img src="/assets/images/bigmask.webp" alt="ماسک مافیا" class="w-full h-full object-contain" />
        </div>
        <p class="text-sm text-[#c9b07a]">لمس کن</p>
      </div>

      <div v-else class="animate-fadeIn">
        <div
          class="max-w-[320px] w-[80vw] mx-auto mb-3 overflow-hidden rounded-2xl border-2"
          style="aspect-ratio: 1"
          :class="items[currentIndex].sideId === 1 ? 'border-[rgba(220,80,80,0.5)] bg-[rgba(220,80,80,0.1)]' : 'border-[rgba(80,150,220,0.5)] bg-[rgba(80,150,220,0.1)]'"
        >
          <img v-if="items[currentIndex].rolePhoto" :src="getPictureUrl(items[currentIndex].rolePhoto)" alt="" class="w-full h-full object-contain p-4" />
          <div v-else class="flex flex-col items-center justify-center w-full h-full">
            <div class="text-lg font-bold" :class="items[currentIndex].sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">
              {{ items[currentIndex].sideId === 1 ? 'مافیا' : 'شهروند' }}
            </div>
            <div class="text-lg text-[#e8e4d9] mt-1">{{ items[currentIndex].roleName }}</div>
          </div>
        </div>
        <div v-if="items[currentIndex].rolePhoto" class="text-center mb-4">
          <div class="text-lg font-bold" :class="items[currentIndex].sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">
            {{ items[currentIndex].sideId === 1 ? 'مافیا' : 'شهروند' }}
          </div>
          <div class="text-lg text-[#e8e4d9] mt-1">{{ items[currentIndex].roleName }}</div>
        </div>
        <button
          @click="nextItem"
          class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition"
        >
          متوجه شدم
        </button>
      </div>

      <div class="mt-6 pt-4 border-t border-[rgba(255,255,255,0.07)]">
        <slot name="reset-btn" />
      </div>
    </div>

    <div v-else class="text-center mt-8">
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-2xl p-10 max-w-sm mx-auto">
        <div class="text-4xl mb-4">✅</div>
        <h2 class="text-xl font-bold text-[#c9b07a] mb-2">پخش نقش کامل شد</h2>
        <p class="text-sm text-[rgba(232,228,217,0.4)] mb-6">همه {{ items.length }} نفر نقش خود را دیدند</p>
        <slot name="completion-actions">
          <button
            @click="$emit('done')"
            class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition"
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

export interface RevealItem {
  label: string
  roleName: string
  rolePhoto: string | null
  sideId: number
  isGuest?: boolean
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