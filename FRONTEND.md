# FRONTEND.md — MafiaPedia Frontend

Stack: Vue 3 + TypeScript + Pinia + Vue Router 4 + Tailwind CSS 3 + Axios + Vite
تم: Dark cinematic، RTL، فارسی

> آخرین به‌روزرسانی: 2026-07-15 — بر اساس بررسی مستقیم `router/index.ts` و `src/`.
> نسخه‌ی قبلی این فایل فقط فاز ۱ را پوشش می‌داد؛ فرانت فاز ۲ (کافه، گرداننده، owner/supervisor/cashier) عملاً کامل ساخته شده.

---

## Router — مسیرها

### فاز ۱ (عمومی)
| Path | Component | Auth |
|---|---|---|
| `/` | `HomePage` | ❌ |
| `/login` | `LoginPage` | ❌ |
| `/register` | `RegisterPage` | ❌ |
| `/ranking/overall` | `OverallRankingPage` | ❌ |
| `/ranking/citizen` | `CitizenRankingPage` | ❌ |
| `/ranking/mafia` | `MafiaRankingPage` | ❌ |
| `/player/:id` | `PlayerProfilePage` | ❌ |
| `/plays-public` | `PlaysPage` | ❌ |
| `/plays` | `PlaysListPage` (مدیریت — با `PlaysPage` عمومی اشتباه نگیر) | ❌* |
| `/plays/:id` | `PlayDetailPage` | ❌ |
| `/plays/create` | `CreatePlayPage` | Login |
| `/statistics` | `StatisticsPage` | ❌ |
| `/head-to-head` | `HeadToHeadPage` | ❌ |
| `/players/list` | `PlayersListPage` | ❌ |
| `/account/profile` | `AccountProfilePage` | Login |
| `/account/password` | `AccountPasswordPage` | Login |
| `*` (هر مسیر نامعتبر) | `NotFoundPage` | ❌ |

### فاز ۱ — Admin
| Path | Component | Auth |
|---|---|---|
| `/admin/players/create` | `CreatePlayerPage` | Admin |
| `/admin/players/:id/edit` | `EditPlayerPage` | Admin |
| `/admin/plays/:id/edit` | `EditPlayPage` | Admin |
| `/admin/users` | `UsersListPage` | Admin |
| `/admin/users/:id/edit` | `EditUserPage` | Admin |

### فاز ۲ — Admin (مدیریت کافه‌ها)
| Path | Component | Auth |
|---|---|---|
| `/admin/clubs` | `ClubsListPage` | Admin |
| `/admin/clubs/create` | `ClubCreateWizard` | Admin |
| `/admin/clubs/:id` | `ClubDetailPage` | Admin |
| `/admin/clubs/:id/members` | `AdminClubMembersPage` | Admin |

### فاز ۲ — انتخاب Context کافه
| Path | Component | Auth |
|---|---|---|
| `/select-club` | `SelectClubPage` | Login — وقتی کاربر عضو چند کافه‌ست، اینجا کافه‌ی فعال انتخاب می‌شود |

### فاز ۲ — Master
| Path | Component | Auth (`requiresClubRoles`) |
|---|---|---|
| `/master` | `MasterDashboardPage` | `master` |
| `/master/plays` | `MasterPlaysListPage` | `master` |
| `/master/plays/create` | `CreateClubPlayPage` | `master, owner, supervisor, cashier` |
| `/master/plays/:id` | `MasterPlayDetailPage` | `master, owner, supervisor, cashier` |
| `/master/plays/reveal/:clubId?/:playId?` | `ClubPlayRevealPage` | `master, owner, supervisor, cashier` |
| `/master/plays/practice` | `PracticeRevealPage` | `master` (تمرین پخش نقش بدون بازی واقعی) |

### فاز ۲ — Owner / Supervisor / Cashier
| Path | Component | Auth (`requiresClubRoles`) |
|---|---|---|
| `/owner` | `OwnerDashboardPage` | `owner` |
| `/owner/members` | `OwnerMembersPage` | `owner` |
| `/supervisor` | `SupervisorDashboardPage` | `supervisor` |
| `/cashier` | `CashierDashboardPage` | `cashier` |

**Route meta:**
- `{ requiresAuth: true }` — نیاز به لاگین
- `{ requiresAuth: true, requiresAdmin: true }` — فقط `role === 'admin'`
- `{ requiresAuth: true, requiresClubRoles: [...] }` — نیاز به `activeClubRole` مطابق لیست (نقش per-club کاربر در کافه‌ی فعال، نه `user.role` کلی)

