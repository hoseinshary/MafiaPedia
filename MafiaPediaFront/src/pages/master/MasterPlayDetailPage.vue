<template>
  <!-- Edit mode -->
  <div v-if="editing && ctx" class="max-w-2xl mx-auto">
    <div class="mb-6">
      <button @click="cancelEdit" class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#e8e4d9] transition mb-3 inline-block">&larr; بازگشت</button>
      <h1 class="text-xl font-bold text-[#c9b07a]">ویرایش بازی</h1>
    </div>
    <ClubPlayForm
      v-if="formReady"
      mode="edit"
      :masterCtx="ctx"
      :rooms="rooms"
      :scenarios="scenarios"
      :events="events"
      :eventsLoading="false"
      :initial="play"
      :submitting="editSubmitting"
      @submit="onEditSubmit"
      @cancel="cancelEdit"
    />
    <div v-else class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>
  </div>

  <!-- Detail mode -->
  <div dir="rtl" class="max-w-4xl mx-auto w-full" v-else-if="play">
    <div class="mb-6">
      <button @click="router.back()" class="text-xs text-[rgba(232,228,217,0.3)] hover:text-[#e8e4d9] transition mb-3 inline-block">&larr; بازگشت</button>
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-xl font-bold text-[#c9b07a]">{{ play.title || 'بازی بدون عنوان' }}</h1>
          <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ play.roomName }} — {{ formatDate(play.dateTime) }} ساعت {{ formatTime(play.dateTime) }}</p>
        </div>
        <div class="flex items-center gap-3">
          <button
            v-if="play.status !== 'done' || isStaff"
            @click="startEdit"
            class="px-4 py-1.5 border border-[rgba(201,176,122,0.3)] text-[#c9b07a] hover:bg-[rgba(201,176,122,0.08)] text-xs rounded font-medium transition"
          >
            ویرایش بازی
          </button>
          <span class="status-badge text-sm px-3 py-1 rounded" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span>
        </div>
      </div>
    </div>

    <!-- Info cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-4">
        <div class="text-xs text-[rgba(232,228,217,0.4)]">سناریو</div>
        <div class="text-sm text-[#e8e4d9] mt-1">{{ play.senarioName }}</div>
      </div>
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-4">
        <div class="text-xs text-[rgba(232,228,217,0.4)]">نوع بازی</div>
        <div class="text-sm text-[#e8e4d9] mt-1">{{ playTypeLabel(play.playType) }}</div>
      </div>
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-4">
        <div class="text-xs text-[rgba(232,228,217,0.4)]">تعداد بازیکن</div>
        <div class="text-sm text-[#e8e4d9] mt-1">{{ play.playersCount }} نفر <span v-if="play.guestCount > 0" class="text-[rgba(232,228,217,0.4)]">({{ play.guestCount }} مهمان)</span></div>
      </div>
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-4">
        <div class="text-xs text-[rgba(232,228,217,0.4)]">گرداننده</div>
        <div class="text-sm text-[#e8e4d9] mt-1">{{ play.masterName }}</div>
      </div>
    </div>

    <!-- Participants table -->
    <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl overflow-hidden mb-8">
      <div class="px-4 py-3 border-b border-[rgba(255,255,255,0.07)] text-sm font-bold text-[#e8e4d9]">شرکت‌کنندگان</div>
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)] bg-[#1a1a1e]">
              <th class="px-4 py-3 text-right">نام</th>
              <th class="px-4 py-3 text-right">نقش</th>
              <th class="px-4 py-3 text-right">طرف</th>
              <th class="px-4 py-3 text-right">وضعیت</th>
              <th v-if="play.status === 'done' && play.playType === 'superrank'" class="px-4 py-3 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.clubPlayerId" class="border-b border-[rgba(255,255,255,0.04)] text-[#e8e4d9]">
              <td class="px-4 py-3">{{ p.name }}</td>
              <td class="px-4 py-3">{{ p.roleName }}</td>
              <td class="px-4 py-3" :class="p.sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">{{ p.sideId === 1 ? 'مافیا' : 'شهروند' }}</td>
              <td class="px-4 py-3"><span v-if="p.isGuest" class="text-xs text-[rgba(232,228,217,0.4)]">مهمان</span></td>
              <td v-if="play.status === 'done' && play.playType === 'superrank'" class="px-4 py-3"></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Action areas -->
    <div v-if="play.status === 'pending'" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 text-center">
      <div class="text-sm text-[rgba(232,228,217,0.5)] mb-4">این بازی هنوز در حال پخش نقش است</div>
      <div class="flex items-center justify-center gap-3">
        <button
          @click="doReshuffle"
          :disabled="reshuffleLoading"
          class="px-5 py-2 border border-[rgba(201,176,122,0.3)] hover:bg-[rgba(201,176,122,0.08)] text-[#c9b07a] text-sm rounded font-medium transition inline-flex items-center gap-2"
        >
          <div v-if="reshuffleLoading" class="w-4 h-4 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
          پخش دوباره نقش‌ها
        </button>
        <router-link
          :to="{ name: 'MasterPlayReveal', params: { clubId: ctx?.clubId ?? clubId, playId: play.id } }"
          class="inline-block px-5 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition"
        >
          رفتن به صفحه پخش نقش
        </router-link>
      </div>
    </div>

    <div v-else-if="play.status === 'notwinside'" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
      <h3 class="text-sm font-bold text-[#e8e4d9] mb-4">ثبت نتیجه</h3>
      <div v-if="sides.length === 0" class="flex justify-center py-4">
        <div class="w-5 h-5 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else class="flex flex-wrap gap-3 mb-4">
        <button
          v-for="s in sides"
          :key="s.id"
          @click="selectedWinnerside = s.id"
          class="px-5 py-2.5 text-sm rounded font-medium border transition"
          :class="selectedWinnerside === s.id ? 'bg-[#c9b07a] text-[#0d0d0f] border-[#c9b07a]' : 'bg-[#0d0d0f] text-[rgba(232,228,217,0.6)] border-[rgba(255,255,255,0.1)] hover:border-[rgba(201,176,122,0.3)]'"
        >
          {{ s.name }}
        </button>
      </div>
      <div class="flex gap-3 items-center">
        <button
          @click="doSubmitWinnerside"
          :disabled="!selectedWinnerside || winnersideLoading"
          class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2"
        >
          <div v-if="winnersideLoading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ثبت نتیجه
        </button>
        <span v-if="winnersideError" class="text-xs text-[#e07070]">{{ winnersideError }}</span>
      </div>
    </div>

    <div v-else-if="play.status === 'notrank'" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 mb-8">
      <h3 class="text-sm font-bold text-[#e8e4d9] mb-4">ثبت رتبه</h3>
      <div class="overflow-x-auto mb-4">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
              <th class="px-4 py-2 text-right">بازیکن</th>
              <th class="px-4 py-2 text-right">نقش</th>
              <th class="px-4 py-2 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.clubPlayerId" class="border-b border-[rgba(255,255,255,0.04)] text-[#e8e4d9]">
              <td class="px-4 py-2">{{ p.name }}</td>
              <td class="px-4 py-2">{{ p.roleName }}</td>
              <td class="px-4 py-2">
                <input
                  v-model.number="rankInputs[p.clubPlayerId]"
                  type="number"
                  min="1"
                  :max="play.playersCount"
                  class="w-20 bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-3 py-1.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="flex gap-3 items-center">
        <button
          @click="doSubmitRanks"
          :disabled="!allRanksFilled || ranksLoading"
          class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2"
        >
          <div v-if="ranksLoading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ثبت رتبه‌ها
        </button>
        <span v-if="ranksError" class="text-xs text-[#e07070]">{{ ranksError }}</span>
      </div>
    </div>

    <div v-else-if="play.status === 'done'" class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 text-center">
      <div class="text-lg mb-2">✅</div>
      <h3 class="text-sm font-bold text-[#4ade80] mb-2">بازی کامل شد</h3>
      <div v-if="winnersideName" class="text-sm text-[rgba(232,228,217,0.6)]">برنده: {{ winnersideName }}</div>
      <div v-if="play.playType === 'superrank'" class="mt-4 overflow-x-auto">
        <table class="w-full text-sm max-w-sm mx-auto">
          <thead>
            <tr class="border-b border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.5)]">
              <th class="px-4 py-2 text-right">بازیکن</th>
              <th class="px-4 py-2 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.clubPlayerId" class="border-b border-[rgba(255,255,255,0.04)] text-[#e8e4d9]">
              <td class="px-4 py-2">{{ p.name }}</td>
              <td class="px-4 py-2">-</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <div v-else class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
  </div>

  <!-- Confirm scenario-change modal -->
  <ConfirmModal
    :isOpen="confirmEditOpen"
    title="تأیید ویرایش"
    :message="confirmEditMessage"
    confirmText="ادامه"
    cancelText="انصراف"
    @confirm="doConfirmedUpdate"
    @cancel="confirmEditOpen = false; pendingDto = null"
  />
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { MasterApi, ClubPlayApi, ClubApi, LookupApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import { useToast } from '@/composables/useToast'
import ClubPlayForm from '@/components/master/ClubPlayForm.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import type { MasterContextDto, ClubPlayDetailDto, EventDto, CreateClubPlayDto } from '@/types/clubPlay'
import type { RoomDto } from '@/types/club'
import type { Senario, DropdownSide } from '@/types'

const router = useRouter()
const route = useRoute()
const { toastError, toastSuccess } = useToast()
const authStore = useAuthStore()

const ctx = ref<MasterContextDto | null>(null)
const clubId = ref<number | null>(null)
const play = ref<ClubPlayDetailDto | null>(null)
const sides = ref<DropdownSide[]>([])
const selectedWinnerside = ref<number | null>(null)
const winnersideLoading = ref(false)
const winnersideError = ref('')
const rankInputs = reactive<Record<number, number>>({})
const ranksLoading = ref(false)
const ranksError = ref('')

const reshuffleLoading = ref(false)

// Edit mode state
const editing = ref(false)
const editSubmitting = ref(false)
const formReady = ref(false)
const confirmEditOpen = ref(false)
const confirmEditMessage = ref('')
const pendingDto = ref<CreateClubPlayDto | null>(null)
const rooms = ref<RoomDto[]>([])
const scenarios = ref<Senario[]>([])
const events = ref<EventDto[]>([])

const isStaff = computed(() =>
  ['owner', 'supervisor', 'cashier'].includes(authStore.activeClubRole)
)

const winnersideName = computed(() => {
  if (!play.value?.winnersideId) return null
  return sides.value.find(s => s.id === play.value!.winnersideId)?.name || null
})

const allRanksFilled = computed(() => {
  if (!play.value) return false
  return play.value.participants.every(p => rankInputs[p.clubPlayerId] != null && rankInputs[p.clubPlayerId] > 0)
})

function formatDate(dt: string) {
  return new Date(dt).toLocaleDateString('fa-IR', { month: 'short', day: 'numeric' })
}

function formatTime(dt: string) {
  return new Date(dt).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })
}

