<template>
  <div style="background:#0d0d0f;color:#e8e4d9;width:100%;min-height:100vh;padding-bottom:3rem;font-family:'Vazirmatn',sans-serif;" dir="rtl">
  <div class="max-w-6xl mx-auto px-6">

    <div v-if="loading" class="flex justify-center items-center py-32">
      <div class="w-10 h-10 border-2 border-[#c9b07a] border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="error" class="text-center py-20" style="color:rgba(232,228,217,0.4);font-size:15px;">
      خطا در دریافت اطلاعات
    </div>

    <template v-else>
      <!-- ===== HERO ===== -->
      <section class="mp-hero">
        <p class="mp-hero-eyebrow">مرجع آمار مافیای حرفه‌ای ایران</p>
        <h1 class="mp-hero-title">اعداد <em>دروغ</em> نمی‌گویند</h1>
        <p class="mp-hero-sub">آمار واقعی بازیکنان حرفه‌ای مافیا — هر بازی، هر نقش، هر شب</p>
        <div class="mp-hero-ctas">
          <router-link to="/ranking/overall" style="text-decoration:none;">
            <button class="mp-cta-primary">مشاهده رتبه‌بندی</button>
          </router-link>
          <button class="mp-cta-secondary" @click="scrollToSearch">جستجوی بازیکن</button>
        </div>
      </section>

      <!-- ===== CLUBS ===== -->
      <section class="mp-clubs-section">
        <div class="mp-clubs-grid">
          <!-- Don Club -->
          <div class="mp-club-card mp-club-don">
            <div class="mp-club-img" :style="{ backgroundImage: `url(${donImage})` }" />
            <div class="mp-club-content">
              <span class="mp-club-badge mp-club-badge-don">Don Club</span>
              <p class="mp-club-name">دن کلاب</p>
              <div class="mp-club-stats">
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-don">{{ formatPersianNumber(stats.donclubStat?.playerCount ?? 0) }}</span>
                  <span class="mp-club-stat-label">بازیکن</span>
                </div>
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-don">{{ formatPersianNumber(stats.donclubStat?.playCount ??0) }}</span>
                  <span class="mp-club-stat-label">بازی</span>
                </div>
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-don">    %{{ formatPersianNumber(Number((stats.donclubStat?.mafiaWinRate ?? 0).toFixed(1))) }}</span>
                  <span class="mp-club-stat-label">برد مافیا</span>
                </div>
              </div>
              <router-link to="/ranking/overall?clubId=1" style="text-decoration:none;">
                <button class="mp-club-btn mp-club-btn-don">مشاهده آمار ←</button>
              </router-link>
            </div>
          </div>

          <!-- Legendary -->
          <div class="mp-club-card mp-club-legendary">
            <div class="mp-club-img" :style="{ backgroundImage: `url(${legendaryImage})` }" />
            <div class="mp-club-content">
              <span class="mp-club-badge mp-club-badge-legendary">Legendary</span>
              <p class="mp-club-name">لجندری</p>
              <div class="mp-club-stats">
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-legendary">{{ formatPersianNumber(stats.legendaryStat?.playerCount?? 0) }}</span>
                  <span class="mp-club-stat-label">بازیکن</span>
                </div>
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-legendary">{{ formatPersianNumber(stats.legendaryStat?.playCount?? 0) }}</span>
                  <span class="mp-club-stat-label">بازی</span>
                </div>
                <div class="mp-club-stat">
                  <span class="mp-club-stat-num mp-club-stat-num-legendary">%{{ formatPersianNumber(Number((stats.legendaryStat?.mafiaWinRate ?? 0).toFixed(1))) }}</span>
                  <span class="mp-club-stat-label">برد مافیا</span>
                </div>  
              </div>
              <router-link to="/ranking/overall?clubId=2" style="text-decoration:none;">
                <button class="mp-club-btn mp-club-btn-legendary">مشاهده آمار ←</button>
              </router-link>
            </div>
          </div>
        </div>
      </section>

      <!-- ===== STATS BAR ===== -->
      <div class="mp-stats-bar">
        <div class="mp-stat-item">
          <span class="mp-stat-num">{{ formatPersianNumber(stats.totalPlayers) }}</span>
          <span class="mp-stat-label">بازیکن فعال</span>
        </div>
        <div class="mp-stat-item">
          <span class="mp-stat-num">{{ formatPersianNumber(stats.totalGames) }}</span>
          <span class="mp-stat-label">بازی ثبت‌شده</span>
        </div>
        <div class="mp-stat-item">
          <span class="mp-stat-num">{{ formatPersianNumber(stats.totalSenarios) }}</span>
          <span class="mp-stat-label">سناریو</span>
        </div>
        <div class="mp-stat-item">
          <span class="mp-stat-num">{{ formatPersianNumber(stats.totalEvents) }}</span>
          <span class="mp-stat-label">رویداد</span>
        </div>
      </div>

      <!-- ===== TOP PLAYERS ===== -->
      <div class="mp-section">
        <div class="mp-section-header">
          <span class="mp-section-title">برترین بازیکنان</span>
          <router-link to="/ranking/overall" class="mp-section-link">همه رتبه‌بندی ←</router-link>
        </div>

        <div class="mp-tabs">
          <button
            v-for="tab in podiumTabs"
            :key="tab.key"
            :class="['mp-tab', { active: activePodiumTab === tab.key }]"
            @click="activePodiumTab = tab.key"
          >
            {{ tab.label }}
          </button>
        </div>

        <div class="mp-podium">
          <template v-if="currentPodium.length > 0">
            <div
              v-for="(player, idx) in currentPodium.slice(0, 3)"
              :key="player.playerId"
              :class="['mp-player-card', { first: idx === 0 }]"
              style="cursor:pointer;"
              @click="goToPlayer(player.playerId)"
            >
              <div :class="['mp-rank-badge', { gold: idx === 0 }]">
                <span v-if="idx === 0" class="rank-crown">♛</span>
                رتبه {{ formatPersianNumber(idx + 1) }}
              </div>
              <div
                class="mp-avatar"
                :style="idx === 1 ? { borderColor: 'rgba(180,180,180,0.2)', color: '#b0b0b0' } : idx === 2 ? { borderColor: 'rgba(160,100,50,0.2)', color: '#c07840' } : {}"
              >
                {{ getInitials(player.playerName) }}
              </div>
              <p class="mp-player-name">{{ player.playerName }}</p>
              <div  class="flex full">
                <span class="w1/3">
                  <p class="mp-player-wr">win rate: <strong>{{ formatWinRatePct(player.winRate) }}</strong></p>
                  <p class="mp-player-games">{{ formatPersianNumber(player.games) }} بازی</p>
                </span>
                <!-- &nbsp;
                &nbsp;
                &nbsp;
                &nbsp;&nbsp;
                <span v-if="activePodiumTab === 'overall'" class="w1/3   " >
                  <p class="mp-player-wr"> مافیایی: <strong>{{ formatWinRatePct(player.winRate) }}</strong></p>
                  <p class="mp-player-games">{{ formatPersianNumber(player.games) }} بازی</p>
                </span>
                &nbsp;
                &nbsp;
                  <span v-if="activePodiumTab === 'overall'" class="w1/3   " >
                  <p class="mp-player-wr"> شهروندی: <strong>{{ formatWinRatePct(player.winRate) }}</strong></p>
                  <p class="mp-player-games">{{ formatPersianNumber(player.games) }} بازی</p>
                </span> -->
              </div>
            </div>
          </template>
          <div v-else class="mp-podium-empty">
            داده‌ای موجود نیست
          </div>
        </div>
      </div>

      <!-- ===== RECENT GAMES ===== -->
      <div class="mp-section mp-section-recent">
        <div class="mp-section-header">
          <span class="mp-section-title">آخرین بازی‌ها</span>
          <router-link to="/plays-public" class="mp-section-link">همه بازی‌ها ←</router-link>
        </div>

        <div v-if="stats.last5Plays && stats.last5Plays.length > 0" class="mp-plays-list">
          <div v-for="play in stats.last5Plays" :key="play.id" class="mp-play-row" style="cursor:pointer;" @click="goToPlay(play.id)">
            <div :class="['mp-play-side-badge', isMafiaWin(play.winnersideName) ? 'mafia' : 'citizen']">
              {{ isMafiaWin(play.winnersideName) ? 'م' : 'ش' }}
            </div>
            <div class="mp-play-info">
              <div class="mp-play-title">{{ play.title }}</div>
              <div class="mp-play-meta">
                <span v-if="play.clubName" class="text-gray-600">{{ play.clubName }}</span>
              <span v-if="play.clubName && play.eventName" class="text-gray-600 mx-1">/</span>
              <span v-if="play.eventName" class="text-gray-600">{{ play.eventName }}</span>
                &nbsp;
                {{ play.senarioName }} ·
                {{ formatDate(play.dateTime) }}
              </div>
            </div>
            <a v-if="play.link" :href="play.link" target="_blank" rel="noopener" class="mp-play-yt" @click.stop>
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
                </svg>
            </a>
            <span v-else class="mp-play-yt" style="opacity:0.2;">▶</span>
          </div>
        </div>
        <div v-else class="mp-plays-empty">
          بازی‌ای یافت نشد
        </div>
      </div>

      <!-- ===== SEARCH ===== -->
      <div ref="searchRef" class="mp-search-section">
        <p class="mp-search-title">دنبال آمار یه بازیکن خاص می‌گردی؟</p>
        <p class="mp-search-sub">اسمش رو بنویس، همه آمارش رو ببین</p>
        <div class="mp-search-bar">
          <button class="mp-search-go" @click="handleSearch">جستجو</button>
          <input
            v-model="searchQuery"
            class="mp-search-input"
            placeholder="نام بازیکن..."
            type="text"
            @keyup.enter="handleSearch"
          />
        </div>
      </div>
    </template>
  </div>
