<template>
  <div dir="rtl">
    <h2 class="text-lg font-semibold mb-4">نظرات</h2>

    <div v-if="loading" class="flex justify-center py-10">
      <div class="w-8 h-8 border-4 border-border border-t-gold rounded-full animate-spin" />
    </div>

    <template v-else>
      <div v-if="likeMessage" class="mb-4 px-4 py-2 rounded text-sm bg-gold/20 border border-gold text-gold-text text-center">
        {{ likeMessage }}
      </div>

      <div v-if="authStore.isAuthenticated" class="mb-6">
        <textarea
          v-model="newComment"
          rows="3"
          class="w-full bg-input border border-border rounded px-3 py-2 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition resize-none"
          placeholder="نظر خود را بنویسید..."
        />
        <div class="flex justify-end mt-2">
          <button
            @click="submitComment"
            :disabled="submitting || !newComment.trim()"
            class="px-4 py-2 bg-gold hover:opacity-80 disabled:opacity-50 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition"
          >
            <span v-if="submitting" class="inline-flex items-center gap-2">
              <div class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin" />
              در حال ارسال...
            </span>
            <span v-else>ارسال</span>
          </button>
        </div>
      </div>
      <div v-else class="mb-6 bg-surface rounded-lg p-4 text-center text-sm text-muted">
        برای کامنت گذاشتن
        <router-link to="/login" class="text-gold-text hover:text-gold-text transition">وارد شوید</router-link>
      </div>

      <div v-if="comments.length === 0 && !loading" class="text-center py-10 text-muted text-sm">
        هنوز نظری ثبت نشده است
      </div>

      <div class="space-y-4">
        <div v-for="comment in rootComments" :key="comment.id" class="bg-surface rounded-lg p-4">
          <CommentItem
            :comment="comment"
            @reply="onReply"
            @delete="onDeleteRequest"
            @like="onLike"
          />
        </div>
      </div>
    </template>

    <ConfirmModal
      :is-open="showDeleteModal"
      title="حذف کامنت"
      message="آیا مطمئن هستید؟"
      confirm-text="بله، حذف کن"
      cancel-text="انصراف"
      :is-loading="isDeleting"
      @confirm="confirmDelete"
      @cancel="closeDeleteModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { CommentApi } from '@/api'
import type { CommentDto } from '@/types'
import CommentItem from './CommentItem.vue'
import ConfirmModal from './ConfirmModal.vue'

const props = defineProps<{
  entityType: string
  entityId: number
}>()

const authStore = useAuthStore()

const comments = ref<CommentDto[]>([])
const loading = ref(true)
const newComment = ref('')
const submitting = ref(false)

const showDeleteModal = ref(false)
const commentToDelete = ref<number | null>(null)
const isDeleting = ref(false)

const likeMessage = ref('')
let likeMessageTimer: ReturnType<typeof setTimeout> | null = null

const rootComments = computed(() => comments.value.filter(c => !c.parentCommentId))

function setLikeMessage(msg: string) {
  likeMessage.value = msg
  if (likeMessageTimer) clearTimeout(likeMessageTimer)
  likeMessageTimer = setTimeout(() => { likeMessage.value = '' }, 3000)
}

async function fetchComments() {
  loading.value = true
  try {
    const res = await CommentApi.getComments(props.entityType, props.entityId)
    comments.value = res.data
  } catch {
    comments.value = []
  } finally {
    loading.value = false
  }
}

async function submitComment() {
  const content = newComment.value.trim()
  if (!content) return
  submitting.value = true
  try {
    await CommentApi.addComment(props.entityType, props.entityId, content)
    newComment.value = ''
    await fetchComments()
  } catch {
    //
  } finally {
    submitting.value = false
  }
}

async function onReply(payload: { parentId: number; content: string }) {
  try {
    await CommentApi.addComment(props.entityType, props.entityId, payload.content, payload.parentId)
    await fetchComments()
  } catch {
    //
  }
}

function onDeleteRequest(commentId: number) {
  commentToDelete.value = commentId
  showDeleteModal.value = true
}

function closeDeleteModal() {
  showDeleteModal.value = false
  commentToDelete.value = null
}

async function confirmDelete() {
  if (commentToDelete.value === null) return
  isDeleting.value = true
  try {
    await CommentApi.deleteComment(commentToDelete.value)
    removeComment(comments.value, commentToDelete.value)
    showDeleteModal.value = false
    commentToDelete.value = null
  } catch {
    //
  } finally {
    isDeleting.value = false
  }
}

async function onLike(commentId: number) {
  if (!authStore.isAuthenticated) {
    setLikeMessage('برای لایک کردن وارد شوید')
    return
  }
  try {
    await CommentApi.toggleLike(commentId)
    updateCommentLike(comments.value, commentId)
  } catch {
    //
  }
}

function removeComment(list: CommentDto[], id: number): boolean {
  const idx = list.findIndex(c => c.id === id)
  if (idx !== -1) {
    list.splice(idx, 1)
    return true
  }
  for (const c of list) {
    if (removeComment(c.replies, id)) return true
  }
  return false
}

function updateCommentLike(list: CommentDto[], id: number) {
  for (const c of list) {
    if (c.id === id) {
      if (c.isLikedByCurrentUser) {
        c.isLikedByCurrentUser = false
        c.likeCount = Math.max(0, c.likeCount - 1)
      } else {
        c.isLikedByCurrentUser = true
        c.likeCount += 1
      }
      return
    }
    if (c.replies.length > 0) {
      updateCommentLike(c.replies, id)
    }
  }
}

onMounted(fetchComments)
</script>
