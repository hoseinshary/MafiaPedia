<template>
  <!-- Edit mode -->
  <div v-if="editing && ctx" class="max-w-2xl mx-auto">
    <div class="mb-6">
      <button @click="cancelEdit" class="text-xs text-muted hover:text-fg transition mb-3 inline-block">&larr; بازگشت</button>
      <h1 class="text-xl font-bold text-gold-text">ویرایش بازی</h1>
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
      <div class="w-6 h-6 border-2 border-gold border-t-transparent rounded-full animate-spin" />
    </div>
  </div>

  <!-- Detail mode -->
  <div dir="rtl" class="max-w-4xl mx-auto w-full" v-else-if="play">
    <div class="mb-6">
      <button @click="router.back()" class="text-xs text-muted hover:text-fg transition mb-3 inline-block">&larr; بازگشت</button>
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-xl font-bold text-gold-text">{{ play.title || 'بازی بدون عنوان' }}</h1>
          <p class="text-sm text-muted mt-1">{{ play.roomName }} — {{ formatDate(play.dateTime) }} ساعت {{ formatTime(play.dateTime) }}</p>
        </div>
        <div class="flex items-center gap-3">
          <button
            v-if="play.status !== 'done' || isStaff"
            @click="startEdit"
            class="px-4 py-1.5 border border-[rgba(201,176,122,0.3)] text-gold-text hover:bg-gold/10 text-xs rounded font-medium transition"
          >
            ویرایش بازی
          </button>
          <button
            v-if="canDelete"
            @click="deleteConfirmOpen = true"
            :disabled="deleteLoading"
            class="px-4 py-1.5 border border-danger/30 text-danger hover:bg-danger/10 text-xs rounded font-medium transition inline-flex items-center gap-2"
          >
            <div v-if="deleteLoading" class="w-3 h-3 border-2 border-danger border-t-transparent rounded-full animate-spin" />
            حذف بازی
          </button>
          <span class="status-badge text-sm px-3 py-1 rounded" :class="statusClass(play.status)">{{ statusLabel(play.status) }}</span>
        </div>
      </div>
    </div>

    <!-- Info cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-surface border border-border rounded-xl p-4">
        <div class="text-xs text-muted">سناریو</div>
        <div class="text-sm text-fg mt-1">{{ play.senarioName }}</div>
      </div>
      <div class="bg-surface border border-border rounded-xl p-4">
        <div class="text-xs text-muted">نوع بازی</div>
        <div class="text-sm text-fg mt-1">{{ playTypeLabel(play.playType) }}</div>
      </div>
      <div class="bg-surface border border-border rounded-xl p-4">
        <div class="text-xs text-muted">تعداد بازیکن</div>
        <div class="text-sm text-fg mt-1">{{ play.playersCount }} نفر <span v-if="play.guestCount > 0" class="text-muted">({{ play.guestCount }} مهمان)</span></div>
      </div>
      <div class="bg-surface border border-border rounded-xl p-4">
        <div class="text-xs text-muted">گرداننده</div>
        <div class="text-sm text-fg mt-1">{{ play.masterName }}</div>
      </div>
    </div>

    <!-- Participants table -->
    <div class="bg-surface border border-border rounded-xl overflow-hidden mb-8">
      <div class="px-4 py-3 border-b border-border text-sm font-bold text-fg flex items-center justify-between">
        <span>شرکت‌کنندگان</span>
        <button
          v-if="entryCountDirty"
          @click="saveEntryCounts"
          :disabled="ecSaveLoading"
          class="px-3 py-1 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-xs rounded font-bold transition inline-flex items-center gap-1.5"
        >
          <div v-if="ecSaveLoading" class="w-3 h-3 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ذخیره تعداد ورودی
        </button>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted bg-surface-hover">
              <th class="px-4 py-3 text-right">نام</th>
              <th class="px-4 py-3 text-right">نقش</th>
              <th class="px-4 py-3 text-right">طرف</th>
              <th class="px-4 py-3 text-right">وضعیت</th>
              <th class="px-4 py-3 text-right">تعداد ورودی</th>
              <th v-if="play.status === 'done' && play.playType === 'superrank'" class="px-4 py-3 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.id" class="border-b border-border text-fg">
              <td class="px-4 py-3">{{ p.name }}</td>
              <td class="px-4 py-3">{{ p.roleName }}</td>
              <td class="px-4 py-3" :class="p.sideId === 1 ? 'text-[#dc5050]' : 'text-[#5096dc]'">{{ p.sideId === 1 ? 'مافیا' : 'شهروند' }}</td>
              <td class="px-4 py-3"><span v-if="p.isGuest" class="text-xs text-muted">مهمان</span></td>
              <td class="px-4 py-3">
                <div v-if="canEditEntryCount && !p.isGuest" class="flex items-center gap-1">
                  <button @click="changeLocalEntryCount(p.id, -1)" class="w-6 h-6 flex items-center justify-center text-xs rounded border border-gold/30 text-gold-text hover:bg-gold/20 transition disabled:opacity-30" :disabled="(localEntryCounts[p.id] ?? p.entryCount) <= 1">−</button>
                  <span class="w-7 text-center text-xs text-fg tabular-nums" dir="ltr">{{ localEntryCounts[p.id] ?? p.entryCount }}</span>
                  <button @click="changeLocalEntryCount(p.id, 1)" class="w-6 h-6 flex items-center justify-center text-xs rounded border border-gold/30 text-gold-text hover:bg-gold/20 transition disabled:opacity-30" :disabled="(localEntryCounts[p.id] ?? p.entryCount) >= 10">+</button>
                </div>
                <span v-else-if="(localEntryCounts[p.id] ?? p.entryCount) > 1"
                      class="inline-flex items-center px-2 py-0.5 text-xs rounded-full bg-gold/20 text-gold-text border border-gold/30 leading-none">
                  ×{{ toPersianDigits(localEntryCounts[p.id] ?? p.entryCount) }}
                </span>
                <span v-else class="text-xs text-muted">۱</span>
              </td>
              <td v-if="play.status === 'done' && play.playType === 'superrank'" class="px-4 py-3"></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Action areas -->
    <div v-if="play.status === 'pending'" class="bg-surface border border-border rounded-xl p-6 text-center">
      <div class="text-sm text-muted mb-4">این بازی هنوز در حال پخش نقش است</div>
      <div class="flex items-center justify-center gap-3">
        <button
          @click="reshuffleConfirmOpen = true"
          :disabled="reshuffleLoading"
          class="px-5 py-2 border border-[rgba(201,176,122,0.3)] hover:bg-gold/10 text-gold-text text-sm rounded font-medium transition inline-flex items-center gap-2"
        >
          <div v-if="reshuffleLoading" class="w-4 h-4 border-2 border-gold border-t-transparent rounded-full animate-spin" />
          پخش دوباره نقش‌ها
        </button>
        <router-link
          :to="{ name: 'MasterPlayReveal', params: { clubId: ctx?.clubId ?? clubId, playId: play.id } }"
          class="inline-block px-5 py-2 bg-gold hover:opacity-80 text-[#0d0d0f] text-sm rounded font-bold transition"
        >
          رفتن به صفحه پخش نقش
        </router-link>
      </div>
    </div>

    <div v-else-if="play.status === 'notwinside'" class="bg-surface border border-border rounded-xl p-6 mb-8">
      <h3 class="text-sm font-bold text-fg mb-4">ثبت نتیجه</h3>
      <div v-if="sides.length === 0" class="flex justify-center py-4">
        <div class="w-5 h-5 border-2 border-gold border-t-transparent rounded-full animate-spin" />
      </div>
      <div v-else class="flex flex-wrap gap-3 mb-4">
        <button
          v-for="s in sides"
          :key="s.id"
          @click="selectedWinnerside = s.id"
          class="px-5 py-2.5 text-sm rounded font-medium border transition"
          :class="selectedWinnerside === s.id ? 'bg-gold text-[#0d0d0f] border-gold' : 'bg-bg text-muted border-border hover:border-gold/30'"
        >
          {{ s.name }}
        </button>
      </div>
      <div class="flex gap-3 items-center">
        <button
          @click="doSubmitWinnerside"
          :disabled="!selectedWinnerside || winnersideLoading"
          class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2"
        >
          <div v-if="winnersideLoading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ثبت نتیجه
        </button>
        <span v-if="winnersideError" class="text-xs text-danger">{{ winnersideError }}</span>
      </div>
    </div>

    <div v-else-if="play.status === 'notrank'" class="bg-surface border border-border rounded-xl p-6 mb-8">
      <h3 class="text-sm font-bold text-fg mb-4">ثبت رتبه</h3>
      <div class="overflow-x-auto mb-4">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted">
              <th class="px-4 py-2 text-right">بازیکن</th>
              <th class="px-4 py-2 text-right">نقش</th>
              <th class="px-4 py-2 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.id" class="border-b border-border text-fg">
              <td class="px-4 py-2">{{ p.name }}</td>
              <td class="px-4 py-2">{{ p.roleName }}</td>
              <td class="px-4 py-2">
                <input
                  v-model.number="rankInputs[p.id]"
                  type="number"
                  min="1"
                  :max="play.playersCount"
                  class="w-20 bg-input border border-border rounded px-3 py-1.5 text-sm text-fg focus:outline-none focus:border-gold transition"
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
          class="px-6 py-2 bg-gold hover:opacity-80 disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2"
        >
          <div v-if="ranksLoading" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          ثبت رتبه‌ها
        </button>
        <span v-if="ranksError" class="text-xs text-danger">{{ ranksError }}</span>
      </div>
    </div>

    <div v-else-if="play.status === 'done'" class="bg-surface border border-border rounded-xl p-6 text-center">
      <div class="text-lg mb-2">✅</div>
      <h3 class="text-sm font-bold text-success mb-2">بازی کامل شد</h3>
      <div v-if="winnersideName" class="text-sm text-muted">برنده: {{ winnersideName }}</div>
      <div v-if="play.playType === 'superrank'" class="mt-4 overflow-x-auto">
        <table class="w-full text-sm max-w-sm mx-auto">
          <thead>
            <tr class="border-b border-border text-muted">
              <th class="px-4 py-2 text-right">بازیکن</th>
              <th class="px-4 py-2 text-right">رتبه</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in play.participants" :key="p.id" class="border-b border-border text-fg">
              <td class="px-4 py-2">{{ p.name }}</td>
              <td class="px-4 py-2">-</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <div v-else class="flex justify-center py-20">
    <div class="w-8 h-8 border-2 border-gold border-t-transparent rounded-full animate-spin" />
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

  <!-- Confirm reshuffle modal -->
  <ConfirmModal
    :isOpen="reshuffleConfirmOpen"
    title="تأیید پخش دوباره نقش‌ها"
    message="با این کار نقش‌های فعلی همه شرکت‌کنندگان به صورت تصادفی از نو پخش می‌شود. آیا مطمئن هستید؟"
    confirmText="بله، دوباره پخش کن"
    cancelText="انصراف"
    @confirm="confirmReshuffle"
    @cancel="reshuffleConfirmOpen = false"
  />

  <!-- Confirm delete modal -->
  <ConfirmModal
    :isOpen="deleteConfirmOpen"
    title="تأیید حذف بازی"
    message="این بازی حذف می‌شود و در لیست‌ها و آمار دیده نخواهد شد. آیا مطمئن هستید؟"
    confirmText="بله، حذف کن"
    cancelText="انصراف"
    @confirm="doDeletePlay"
    @cancel="deleteConfirmOpen = false"
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
const reshuffleConfirmOpen = ref(false)
const deleteLoading = ref(false)
const deleteConfirmOpen = ref(false)

