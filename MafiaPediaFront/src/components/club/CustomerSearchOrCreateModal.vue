<template>
  <Modal :is-open="isOpen" title="انتخاب مشتری" max-width="md" @close="handleClose">
    <div class="flex flex-col gap-3">
      <!-- Search input -->
      <div class="relative">
        <input
          v-model="query"
          type="text"
          placeholder="نام یا موبایل مشتری..."
          class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition ltr text-left"
          @input="onSearchDebounce"
        />
        <div v-if="searching" class="absolute left-3 top-1/2 -translate-y-1/2">
          <div class="w-4 h-4 border-2 border-muted border-t-transparent rounded-full animate-spin" />
        </div>
      </div>

      <!-- In-club results -->
      <div v-if="inClubResults.length > 0">
        <p class="text-xs text-muted mb-1 px-1">عضو این کافه</p>
        <div class="border border-border rounded-lg overflow-hidden">
          <div
            v-for="p in inClubResults"
            :key="'ic-' + p.id"
            @click="selectExisting(p)"
            class="px-3 py-2.5 text-sm hover:bg-surface-hover cursor-pointer transition border-b border-border last:border-0 flex items-center justify-between"
          >
            <div class="flex flex-col">
              <span class="text-fg font-medium">{{ p.name }}</span>
              <span class="text-xs text-muted ltr text-left">{{ p.mobile }}</span>
            </div>
            <svg class="w-4 h-4 text-gold-text shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Global others (not in club) -->
      <div v-if="globalResults.length > 0">
        <p class="text-xs text-muted mb-1 px-1">سایر مشتریان</p>
        <div class="border border-border rounded-lg overflow-hidden">
          <div
            v-for="p in globalResults"
            :key="'go-' + p.id"
            @click="autoJoinAndSelect(p)"
            class="px-3 py-2.5 text-sm hover:bg-surface-hover cursor-pointer transition border-b border-border last:border-0 flex items-center justify-between"
            :class="{ 'opacity-50 pointer-events-none': joiningId === p.id }"
          >
            <div class="flex flex-col">
              <span class="text-fg font-medium">{{ p.name }}</span>
              <span class="text-xs text-muted ltr text-left">{{ p.mobile }}</span>
            </div>
            <div v-if="joiningId === p.id" class="w-4 h-4 border-2 border-gold border-t-transparent rounded-full animate-spin shrink-0" />
            <span v-else class="text-xs text-gold-text shrink-0">افزودن به کافه</span>
          </div>
        </div>
      </div>

      <!-- No results + inline create -->
      <div v-if="!searching && query.trim().length >= 2 && inClubResults.length === 0 && globalResults.length === 0 && !justSelected">
        <div class="border border-border rounded-lg p-4">
          <p class="text-sm text-muted mb-3">مشتری با این مشخصات یافت نشد</p>
          <p class="text-xs text-gold-text mb-3">مشتری جدید ثبت کنید:</p>
          <div class="flex flex-col gap-3">
            <input
              v-model="newForm.name"
              type="text"
              placeholder="نام مشتری *"
              class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
            />
            <input
              v-model="newForm.mobile"
              type="text"
              maxlength="11"
              placeholder="شماره موبایل *"
              class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition ltr text-left"
            />
            <input
              v-model="newForm.birthday"
              type="date"
              class="w-full bg-input border border-border rounded px-4 py-2.5 text-sm text-fg focus:outline-none focus:border-gold transition"
            />
            <div class="flex gap-3">
              <input
                v-model="newForm.code"
                type="text"
                placeholder="کد"
                class="flex-1 bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
              />
              <input
                v-model="newForm.desc"
                type="text"
                placeholder="توضیحات"
                class="flex-1 bg-input border border-border rounded px-4 py-2.5 text-sm text-fg placeholder-muted focus:outline-none focus:border-gold transition"
              />
            </div>
            <p v-if="createError" class="text-xs text-danger">{{ createError }}</p>
            <button
              @click="createNew"
              :disabled="!newForm.name.trim() || newForm.mobile.trim().length !== 11 || creating"
              class="w-full px-4 py-2.5 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-medium transition inline-flex items-center justify-center gap-2"
            >
              <div v-if="creating" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
              {{ creating ? 'در حال ثبت...' : 'ثبت و انتخاب مشتری' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </Modal>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import Modal from '@/components/shared/Modal.vue'
import { ClubPlayerApi } from '@/api'
import { useToast } from '@/composables/useToast'
import type { ClubPlayerDto } from '@/types/clubPlayer'

const props = defineProps<{
  isOpen: boolean
  clubId: number
}>()

const emit = defineEmits<{
  selected: [player: ClubPlayerDto]
  close: []
}>()

const { toastSuccess, toastError } = useToast()

const query = ref('')
const inClubResults = ref<ClubPlayerDto[]>([])
const globalResults = ref<ClubPlayerDto[]>([])
const searching = ref(false)
const joiningId = ref<number | null>(null)
const creating = ref(false)
const createError = ref('')
const justSelected = ref(false)

const newForm = reactive({
  name: '',
  mobile: '',
  birthday: '',
  code: '',
  desc: '',
})

let searchTimer: ReturnType<typeof setTimeout>

function onSearchDebounce() {
  clearTimeout(searchTimer)
  justSelected.value = false
  createError.value = ''
  const q = query.value.trim()
  if (q.length < 2) {
    inClubResults.value = []
    globalResults.value = []
    return
  }
  searching.value = true
  searchTimer = setTimeout(async () => {
    try {
      const res = await ClubPlayerApi.searchAllCustomers(props.clubId, q)
      inClubResults.value = res.data.inClub
      globalResults.value = res.data.globalOthers
    } catch {
      inClubResults.value = []
      globalResults.value = []
    } finally {
      searching.value = false
    }
  }, 300)
}

function selectExisting(player: ClubPlayerDto) {
  justSelected.value = true
  emit('selected', player)
  handleClose()
}

async function autoJoinAndSelect(player: ClubPlayerDto) {
  joiningId.value = player.id
  try {
    const fd = new FormData()
    fd.append('name', player.name)
    fd.append('mobile', player.mobile)
    if (player.birthday) fd.append('birthday', player.birthday)
    if (player.code) fd.append('code', player.code)
    if (player.desc) fd.append('desc', player.desc)
    const res = await ClubPlayerApi.createOrJoin(props.clubId, fd)
    toastSuccess(res.data.wasExistingCustomer ? 'مشتری به کافه اضافه شد' : 'مشتری جدید ثبت و به کافه اضافه شد')
    emit('selected', res.data.clubPlayer)
    handleClose()
  } catch (e: unknown) {
    const msg = extractError(e)
    if (msg.includes('قبلاً عضو')) {
      emit('selected', player)
      handleClose()
    } else {
      toastError(msg)
    }
  } finally {
    joiningId.value = null
  }
}

async function createNew() {
  if (!newForm.name.trim() || newForm.mobile.trim().length !== 11) return
  creating.value = true
  createError.value = ''
  try {
    const fd = new FormData()
    fd.append('name', newForm.name.trim())
    fd.append('mobile', newForm.mobile.trim())
    if (newForm.birthday) fd.append('birthday', newForm.birthday)
    if (newForm.code) fd.append('code', newForm.code)
    if (newForm.desc) fd.append('desc', newForm.desc)
    const res = await ClubPlayerApi.createOrJoin(props.clubId, fd)
    toastSuccess('مشتری جدید ثبت شد')
    emit('selected', res.data.clubPlayer)
    handleClose()
  } catch (e: unknown) {
    createError.value = extractError(e)
  } finally {
    creating.value = false
  }
}

function extractError(e: unknown): string {
  if (e && typeof e === 'object' && 'response' in e) {
    const err = e as { response?: { data?: { message?: string } } }
    return err.response?.data?.message || 'خطا در ثبت مشتری'
  }
  return 'خطا در برقراری ارتباط'
}

function handleClose() {
  query.value = ''
  inClubResults.value = []
  globalResults.value = []
  newForm.name = ''
  newForm.mobile = ''
  newForm.birthday = ''
  newForm.code = ''
  newForm.desc = ''
  createError.value = ''
  joiningId.value = null
  justSelected.value = false
  emit('close')
}

watch(() => props.isOpen, (open) => {
  if (!open) handleClose()
})
</script>
