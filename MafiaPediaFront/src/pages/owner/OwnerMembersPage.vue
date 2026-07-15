<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link to="/owner" class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a] transition">
        &larr; بازگشت به داشبورد
      </router-link>
      <h1 class="text-xl font-bold text-[#e8e4d9] mt-2">مدیریت اعضا</h1>
    </div>
    <ClubMembersManager :club-id="authStore.activeClubId!" :allow-owner-role="false" />
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore'
import ClubMembersManager from '@/components/club/ClubMembersManager.vue'

const authStore = useAuthStore()
</script>
