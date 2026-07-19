<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-8">
      <h1 class="text-xl font-bold text-pink-400">داشبورد صندوق‌دار</h1>
      <p class="text-sm text-muted mt-1">{{ authStore.activeClubContext?.clubName }}</p>
    </div>

    <!-- Quick links -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
      <router-link to="/finance/today" class="block bg-surface border border-border rounded-xl p-6 hover:border-gold/30 transition text-center">
        <div class="text-2xl mb-2">📋</div>
        <div class="text-sm font-bold text-gold-text">حساب‌های امروز</div>
        <div class="text-xs text-muted mt-1">مشاهده حساب‌های باز امروز</div>
      </router-link>
      <router-link to="/finance/order" class="block bg-surface border border-border rounded-xl p-6 hover:border-gold/30 transition text-center">
        <div class="text-2xl mb-2">🛒</div>
        <div class="text-sm font-bold text-gold-text">ثبت سفارش</div>
        <div class="text-xs text-muted mt-1">ثبت سفارش جدید برای مشتری</div>
      </router-link>
    </div>

    <ClubTodaysPlaysCard :club-id="authStore.activeClubId!" />
    <ClubStatsCard :club-id="authStore.activeClubId!" />
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore'
import ClubTodaysPlaysCard from '@/components/club/ClubTodaysPlaysCard.vue'
import ClubStatsCard from '@/components/club/ClubStatsCard.vue'

const authStore = useAuthStore()
</script>
