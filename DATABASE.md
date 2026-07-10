# DATABASE.md — MafiaPedia Database Schema

> این فایل ساختار کامل دیتابیس MySQL پروژه MafiaPedia را توضیح می‌دهد.
> DB-First: تغییرات schema در MySQL اعمال می‌شوند، سپس با EF Core scaffold می‌شوند.
> آخرین به‌روزرسانی: بر اساس dump تاریخ 2026-07-04

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
      │              │      └──> side (مشترک، winnerside)
      │              │      └──> user
      │              └──< clubplayplayer >── clubplayer
      │                                  └──> role (مشترک)
      ├──< clubuser >── user
      ├──< club_clubplayer >── clubplayer
      └── master (ClubID)

clubplayer ──< masterlist_clubplayer >── masterlist >── master
user ──< comment ──< commentlike
     ──< refresh_token
     ──< subscription
     ──  player (1:1 nullable, فاز ۱)
     ──  clubplayer (1:1 nullable, فاز ۲)
```

---

## جداول مشترک (فاز ۱ و ۲)

### `club`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| name | VARCHAR(45) | نام کلاب/کافه |

---

### `event`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام رویداد/مسابقه |
| ClubId | INT NOT NULL FK → club(id) | |

> برای بازی‌های روزمره‌ی کافه (فاز ۲)، هر کافه یک Event فصلی default دارد.

---

### `senario`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام سناریو |

---

### `role`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(20) | نام نقش (دکتر، کاراگاه...) |
| SenarioId | INT NOT NULL FK → senario(Id) | |
| SideId | INT NOT NULL FK → side(Id) | |

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

---

### `master`
| ستون | نوع | توضیح |
|---|---|---|
| Id | INT PK AUTO_INCREMENT | |
| Name | VARCHAR(50) | نام گرداننده |
| ClubID | INT NOT NULL FK → club(id) | |
| userId | INT NULL UNIQUE FK → user(Id) | لینک به اکانت کاربری |

> گرداننده می‌تواند هم در جدول `master` (موجودیت مستقل) و هم در `clubuser`
> با `clubuserRole='master'` وجود داشته باشد. این دو موازی هستند:
> `clubplay.MasterId` به `master.Id` اشاره می‌کند.

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
| user_id | INT NULL UNIQUE FK → user(Id) | لینک به اکانت (فاز ۲) |

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

### `clubplayer` — مشتریان کافه (معادل `customer` در مستندات معماری)
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

---

### `club_clubplayer` — عضویت مشتری در کافه
| ستون | نوع | توضیح |
|---|---|---|
| clubId | INT NOT NULL FK → club(id) | کلید ترکیبی |
| clubplayerId | INT NOT NULL FK → clubplayer(Id) | کلید ترکیبی |

> هر مشتری که حتی یک بازی در یک کافه داشته باشد، اینجا ثبت می‌شود.
> هر کافه فقط مشتریان خودش را می‌بیند (multi-tenancy).

---

### `clubuser` — نقش کاربران در کافه
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| userId | INT NOT NULL UNIQUE FK → user(Id) | |
| clubId | INT NOT NULL UNIQUE FK → club(id) | |
| clubuserRole | ENUM NOT NULL | `cashier` \| `master` \| `supervisor` \| `owner` |

> ⚠️ **مشکل constraint فعلی:** `userId` و `clubId` هر کدام جداگانه UNIQUE هستند —
> این یعنی هر user فقط در **یک** کلاب می‌تواند نقش داشته باشد و هر کلاب فقط
> **یک** نفر می‌تواند عضو داشته باشد. این اشتباه است و باید در آینده اصلاح شود:
> `UNIQUE KEY` باید روی ترکیب `(userId, clubId)` باشد، نه روی هرکدام جداگانه.

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
| WinnersideId | INT NULL FK → side(Id) | NULL برای بازی‌های normal |
| UserId | INT NOT NULL FK → user(Id) | ثبت‌کننده |
| EventId | INT NOT NULL FK → event(Id) | Event فصلی کافه |
| Link | VARCHAR(300) | |
| playType | ENUM NOT NULL | `normal` \| `rank` \| `superrank` \| `etc` |
| status | ENUM NOT NULL | `pending` \| `notwinside` \| `notrank` \| `done` |

**چرخه‌ی status:**
```
pending    → نقش‌ها پخش شده، بازی شروع نشده
notwinside → بازی تمام شده، winner side هنوز ثبت نشده
notrank    → winner ثبت شده، رتبه‌بندی نفرات ثبت نشده (فقط superrank)
done       → همه چیز کامل
```

> **نکته:** `playType` مقادیر فعلی (`normal/rank/superrank/etc`) با مقادیر توافق‌شده
> در مستندات معماری (`regular/rank/super_rank`) فرق دارند. این را هنگام پیاده‌سازی
> سرویس‌ها در نظر بگیر.

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

---

### `masterlist` — لیست‌های شخصی گرداننده
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| name | VARCHAR(45) | نام لیست |
| masteId | INT NOT NULL FK → master(Id) | گرداننده‌ی صاحب لیست |

> گرداننده می‌تواند از مشتریان کافه لیست‌های شخصی بسازد (مثلاً «مشتریان VIP»).

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
| role | ENUM | `admin` \| `user` \| `master` \| `cafe_owner` |
| is_active | BOOLEAN | |
| mobile | VARCHAR(11) NOT NULL UNIQUE | |
| mobile_verified | BOOLEAN | |
| otp_code | VARCHAR(6) NULL | |
| otp_expires_at | DATETIME NULL | |
| display_name | VARCHAR(50) | |
| created_at | DATETIME | |
| last_login_at | DATETIME NULL | |

> نقش per-club (owner/cashier/master/supervisor) در `clubuser` ذخیره می‌شود.
> `user.role` برای تشخیص ادمین کل (`admin`) در مقابل کاربر عادی (`user`) استفاده می‌شود.

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
- **Side IDs هاردکد** — 1=Mafia, 2=Citizen در RankingService (فاز ۱)
- **نقش نهایی** — در Playplayer و Clubplayplayer نقش نهایی ثبت می‌شود
- **عکس بازیکنان** — در `wwwroot/uploads/players/` ذخیره، مسیر نسبی در DB
- **Comment depth** — فقط یک سطح reply
- **clubuser constraint باگ** — `userId UNIQUE` و `clubId UNIQUE` جداگانه هستند (اشتباه)؛
  فعلاً هر user فقط در یک کلاب نقش می‌تواند داشته باشد — این باید در آینده اصلاح شود
- **جداسازی فاز ۱ و ۲** — `play`/`playplayer`/`player` هرگز با `clubplay`/`clubplayplayer`/`clubplayer` مخلوط نشوند
- **masterlist.masteId** — تایپو در نام ستون (باید `masterId` باشد) — دستی تغییر نده، همینطور scaffold می‌شود
