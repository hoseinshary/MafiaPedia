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
        path: 'admin/clubs/:id/members',
        name: 'AdminClubMembers',
        component: () => import('@/pages/admin/AdminClubMembersPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'admin/clubs/:id/deleted-plays',
        name: 'AdminDeletedPlays',
        component: () => import('@/pages/owner/DeletedPlaysPage.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'select-club',
        name: 'SelectClub',
        component: () => import('@/pages/SelectClubPage.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'master/plays/create',
        name: 'MasterCreatePlay',
        component: () => import('@/pages/master/CreateClubPlayPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master', 'owner', 'supervisor', 'cashier'] },
      },
      {
        path: 'master/plays/reveal/:clubId?/:playId?',
        name: 'MasterPlayReveal',
        component: () => import('@/pages/master/ClubPlayRevealPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master', 'owner', 'supervisor', 'cashier'] },
      },
      {
        path: 'master/plays/practice',
        name: 'MasterPlayPractice',
        component: () => import('@/pages/master/PracticeRevealPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master'] },
      },
      {
        path: 'master',
        name: 'MasterDashboard',
        component: () => import('@/pages/master/MasterDashboardPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master'] },
      },
      {
        path: 'master/plays',
        name: 'MasterPlaysList',
        component: () => import('@/pages/master/MasterPlaysListPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master'] },
      },
      {
        path: 'master/plays/:id',
        name: 'MasterPlayDetail',
        component: () => import('@/pages/master/MasterPlayDetailPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['master', 'owner', 'supervisor', 'cashier'] },
      },
      {
        path: 'owner',
        name: 'OwnerDashboard',
        component: () => import('@/pages/owner/OwnerDashboardPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner'] },
      },
      {
        path: 'owner/members',
        name: 'OwnerMembers',
        component: () => import('@/pages/owner/OwnerMembersPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner'] },
      },
      {
        path: 'owner/deleted-plays',
        name: 'OwnerDeletedPlays',
        component: () => import('@/pages/owner/DeletedPlaysPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner'] },
      },
      {
        path: 'owner/nerkh',
        name: 'OwnerNerkh',
        component: () => import('@/pages/owner/NerkhManagementPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'owner/products',
        name: 'OwnerProducts',
        component: () => import('@/pages/owner/ProductManagementPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'owner/finance-audit',
        name: 'OwnerFinanceAudit',
        component: () => import('@/pages/owner/FinanceAuditPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner'] },
      },
      {
        path: 'supervisor',
        name: 'SupervisorDashboard',
        component: () => import('@/pages/supervisor/SupervisorDashboardPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['supervisor'] },
      },
      {
        path: 'cashier',
        name: 'CashierDashboard',
        component: () => import('@/pages/cashier/CashierDashboardPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['cashier'] },
      },
      {
        path: 'finance/order/:clubPlayerId?/:orderId?',
        name: 'FinanceOrder',
        component: () => import('@/pages/ClubOrderBuilderPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier', 'supervisor', 'master'] },
      },
      {
        path: 'finance/settlement/:clubPlayerId?',
        name: 'FinanceSettlement',
        component: () => import('@/pages/cashier/ClubSettlementPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'finance/today',
        name: 'FinanceToday',
        component: () => import('@/pages/TodayAccountsPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier', 'supervisor'] },
      },
      {
        path: 'finance/debtors',
        name: 'FinanceDebtors',
        component: () => import('@/pages/DebtorsListPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'finance/ledger/:playerId?',
        name: 'FinanceLedger',
        component: () => import('@/pages/cashier/ClubPlayerLedgerPage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'finance/invoices',
        name: 'FinanceInvoiceArchive',
        component: () => import('@/pages/cashier/InvoiceArchivePage.vue'),
        meta: { requiresAuth: true, requiresClubRoles: ['owner', 'cashier'] },
      },
      {
        path: 'cashier/order',
        redirect: to => ({ path: '/finance/order', query: to.query }),
      },
      {
        path: 'cashier/settlement',
        redirect: to => ({ path: '/finance/settlement', query: to.query }),
      },
      {
        path: 'cashier/ledger/:playerId?',
        redirect: to => ({ path: `/finance/ledger${to.params.playerId ? '/' + to.params.playerId : ''}`, query: to.query }),
      },
      {
        path: 'account/profile',
        component: () => import('@/pages/AccountProfilePage.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'account/password',
        component: () => import('@/pages/AccountPasswordPage.vue'),
        meta: { requiresAuth: true },
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

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore()

  await authStore.authReady

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
    return
  }

  if (authStore.isAuthenticated && !authStore.clubContextsLoaded) {
    await authStore.loadClubContexts()
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'Home' })
    return
  }

  const requiredClubRoles = to.meta.requiresClubRoles as string[] | undefined
  if (requiredClubRoles && !requiredClubRoles.includes(authStore.activeClubRole)) {
    next({ name: 'Home' })
    return
  }

  next()
})

export default router
