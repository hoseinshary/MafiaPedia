# DATABASE.md — MafiaPedia Database Schema

> این فایل ساختار کامل دیتابیس MySQL پروژه MafiaPedia را توضیح می‌دهد.
> DB-First: تغییرات schema در MySQL اعمال می‌شوند، سپس با EF Core scaffold می‌شوند.
> آخرین به‌روزرسانی: بر اساس بررسی مستقیم Entities + MafiaContext.cs در ریپو — 2026-07-15

> ⚠️ **هشدار مهم:** ستون `user.role` روی MySQL واقعی همین الان `enum('admin','user','club')` است
> (مقادیر قدیمی `master`/`cafe_owner` حذف شدند)، ولی فایل scaffold‌شده‌ی `MafiaContext.cs`
> در ریپو هنوز enum قدیمی (`admin,user,master,cafe_owner`) را نشان می‌دهد.
> **باید `dotnet ef dbcontext scaffold` دوباره اجرا شود** تا Entity با DB واقعی هماهنگ شود.
> این فایل بر اساس enum واقعی (admin/user/club) نوشته شده، نه بر اساس فایل scaffold فعلی.

---

## دیاگرام کلی

```
── فاز ۱ (آمار عمومی) ──────────────────────────────────────
club ──< event ──< play >── master (ClubID)
                   │    └──> room  (ClubId)
                   │    └──> senario ──< role >── side
                   │    └──> side (winnerside)
                   │    └──> user
                   └──< playplayer >── player
                                   └──> role

── فاز ۲ (مدیریت کافه) ─────────────────────────────────────
club ──< event ──< clubplay >── master (ClubID)
      │              │      └──> room  (ClubId)
      │              │      └──> senario (مشترک)
      │              │      └──> side (مشترک، winnerside، NULL تا ثبت)
      │              │      └──> user
      │              └──< clubplayplayer >── clubplayer
      │                                  └──> role (مشترک)
      ├──< clubuser >── user            (چند-کلاب: هر user در چند club نقش دارد)
      ├──< club_clubplayer >── clubplayer
      └── master (ClubID)

clubplayer ──< masterlist_clubplayer >── masterlist >── master
senario ──< senario_role_set (ترکیب نقش‌ها به ازای PlayerCount، JSON)
user ──< comment ──< commentlike
     ──< refresh_token
     ──< subscription
     ──  player (1:1 nullable, فاز ۱)
     ──  clubplayer (1:1 nullable, فاز ۲)
     ──  master (1:1 nullable, فاز ۲ — لینک اکانت گرداننده)
```

---

## جداول مشترک (فاز ۱ و ۲)

### `club`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| name | VARCHAR(45) | نام کلاب/کافه |
| address | VARCHAR(300) | آدرس کافه |
| phone | VARCHAR(11) | شماره تلفن |
| city | VARCHAR(50) | شهر |
| description | VARCHAR(300) | توضیحات |
| logo | VARCHAR(300) | مسیر لوگوی کافه |

---

### `event`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام رویداد/مسابقه |
| ClubId | INT NOT NULL FK → club(id) | |
| IsDefault | BOOLEAN | آیا رویداد پیش‌فرض کافه است؟ |
| DefaultClubId | INT NULL — **GENERATED (STORED)** | `CASE WHEN IsDefault=1 THEN ClubId ELSE NULL END` |

> `UNIQUE KEY uq_event_default_per_club` روی `DefaultClubId` — هر کافه فقط یک Event پیش‌فرض می‌تواند داشته باشد
> (به لطف ستون محاسباتی، این constraint فقط وقتی `IsDefault=1` است اعمال می‌شود).
> برای بازی‌های روزمره‌ی کافه (فاز ۲) که `EventId` صریح نفرستند، همین Event پیش‌فرض استفاده می‌شود.

---

### `senario`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام سناریو |

---

