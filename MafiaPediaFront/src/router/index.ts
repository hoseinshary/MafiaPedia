import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import DefaultLayout from '@/layouts/DefaultLayout.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/pages/LoginPage.vue'),
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/pages/RegisterPage.vue'),
  },
  {
    path: '/',
    component: DefaultLayout,
    children: [
      {
        path: '',
        name: 'Home',
        component: () => import('@/pages/HomePage.vue'),
      },
      {
        path: 'ranking/overall',
        name: 'OverallRanking',
        component: () => import('@/pages/OverallRankingPage.vue'),
      },
      {
        path: 'ranking/citizen',
        name: 'CitizenRanking',
        component: () => import('@/pages/CitizenRankingPage.vue'),
      },
      {
        path: 'ranking/mafia',
        name: 'MafiaRanking',
        component: () => import('@/pages/MafiaRankingPage.vue'),
      },
      {
        path: 'player/:id',
        name: 'PlayerProfile',
        component: () => import('@/pages/PlayerProfilePage.vue'),
      },
      {
        path: 'plays/create',
        name: 'CreatePlay',
        component: () => import('@/pages/CreatePlayPage.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'admin/players/create',
        name: 'CreatePlayer',
        component: () => import('@/pages/admin/CreatePlayerPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'plays-public',
        name: 'PlaysPublic',
        component: () => import('@/pages/PlaysPage.vue'),
      },
      {
        path: 'plays',
        name: 'PlaysList',
        component: () => import('@/pages/PlaysListPage.vue'),
      },
      {
        path: 'plays/:id',
        name: 'PlayDetail',
        component: () => import('@/pages/PlayDetailPage.vue'),
      },
      {
        path: 'admin/plays/:id/edit',
        name: 'EditPlay',
        component: () => import('@/pages/admin/EditPlayPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'statistics',
        name: 'Statistics',
        component: () => import('@/pages/StatisticsPage.vue'),
      },
      {
        path: 'head-to-head',
        name: 'HeadToHead',
        component: () => import('@/pages/HeadToHeadPage.vue'),
      },
      {
        path: 'players/list',
        name: 'PlayersList',
        component: () => import('@/pages/PlayersListPage.vue'),
      },
      {
        path: 'admin/players/:id/edit',
        name: 'EditPlayer',
        component: () => import('@/pages/admin/EditPlayerPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/users',
        name: 'UsersList',
        component: () => import('@/pages/admin/UsersListPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/users/:id/edit',
        name: 'EditUser',
        component: () => import('@/pages/admin/EditUserPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/clubs',
        name: 'ClubsList',
        component: () => import('@/pages/admin/ClubsListPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/clubs/create',
        name: 'ClubCreate',
        component: () => import('@/pages/admin/ClubCreateWizard.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/clubs/:id',
        name: 'ClubDetail',
        component: () => import('@/pages/admin/ClubDetailPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'master/plays/create',
        name: 'MasterCreatePlay',
        component: () => import('@/pages/master/CreateClubPlayPage.vue'),
        meta: { requiresAuth: true, requiresMaster: true },
      },
      {
        path: 'master/plays/reveal/:clubId?/:playId?',
        name: 'MasterPlayReveal',
        component: () => import('@/pages/master/ClubPlayRevealPage.vue'),
        meta: { requiresAuth: true, requiresMaster: true },
      },
      {
        path: 'master/plays/practice',
        name: 'MasterPlayPractice',
        component: () => import('@/pages/master/PracticeRevealPage.vue'),
        meta: { requiresAuth: true, requiresMasterOrAdmin: true },
      },
      {
        path: 'master',
        name: 'MasterDashboard',
        component: () => import('@/pages/master/MasterDashboardPage.vue'),
        meta: { requiresAuth: true, requiresMaster: true },
      },
      {
        path: 'master/plays',
        name: 'MasterPlaysList',
        component: () => import('@/pages/master/MasterPlaysListPage.vue'),
        meta: { requiresAuth: true, requiresMaster: true },
      },
      {
        path: 'master/plays/:id',
        name: 'MasterPlayDetail',
        component: () => import('@/pages/master/MasterPlayDetailPage.vue'),
        meta: { requiresAuth: true, requiresMaster: true },
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/pages/NotFoundPage.vue'),
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      next({ name: 'Login', query: { redirect: to.fullPath } })
      return
    }
  }
  if (to.meta.requiresAdmin) {
    if (!authStore.isAdmin) {
      next({ name: 'Home' })
      return
    }
  }
  if (to.meta.requiresMaster) {
    if (!authStore.isMaster) {
      next({ name: 'Home' })
      return
    }
  }
  if (to.meta.requiresMasterOrAdmin) {
    if (!authStore.isMaster && !authStore.isAdmin) {
      next({ name: 'Home' })
      return
    }
  }
  next()
})

export default router
