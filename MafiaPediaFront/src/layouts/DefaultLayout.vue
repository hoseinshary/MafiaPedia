<template>
  <div class="min-h-screen flex flex-col">
    <header class="bg-gray-900 text-white shadow-md">
      <div class="container mx-auto px-4 py-4 flex items-center justify-between">
        <router-link to="/" class="text-xl font-bold">MafiaPedia</router-link>
        <nav class="flex gap-6">
          <router-link to="/" class="hover:text-gray-300 transition">Home</router-link>
          <router-link to="/ranking/citizen" class="hover:text-gray-300 transition">Citizen Ranking</router-link>
          <router-link to="/ranking/mafia" class="hover:text-gray-300 transition">Mafia Ranking</router-link>
        </nav>
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
