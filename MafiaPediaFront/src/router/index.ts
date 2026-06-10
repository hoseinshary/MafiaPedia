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
        path: 'players/create',
        name: 'CreatePlayer',
        component: () => import('@/pages/CreatePlayerPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'plays',
        name: 'PlaysList',
        component: () => import('@/pages/PlaysListPage.vue'),
      },
      {
        path: 'plays/:id/edit',
        name: 'EditPlay',
        component: () => import('@/pages/EditPlayPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'players/list',
        name: 'PlayersList',
        component: () => import('@/pages/PlayersListPage.vue'),
      },
      {
        path: 'players/:id/edit',
        name: 'EditPlayer',
        component: () => import('@/pages/EditPlayerPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/users',
        name: 'UsersList',
        component: () => import('@/pages/UsersListPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/users/:id/edit',
        name: 'EditUser',
        component: () => import('@/pages/EditUserPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
    ],
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
  next()
})

export default router
