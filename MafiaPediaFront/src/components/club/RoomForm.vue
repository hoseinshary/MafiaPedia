<template>
  <div class="flex items-center gap-2">
    <input
      v-model="name"
      type="text"
      placeholder="نام اتاق"
      class="flex-1 bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
      @keydown.enter.prevent="submit"
    />
    <label class="flex items-center gap-1.5 text-xs text-muted cursor-pointer">
      <input
        v-model="isActive"
        type="checkbox"
        class="accent-gold"
      />
      فعال
    </label>
    <button
      @click="submit"
      :disabled="!name.trim() || loading"
      class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition whitespace-nowrap inline-flex items-center gap-2"
    >
      <div v-if="loading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
      {{ room ? 'ویرایش' : 'افزودن اتاق' }}
    </button>
    <button
      v-if="room"
      @click="$emit('cancelled')"
      class="px-3 py-2 text-sm text-muted hover:text-fg transition"
    >
      انصراف
    </button>
  </div>
  <p v-if="error" class="text-xs text-danger mt-1">{{ error }}</p>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ClubApi } from '@/api'
import { useToast } from '@/composables/useToast'
import type { RoomDto } from '@/types/club'

const props = defineProps<{
  clubId: number
  room?: RoomDto
}>()

const emit = defineEmits<{
  saved: [room: RoomDto]
  cancelled: []
}>()

const name = ref(props.room?.name ?? '')
const isActive = ref(props.room?.isActive ?? true)
const loading = ref(false)
const error = ref('')
const { toastSuccess } = useToast()

async function submit() {
  const trimmed = name.value.trim()
  if (!trimmed) return
  loading.value = true
  error.value = ''
  try {
    if (props.room) {
      const res = await ClubApi.updateRoom(props.clubId, props.room.id, { name: trimmed, isActive: isActive.value })
      emit('saved', res.data)
      toastSuccess('اتاق با موفقیت ویرایش شد')
    } else {
      const res = await ClubApi.createRoom(props.clubId, { name: trimmed, isActive: isActive.value })
      emit('saved', res.data)
      name.value = ''
      isActive.value = true
      toastSuccess('اتاق با موفقیت اضافه شد')
    }
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message || 'خطا در ثبت اتاق'
    } else {
      error.value = 'خطا در برقراری ارتباط'
    }
  } finally {
    loading.value = false
  }
}
</script>