function playTypeLabel(t: string) {
  const map: Record<string, string> = { normal: 'عادی', rank: 'رنک', superrank: 'سوپر رنک', etc: 'و ...' }
  return map[t] || t
}

function statusClass(status: string) {
  if (status === 'done') return 'status-done'
  if (status === 'pending') return 'status-pending'
  if (status === 'notwinside') return 'status-notwinside'
  if (status === 'notrank') return 'status-notrank'
  return ''
}

function statusLabel(status: string) {
  const map: Record<string, string> = {
    pending: 'در حال پخش',
    notwinside: 'ثبت برنده',
    notrank: 'ثبت رتبه',
    done: 'کامل شد',
  }
  return map[status] || status
}

async function startEdit() {
  const cId = clubId.value
  if (!cId) return
  editing.value = true
  try {
    const [clubRes, scenariosRes, eventsRes] = await Promise.all([
      ClubApi.getClubDetail(cId),
      LookupApi.getScenarios(),
      ClubPlayApi.getClubEvents(cId),
    ])
    rooms.value = clubRes.data.rooms.filter((r: any) => r.isActive !== false)
    scenarios.value = scenariosRes.data
    events.value = eventsRes.data
    formReady.value = true
  } catch {
    toastError('خطا در بارگذاری داده‌های ویرایش')
    editing.value = false
  }
}

