<template>
  <div>
    <div class="flex items-start justify-between gap-3">
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-1">
          <span class="text-sm font-medium text-blue-400">{{ comment.userDisplayName }}</span>
          <span class="text-xs text-gray-500">{{ formatDate(comment.createdAt) }}</span>
        </div>
        <p class="text-sm text-gray-300 whitespace-pre-wrap break-words">{{ comment.content }}</p>
      </div>
      <button
        v-if="authStore.userId === comment.userId || authStore.isAdmin"
        @click="$emit('delete', comment.id)"
        class="text-gray-500 hover:text-red-400 transition shrink-0 p-1"
        title="حذف"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
        </svg>
      </button>
    </div>

    <div class="flex items-center gap-4 mt-2">
      <button
        @click="$emit('like', comment.id)"
        class="inline-flex items-center gap-1.5 text-xs transition"
        :class="comment.isLikedByCurrentUser ? 'text-red-400' : 'text-gray-500 hover:text-red-400'"
        title="لایک"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" :fill="comment.isLikedByCurrentUser ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="1.5">
          <path fill-rule="evenodd" d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z" clip-rule="evenodd" />
        </svg>
        <span>{{ comment.likeCount }}</span>
      </button>
      <button
        v-if="authStore.isAuthenticated && !comment.parentCommentId"
        @click="toggleReply"
        class="text-xs text-gray-500 hover:text-blue-400 transition"
      >
        {{ showReplyForm ? 'انصراف' : 'پاسخ' }}
      </button>
    </div>

    <div v-if="showReplyForm" class="mt-3 mr-6">
      <textarea
        v-model="replyContent"
        rows="2"
        class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-blue-500 transition resize-none"
        placeholder="پاسخ خود را بنویسید..."
      />
      <div class="flex justify-end mt-2">
        <button
          @click="submitReply"
          :disabled="sendingReply || !replyContent.trim()"
          class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-xs rounded font-medium transition"
        >
          <span v-if="sendingReply" class="inline-flex items-center gap-1">
            <div class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin" />
            در حال ارسال...
          </span>
          <span v-else>ارسال پاسخ</span>
        </button>
      </div>
    </div>

    <div v-if="comment.replies && comment.replies.length > 0" class="mr-6 mt-3 space-y-3">
      <div
        v-for="reply in comment.replies"
        :key="reply.id"
        class="border-r-2 border-gray-700 pr-4"
      >
        <CommentItem
          :comment="reply"
          @delete="onChildDelete"
          @reply="onChildReply"
          @like="onChildLike"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import type { CommentDto } from '@/types'

const props = defineProps<{
  comment: CommentDto
}>()

const emit = defineEmits<{
  delete: [commentId: number]
  reply: [payload: { parentId: number; content: string }]
  like: [commentId: number]
}>()

const authStore = useAuthStore()

const showReplyForm = ref(false)
const replyContent = ref('')
const sendingReply = ref(false)

function toggleReply() {
  showReplyForm.value = !showReplyForm.value
  if (!showReplyForm.value) replyContent.value = ''
}

async function submitReply() {
  const content = replyContent.value.trim()
  if (!content) return
  sendingReply.value = true
  try {
    emit('reply', { parentId: props.comment.id, content })
    showReplyForm.value = false
    replyContent.value = ''
  } finally {
    sendingReply.value = false
  }
}

function onChildDelete(id: number) {
  emit('delete', id)
}

function onChildReply(payload: { parentId: number; content: string }) {
  emit('reply', payload)
}

function onChildLike(id: number) {
  emit('like', id)
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fa-IR')
}
</script>
