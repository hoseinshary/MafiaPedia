<template>
  <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-xl p-6 space-y-5">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">عنوان</label>
        <input v-model="form.title" type="text" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" placeholder="اختیاری" @input="onTitleInput" />
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">فصل / Event</label>
        <div v-if="events.length === 0 && !eventsLoading">
          <p class="text-xs text-[#e07070]">این کافه هنوز فصل پیش‌فرض ندارد</p>
        </div>
        <select v-else v-model="form.eventId" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition">
          <option value="" disabled>انتخاب فصل</option>
          <option v-for="ev in events" :key="ev.id" :value="ev.id">
            {{ ev.name }} <span v-if="ev.isDefault" class="text-[#c9b07a]">(پیش‌فرض)</span>
          </option>
        </select>
      </div>
      <div>
        <label for="dtpicker-cf" class="text-sm text-[rgba(232,228,217,0.4)]">تاریخ و ساعت <span class="text-[#e07070]">*</span></label>
        <input id="dtpicker-cf" v-model="form.dateTime" type="datetime-local" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" @change="onDateTimeChange" @click="($event.target as HTMLInputElement).showPicker?.()" />
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">سالن <span class="text-[#e07070]">*</span></label>
        <select v-model="form.roomId" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition">
          <option value="" disabled>انتخاب سالن</option>
          <option v-for="room in rooms" :key="room.id" :value="room.id">{{ room.name }}</option>
        </select>
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">سناریو <span class="text-[#e07070]">*</span></label>
        <select v-model="form.senarioId" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition">
          <option value="" disabled>انتخاب سناریو</option>
          <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
        </select>
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">تعداد بازیکن <span class="text-[#e07070]">*</span></label>
        <input v-model.number="form.playersCount" type="number" min="2" max="30" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition ltr" />
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">نوع بازی <span class="text-[#e07070]">*</span></label>
        <select v-model="form.playType" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition">
          <option value="normal">Normal</option>
          <option value="rank">Rank</option>
          <option value="superrank">Super Rank</option>
          <option value="etc">و ...</option>
        </select>
      </div>
      <div>
        <label class="text-sm text-[rgba(232,228,217,0.4)]">لینک یوتیوب</label>
        <input v-model="form.link" type="text" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition" placeholder="اختیاری" />
      </div>
    </div>

    <div>
      <label class="text-sm text-[rgba(232,228,217,0.4)]">توضیحات</label>
      <textarea v-model="form.desc" rows="2" class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.07)] rounded px-4 py-2.5 text-sm text-[#e8e4d9] placeholder-[rgba(232,228,217,0.25)] focus:outline-none focus:border-[rgba(201,176,122,0.3)] transition resize-none" placeholder="اختیاری"></textarea>
    </div>

    <div class="flex items-start gap-3">
      <input id="shuffleToggle-cf" type="checkbox" v-model="form.shuffleRoles" class="mt-1 w-4 h-4 accent-[#c9b07a]" />
      <div>
        <label for="shuffleToggle-cf" class="text-sm text-[#e8e4d9] cursor-pointer">پخش تصادفی نقش‌ها</label>
        <p class="text-xs text-[rgba(232,228,217,0.3)] mt-0.5">در حالت غیرفعال، نقش‌ها به همان ترتیبی که بازیکنان را اضافه کردید داده می‌شوند (فقط برای تست).</p>
      </div>
    </div>

    <div>
      <label class="text-sm text-[rgba(232,228,217,0.4)] mb-2 block">
        شرکت‌کنندگان <span class="text-[#e07070]">*</span>
        <span class="text-xs mr-2" :class="participantCount === form.playersCount ? 'text-[#4ade80]' : 'text-[rgba(232,228,217,0.3)]'">
          {{ participantCount }} از {{ form.playersCount }} انتخاب شده
          <span v-if="guestCount > 0" class="text-[rgba(232,228,217,0.4)]">({{ guestCount }} نفر مهمان)</span>
        </span>
      </label>
      <ParticipantPicker :clubId="masterCtx.clubId" :initialSelected="initialParticipants" @change="onParticipantsChange" />
    </div>

    <p v-if="error" class="text-sm text-[#e07070]">{{ error }}</p>

    <div class="flex gap-3 justify-end pt-4 border-t border-[rgba(255,255,255,0.07)]">
      <button type="button" @click="$emit('cancel')" class="px-4 py-2 border border-[rgba(255,255,255,0.07)] text-[rgba(232,228,217,0.4)] hover:text-[#e8e4d9] text-sm rounded font-medium transition">انصراف</button>
      <button type="button" @click="handleSubmit" :disabled="!canSubmit || submitting || events.length === 0" class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] disabled:opacity-40 disabled:cursor-not-allowed text-[#0d0d0f] text-sm rounded font-bold transition inline-flex items-center gap-2">
        <div v-if="submitting" class="w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
        {{ mode === 'edit' ? 'ویرایش بازی' : 'ثبت و پخش نقش' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { ClubPlayApi } from '@/api'
import { toPersianOrdinal } from '@/utils/persianOrdinal'
import type { MasterContextDto, ClubPlayDetailDto, EventDto, CreateClubPlayDto } from '@/types/clubPlay'
import type { RoomDto } from '@/types/club'
import type { Senario } from '@/types'
import ParticipantPicker, { type PickerParticipant } from '@/components/master/ParticipantPicker.vue'

const props = defineProps<{
  mode: 'create' | 'edit'
  masterCtx: MasterContextDto
  rooms: RoomDto[]
  scenarios: Senario[]
  events: EventDto[]
  eventsLoading?: boolean
  initial?: ClubPlayDetailDto | null
  submitting?: boolean
}>()

const emit = defineEmits<{
  submit: [dto: CreateClubPlayDto]
  cancel: []
}>()

const participants = ref<PickerParticipant[]>([])
const error = ref('')
const titleUserEdited = ref(props.mode === 'edit')
const playCountToday = ref(0)
let titleDebounce: ReturnType<typeof setTimeout>

function mapParticipants(p: ClubPlayDetailDto['participants'][number]): PickerParticipant {
  return {
    player: { id: p.clubPlayerId, name: p.name, mobile: '', picture: null } as any,
    isGuest: p.isGuest,
  }
}

const initialParticipants = props.mode === 'edit' && props.initial
  ? props.initial.participants.map(mapParticipants)
  : undefined

const now = new Date()
const nowLocal = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 16)

const form = reactive({
  title: props.initial?.title || '',
  dateTime: props.initial ? props.initial.dateTime.slice(0, 16) : nowLocal,
  roomId: props.initial ? props.initial.roomId : ('' as string | number),
  senarioId: props.initial ? props.initial.senarioId : ('' as string | number),
  playersCount: props.initial?.playersCount || 10,
  playType: props.initial?.playType || 'normal',
  link: props.initial?.link || '',
  desc: props.initial?.desc || '',
  eventId: props.initial?.eventId || ('' as number | ''),
  shuffleRoles: true,
})

const participantCount = computed(() => participants.value.length)
const guestCount = computed(() => participants.value.filter(p => p.isGuest).length)

const canSubmit = computed(() => {
  return form.dateTime
    && form.roomId !== ''
    && form.senarioId !== ''
    && form.eventId !== ''
    && form.playersCount >= 2
    && participantCount.value === form.playersCount
})

function onTitleInput() {
  if (props.mode === 'create') titleUserEdited.value = true
}

function computeGeneratedTitle(): string {
  return `بازی ${toPersianOrdinal(playCountToday.value + 1)} ${props.masterCtx.masterName}`
}

function onDateTimeChange() {
  if (props.mode === 'edit') return
  clearTimeout(titleDebounce)
  titleDebounce = setTimeout(fetchPlayCount, 300)
}

async function fetchPlayCount() {
  if (props.mode === 'edit' || !form.dateTime) return
  try {
    const dateOnly = form.dateTime.slice(0, 10)
    const res = await ClubPlayApi.getPlayCountByDate(props.masterCtx.clubId, dateOnly)
    playCountToday.value = res.data.count
    if (!titleUserEdited.value) {
      form.title = computeGeneratedTitle()
    }
  } catch { /* ignore */ }
}

function onParticipantsChange(selected: PickerParticipant[]) {
  participants.value = selected
}

function handleSubmit() {
  if (!canSubmit.value) return
  error.value = ''
  const dto: CreateClubPlayDto = {
    title: form.title || undefined,
    dateTime: form.dateTime || now.toISOString().slice(0, 16),
    roomId: Number(form.roomId),
    senarioId: Number(form.senarioId),
    playersCount: form.playersCount,
    playType: form.playType as any,
    eventId: form.eventId || undefined,
    shuffleRoles: form.shuffleRoles,
    participants: participants.value.map(p => ({ clubPlayerId: p.player.id, isGuest: p.isGuest })),
    desc: form.desc || undefined,
    link: form.link || undefined,
  }
  emit('submit', dto)
}
</script>

<style scoped>
input[type="datetime-local"] {
  color-scheme: dark;
}
input[type="datetime-local"]::-webkit-calendar-picker-indicator {
  filter: invert(0.8);
  cursor: pointer;
  opacity: 1;
}
</style>