function cancelEdit() {
  editing.value = false
  formReady.value = false
}

async function onEditSubmit(dto: CreateClubPlayDto) {
  const cId = clubId.value
  if (!cId || !play.value) return

  const senarioChanged = dto.senarioId !== play.value.senarioId
  const countChanged = dto.playersCount !== play.value.playersCount

  if (senarioChanged || countChanged) {
    confirmEditMessage.value = 'تغییر سناریو یا شرکت‌کنندگان باعث باز پخش نقش فعلی بازی می‌شود'
    pendingDto.value = dto
    confirmEditOpen.value = true
    return
  }

  await doActualUpdate(dto, cId)
}

async function doConfirmedUpdate() {
  if (!pendingDto.value || !clubId.value || !play.value) return
  confirmEditOpen.value = false
  const dto = pendingDto.value
  const cId = clubId.value
  pendingDto.value = null
  await doActualUpdate(dto, cId)
}

async function doActualUpdate(dto: CreateClubPlayDto, cId: number) {
  if (!play.value) return
  const oldStatus = play.value.status
  editSubmitting.value = true
  try {
    const res = await ClubPlayApi.updateClubPlay(cId, play.value.id, dto)
    play.value = res.data
    toastSuccess('بازی با موفقیت ویرایش شد')
    editing.value = false
    formReady.value = false

    if (oldStatus !== 'pending' && play.value.status === 'pending') {
      router.push({
        name: 'MasterPlayReveal',
        params: { clubId: cId, playId: play.value.id },
        state: { playDetail: JSON.parse(JSON.stringify(res.data)) },
      })
    }
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در ویرایش بازی')
  } finally {
    editSubmitting.value = false
  }
}

