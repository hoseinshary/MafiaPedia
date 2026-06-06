<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-gray-900 text-white shadow-md">
      <div class="container mx-auto px-4 py-4 flex items-center justify-between" dir="rtl">
        <div class="flex items-center gap-6">
          <router-link to="/" class="text-xl font-bold">Mafia<span class="text-red-500">Pedia</span></router-link>
          <nav class="flex gap-4">
            <router-link to="/" class="hover:text-gray-300 transition">خانه</router-link>
            <span class="hover:text-gray-300 transition opacity-60 cursor-not-allowed">رنکینگ کلی</span>
            <router-link to="/ranking/mafia" class="hover:text-gray-300 transition">رنکینگ مافیا</router-link>
            <router-link to="/ranking/citizen" class="hover:text-gray-300 transition">رنکینگ شهروندی</router-link>
          </nav>
        </div>
        <div class="flex items-center gap-4">
          <div class="relative opacity-60 cursor-not-allowed">
            <input type="text" placeholder="جستجو بازیکن" class="bg-gray-800 text-white rounded px-3 py-1 text-sm w-40 placeholder-gray-400" disabled />
          </div>
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
const currentYear = new Date().getFullYear()
</script>