// EntryCount inline editing
const localEntryCounts = ref<Record<number, number>>({})
const ecSaveLoading = ref(false)
const entryCountDirty = ref(false)

function toPersianDigits(n: number): string {
  return n.toString().replace(/\d/g, d => '۰۱۲۳۴۵۶۷۸۹'[parseInt(d)])
}

const canEditEntryCount = computed(() => {
  if (!play.value) return false
  if (authStore.isAdmin) return true
  if (authStore.activeClubRole === 'owner' || authStore.activeClubRole === 'cashier') return true
  if (authStore.activeClubRole === 'master' && ctx.value?.masterId === play.value.masterId) return true
  return false
})

function changeLocalEntryCount(participantRowId: number, delta: number) {
  const current = localEntryCounts.value[participantRowId] ?? 1
  const newVal = current + delta
  if (newVal < 1 || newVal > 10) return
  localEntryCounts.value[participantRowId] = newVal
  entryCountDirty.value = true
}

async function saveEntryCounts() {
  if (!play.value || !clubId.value) return
  ecSaveLoading.value = true
  try {
    const participants = play.value.participants
      .filter((p, i, arr) => arr.findIndex(x => x.clubPlayerId === p.clubPlayerId) === i)
      .map(p => ({
      clubPlayerId: p.clubPlayerId,
      isGuest: p.isGuest,
      entryCount: localEntryCounts.value[p.id] ?? p.entryCount,
    }))
    const dto: CreateClubPlayDto = {
      title: play.value.title || undefined,
      dateTime: play.value.dateTime,
      roomId: play.value.roomId,
      senarioId: play.value.senarioId,
      playersCount: play.value.playersCount,
      desc: play.value.desc || undefined,
      link: play.value.link || undefined,
      playType: play.value.playType as CreateClubPlayDto['playType'],
      eventId: play.value.eventId,
      nerkhId: play.value.nerkhId,
      participants,
    }
    const res = await ClubPlayApi.updateClubPlay(clubId.value, play.value.id, dto)
    play.value = res.data
    // Re-init local counts from response
    res.data.participants.forEach(p => {
      localEntryCounts.value[p.id] = p.entryCount
    })
    entryCountDirty.value = false
    toastSuccess('تعداد ورودی‌ها با موفقیت ذخیره شد')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در ذخیره تعداد ورودی‌ها')
  } finally {
    ecSaveLoading.value = false
  }
}

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

