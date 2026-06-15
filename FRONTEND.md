# FRONTEND.md — MafiaPedia Frontend

Stack: Vue 3 + TypeScript + Pinia + Vue Router 4 + Tailwind CSS 3 + Axios + Vite
تم: Dark cinematic، RTL، فارسی

---

## Router — مسیرها

| Path | Component | Auth |
|---|---|---|
| `/` | `HomePage` | ❌ |
| `/login` | `LoginPage` | ❌ |
| `/register` | `RegisterPage` | ❌ |
| `/ranking/citizen` | `CitizenRankingPage` | ❌ |
| `/ranking/mafia` | `MafiaRankingPage` | ❌ |
| `/player/:id` | `PlayerProfilePage` | ❌ |
| `/plays-public` | `PlaysPage` | ❌ |
| `/plays/:id` | `PlayDetailPage` | ❌ |
| `/plays/create` | `CreatePlayPage` | Login |
| `/plays/:id/edit` | `EditPlayPage` | Admin |
| `/players/list` | `PlayersListPage` | ❌ |
| `/players/create` | `CreatePlayerPage` | Admin |
| `/players/:id/edit` | `EditPlayerPage` | Admin |
| `/admin/users` | `UsersListPage` | Admin |
| `/admin/users/:id/edit` | `EditUserPage` | Admin |

**Route meta:** `{ requiresAuth: true }` یا `{ requiresAuth: true, requiresAdmin: true }`

---

## Stores (Pinia)

### authStore — `src/stores/authStore.ts`
```typescript
// state
accessToken: string  // در localStorage
refreshToken: string // در localStorage
displayName: string  // در localStorage

// computed
role: string          // از JWT decode (نه localStorage)
isAuthenticated: bool
isAdmin: bool         // role === 'admin'
userId: number | null // از JWT claim NameIdentifier

// actions
login(mobile, password)
register(username, mobile, password)
logout()
refreshAccessToken() → string
```

---

## API Layer — `src/api/`

| فایل | توابع مهم |
|---|---|
| `apiClient.ts` | Axios instance، JWT interceptor، auto-refresh (5 دقیقه) |
| `AuthApi.ts` | login, register, refresh |
| `PlayerApi.ts` | getPlayers, getPlayer, getPlayerDetail, createPlayer, updatePlayer, deletePlayer, searchPlayers |
| `PlaysApi.ts` | getPlays, getPlay, createPlay, updatePlay, deletePlay |
| `CommentApi.ts` | getComments, addComment, deleteComment, toggleLike |
| `RankingApi.ts` | getOverallRanking, getSideRanking |
| `LookupApi.ts` | getDropdown |
| `userApi.ts` | getUsers, getUserDetail, createUser, updateUser, deleteUser |

**مهم:** در `apiClient.ts` هیچ default `Content-Type` set نشده — axios خودش handle می‌کند.

---

## Components

| کامپوننت | props | کاربرد |
|---|---|---|
| `AdminSidebar.vue` | — | فقط admin، fixed RTL، لینک‌های مدیریت |
| `ConfirmModal.vue` | isOpen, title, message, confirmText, cancelText, isLoading | تایید حذف |
| `PlayerSearchAutocomplete.vue` | — | جستجوی live بازیکن (debounced) |
| `PlayerComments.vue` | entityType, entityId | کامنت + reply + لایک + حذف (هر entityType) |
| `PlayerStatisticsCard.vue` | — | کارت آمار کلی بازیکن |
| `RecentGamesTable.vue` | — | جدول آخرین بازی‌ها |
| `RolesTable.vue` | — | عملکرد بر اساس نقش |

---

## نکات مهم

- **PlayerComments** از هر entityType پشتیبانی می‌کند: `entityType="player"` یا `entityType="play"`
- **ConfirmModal** در همه صفحات حذف استفاده می‌شود — همیشه از همین کامپوننت استفاده کن
- **PlayerSearchAutocomplete** در CreatePlay و EditPlay استفاده می‌شود
- **AdminSidebar** در DefaultLayout رندر می‌شود، فقط اگه `isAdmin` باشد
- **PlaysListPage** صفحه مدیریت admin است — با PlaysPage (عمومی) اشتباه نگیر
- **Initials Avatar** — بازیکنان بدون عکس باید حرف اول اسم نشان دهند (هنوز پیاده نشده)
