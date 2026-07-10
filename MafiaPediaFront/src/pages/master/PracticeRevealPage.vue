<template>
  <div class="min-h-[80vh] flex flex-col items-center px-4 py-8">
    <!-- banner -->
    <div class="w-full max-w-md mb-6 px-4 py-2 rounded-lg text-center text-xs bg-[rgba(201,176,122,0.12)] border border-[rgba(201,176,122,0.25)] text-[#c9b07a]">
      این یک پیش‌نمایش است — هیچ بازی‌ای ثبت نمی‌شود
    </div>

    <!-- step 1: setup -->
    <div v-if="step === 'setup'" class="w-full max-w-sm">
      <div class="bg-[var(--color-card)] border border-[rgba(255,255,255,0.07)] rounded-2xl p-8">
        <h1 class="text-lg font-bold text-[#c9b07a] text-center mb-8">پیش‌نمایش نقش‌ها</h1>

        <div class="mb-4">
          <label class="block text-sm text-[rgba(232,228,217,0.5)] mb-2">سناریو</label>
          <select v-model="selectedSenarioId" class="w-full p-3 rounded-lg bg-[#0d0d0f] border border-[rgba(255,255,255,0.1)] text-[#e8e4d9] text-sm appearance-none">
            <option :value="0" disabled>انتخاب سناریو</option>
            <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>

        <div class="mb-6">
          <label class="block text-sm text-[rgba(232,228,217,0.5)] mb-2">تعداد بازیکن</label>
          <input
            v-model.number="playerCount"
            type="number"
            min="2"
            max="30"
            class="w-full bg-[#0d0d0f] border border-[rgba(255,255,255,0.1)] rounded-xl px-3 py-2.5 text-[#e8e4d9] text-sm text-center"
          />
        </div>

        <div v-if="errorMsg" class="text-xs text-[#e07070] text-center mb-4">{{ errorMsg }}</div>

        <button
          @click="startPreview"
          :disabled="loading"
          class="w-full py-2.5 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition disabled:opacity-50"
        >
          <span v-if="loading" class="inline-block w-4 h-4 border-2 border-[#0d0d0f] border-t-transparent rounded-full animate-spin" />
          <span v-else>شروع پیش‌نمایش</span>
        </button>
      </div>
    </div>

    <!-- step 2: reveal -->
    <div v-else-if="step === 'reveal'" class="flex flex-col items-center w-full">
      <div class="text-center mb-6">
        <h1 class="text-lg font-bold text-[#c9b07a]">پیش‌نمایش نقش‌ها</h1>
        <p class="text-sm text-[rgba(232,228,217,0.4)] mt-1">{{ selectedScenarioName }} — {{ playerCount }} نفر</p>
      </div>

      <RoleRevealStepper
        ref="stepperRef"
        :items="revealItems"
      >
        <template #reset-btn>
          <button
            @click="reshufflePractice"
            class="text-xs text-[rgba(232,228,217,0.25)] hover:text-[#c9b07a] transition underline underline-offset-2"
          >
            ریست و پخش دوباره
          </button>
        </template>
        <template #completion-actions>
          <button
            @click="resetAll"
            class="px-6 py-2 bg-[#c9b07a] hover:bg-[#b8a16e] text-[#0d0d0f] text-sm rounded font-bold transition"
          >
            شروع دوباره
          </button>
        </template>
      </RoleRevealStepper>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { LookupApi } from '@/api'
import RoleRevealStepper from '@/components/clubplay/RoleRevealStepper.vue'
import type { Senario, RoleSetEntryDto } from '@/types'
import type { RevealItem } from '@/components/clubplay/RoleRevealStepper.vue'

const step = ref<'setup' | 'reveal'>('setup')
const scenarios = ref<Senario[]>([])
const selectedSenarioId = ref(0)
const playerCount = ref(10)
const errorMsg = ref('')
const loading = ref(false)
const roleSet = ref<RoleSetEntryDto[]>([])
const stepperRef = ref<InstanceType<typeof RoleRevealStepper> | null>(null)

const selectedScenarioName = computed(() =>
  scenarios.value.find(s => s.id === selectedSenarioId.value)?.name || ''
)

const revealItems = computed<RevealItem[]>(() =>
  roleSet.value.map((r, i) => ({
    label: `بازیکن ${i + 1}`,
    roleName: r.roleName,
    rolePhoto: r.rolePhoto,
    sideId: r.sideId,
  }))
)

onMounted(async () => {
  try {
    const res = await LookupApi.getScenarios()
    scenarios.value = res.data
  } catch {
    // scenarios loading silently handled
  }
})

async function startPreview() {
  if (!selectedSenarioId.value) {
    errorMsg.value = 'لطفاً سناریو را انتخاب کنید'
    return
  }
  if (playerCount.value < 2) {
    errorMsg.value = 'تعداد بازیکن باید حداقل ۲ باشد'
    return
  }

  errorMsg.value = ''
  loading.value = true
  try {
    const res = await LookupApi.getRoleSet(selectedSenarioId.value, playerCount.value)
    roleSet.value = shuffle(res.data)
    step.value = 'reveal'
  } catch (e: unknown) {
    const err = e as { response?: { status?: number } }
    if (err?.response?.status === 404) {
      errorMsg.value = `این سناریو برای ${playerCount.value} نفر پیکربندی نشده است`
    } else {
      errorMsg.value = 'خطا در دریافت نقش‌ها'
    }
  } finally {
    loading.value = false
  }
}

function reshufflePractice() {
  errorMsg.value = ''
  roleSet.value = shuffle([...roleSet.value])
  stepperRef.value?.reset()
}

function resetAll() {
  step.value = 'setup'
  roleSet.value = []
}

function shuffle<T>(arr: T[]): T[] {
  const a = [...arr]
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]]
  }
  return a
}
</script>