> Router guard (`router/index.ts` → `beforeEach`) اول `authStore.loadClubContexts()` را صدا می‌زند (اگه هنوز لود نشده) تا `activeClubRole` قبل از چک `requiresClubRoles` آماده باشد.

---

## Stores (Pinia)

### authStore — `src/stores/authStore.ts`
```typescript
// state
accessToken, refreshToken, displayName   // در localStorage یا sessionStorage (بر اساس rememberMe)

// computed
role: string              // از JWT decode — 'admin' | 'user' | 'club'
isAuthenticated: bool
isAdmin: bool              // role === 'admin'
isClub: bool               // role === 'club'
userId: number | null

// چندکلاب — Context فعال
clubContexts: ClubUserContextDto[]   // همه‌ی کلاب‌هایی که کاربر توشون عضویت clubuser داره
clubContextsLoaded: bool
activeClubId: number | null          // در localStorage
activeClubContext: ClubUserContextDto | null
activeClubRole: string               // clubuserRole در کلاب فعال — 'master' | 'owner' | 'supervisor' | 'cashier'
isMaster / isOwner / isSupervisor / isCashier: bool   // بر اساس activeClubRole

// actions
login(mobile, password, rememberMe?)
register(username, mobile, password)
logout()
refreshAccessToken() → string
loadClubContexts()     // GET /api/clubusers/me → پر کردن clubContexts؛ اگه فقط یک کافه داشت، خودکار activeClub ست می‌شود
setActiveClub(clubId)
```

> نکته‌ی مهم: `role` (از JWT) فقط سه مقدار کلی (`admin`/`user`/`club`) دارد. نقش دقیق کاربر در یک کافه‌ی خاص (`master`/`owner`/`supervisor`/`cashier`) از `activeClubRole` می‌آید که بعد از انتخاب کافه (یا خودکار اگه فقط یک کافه داشت) پر می‌شود. همیشه از `activeClubRole` برای نمایش/مسیریابی UI فاز ۲ استفاده کن، نه از `role`.

### playerStore — `src/stores/playerStore.ts`
لیست بازیکنان رتبه‌بندی + پروفایل جاری (فاز ۱).

### themeStore — `src/stores/themeStore.ts`
حالت dark/light، پیش‌فرض dark، در localStorage ذخیره می‌شود.

---

## API Layer — `src/api/`

| فایل | توابع مهم |
|---|---|
| `apiClient.ts` | Axios instance، JWT interceptor، auto-refresh (۵ دقیقه) |
| `AuthApi.ts` | login, register, refresh |
| `AccountApi.ts` | getMyAccount, updateMyAccount, changePassword, uploadLinkedPicture |
| `PlayerApi.ts` | getPlayers, getPlayer, getPlayerDetail, createPlayer, updatePlayer, deletePlayer, searchPlayers |
| `PlaysApi.ts` | getPlays, getPlay, createPlay, updatePlay, deletePlay |
| `CommentApi.ts` | getComments, addComment, deleteComment, toggleLike |
| `RankingApi.ts` | getOverallRanking, getSideRanking |
| `StatisticsApi.ts` | getStatistics, getStatisticsHome |
| `LookupApi.ts` | getDropdown |
| `userApi.ts` | getUsers, getUserDetail, createUser, updateUser, deleteUser |
| `ClubApi.ts` | CRUD کلاب (admin) + room + master (تماس با `ClubManagementController`) |
| `ClubPlayerApi.ts` | CRUD مشتری کافه (تماس با `ClubPlayerController`) |
| `ClubPlayApi.ts` | چرخه‌ی کامل بازی کافه: create, detail, reshuffle, confirm-reveal, submit-winnerside, submit-ranks, update, replace-participant, لیست‌های by-date/mine/open، آمار (تماس با `ClubPlayController`) |
| `ClubUserApi.ts` | لیست/ساخت/ویرایش/حذف عضو کافه + `getMyClubs()` (`GET /api/clubusers/me`) |
| `MasterApi.ts` | `GET /api/masters/me` — context گرداننده‌ی کاربر لاگین‌شده |

**مهم:** در `apiClient.ts` هیچ default `Content-Type` set نشده — axios خودش handle می‌کند.

---

