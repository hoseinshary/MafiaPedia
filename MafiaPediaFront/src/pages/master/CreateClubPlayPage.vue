<template>
  <div class="max-w-2xl mx-auto" v-if="masterCtx">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-[#c9b07a]">ثبت بازی جدید</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ masterCtx.clubName }}</p>
    </div>
    <ClubPlayForm
      v-if="ready"
      mode="create"
      :masterCtx="masterCtx"
      :rooms="rooms"
      :scenarios="scenarios"
      :events="events"
      :eventsLoading="eventsLoading"
      :submitting="submitting"
      @submit="onSubmit"
      @cancel="router.back()"
    />
  </div>
  <div v-else-if="loading" class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
  </div>
  <div v-else class="text-center py-20 text-[rgba(232,228,217,0.4)]">
    دسترسی غیرمجاز
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from '@/composables/useToast'
import { MasterApi, ClubApi, LookupApi, ClubPlayApi } from '@/api'
import ClubPlayForm from '@/components/master/ClubPlayForm.vue'
import type { MasterContextDto, EventDto, CreateClubPlayDto } from '@/types/clubPlay'
import type { RoomDto } from '@/types/club'
import type { Senario } from '@/types'

const router = useRouter()
const { toastError } = useToast()

const masterCtx = ref<MasterContextDto | null>(null)
const loading = ref(true)
const ready = ref(false)
const submitting = ref(false)
const rooms = ref<RoomDto[]>([])
const scenarios = ref<Senario[]>([])
const events = ref<EventDto[]>([])
const eventsLoading = ref(true)

async function loadInitialData() {
  try {
    const ctxRes = await MasterApi.getMasterContext()
    masterCtx.value = ctxRes.data

    const [clubRes, scenariosRes, eventsRes] = await Promise.all([
      ClubApi.getClubDetail(ctxRes.data.clubId),
      LookupApi.getScenarios(),
      ClubPlayApi.getClubEvents(ctxRes.data.clubId),
    ])
    rooms.value = clubRes.data.rooms.filter((r: any) => r.isActive !== false)
    scenarios.value = scenariosRes.data
    events.value = eventsRes.data
    eventsLoading.value = false
    ready.value = true
  } catch {
    masterCtx.value = null
  } finally {
    loading.value = false
  }
}

async function onSubmit(dto: CreateClubPlayDto) {
  if (!masterCtx.value) return
  submitting.value = true
  try {
    const res = await ClubPlayApi.createClubPlay(masterCtx.value.clubId, dto)
    router.push({
      name: 'MasterPlayReveal',
      params: { clubId: masterCtx.value.clubId, playId: res.data.id },
      state: { playDetail: JSON.parse(JSON.stringify(res.data)) },
    })
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در ثبت بازی')
  } finally {
    submitting.value = false
  }
}

onMounted(loadInitialData)
</script>