const canDelete = computed(() => {
  if (authStore.activeClubRole === 'cashier') return false
  if (authStore.isAdmin || authStore.activeClubRole === 'owner' || authStore.activeClubRole === 'supervisor') return true
  return play.value?.status !== 'done'
})

const winnersideName = computed(() => {
  if (!play.value?.winnersideId) return null
  return sides.value.find(s => s.id === play.value!.winnersideId)?.name || null
})

const allRanksFilled = computed(() => {
  if (!play.value) return false
  return play.value.participants.every(p => rankInputs[p.id] != null && rankInputs[p.id] > 0)
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
    confirmEditMessage.value = 'تغییر سناریو یا تعداد بازیکنان باعث پخش مجدد و تصادفی نقش‌ها می‌شود و باید دوباره وارد صفحه پخش نقش شوید'
    pendingDto.value = dto
    confirmEditOpen.value = true
    return
  }

  await doActualUpdate(dto, cId, false)
}

async function doConfirmedUpdate() {
  if (!pendingDto.value || !clubId.value || !play.value) return
  confirmEditOpen.value = false
  const dto = pendingDto.value
  const cId = clubId.value
  pendingDto.value = null
  await doActualUpdate(dto, cId, true)
}

async function doActualUpdate(dto: CreateClubPlayDto, cId: number, roleRebuildExpected: boolean) {
  if (!play.value) return
  editSubmitting.value = true
  try {
    const res = await ClubPlayApi.updateClubPlay(cId, play.value.id, dto)
    play.value = res.data
    res.data.participants.forEach(p => {
      localEntryCounts.value[p.id] = p.entryCount
    })
    toastSuccess('بازی با موفقیت ویرایش شد')
    editing.value = false
    formReady.value = false

    if (roleRebuildExpected && play.value.status === 'pending') {
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
      id: p.id,
      rank: rankInputs[p.id] || 0,
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

async function confirmReshuffle() {
  reshuffleConfirmOpen.value = false
  await doReshuffle()
}

async function doDeletePlay() {
  const cId = clubId.value
  if (!cId || !play.value) return
  deleteLoading.value = true
  deleteConfirmOpen.value = false
  try {
    await ClubPlayApi.deleteClubPlay(cId, play.value.id)
    toastSuccess('بازی با موفقیت حذف شد')
    router.push({ name: 'MasterPlaysList' })
  } catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string } } }
    toastError(err?.response?.data?.message || 'خطا در حذف بازی')
  } finally {
    deleteLoading.value = false
  }
}

async function doReshuffle() {
  const cId = clubId.value
  if (!cId || !play.value) return
  reshuffleLoading.value = true
  try {
    const res = await ClubPlayApi.reshuffleRoles(cId, play.value.id)
    play.value = res.data
    res.data.participants.forEach(p => {
      localEntryCounts.value[p.id] = p.entryCount
    })
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
    playRes.data.participants.forEach(p => {
      localEntryCounts.value[p.id] = p.entryCount
    })

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
