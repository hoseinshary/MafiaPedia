import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import DefaultLayout from '@/layouts/DefaultLayout.vue'

const routes: RouteRecordRaw[] = [
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
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