### `senario_role_set` — ترکیب نقش‌ها برای پخش خودکار
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| SenarioId | INT NOT NULL FK → senario(Id) | |
| PlayerCount | INT NOT NULL | تعداد بازیکنان این ترکیب |
| RoleIds | JSON NOT NULL | آرایه‌ی RoleId ها به همان تعداد PlayerCount |

> `UNIQUE KEY uq_srs_senario_count` روی `(SenarioId, PlayerCount)` — هر سناریو برای هر تعداد بازیکن فقط یک ترکیب نقش دارد.
> این جدول منبع پخش خودکار نقش (`ReshuffleRolesAsync`, `CreateClubPlayAsync`) در فاز ۲ است.

---

### `role`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام نقش (دکتر، کاراگاه...) |
| SenarioId | INT NOT NULL FK → senario(Id) | |
| SideId | INT NOT NULL FK → side(Id) | |
| Photo | VARCHAR(300) | مسیر عکس نقش |

---

### `side`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام طرف (مافیا/شهروند) |

> ⚠️ مقادیر ثابت هاردکد شده در کد: **1=Mafia, 2=Citizen**

---

### `room`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام اتاق/سالن |
| ClubId | INT NOT NULL FK → club(id) | هر اتاق متعلق به یک کافه |
| IsActive | BOOLEAN NULL | فعال/غیرفعال |

---

### `master`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(50) | نام گرداننده |
| ClubID | INT NOT NULL FK → club(id) | |
| userId | INT NULL UNIQUE FK → user(Id) | لینک به اکانت کاربری |
| RatePerGame | DECIMAL(10,2) NULL | دستمزد هر بازی |
| Photo | VARCHAR(300) | مسیر عکس گرداننده |
| Bio | VARCHAR(300) | بیوگرافی |

> گرداننده می‌تواند هم در جدول `master` (موجودیت مستقل با اطلاعات دستمزد/عکس/بیو) و هم در `clubuser`
> با `clubuserRole='master'` وجود داشته باشد. این دو موازی هستند:
> `clubplay.MasterId` به `master.Id` اشاره می‌کند، نه به `clubuser`.
> وقتی یک `User` جدید به‌عنوان گرداننده لینک می‌شود، `ClubManagementService`/`ClubUserService`
> هم رکورد `master` را می‌سازند هم `clubuser` با نقش `master` را (و `user.role` را به `club` تغییر می‌دهند).

---

## فاز ۱ — جداول آمار عمومی

### `player`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(50) | نام بازیکن |
| Mobile | VARCHAR(11) UNIQUE | |
| Birthday | DATE | |
| Code | VARCHAR(10) | |
| Picture | VARCHAR(300) | مسیر: `/uploads/players/{filename}` |
| Desc | VARCHAR(300) | |
| user_id | INT NULL UNIQUE FK → user(Id) | لینک به اکانت |

---

### `play`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Title | VARCHAR(50) | |
| DateTime | DATETIME | |
| PlayersCount | INT | |
| GuestCount | INT | |
| Desc | VARCHAR(300) | |
| RoomId | INT NOT NULL FK → room(Id) | |
| SenarioId | INT NOT NULL FK → senario(Id) | |
| MasterId | INT NOT NULL FK → master(Id) | |
| WinnersideId | INT NOT NULL FK → side(Id) | |
| UserId | INT NOT NULL FK → user(Id) | ثبت‌کننده |
| EventId | INT NOT NULL FK → event(Id) | |
| Link | VARCHAR(300) | لینک YouTube |
| picture | VARCHAR(300) | |

---

### `playplayer` — مرکز آمار فاز ۱
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Rank | INT NULL | رتبه (-1=اخراج انضباطی) |
| Action | INT NULL | رویداد خاص |
| RoleId | INT NOT NULL FK → role(Id) | نقش نهایی |
| PlayerId | INT NOT NULL FK → player(Id) | |
| PlayId | INT NOT NULL FK → play(Id) | |

