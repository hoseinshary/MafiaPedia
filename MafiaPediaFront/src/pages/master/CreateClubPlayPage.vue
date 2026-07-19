<template>
  <div class="max-w-2xl mx-auto" v-if="isStaff ? clubInfo : masterCtx">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-gold-text">ثبت بازی جدید</h1>
      <p class="text-sm text-muted mt-1">{{ isStaff ? clubInfo?.name : masterCtx?.clubName }}</p>
    </div>

    <div v-if="isStaff && !selectedMasterId" class="mb-6">
      <label class="block text-sm text-muted mb-2">انتخاب گرداننده</label>
      <select
        v-model="selectedMasterId"
        class="w-full bg-surface border border-border rounded px-4 py-2 text-fg"
      >
        <option value="" disabled>یک گرداننده انتخاب کنید</option>
        <option v-for="m in masters" :key="m.id" :value="m.id">{{ m.name }}</option>
      </select>
    </div>

    <ClubPlayForm
      v-if="ready && effectiveMasterCtx"
      mode="create"
      :masterCtx="effectiveMasterCtx"
      :masters="masters"
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
    <div class="w-8 h-8 border-2 border-gold border-t-transparent rounded-full animate-spin" />
  </div>
  <div v-else class="text-center py-20 text-muted">
    دسترسی غیرمجاز
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from '@/composables/useToast'
import { MasterApi, ClubApi, LookupApi, ClubPlayApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import ClubPlayForm from '@/components/master/ClubPlayForm.vue'
import type { MasterContextDto, EventDto, CreateClubPlayDto } from '@/types/clubPlay'
import type { RoomDto, MasterDto } from '@/types/club'
import type { Senario } from '@/types'

const router = useRouter()
const { toastError } = useToast()
const authStore = useAuthStore()

const masterCtx = ref<MasterContextDto | null>(null)
const masters = ref<MasterDto[]>([])
const loading = ref(true)
const ready = ref(false)
const submitting = ref(false)
const rooms = ref<RoomDto[]>([])
const scenarios = ref<Senario[]>([])
const events = ref<EventDto[]>([])
const eventsLoading = ref(true)
const selectedMasterId = ref<number | ''>('')
const clubInfo = ref<{ id: number; name: string } | null>(null)

const isStaff = computed(() =>
  ['owner', 'supervisor', 'cashier'].includes(authStore.activeClubRole)
)

const effectiveMasterCtx = computed<MasterContextDto | null>(() => {
  if (!isStaff.value) return masterCtx.value
  if (!selectedMasterId.value || !clubInfo.value) return null
  const m = masters.value.find(x => x.id === selectedMasterId.value)
  if (!m) return null
  return {
    masterId: m.id,
    masterName: m.name,
    clubId: clubInfo.value.id,
    clubName: clubInfo.value.name,
  }
})

async function loadInitialData() {
  try {
    let clubId: number

    if (isStaff.value) {
      clubId = authStore.activeClubId!
    } else {
      const ctxRes = await MasterApi.getMasterContext()
      masterCtx.value = ctxRes.data
      clubId = ctxRes.data.clubId
    }

    const [clubRes, scenariosRes, eventsRes] = await Promise.all([
      ClubApi.getClubDetail(clubId),
      LookupApi.getScenarios(),
      ClubPlayApi.getClubEvents(clubId),
    ])

    clubInfo.value = { id: clubId, name: clubRes.data.name }
    rooms.value = clubRes.data.rooms.filter((r: any) => r.isActive !== false)
    scenarios.value = scenariosRes.data
    events.value = eventsRes.data
    eventsLoading.value = false

    if (isStaff.value) {
      masters.value = clubRes.data.masters
    }

    ready.value = true
  } catch {
    if (!isStaff.value) masterCtx.value = null
  } finally {
    loading.value = false
  }
}

async function onSubmit(dto: CreateClubPlayDto) {
  const ctx = effectiveMasterCtx.value
  if (!ctx) return
  submitting.value = true
  try {
    const res = await ClubPlayApi.createClubPlay(ctx.clubId, { ...dto, masterId: ctx.masterId })
    router.push({
      name: 'MasterPlayReveal',
      params: { clubId: ctx.clubId, playId: res.data.id },
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
