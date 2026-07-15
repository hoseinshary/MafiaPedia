<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link :to="`/admin/clubs/${clubId}`" class="text-xs text-[rgba(232,228,217,0.4)] hover:text-[#c9b07a] transition">
        &larr; بازگشت به کافه
      </router-link>
      <h1 class="text-xl font-bold text-[#e8e4d9] mt-2">مدیریت اعضای کافه</h1>
    </div>
    <ClubMembersManager :club-id="clubId" :allow-owner-role="true" />
  </div>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import ClubMembersManager from '@/components/club/ClubMembersManager.vue'

const route = useRoute()
const clubId = Number(route.params.id)
</script>
