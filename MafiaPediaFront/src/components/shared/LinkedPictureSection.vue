<template>
  <div class="flex items-center gap-4">
    <div class="w-14 h-14 rounded-full overflow-hidden flex-shrink-0 bg-[rgba(255,255,255,0.05)] flex items-center justify-center">
      <img v-if="picture" :src="picture" class="w-full h-full object-cover" alt="" />
      <span v-else class="text-lg font-bold text-[#c9b07a]">{{ name.charAt(0) }}</span>
    </div>
    <div class="flex-1 min-w-0">
      <p class="text-sm text-[#e8e4d9] truncate">{{ name }}</p>
      <div class="flex items-center gap-2 mt-2">
        <input ref="fileInputRef" type="file" accept=".jpg,.jpeg,.png,.webp" class="hidden" @change="onFileChange" />
        <button @click="fileInputRef?.click()" class="text-xs px-3 py-1.5 border border-[rgba(255,255,255,0.07)] rounded text-[rgba(232,228,217,0.5)] hover:text-[#c9b07a] transition">
          انتخاب عکس
        </button>
        <button v-if="selectedFile" @click="upload" :disabled="uploading" class="text-xs px-3 py-1.5 bg-[#c9b07a] text-[#141416] font-bold rounded hover:opacity-90 transition disabled:opacity-50 disabled:cursor-not-allowed">
          <div v-if="uploading" class="w-3 h-3 border-2 border-[#141416] border-t-transparent rounded-full animate-spin inline-block" />
          <span v-else>آپلود</span>
        </button>
      </div>
      <div v-if="errorMsg" class="text-xs text-red-400 mt-1">{{ errorMsg }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

defineProps<{
  name: string
  picture: string | null
}>()

const emit = defineEmits<{ upload: [file: File] }>()

const fileInputRef = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const uploading = ref(false)
const errorMsg = ref('')

function onFileChange() {
  errorMsg.value = ''
  const files = fileInputRef.value?.files
  if (files && files.length > 0) {
    const file = files[0]
    const allowed = ['.jpg', '.jpeg', '.png', '.webp']
    const ext = '.' + file.name.split('.').pop()?.toLowerCase()
    if (!allowed.includes(ext)) {
      errorMsg.value = 'فقط jpg، jpeg، png و webp مجاز هستند'
      selectedFile.value = null
      return
    }
    if (file.size > 5 * 1024 * 1024) {
      errorMsg.value = 'حجم فایل باید کمتر از ۵ مگابایت باشد'
      selectedFile.value = null
      return
    }
    selectedFile.value = file
  }
}

async function upload() {
  if (!selectedFile.value) return
  uploading.value = true
  emit('upload', selectedFile.value)
  // Parent handles the upload; after success parent updates picture prop
  selectedFile.value = null
  uploading.value = false
  if (fileInputRef.value) fileInputRef.value.value = ''
}
</script>