> **مرکز تمام محاسبات آماری فاز ۱** — win rate و رتبه‌بندی همه از اینجا محاسبه می‌شوند.

---

## فاز ۲ — جداول مدیریت کافه

### `clubplayer` — مشتریان کافه
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(50) | |
| Mobile | VARCHAR(11) NOT NULL UNIQUE | کلید یکتاسازی سراسری |
| Birthday | DATE | |
| Code | VARCHAR(10) | |
| Picture | VARCHAR(300) | |
| Desc | VARCHAR(300) | |
| user_id | INT NULL UNIQUE FK → user(Id) | لینک به اکانت (وقتی ثبت‌نام کرد) |

> `Mobile UNIQUE` در سطح کل سیستم — هر شماره موبایل فقط یک ردیف دارد (global).
> جداسازی از `player`: این جدول برای مشتریان کافه است، آمارش با رتبه‌بندی عمومی فاز ۱ قاطی نمی‌شود.
> عضویت در کافه دستی نیست — همین که یک `clubplayer` برای اولین‌بار در یک بازی کافه ثبت شود،
> به‌صورت خودکار در `club_clubplayer` عضو آن کافه می‌شود (`ClubPlayerService.CreateOrJoinAsync`).

---

### `club_clubplayer` — عضویت مشتری در کافه
| ستون | نوع | توضیح |
|---|---|---|
| clubId | INT NOT NULL FK → club(id) | کلید ترکیبی |
| clubplayerId | INT NOT NULL FK → clubplayer(Id) | کلید ترکیبی |
| JoinedAt | DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP | تاریخ عضویت |

> هر مشتری که حتی یک بازی در یک کافه داشته باشد، اینجا ثبت می‌شود (خودکار، نه دستی).
> هر کافه فقط مشتریان خودش را می‌بیند (multi-tenancy).

---

### `clubuser` — نقش کاربران در کافه
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| userId | INT NOT NULL FK → user(Id) | |
| clubId | INT NOT NULL FK → club(id) | |
| clubuserRole | ENUM NOT NULL | `cashier` \| `master` \| `supervisor` \| `owner` |

> ✅ **اصلاح شده:** `UNIQUE KEY uq_clubuser_user_club` روی ترکیب `(userId, clubId)` —
> هر کاربر می‌تواند در **چندین کافه‌ی مختلف** نقش داشته باشد (قبلاً هر کدام جداگانه UNIQUE بود که باگ بود).
> وقتی یک `clubuser` ساخته می‌شود، `user.role` هم به `club` تغییر می‌کند (`UpsertClubUserAsync`).

---

### `clubplay` — بازی‌های کافه
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Title | VARCHAR(50) | |
| DateTime | DATETIME | |
| PlayersCount | INT | |
| GuestCount | INT | |
| Desc | VARCHAR(300) | |
| RoomId | INT NOT NULL FK → room(Id) | |
| SenarioId | INT NOT NULL FK → senario(Id) | |
| MasterId | INT NOT NULL FK → master(Id) | |
| WinnersideId | INT NULL FK → side(Id) | NULL تا وقتی ثبت شود |
| UserId | INT NOT NULL FK → user(Id) | ثبت‌کننده |
| EventId | INT NOT NULL FK → event(Id) | Event فصلی کافه |
| Link | VARCHAR(300) | |
| playType | ENUM NOT NULL | `normal` \| `rank` \| `superrank` \| `etc` |
| status | ENUM NOT NULL | `pending` \| `notwinside` \| `notrank` \| `done` |
| BusinessDate | DATE NULL | «روز کاری» بازی (برای گزارش‌گیری روزانه/شیفتی — جدا از تاریخ واقعی `DateTime`) |