async function doSubmitWinnerside() {
  const cId = clubId.value
  if (!play.value || !selectedWinnerside.value || !cId) return
  winnersideLoading.value = true
  winnersideError.value = ''
  try {
    const res = await ClubPlayApi.submitWinnerside(cId, play.value.id, selectedWinnerside.value)
    play.value = res.data
    toastSuccess('نتیجه با موفقیت ثبت شد')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    winnersideError.value = err?.response?.data?.message || 'خطا در ثبت نتیجه'
    toastError(winnersideError.value)
  } finally {
    winnersideLoading.value = false
  }
}

async function doSubmitRanks() {
  const cId = clubId.value
  if (!play.value || !cId) return
  ranksLoading.value = true
  ranksError.value = ''
  try {
    const ranks = play.value.participants.map(p => ({
      clubPlayerId: p.clubPlayerId,
      rank: rankInputs[p.clubPlayerId] || 0,
    }))
    const res = await ClubPlayApi.submitRanks(cId, play.value.id, ranks)
    play.value = res.data
    toastSuccess('رتبه‌ها با موفقیت ثبت شد')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    ranksError.value = err?.response?.data?.message || 'خطا در ثبت رتبه‌ها'
    toastError(ranksError.value)
  } finally {
    ranksLoading.value = false
  }
}

async function doReshuffle() {
  const cId = clubId.value
  if (!cId || !play.value) return
  reshuffleLoading.value = true
  try {
    const res = await ClubPlayApi.reshuffleRoles(cId, play.value.id)
    play.value = res.data
    toastSuccess('نقش‌ها دوباره پخش شدند')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در پخش دوباره نقش‌ها')
  } finally {
    reshuffleLoading.value = false
  }
}

onMounted(async () => {
  try {
    const playId = Number(route.params.id)
    if (!playId) {
      router.push({ name: 'MasterDashboard' })
      return
    }

    let cId: number
    let masterCtx: MasterContextDto | null = null

    if (isStaff.value) {
      cId = authStore.activeClubId!
      masterCtx = null
    } else {
      const ctxRes = await MasterApi.getMasterContext()
      masterCtx = ctxRes.data
      cId = ctxRes.data.clubId
    }

    clubId.value = cId

    const [playRes, sidesRes] = await Promise.all([
      ClubPlayApi.getClubPlayDetail(cId, playId),
      LookupApi.getDropdown(),
    ])
    play.value = playRes.data
    sides.value = sidesRes.data.sides

    if (masterCtx) {
      ctx.value = masterCtx
    } else {
      ctx.value = {
        masterId: playRes.data.masterId,
        masterName: playRes.data.masterName,
        clubId: cId,
        clubName: '',
      }
    }
  } catch {
    router.push({ name: 'MasterDashboard' })
  }
})
</script>

<style scoped>
.status-badge {
  display: inline-block;
  font-size: 11px;
  padding: 3px 10px;
  border-radius: 6px;
  font-weight: 600;
}
.status-pending {
  background: rgba(128, 128, 128, 0.15);
  color: #a0a0a0;
}
.status-notwinside {
  background: rgba(255, 165, 0, 0.15);
  color: #ffa500;
}
.status-notrank {
  background: rgba(255, 255, 0, 0.12);
  color: #d4d43a;
}
.status-done {
  background: rgba(74, 222, 128, 0.12);
  color: #4ade80;
}
</style>