</div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, type Ref } from 'vue'
import { useRouter } from 'vue-router'
import { getStatisticsHome } from '@/api'
import type { StatisticsHomeDto } from '@/types'
import donImage from '@/assets/images/slider/don.jpeg'
import legendaryImage from '@/assets/images/slider/legendary.jpeg'

const router = useRouter()

const loading = ref(true)
const error = ref(false)
const stats = ref<StatisticsHomeDto>({
  totalGames: 0,
  totalPlayers: 0,
  totalSenarios: 0,
  totalEvents: 0,
  citizenTop3Player: [],
  mafiaTop3Player: [],
  allTop3Player: [],
  last5Plays: [],
  donclubStat : undefined ,
  legendaryStat : undefined ,
})

const searchQuery = ref('')
const searchRef: Ref<HTMLElement | null> = ref(null)

const activePodiumTab = ref<'overall' | 'mafia' | 'citizen'>('overall')

const podiumTabs = [
  { key: 'overall' as const, label: 'کلی' },
  { key: 'mafia' as const, label: 'مافیا' },
  { key: 'citizen' as const, label: 'شهروند' },
]

interface PodiumPlayer {
  playerId: number
  playerName: string
  winRate: number
  games: number
}

const currentPodium = computed<PodiumPlayer[]>(() => {
  const tab = activePodiumTab.value
  if (tab === 'overall') {
    return (stats.value.allTop3Player || []).map(p => ({
      playerId: p.playerId,
      playerName: p.playerName,
      winRate: p.overallWinRate,
      games: p.totalGames,
    }))
  } else if (tab === 'mafia') {
    return (stats.value.mafiaTop3Player || []).map(p => ({
      playerId: p.playerId,
      playerName: p.playerName ?? '',
      winRate: p.winRate,
      games: p.games,
    }))
  } else {
    return (stats.value.citizenTop3Player || []).map(p => ({
      playerId: p.playerId,
      playerName: p.playerName ?? '',
      winRate: p.winRate,
      games: p.games,
    }))
  }
})

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/)
  if (parts.length >= 2) {
    return `${parts[0][0]}.${parts[parts.length - 1][0]}`
  }
  return name.slice(0, 2)
}