**مسیر تعیین clubId مالک این بازی:** از طریق `RoomId → room.ClubId` (و به‌صورت موازی/تأییدی از طریق `MasterId → master.ClubID` و `EventId → event.ClubId`). همه‌ی متدهای سرویس (`GetClubPlayDetailAsync`, `ReshuffleRolesAsync`, `SubmitWinnersideAsync`, ...) با `play.Room.ClubId != clubId` مالکیت را تأیید می‌کنند.

**چرخه‌ی status بر اساس playType:**
```
normal / etc → pending → done                          (winnerside/rank هرگز لازم نیست)
rank         → pending → notwinside → done              (فقط winnerside لازم است)
superrank    → pending → notwinside → notrank → done    (winnerside و rank هر دو لازم)
```
- `pending`: نقش‌ها پخش شده، بازی شروع نشده
- `notwinside`: بازی تمام شده، winner side هنوز ثبت نشده
- `notrank`: winner ثبت شده، رتبه‌بندی نفرات ثبت نشده (فقط superrank)
- `done`: همه‌چیز کامل

> وقتی `playType` هنگام ویرایش تغییر کند، `status` با `ClubPlayStatusResolver` بر اساس داده‌ی واقعی
> (`WinnersideId`, `Clubplayplayer.Rank`) از نو محاسبه می‌شود — نه بر اساس نگاشت ثابت.
> فرستادن `status` صریح در درخواست ویرایش که با مقدار محاسبه‌شده فرق کند → خطای 400.

---

### `clubplayplayer` — بازیکنان در بازی‌های کافه
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Rank | INT NULL | رتبه، فقط برای superrank |
| Action | INT NULL | رویداد خاص |
| RoleId | INT NOT NULL FK → role(Id) | نقش نهایی |
| PlayerId | INT NOT NULL FK → clubplayer(Id) | |
| PlayId | INT NOT NULL FK → clubplay(Id) | |
| IsGuest | BOOLEAN NOT NULL DEFAULT 0 | آیا بازیکن مهمان است؟ (در آمار جدا شمرده می‌شود) |

---

### `masterlist` — لیست‌های شخصی گرداننده
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| name | VARCHAR(45) | نام لیست |
| masteId | INT NOT NULL FK → master(Id) | گرداننده‌ی صاحب لیست |

> ⚠️ تایپو در نام ستون: `masteId` (باید `masterId` باشد) — دستی تغییر نده، همینطور scaffold می‌شود.

---

### `masterlist_clubplayer` — اعضای لیست گرداننده
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| clubplayerId | INT NOT NULL FK → clubplayer(Id) | |
| masterlistId | INT NOT NULL FK → masterlist(id) | |

---

## جداول Auth و User

### `user`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Username | VARCHAR(20) | |
| Password | VARCHAR(20) | ⚠️ deprecated — استفاده نشود |
| password_hash | VARCHAR(255) NOT NULL | BCrypt hash |
| password_salt | VARCHAR(255) | استفاده نمی‌شود (BCrypt داخلی) |
| role | **ENUM('admin','user','club')** | ⚠️ روی MySQL واقعی به‌روز است؛ `MafiaContext.cs` هنوز enum قدیمی (`admin,user,master,cafe_owner`) دارد — نیاز به rescaffold |
| is_active | BOOLEAN | |
| mobile | VARCHAR(11) NOT NULL UNIQUE | |
| mobile_verified | BOOLEAN | |
| otp_code | VARCHAR(6) NULL | |
| otp_expires_at | DATETIME NULL | |
| display_name | VARCHAR(50) | |
| created_at | DATETIME | |
| last_login_at | DATETIME NULL | |

> نقش per-club (owner/cashier/master/supervisor) در `clubuser` ذخیره می‌شود.
> `user.role` فقط سه حالت دارد: `admin` (ادمین کل)، `user` (کاربر عادی/بازیکن فاز ۱)، `club` (هر کاربری که حداقل یک رکورد `clubuser` دارد).
> JWT claim نقش مستقیماً از همین ستون خوانده می‌شود (`AuthService.cs`) — یعنی هر عملیات Phase 2 که نیاز به `[Authorize(Policy="ClubOnly")]` دارد، فقط با `role=club` عبور می‌کند.