## Components

### فاز ۱
| کامپوننت | کاربرد |
|---|---|
| `ConfirmModal.vue` | تایید حذف — در همه‌ی صفحات حذف استفاده می‌شود |
| `PlayerSearchAutocomplete.vue` | جستجوی live بازیکن (debounced) — در CreatePlay/EditPlay |
| `PlayerComments.vue` / `CommentItem.vue` | کامنت + reply + لایک + حذف (پشتیبانی از `entityType="player"` یا `"play"`) |
| `PlayerStatisticsCard.vue`, `RecentGamesTable.vue`, `RolesTable.vue`, `WinRateTable.vue` | کارت‌ها و جدول‌های آماری پروفایل بازیکن |
| `ToastContainer.vue` | نوتیفیکیشن سراسری (با `useToast` composable) |

### فاز ۲ — `components/club/`
| کامپوننت | کاربرد |
|---|---|
| `ClubMemberForm.vue`, `ClubMembersManager.vue` | ساخت/ویرایش/لیست اعضای کافه (owner/supervisor/cashier/master) |
| `NewMasterUserModal.vue`, `AddMasterProfileModal.vue`, `MasterForm.vue` | ساخت/لینک گرداننده — الگوی two-tab (یوزر موجود / یوزر جدید) |
| `CustomerForm.vue` | فرم مشتری کافه |
| `RoomForm.vue` | فرم اتاق/سالن |
| `ClubStatsCard.vue`, `ClubTodaysPlaysCard.vue`, `MasterPerformanceTable.vue` | داشبوردهای آماری کافه |

### فاز ۲ — `components/master/`
| کامپوننت | کاربرد |
|---|---|
| `ClubPlayForm.vue` | فرم ثبت/ویرایش بازی کافه |
| `ParticipantPicker.vue` | انتخاب شرکت‌کنندگان از بین مشتریان کافه |

### فاز ۲ — `components/clubplay/`
| کامپوننت | کاربرد |
|---|---|
| `RoleRevealStepper.vue` | فلوی گام‌به‌گام پخش/نمایش نقش هنگام شروع بازی |

### مشترک — `components/shared/`
| کامپوننت | کاربرد |
|---|---|
| `Modal.vue` | مودال پایه (پایه‌ی همه‌ی مودال‌های دیگر) |
| `NavDrawer.vue` | منوی کناری — شامل لینک‌های admin و لینک‌های نقش فعال کافه (master/owner/supervisor/cashier) |
| `UserMenu.vue` | منوی کاربر (پروفایل، خروج، انتخاب کافه اگه چندتا داشته باشه) |
| `LinkedPictureSection.vue` | آپلود/نمایش عکس (استفاده مشترک برای عکس بازیکن، گرداننده، مشتری، لوگوی کافه) |

---

## نکات مهم

- **PlayerComments** از هر entityType پشتیبانی می‌کند: `entityType="player"` یا `entityType="play"`
- **ConfirmModal** در همه صفحات حذف استفاده می‌شود — همیشه از همین کامپوننت استفاده کن
- **PlayerSearchAutocomplete** در CreatePlay و EditPlay استفاده می‌شود (فاز ۱ — نه فاز ۲؛ فاز ۲ از `ParticipantPicker` استفاده می‌کند)
- **PlaysListPage** صفحه مدیریت است — با `PlaysPage` (عمومی) اشتباه نگیر
- **DefaultLayout** شامل `NavDrawer` است که بر اساس `authStore.isAdmin` و `authStore.activeClubRole` لینک‌های مربوطه را نشون می‌دهد
- **چندکلاب:** اگر کاربر در چند کافه نقش داشته باشد، بعد از لاگین به `/select-club` هدایت می‌شود تا `activeClubId` را انتخاب کند؛ همه‌ی مسیرهای فاز ۲ بر اساس `activeClubRole` (نه `role` کلی) محافظت می‌شوند
- **پخش نقش:** `RoleRevealStepper` + صفحات `ClubPlayRevealPage`/`PracticeRevealPage` — پخش واقعی نقش سمت سرور انجام می‌شود (فرانت فقط نمایش می‌دهد)
- **Initials Avatar** — هنوز مشخص نیست پیاده شده یا نه؛ نیاز به بررسی مجدد دارد (در نسخه‌ی قبلی این فایل «هنوز پیاده نشده» ذکر شده بود ولی تأیید نشده)