function formatPersianNumber(num: number): string {
  try {
    return new Intl.NumberFormat('fa-IR').format(num)
  } catch {
    return String(num)
  }
}

function formatWinRatePct(value: number): string {
  return `%${formatPersianNumber(Math.round(value))}`
}

function isMafiaWin(winnersideName: string): boolean {
  return winnersideName?.includes('ماف') ?? true
}

function formatDate(dateStr: string): string {
  if (!dateStr) return ''
  try {
    const date = new Date(dateStr)
    return new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(date)
  } catch {
    return dateStr
  }
}

function goToPlayer(playerId: number) {
  router.push(`/player/${playerId}`)
}

function goToPlay(playId: number) {
  router.push(`/plays/${playId}`)
}

function scrollToSearch() {
  searchRef.value?.scrollIntoView({ behavior: 'smooth' })
}

function handleSearch() {
  const q = searchQuery.value.trim()
  if (!q) return
  router.push(`/players/list?search=${encodeURIComponent(q)}`)
}

onMounted(async () => {
  try {
    const res = await getStatisticsHome()
    stats.value = res.data
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* ===== HERO ===== */
.mp-hero {
  padding: 3.5rem 2rem 2.5rem;
  text-align: center;
}
.mp-hero-eyebrow {
  font-size: 13px;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: #c9b07a;
  margin-bottom: 1rem;
}
.mp-hero-title {
  font-size: 36px;
  font-weight: 700;
  line-height: 1.2;
  margin: 0 0 0.75rem;
  color: #e8e4d9;
}
.mp-hero-title em {
  font-style: normal;
  color: #c9b07a;
}
.mp-hero-sub {
  font-size: 16px;
  color: rgba(232, 228, 217, 0.45);
  max-width: 420px;
  margin: 0 auto 2rem;
  line-height: 1.7;
}
.mp-hero-ctas {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
  flex-wrap: wrap;
}
.mp-cta-primary {
  padding: 10px 24px;
  background: #c9b07a;
  color: #0d0d0f;
  border: none;
  border-radius: 8px;
  font-weight: 700;
  font-size: 15px;
  cursor: pointer;
  font-family: inherit;
}
.mp-cta-secondary {
  padding: 10px 24px;
  background: transparent;
  color: rgba(232, 228, 217, 0.6);
  border: 0.5px solid rgba(255, 255, 255, 0.15);
  border-radius: 8px;
  font-size: 15px;
  cursor: pointer;
  font-family: inherit;
}
.mp-cta-secondary:hover {
  color: #e8e4d9;
}

/* ===== CLUBS ===== */
.mp-clubs-section {
  padding: 0 2rem 1.5rem;
}
.mp-clubs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.mp-club-card {
  border-radius: 14px;
  overflow: hidden;
  position: relative;
  cursor: pointer;
  border: 0.5px solid rgba(255, 255, 255, 0.06);
  min-height: 200px;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  background-size: cover;
  background-position: center;
}

.mp-club-img {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center top;
  filter: brightness(0.55) saturate(0.9);
  transition: filter 0.3s;
}
.mp-club-card:hover .mp-club-img {
  filter: brightness(0.7) saturate(1.1);
}

.mp-club-don::after,
.mp-club-legendary::after {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
}
.mp-club-don::after {
  background: linear-gradient(to top, rgba(120, 15, 15, 0.85) 0%, rgba(80, 10, 10, 0.5) 40%, transparent 100%);
}
.mp-club-legendary::after {
  background: linear-gradient(to top, rgba(10, 90, 80, 0.88) 0%, rgba(10, 70, 65, 0.5) 40%, transparent 100%);
}

.mp-club-content {
  position: relative;
  z-index: 2;
  padding: 1rem 1.1rem 1.1rem;
}

.mp-club-badge {
  display: inline-block;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 3px 8px;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}
.mp-club-badge-don {
  background: rgba(220, 50, 50, 0.25);
  color: #ff7070;
  border: 0.5px solid rgba(220, 50, 50, 0.3);
}
.mp-club-badge-legendary {
  background: rgba(30, 200, 170, 0.2);
  color: #3de8cc;
  border: 0.5px solid rgba(30, 200, 170, 0.3);
}

.mp-club-name {
  font-size: 18px;
  font-weight: 700;
  color: #fff;
  margin: 0 0 3px;
  line-height: 1.1;
}

.mp-club-stats {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 0.85rem;
}
.mp-club-stat {
  text-align: center;
}
.mp-club-stat-num {
  font-size: 17px;
  font-weight: 700;
  display: block;
}
.mp-club-stat-num-don { color: #ff8080; }
.mp-club-stat-num-legendary { color: #3de8cc; }
.mp-club-stat-label {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.35);
  display: block;
}

.mp-club-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 700;
  padding: 7px 14px;
  border-radius: 7px;
  border: none;
  cursor: pointer;
  font-family: inherit;
}
.mp-club-btn-don { background: rgba(220, 50, 50, 0.9); color: #fff; }
.mp-club-btn-legendary { background: rgba(30, 200, 170, 0.9); color: #0a1a17; }

/* ===== STATS BAR ===== */
.mp-stats-bar {
  display: flex;
  justify-content: center;
  gap: 3rem;
  padding: 1.5rem 2rem;
  border-top: 0.5px solid rgba(255, 255, 255, 0.06);
  border-bottom: 0.5px solid rgba(255, 255, 255, 0.06);
}
.mp-stat-item {
  text-align: center;
}
.mp-stat-num {
  font-size: 24px;
  font-weight: 700;
  color: #c9b07a;
  display: block;
}
.mp-stat-label {
  font-size: 13px;
  color: rgba(232, 228, 217, 0.35);
  margin-top: 2px;
  display: block;
}

/* ===== SECTION ===== */
.mp-section {
  padding: 2rem 2rem 0;
}
.mp-section-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 1rem;
}
.mp-section-title {
  font-size: 16px;
  font-weight: 700;
  color: rgba(232, 228, 217, 0.5);
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
.mp-section-link {
  font-size: 13px;
  color: #c9b07a;
  cursor: pointer;
  text-decoration: none;
}

/* ===== TABS ===== */
.mp-tabs {
  display: flex;
  gap: 0;
  margin-bottom: 1rem;
  border-bottom: 0.5px solid rgba(255, 255, 255, 0.08);
}
.mp-tab {
  font-size: 14px;
  padding: 6px 14px;
  color: rgba(232, 228, 217, 0.35);
  cursor: pointer;
  border-bottom: 1.5px solid transparent;
  margin-bottom: -0.5px;
  font-family: inherit;
  background: none;
  border-top: none;
  border-left: none;
  border-right: none;
  transition: color 0.15s;
}
.mp-tab.active {
  color: #c9b07a;
  border-bottom-color: #c9b07a;
}

/* ===== PODIUM ===== */
.mp-podium {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 0.75rem;
}
.mp-podium-empty {
  grid-column: 1 / -1;
  text-align: center;
  padding: 2rem;
  color: rgba(232, 228, 217, 0.3);
  font-size: 15px;
}

.mp-player-card {
  background: #141416;
  border: 0.5px solid rgba(255, 255, 255, 0.07);
  border-radius: 10px;
  padding: 1rem;
  position: relative;
  overflow: hidden;
}
.mp-player-card.first {
  border-color: rgba(201, 176, 122, 0.3);
  background: #161510;
}

.mp-rank-badge {
  font-size: 13px;
  font-weight: 700;
  color: rgba(232, 228, 217, 0.25);
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 4px;
}
.mp-rank-badge.gold { color: #c9b07a; }
.rank-crown { font-size: 12px; }

.mp-avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: #2a2820;
  border: 1.5px solid rgba(201, 176, 122, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 700;
  color: #c9b07a;
  margin-bottom: 0.6rem;
}

.mp-player-name {
  font-size: 15px;
  font-weight: 700;
  color: #e8e4d9;
  margin: 0 0 2px;
}
.mp-player-wr {
  font-size: 13px;
  color: rgba(232, 228, 217, 0.5);
  margin: 0;
}
.mp-player-wr strong {
  color: #6fcf8a;
  font-weight: 500;
}
.mp-player-games {
  font-size: 13px;
  color: rgba(232, 228, 217, 1);
  margin: 4px 0 0;
}

/* ===== RECENT PLAYS ===== */
.mp-plays-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.mp-plays-empty {
  text-align: center;
  padding: 2rem;
  color: rgba(232, 228, 217, 0.3);
  font-size: 15px;
}

.mp-play-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #141416;
  border: 0.5px solid rgba(255, 255, 255, 0.06);
  border-radius: 8px;
  padding: 0.7rem 1rem;
}

.mp-play-side-badge {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  flex-shrink: 0;
}
.mp-play-side-badge.mafia { background: rgba(180, 50, 50, 0.2); color: #e07070; }
.mp-play-side-badge.citizen { background: rgba(50, 120, 180, 0.2); color: #70a8e0; }

.mp-play-info {
  flex: 1;
  min-width: 0;
}
.mp-play-title {
  font-size: 15px;
  font-weight: 500;
  color: #e8e4d9;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.mp-play-meta {
  font-size: 13px;
  color: rgba(232, 228, 217, 0.3);
  margin-top: 1px;
}
.mp-play-yt {
  font-size: 14px;
  color: rgba(201, 176, 122, 0.5);
  cursor: pointer;
  flex-shrink: 0;
  text-decoration: none;
}
.mp-play-yt:hover {
  color: #c9b07a;
}

/* ===== SEARCH ===== */
.mp-search-section {
  margin: 2rem;
  background: #141416;
  border: 0.5px solid rgba(255, 255, 255, 0.07);
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
}
.mp-search-title {
  font-size: 16px;
  font-weight: 700;
  color: rgba(232, 228, 217, 0.7);
  margin-bottom: 0.5rem;
}
.mp-search-sub {
  font-size: 13px;
  color: rgba(232, 228, 217, 0.25);
  margin-bottom: 1rem;
}
.mp-search-bar {
  display: flex;
  gap: 0.5rem;
  max-width: 340px;
  margin: 0 auto;
  flex-direction: row-reverse;
}
.mp-search-input {
  flex: 1;
  background: #0d0d0f;
  border: 0.5px solid rgba(255, 255, 255, 0.1);
  border-radius: 7px;
  padding: 8px 12px;
  font-size: 14px;
  color: #e8e4d9;
  font-family: inherit;
  outline: none;
  text-align: right;
}
.mp-search-input:focus {
  border-color: rgba(201, 176, 122, 0.3);
}
.mp-search-go {
  padding: 8px 16px;
  background: #c9b07a;
  border: none;
  border-radius: 7px;
  color: #0d0d0f;
  font-weight: 700;
  font-size: 14px;
  cursor: pointer;
  font-family: inherit;
}

/* ===== RESPONSIVE ===== */
@media (max-width: 640px) {
  .mp-hero-title { font-size: 26px; }
  .mp-hero { padding: 2.5rem 1rem 2rem; }
  .mp-stats-bar { gap: 1rem; }
  .mp-clubs-grid { grid-template-columns: 1fr; }
  .mp-podium { grid-template-columns: 1fr; }
  .mp-section { padding: 1.5rem 1rem 0; }
  .mp-search-section { margin: 1rem; }
  .mp-search-bar { max-width: 100%; }
}
</style>