---

### `refresh_tokens`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| user_id | INT NOT NULL FK → user(Id) CASCADE | |
| token | VARCHAR(500) | GUID تصادفی |
| expires_at | DATETIME | انقضا (30 روز) |
| created_at | DATETIME | |
| is_revoked | BOOLEAN | |

---

### `comments`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| UserId | INT NOT NULL FK → user(Id) | |
| EntityType | VARCHAR(50) | `player` یا `play` |
| EntityId | BIGINT | |
| ParentCommentId | INT NULL FK → comments(Id) CASCADE | |
| Content | TEXT NOT NULL | |
| IsDeleted | BOOLEAN | soft delete |
| IsHidden | BOOLEAN | |
| CreatedAt | DATETIME | |
| UpdatedAt | DATETIME NULL | |

---

### `commentlikes`
| ستون | نوع | توضیح |
|---|---|---|
| CommentId | INT PK+FK → comments(Id) CASCADE | کلید ترکیبی |
| UserId | INT PK+FK → user(Id) CASCADE | کلید ترکیبی |
| CreatedAt | DATETIME | |

---

### `subscriptions`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| user_id | INT NOT NULL FK → user(Id) | |
| plan | ENUM | `player_basic` \| `player_pro` \| `cafe_basic` \| `cafe_pro` |
| started_at | DATETIME NOT NULL | |
| expires_at | DATETIME NOT NULL | |
| is_active | BOOLEAN | |

---

## نکات مهم برای AI و OpenCode

- **DB-First** — Entities را دستی تغییر نده، فقط scaffold کن
- ⚠️ **rescaffold معوق:** `user.role` روی MySQL واقعی `admin/user/club` است ولی `MafiaContext.cs` قدیمی مونده — قبل از هر تغییر روی `User` entity، `dotnet ef dbcontext scaffold` رو دوباره اجرا کن
- **Side IDs هاردکد** — 1=Mafia, 2=Citizen در RankingService (فاز ۱)
- **نقش نهایی** — در Playplayer و Clubplayplayer نقش نهایی ثبت می‌شود
- **عکس‌ها** — بازیکنان در `wwwroot/uploads/players/`؛ مسیر نسبی در DB ذخیره می‌شود (همین الگو برای Club.Logo، Master.Photo، Role.Photo، Clubplayer.Picture هم صدق می‌کند)
- **Comment depth** — فقط یک سطح reply
- **جداسازی فاز ۱ و ۲** — `play`/`playplayer`/`player` هرگز با `clubplay`/`clubplayplayer`/`clubplayer` مخلوط نشوند
- **masterlist.masteId** — تایپو در نام ستون (باید `masterId` باشد) — دستی تغییر نده، همینطور scaffold می‌شود
- **clubuser constraint** — ✅ اصلاح شده، composite unique روی `(userId, clubId)`
- **senario_role_set** — منبع پخش خودکار نقش؛ اگر برای یک `(SenarioId, PlayerCount)` ترکیبی تعریف نشده باشد، ثبت بازی با خطا رد می‌شود
- **event.DefaultClubId** — ستون محاسباتی (GENERATED STORED)، فقط برای اطمینان از یک Event پیش‌فرض در هر کافه؛ دستی ست نکن
- **clubplay ownership** — منبع واحد حقیقت برای تشخیص clubId مالک یک `clubplay`، `Room.ClubId` است (نه Event، نه ستون مستقل) — همه‌جا همین الگو استفاده شده
- **clubplay.status پس از تغییر playType** — با `ClubPlayStatusResolver` بر اساس داده‌ی واقعی محاسبه می‌شود، نه نگاشت ثابت
