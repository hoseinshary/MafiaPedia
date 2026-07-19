<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-lg font-bold text-fg">لیست بدهکاران</h1>
    </div>

    <div class="bg-surface border border-border rounded-xl overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-border text-muted text-xs">
              <th class="text-right px-4 py-3 font-medium">نام</th>
              <th class="text-right px-4 py-3 font-medium">موبایل</th>
              <th class="text-center px-4 py-3 font-medium">کل بدهی</th>
              <th class="text-center px-4 py-3 font-medium">قدیمی‌ترین بدهی</th>
              <th class="text-center px-4 py-3 font-medium">عملیات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="d in debtors" :key="d.clubPlayerId" class="border-b border-border last:border-0 hover:bg-surface-hover/50 transition">
              <td class="px-4 py-3 text-fg font-medium">{{ d.name }}</td>
              <td class="px-4 py-3 text-muted text-xs">{{ d.mobile }}</td>
              <td class="px-4 py-3 text-center text-danger font-bold">{{ d.totalDebt.toLocaleString() }}</td>
              <td class="px-4 py-3 text-center text-muted text-xs">{{ toJalali(d.oldestUnpaidDate) || '-' }}</td>
              <td class="px-4 py-3 text-center">
                <button @click="$router.push(`/finance/settlement/${d.clubPlayerId}`)" class="px-3 py-1.5 dark:text-gold bg-gold/10 text-gold-text text-xs rounded font-medium hover:bg-gold/20 transition">تسویه</button>
              </td>
            </tr>
            <tr v-if="debtors.length === 0">
              <td colspan="5" class="text-center py-8 text-muted text-xs">هیچ بدهکاری وجود ندارد</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { FinanceApi } from '@/api'
import { useAuthStore } from '@/stores/authStore'
import { toJalali } from '@/utils/jalaliDate'
import type { DebtorDto } from '@/types/finance'

const authStore = useAuthStore()
const clubId = () => authStore.activeClubId!
const debtors = ref<DebtorDto[]>([])

onMounted(load)
async function load() {
  if (!authStore.activeClubId) return
  try {
    const res = await FinanceApi.getDebtors(clubId())
    debtors.value = res.data
  } catch { /* toast handled */ }
}
</script>
