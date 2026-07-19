<template>
  <div dir="rtl" class="max-w-4xl mx-auto w-full">
    <div class="mb-6">
      <router-link :to="`/admin/clubs/${clubId}`" class="text-xs text-muted hover:text-gold-text transition">
        &larr; بازگشت 
      </router-link>
      <h1 class="text-xl font-bold text-fg mt-2">مدیریت اعضا</h1>
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
