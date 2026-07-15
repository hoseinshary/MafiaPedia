<template>
  <div class="min-h-[80vh] flex flex-col items-center justify-center px-4 py-8" v-if="play">
    <div class="text-center mb-8">
      <h1 class="text-lg font-bold text-[#c9b07a]">پخش نقش</h1>
      <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ play.title || 'بازی بدون عنوان' }} — {{ play.roomName }}</p>
      <p class="text-lg text-[rgba(232,228,217,0.9)] mt-1">{{ play.senarioName }}</p>
    </div>

    <RoleRevealStepper
      ref="stepperRef"
      :items="stepperItems"
    >
      <template #reset-btn>
        <button
          @click="showConfirm = true"
          class="text-xs text-[rgba(232,228,217,0.25)] hover:text-[#e07070] transition underline underline-offset-2"
        >
          ریست و پخش دوباره
        </button>
      </template>
      <template #completion-actions>
        <div class="flex flex-col gap-3">
          <button
            @click="doConfirmReveal"
            :disabled="confirming"
            class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center justify-center gap-2"
          >
            <div v-if="confirming" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
            تایید و شروع بازی
          </button>
          <button
            @click="goBack"
            class="px-6 py-2 bg-transparent border border-[rgba(255,255,255,0.1)] hover:border-[rgba(201,176,122,0.3)] text-[rgba(232,228,217,0.5)] hover:text-[#e8e4d9] text-sm rounded font-medium transition"
          >
            بازگشت
          </button>
        </div>
      </template>
    </RoleRevealStepper>

    <ConfirmModal
      :isOpen="showConfirm"
      title="پخش مجدد نقش‌ها"
      message="نقش‌ها دوباره و به صورت تصادفی پخش می‌شوند. ادامه می‌دهید؟"
      confirmText="بله، پخش کن"
      cancelText="انصراف"
      :isLoading="reshuffling"
      @confirm="doReshuffle"
      @cancel="showConfirm = false"
    />
  </div>
  <div v-else class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ClubPlayApi } from '@/api'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/authStore'
import ConfirmModal from '@/components/ConfirmModal.vue'
import RoleRevealStepper from '@/components/clubplay/RoleRevealStepper.vue'
import type { ClubPlayDetailDto, ClubPlayParticipantDto } from '@/types/clubPlay'
import type { RevealItem } from '@/components/clubplay/RoleRevealStepper.vue'

const router = useRouter()
const route = useRoute()
const { toastError } = useToast()
const authStore = useAuthStore()

const play = ref<ClubPlayDetailDto | null>(null)
const participants = ref<ClubPlayParticipantDto[]>([])
const showConfirm = ref(false)
const reshuffling = ref(false)
const confirming = ref(false)
const stepperRef = ref<InstanceType<typeof RoleRevealStepper> | null>(null)

const stepperItems = computed<RevealItem[]>(() =>
  participants.value.map(p => ({
    label: p.name,
    roleName: p.roleName,
    rolePhoto: p.rolePhoto,
    sideId: p.sideId,
    isGuest: p.isGuest,
  }))
)

function resolveClubId(): number | null {
  if (route.params.clubId) return Number(route.params.clubId)
  if (authStore.activeClubId) return authStore.activeClubId
  return null
}

onMounted(async () => {
  const rawState = history.state?.playDetail
  if (rawState) {
    play.value = rawState as ClubPlayDetailDto
    participants.value = (rawState as ClubPlayDetailDto).participants
    return
  }
  const clubId = resolveClubId()
  const playId = route.params.playId ? Number(route.params.playId) : null
  if (clubId && playId) {
    try {
      const res = await ClubPlayApi.getClubPlayDetail(clubId, playId)
      play.value = res.data
      participants.value = res.data.participants
    } catch {
      router.push({ name: 'Home' })
    }
    return
  }
  router.push({ name: 'Home' })
})

async function doReshuffle() {
  if (!play.value) return
  const clubId = resolveClubId()
  const playId = play.value.id
  if (!clubId || !playId) return

  reshuffling.value = true
  try {
    const res = await ClubPlayApi.reshuffleRoles(clubId, playId)
    play.value = res.data
    participants.value = res.data.participants
    stepperRef.value?.reset()
    showConfirm.value = false
  } catch (e: unknown) {
    const err = e as { response?: { status?: number; data?: { message?: string } } }
    if (err?.response?.status === 400) {
      toastError(err.response.data?.message || 'پس از پایان بازی امکان پخش دوباره نیست')
    }
  } finally {
    reshuffling.value = false
  }
}

async function doConfirmReveal() {
  if (!play.value) return
  confirming.value = true
  try {
    const clubId = resolveClubId()
    if (!clubId) throw new Error('no club id')
    await ClubPlayApi.confirmReveal(clubId, play.value.id)
    router.push({ name: 'MasterPlayDetail', params: { id: play.value.id } })
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در تایید شروع بازی')
  } finally {
    confirming.value = false
  }
}

function goBack() {
  const clubId = resolveClubId()
  if (clubId) {
    router.push({ name: 'ClubDetail', params: { id: clubId } })
  } else {
    router.push({ name: 'Home' })
  }
}
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
