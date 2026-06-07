<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-gray-900 text-white shadow-md">
      <div class="container mx-auto px-4 py-4 flex items-center justify-between" dir="rtl">
        <div class="flex items-center gap-6">
          <router-link to="/" class="text-xl font-bold">Mafia<span class="text-red-500">Pedia</span></router-link>
          <nav class="hidden md:flex gap-4">
            <router-link to="/" class="hover:text-gray-300 transition">خانه</router-link>
            <span class="hover:text-gray-300 transition opacity-60 cursor-not-allowed">رنکینگ کلی</span>
            <router-link to="/ranking/mafia" class="hover:text-gray-300 transition">رنکینگ مافیا</router-link>
            <router-link to="/ranking/citizen" class="hover:text-gray-300 transition">رنکینگ شهروندی</router-link>
          </nav>
        </div>
        <div class="flex items-center gap-4">
          <div class="hidden md:block">
            <PlayerSearchAutocomplete />
          </div>
          <button @click="mobileSearchOpen = !mobileSearchOpen" class="md:hidden hover:text-gray-300 transition p-1">
            <svg v-if="!mobileSearchOpen" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd" />
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
          <button class="hover:text-gray-300 transition opacity-60 cursor-not-allowed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M10 2a6 6 0 00-6 6v3.586l-.707.707A1 1 0 004 14h12a1 1 0 00.707-1.707L16 11.586V8a6 6 0 00-6-6zM10 18a3 3 0 01-3-3h6a3 3 0 01-3 3z" />
            </svg>
          </button>
          <button class="hover:text-gray-300 transition opacity-60 cursor-not-allowed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
      </div>
      <div v-if="mobileSearchOpen" class="md:hidden border-t border-gray-700 px-4 py-3">
        <PlayerSearchAutocomplete />
      </div>
    </header>

    <main class="flex-1 container mx-auto px-4 py-6">
      <router-view />
    </main>

    <footer class="bg-gray-900 text-white text-center py-4 text-sm">
      &copy; {{ currentYear }} MafiaPedia. All rights reserved.
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import PlayerSearchAutocomplete from '@/components/PlayerSearchAutocomplete.vue'

const mobileSearchOpen = ref(false)
const currentYear = new Date().getFullYear()
</script>
