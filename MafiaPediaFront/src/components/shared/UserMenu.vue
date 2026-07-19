<template>
  <div ref="containerRef" class="relative" dir="rtl">
    <button @click.stop="open = !open" class="btn-ghost flex items-center gap-2 text-sm">
      <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.121 17.804A9 9 0 0112 15a9 9 0 016.879 2.804M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>
      <span class="hidden md:inline">{{ authStore.displayName }}</span>
    </button>
    <div v-show="open" class="dropdown-menu" @click.stop>
      <router-link to="/account/profile" class="dropdown-link" @click="open = false">پروفایل</router-link>
      <router-link to="/account/password" class="dropdown-link" @click="open = false">تغییر رمز عبور</router-link>
      <div class="border-t border-border my-1" />
      <button @click="handleLogout" class="dropdown-link w-full text-right" style="color:#e07070;">خروج</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const emit = defineEmits<{ logout: [] }>()

const authStore = useAuthStore()
const open = ref(false)
const containerRef = ref<HTMLElement | null>(null)

function handleLogout() {
  open.value = false
  emit('logout')
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') open.value = false
}

function onClickOutside(e: MouseEvent) {
  if (containerRef.value && !containerRef.value.contains(e.target as Node)) {
    open.value = false
  }
}

onMounted(() => {
  document.addEventListener('keydown', onKeydown)
  document.addEventListener('click', onClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('keydown', onKeydown)
  document.removeEventListener('click', onClickOutside)
})
</script>

<style scoped>
.btn-ghost {
  font-size: 12px;
  padding: 6px 14px;
  border: 0.5px solid var(--color-border);
  border-radius: 6px;
  background: transparent;
  color: var(--color-muted);
  cursor: pointer;
  font-family: inherit;
  transition: color 0.15s;
}
.btn-ghost:hover {
  color: var(--color-fg);
}
.dropdown-menu {
  position: absolute;
  left: 0;
  right: auto;
  top: 100%;
  margin-top: 4px;
  min-width: 180px;
  background: var(--color-surface);
  border: 0.5px solid var(--color-border);
  border-radius: 8px;
  padding: 4px 0;
  box-shadow: 0 4px 20px rgba(0,0,0,0.4);
  z-index: 50;
}
.dropdown-link {
  display: block;
  padding: 7px 16px;
  font-size: 13px;
  color: var(--color-muted);
  text-decoration: none;
  transition: all 0.12s;
  background: none;
  border: none;
  cursor: pointer;
  font-family: inherit;
}
.dropdown-link:hover {
  color: var(--color-gold);
  background: var(--color-surface-hover);
}
</style>
