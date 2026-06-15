# DATABASE.md — MafiaPedia Database Schema

> این فایل ساختار کامل دیتابیس MySQL پروژه MafiaPedia را توضیح می‌دهد.
> DB-First: تغییرات schema در MySQL اعمال می‌شوند، سپس با EF Core scaffold می‌شوند.

---

## دیاگرام روابط

```
Club ──< Event ──< Play >── Master
                    │  └──> Room
                    │  └──> Senario ──< Role >── Side
                    │  └──> Side (winnerside)
                    │  └──> User
                    └──< Playplayer >── Player >── User
                                    └──> Role

User ──< Comment ──< Commentlike
     ──< RefreshToken
     ──< Subscription
     ──  Player (1:1 nullable)
```

---

## جداول

### `club`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام کلاب مافیا |

**روابط:** یک کلاب چند رویداد دارد (`events`)

---

### `event`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام رویداد/مسابقه |
| club_id | INT FK → club | |

**روابط:** یک رویداد چند بازی دارد (`plays`)

---

### `master`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام گرداننده بازی |

---

### `room`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام اتاق/سالن |

---

### `side`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام طرف (مافیا/شهروند) |

> ⚠️ مقادیر ثابت هاردکد شده در کد: **1=Mafia, 2=Citizen**

---

### `senario`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام سناریو |

**روابط:** یک سناریو چند نقش دارد (`roles`)

---

### `role`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام نقش (دکتر، کاراگاه، مافیا سایه...) |
| senario_id | INT FK → senario | |
| side_id | INT FK → side | |

---

### `player`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| name | VARCHAR | نام بازیکن |
| mobile | VARCHAR | شماره موبایل |
| birthday | DATE | تاریخ تولد |
| code | VARCHAR | کد بازیکن |
| picture | VARCHAR | مسیر عکس: `/uploads/players/{filename}` |
| desc | TEXT | توضیحات |
| user_id | INT FK → user NULL | لینک به اکانت کاربری (فاز ۲) |

---

### `play` — جدول اصلی بازی‌ها
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| title | VARCHAR | عنوان بازی |
| date_time | DATETIME | زمان برگزاری |
| players_count | INT | تعداد بازیکنان |
| guest_count | INT | تعداد مهمانان |
| desc | TEXT | توضیحات |
| link | VARCHAR | لینک ویدیو YouTube |
| room_id | INT FK → room | |
| senario_id | INT FK → senario | |
| master_id | INT FK → master | |
| winnerside_id | INT FK → side | طرف برنده |
| user_id | INT FK → user | ثبت‌کننده بازی |
| event_id | INT FK → event | |

---

### `playplayer` — جدول واسط بازیکن-بازی (مرکز آمار)
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| play_id | INT FK → play | |
| player_id | INT FK → player | |
| role_id | INT FK → role | نقش نهایی بازیکن در این بازی |
| rank | INT NULL | رتبه/نتیجه (-1=اخراج انضباطی) |
| action | INT NULL | رویداد خاص (خرید هانیبال، رد پیشنهاد...) |

> **مرکز تمام محاسبات آماری** — win rate، رتبه‌بندی همه از این جدول محاسبه می‌شوند.
> نقش نهایی ثبت می‌شود (نه نقش اولیه — مهم برای سناریوهای تعویض نقش مثل طرفدار)

---

### `user`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| username | VARCHAR UNIQUE | نام کاربری |
| password | VARCHAR | deprecated — استفاده نشود |
| password_hash | VARCHAR | BCrypt hash |
| password_salt | VARCHAR | استفاده نمی‌شود (BCrypt داخلی) |
| role | ENUM | admin \| player \| master \| cafe_owner |
| is_active | BOOLEAN | |
| mobile | VARCHAR UNIQUE | شماره موبایل (11 رقم) |
| mobile_verified | BOOLEAN | |
| otp_code | VARCHAR(6) NULL | کد OTP فعلی |
| otp_expires_at | DATETIME NULL | انقضای OTP |
| display_name | VARCHAR | نام نمایشی |
| created_at | DATETIME | |
| last_login_at | DATETIME NULL | |

> ⚠️ فیلد `password` deprecated است — از `password_hash` استفاده کنید

---

### `comment`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| user_id | INT FK → user | |
| entity_type | VARCHAR | 'player' یا 'play' |
| entity_id | BIGINT | id بازیکن یا بازی |
| parent_comment_id | INT FK → comment NULL | برای reply (فقط یک سطح) |
| content | TEXT | متن کامنت |
| is_deleted | BOOLEAN | soft delete |
| is_hidden | BOOLEAN | مخفی کردن توسط admin |
| created_at | DATETIME | |
| updated_at | DATETIME NULL | |

---

### `commentlike`
| ستون | نوع | توضیح |
|---|---|---|
| comment_id | INT PK+FK → comment | کلید ترکیبی |
| user_id | INT PK+FK → user | کلید ترکیبی |
| created_at | DATETIME | |

> کلید ترکیبی — هر کاربر فقط یک لایک برای هر کامنت

---

### `refresh_token`
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| user_id | INT FK → user | |
| token | VARCHAR(500) | GUID تصادفی |
| expires_at | DATETIME | انقضا (30 روز) |
| created_at | DATETIME | |
| is_revoked | BOOLEAN | آیا باطل شده |

---

### `subscription` (آماده برای فاز ۲)
| ستون | نوع | توضیح |
|---|---|---|
| id | INT PK | |
| user_id | INT FK → user | |
| plan | ENUM | player_free \| player_pro \| cafe_basic \| cafe_pro |
| started_at | DATETIME | |
| expires_at | DATETIME | |
| is_active | BOOLEAN | |

---

## نکات مهم برای AI

- **DB-First** — Entity ها از دیتابیس scaffold شده‌اند، دستی تغییر نده
- **Side IDs هاردکد** — 1=Mafia, 2=Citizen در RankingService
- **نقش نهایی** — در Playplayer نقش نهایی ثبت می‌شه (مهم برای سناریوی طرفدار)
- **Action field** — عدد بدون معنای استاندارد (پیشنهاد: جدول action_types در آینده)
- **عکس بازیکنان** — در `wwwroot/uploads/players/` ذخیره، مسیر نسبی در DB
- **Comment depth** — فقط یک سطح reply (parent_comment_id فقط به root comment اشاره می‌کند)
- **player.user_id** — nullable، فقط بازیکنانی که ثبت‌نام کردن لینک دارن (فاز ۲)
