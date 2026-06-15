<template>
  <div dir="rtl" class="w-full md:w-3/4 mx-auto">
    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-10 h-10 border-4 border-gray-600 border-t-red-500 rounded-full animate-spin" />
    </div>

    <div v-else-if="notFound" class="text-center py-20 text-gray-500 text-lg">
      بازی مورد نظر یافت نشد
    </div>

    <template v-else-if="play">
      <section class="bg-gray-800 rounded-lg border border-gray-700 p-6 mb-6">
        <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-4 mb-4">
          <div>
            <h1 class="text-2xl md:text-3xl font-bold text-white">{{ play.title }}</h1>
            <p class="text-sm text-gray-400 mt-1">{{ formatDate(play.dateTime) }}</p>
          </div>
          <span
            class="inline-block self-start px-3 py-1 rounded text-sm font-medium shrink-0"
            :class="play.winnersideId === 1 ? 'bg-blue-900 text-blue-300' : 'bg-red-900 text-red-300'"
          >
            {{ play.winnersideName }}
          </span>
        </div>

        <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mb-4">
          <div>
            <span class="text-xs text-gray-500 block">سناریو</span>
            <span class="text-sm text-gray-200">{{ play.senarioName }}</span>
          </div>
      
          <div>
            <span class="text-xs text-gray-500 block">کلاب</span>
            <span class="text-sm text-gray-200">{{ play.clubName || '—' }}</span>
          </div>
          <div>
            <span class="text-xs text-gray-500 block">رویداد</span>
            <span class="text-sm text-gray-200">{{ play.eventName || '—' }}</span>
          </div>
          <div>
            <span class="text-xs text-gray-500 block">تعداد بازیکنان</span>
            <span class="text-sm text-gray-200">{{ play.players?.length || play.playersCount }}</span>
          </div>
        </div>

        <p v-if="play.desc" class="text-sm text-gray-400 leading-relaxed mb-4">{{ play.desc }}</p>

        <a
          v-if="play.link"
          :href="play.link"
          target="_blank"
          rel="noopener noreferrer"
          class="inline-flex items-center gap-2 px-4 py-2 rounded text-sm font-medium bg-red-600 text-white hover:bg-red-700 transition"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
            <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
          </svg>
          مشاهده ویدیو در یوتیوب
        </a>
      </section>

      <section v-if="play.players && play.players.length > 0" class="bg-gray-800 rounded-lg border border-gray-700 p-6 mb-6">
        <h2 class="text-lg font-semibold mb-4 text-white">بازیکنان</h2>
        <div class="overflow-x-auto">
          <table class="w-full text-sm border-collapse">
            <thead>
              <tr class="border-b border-gray-700 bg-gray-750 text-gray-300">
                <th class="px-4 py-3 text-right">ردیف</th>
                <th class="px-4 py-3 text-right">بازیکن</th>
                <th class="px-4 py-3 text-right">نقش</th>
            
                <th class="px-4 py-3 text-right">امتیاز</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(player, index) in play.players"
                :key="player.playerId"
                class="border-b border-gray-700 transition"
                :class="rowBgClass(player.winnersideId)"
              >
                <td class="px-4 py-3 text-gray-400">{{ index + 1 }}</td>
                <td class="px-4 py-3">
                  <router-link
                    :to="`/player/${player.playerId}`"
                    class="flex items-center gap-2 text-blue-400 hover:text-blue-300 transition"
                  >
                    <img
                      v-if="player.picture"
                      :src="player.picture"
                      :alt="player.playerName"
                      class="w-8 h-8 rounded-full object-cover shrink-0"
                    />
                    <div
                      v-else
                      class="w-8 h-8 rounded-full bg-gray-700 flex items-center justify-center text-xs text-gray-400 shrink-0"
                    >
                      {{ player.playerName.charAt(0) }}
                    </div>
                    <span class="font-medium">{{ player.playerName }}</span>
                  </router-link>
                </td>
                <td class="px-4 py-3 text-gray-200">{{ player.roleName }}</td>
         
                <td class="px-4 py-3 text-gray-400">{{ player.rank }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="mb-8">
        <PlayerComments entityType="play" :entityId="play.id" />
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { PlaysApi } from '@/api'
import type { PlayDetailDto } from '@/types'
import PlayerComments from '@/components/PlayerComments.vue'

const route = useRoute()
const play = ref<PlayDetailDto | null>(null)
const loading = ref(true)
const notFound = ref(false)

function formatDate(dateStr: string): string {
  try {
    return new Intl.DateTimeFormat('fa-IR', { year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

function rowBgClass(winnersideId: number): string {
  if (winnersideId === 1) return 'bg-blue-900/10'
  if (winnersideId === 2) return 'bg-red-900/10'
  return ''
}

onMounted(async () => {
  try {
    const res = await PlaysApi.getPlay(Number(route.params.id))
    play.value = res.data
  } catch {
    notFound.value = true
  } finally {
    loading.value = false
  }
})
</script>
