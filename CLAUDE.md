# CLAUDE.md — MafiaPedia

> این فایل را اول بخوان. برای جزئیات بیشتر فایل‌های زیر را ببین:
> - `DATABASE.md` — ساختار کامل دیتابیس (فاز ۱ + فاز ۲)
> - `API.md` — endpoints فاز ۱ (فاز ۲ هنوز مستند نشده — پایین را ببین)
> - `FRONTEND.md` — router، components، stores (فاز ۱ + فاز ۲ کامل، تازه به‌روز شد)
> - `ROADMAP.md` — نقشه راه و تصمیمات معماری

> آخرین بازبینی: 2026-07-15 — بر اساس بررسی مستقیم کد ریپو (نه فرض قبلی).
> نسخه‌ی قبلی این فایل عقب‌افتاده بود و فاز ۲ را «فقط اسکلت اولیه» نشان می‌داد؛
> در واقع بک‌اند فاز ۲ (بازی/مشتری/نقش‌های کافه) عملاً تکمیل شده.

---

## پروژه

**MafiaPedia** — پلتفرم دوفازه برای اکوسیستم بازی مافیا در ایران:
- **فاز ۱ (کامل):** آمار و رتبه‌بندی بازیکنان حرفه‌ای — داده‌ها دستی وارد می‌شوند
- **فاز ۲ (بک‌اند تکمیل شده، فرانت در حال توسعه):** سیستم مدیریت کافه مافیا (B2B SaaS)

زبان UI: فارسی (RTL) — تم: Dark cinematic

---

## ساختار ریپو

```
MafiaPedia/
├── MafiaPedia.Api/        ← ASP.NET Core 9 + EF Core + MySQL
│   ├── Controllers/Phase1/  Phase2/
│   ├── Services/Phase1/     Phase2/
│   ├── IServices/Phase1/    Phase2/
│   ├── Entities/          ← DB-first scaffold (شامل entities فاز ۲)
│   ├── DTOs/Phase1/         Phase2/
│   ├── Data/ (یا Entities/MafiaContext.cs — DbContext)
│   └── Program.cs
├── MafiaPediaFront/       ← Vue 3 + TypeScript + Pinia + Tailwind
│   └── src/
│       ├── api/
│       ├── components/
│       ├── pages/
│       ├── stores/
│       └── router/
├── CLAUDE.md
├── API.md
├── DATABASE.md
├── FRONTEND.md
└── ROADMAP.md
```

⚠️ توی ریپو یک فایل اضافه‌ی `DATABASE2.md` هست (احتمالاً یک dump آزمایشی/فرمت‌نشده) — پیشنهاد می‌شود حذف شود تا با `DATABASE.md` قاطی نشود.

---

## Backend — راه‌اندازی

```bash
cd MafiaPedia.Api && dotnet run
# http://localhost:5272 | Swagger: /swagger
```

```json
// appsettings.json
{
  "ConnectionStrings": { "DefaultConnection": "Server=...;Database=mafia;..." },
  "JwtSettings": {
    "SecretKey": "فقط انگلیسی — حداقل 32 کاراکتر",
    "Issuer": "MafiaPedia",
    "Audience": "MafiaPediaUsers",
    "AccessTokenExpiryMinutes": 30,
    "RefreshTokenExpiryDays": 30
  }
}
```

**ترتیب Middleware (تغییر نده):**
```csharp
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.MapControllers();
```

**CORS origins:** `localhost:5173`, `localhost:5272`, `localhost:7097`, `hoseinshary.ir`
**عکس بازیکنان فاز ۱:** `wwwroot/uploads/players/` → DB: `/uploads/players/{filename}`

---

## Frontend — راه‌اندازی

```bash
cd MafiaPediaFront && npm install && npm run dev
# http://localhost:5173
```

```env
VITE_API_BASE_URL=http://localhost:5272/api
```

**Auth Store:** token در localStorage، role از JWT decode می‌شود (نه localStorage)
**Auto refresh:** اگه کمتر از ۵ دقیقه به انقضا مانده → پشت صحنه refresh می‌شود

---

## Authorization Policies (`Program.cs`)

| Policy | شرط |
|---|---|
| `AdminOnly` | `role == admin` |
| `ClubOnly` | `role == club` (هر عضو کافه — owner/supervisor/cashier/master) |
| `AdminOrClub` | `admin` یا `club` |

> کنترل دقیق‌تر (کدوم clubuserRole دقیقاً مجازه) داخل خود کنترلرها از طریق `ClubControllerBase.VerifyClubAccess(clubId, ...allowedRoles)` انجام می‌شود، نه در سطح Policy.

⚠️ **این پالیسی‌ها فقط با `user.role = 'club'` کار می‌کنند.** روی MySQL واقعی این enum به‌روز شده، ولی `MafiaContext.cs` هنوز enum قدیمی رو نشون می‌ده — **باید rescaffold بشه** (پایین، بخش «مشکلات شناخته‌شده» رو ببین).

---

## Services — فاز ۱ (کامل)

| Interface | مسئولیت |
|---|---|
| `IAuthService` | register, login, refresh, OTP |
| `IPlayerService` | CRUD بازیکن + پروفایل + جستجو |
| `ICommentService` | کامنت + like |
| `IPlayWriteService` | ثبت، ویرایش، حذف بازی |
| `IPlayReadService` | خواندن بازی‌ها |
| `IRankingService` | محاسبه رتبه‌بندی |
| `IDropdownService` | داده‌های dropdown |
| `IUserService` | CRUD کاربران (admin) |
| `IStatisticsService` | آمار کلی فاز ۱ |

## Services — فاز ۲ (بک‌اند تکمیل شده)

| Interface | Controller | مسئولیت |
|---|---|---|
| `IClubManagementService` | `ClubManagementController` | CRUD کلاب/کافه، Room، Master (با آپلود لوگو/عکس) |
| `IClubPlayerService` | `ClubPlayerController` | CRUD مشتریان کافه (`clubplayer`) + جستجو (سراسری/داخل کافه) + عضویت خودکار |
| `IClubUserService` | `ClubUserController` + `ClubUserContextController` | مدیریت نقش‌های per-club (owner/cashier/master/supervisor) — ساخت/ویرایش/حذف عضو + `GET /api/clubusers/me` (کلاب‌های کاربر فعلی) |
| `IClubPlayService` | `ClubPlayController` | چرخه‌ی کامل بازی کافه: ثبت، ویرایش، پخش/بازپخش نقش، ثبت winnerside/rank، گزارش‌های آماری، دسترسی مبتنی بر مالکیت (master/privileged staff) |
| `IMasterAuthService` | `MasterAuthController` | `GET /api/masters/me` — context گرداننده (MasterId, ClubId) برای کاربر لاگین‌شده |
| `IEventService` | `EventController` | لیست Eventهای کافه، ساخت Event، تعیین Event پیش‌فرض |
| (بدون سرویس جدا) | `RoleController` | آپلود عکس نقش |
| (بدون سرویس جدا) | `SenarioController` | گرفتن `senario_role_set` برای یک سناریو + تعداد بازیکن (بدون نیاز به clubId scoping — سناریو سراسری است) |

> نکته: `IMasterListService` و `IClubReportService` که در نسخه‌ی قبلی این فایل به‌عنوان سرویس‌های برنامه‌ریزی‌شده ذکر شده بودند، هنوز endpoint/سرویس مجزا ندارند — گزارش‌های آماری فعلاً داخل خود `IClubPlayService` هستند (`GetClubStatsAsync`, `GetMasterPerformanceAsync`, `GetMyStatsAsync`).

---

## قوانین — تغییر نده

- **DB-first** — Entities را دستی تغییر نده، فقط scaffold کن
- **SideId هاردکد** — 1=Mafia, 2=Citizen در RankingService (فاز ۱)
- **SecretKey** — باید انگلیسی بماند
- **Content-Type** — در apiClient.ts set نشود (axios خودش handle می‌کند)
- **Middleware order** — ترتیب بالا ثابت است
- **جداسازی فاز ۱ و ۲** — هرگز play/playplayer/player را با clubplay/clubplayplayer/clubplayer مخلوط نکن
- **clubplay ownership** — همیشه از `Room.ClubId` برای تشخیص مالکیت clubId استفاده کن، نه Event و نه ستون مستقل

## الگوهای پروژه

- هر Service → Interface + Implementation + AddScoped در Program.cs
- Controllers فقط routing — منطق در Services
- DTOs برای ورودی/خروجی (نه Entity مستقیم)
- multipart/form-data → `[Consumes("multipart/form-data")]` + `[FromForm]`
- Admin endpoints → `[Authorize(Policy = "AdminOnly")]`
- Club-scoped endpoints → `[Authorize(Policy = "ClubOnly")]` یا `"AdminOrClub"` + بررسی دقیق‌تر نقش با `ClubControllerBase.VerifyClubAccess`
- Reply کامنت فقط یک سطح عمق
- حذف Play → Playplayers هم حذف می‌شوند
- حذف Player با Playplayer → 409
- حذف User با Comment → 409
- Admin نمی‌تواند خودش را حذف کند

## نکات مهم فاز ۲

- **مدل نقش:** `user.role` سه‌حالته: `admin` | `user` | `club`. نقش دقیق per-club (owner/master/supervisor/cashier) در `clubuser.clubuserRole` است. ساخت هر `clubuser` جدید به‌صورت خودکار `user.role` را به `club` تغییر می‌دهد.
- **گرداننده دوگانه:** هر Master هم رکورد مستقل `master` دارد (با دستمزد/عکس/بیو) هم می‌تواند `clubuser` با نقش `master` داشته باشد — این دو موازی‌اند، به هم sync نیستند به‌صورت خودکار مگر در سرویس‌های مربوطه.
- **دسترسی به بازی‌های کافه:** Master فقط بازی‌های خودش را می‌سازد/می‌بیند/ویرایش می‌کند؛ owner/supervisor/cashier می‌توانند برای هر Masterای در همان کافه عمل کنند (با انتخاب صریح `masterId`).
- **clubuser constraint:** ✅ اصلاح شده — composite unique روی `(userId, clubId)`، هر کاربر می‌تواند در چند کافه نقش داشته باشد.
- **clubplay.playType/status:** چرخه‌ی status بر اساس playType متفاوت است؛ تغییر playType هنگام ویرایش، status را با `ClubPlayStatusResolver` از نو محاسبه می‌کند (به `DATABASE.md` مراجعه شود).
- **پخش نقش:** سرور نقش‌ها را بر اساس `senario_role_set` رندوم assign می‌کند (نه فرانت) — برای fairness.
- **masterlist.masteId:** تایپو در نام ستون — دستی تغییر نده، همینطور scaffold شده.
- **Event پیش‌فرض:** هر کافه یک Event با `IsDefault=1` دارد (تضمین‌شده با ستون محاسباتی `DefaultClubId` + unique constraint)؛ بازی‌های بدون `EventId` صریح از همین استفاده می‌کنند.

---

## مشکلات شناخته‌شده (نیاز به اقدام)

1. ⚠️ **rescaffold معوق:** `user.role` روی MySQL واقعی به `enum('admin','user','club')` تغییر کرده، ولی `MafiaContext.cs` در ریپو هنوز enum قدیمی را دارد. تا وقتی rescaffold نشه، خود فایل C# با DB واقعی هم‌خوان نیست (فقط جنبه‌ی مستندسازی مشکل داره، چون EF از enum‌های MySQL آگاه نیست و رشته‌ی خام رو ذخیره می‌کنه — ولی برای هر توسعه‌ی بعدی روی `User` entity باید اول rescaffold بشه).
2. 🧹 **فایل تکراری `DATABASE2.md`** در ریشه‌ی ریپو — به نظر می‌رسه یک خروجی dump قدیمی/آزمایشی باشه با فرمت جدول خراب (tab به‌جای markdown pipe). پیشنهاد می‌شود حذف شود.

---

## وضعیت فعلی

**✅ فاز ۱ کامل:** Auth، Player CRUD، Play CRUD، Comment+Like، Ranking، User CRUD، Dropdown، Admin Panel، Statistics

**🚧 فاز ۱ مانده:** SMS واقعی OTP، Initials avatar، صفحه 404، Pagination رتبه‌بندی

**🔄 فاز ۲:**
- ✅ Database schema کامل (club/event/senario_role_set/room/master/clubuser/clubplay/clubplayplayer/clubplayer/club_clubplayer/masterlist با فیلدهای کامل)
- ✅ Club/Room/Master management (`ClubManagementController`)
- ✅ ClubPlayer (مشتری) CRUD + جستجو + عضویت خودکار (`ClubPlayerController`)
- ✅ ClubUser access control + مدیریت نقش‌ها (owner/supervisor/cashier) (`ClubUserController`, `ClubUserContextController`)
- ✅ ClubPlay کامل: ثبت، ویرایش، پخش/بازپخش نقش، ثبت winnerside/rank، جایگزینی بازیکن، دسترسی مبتنی بر مالکیت (`ClubPlayController`)
- ✅ گزارش‌های آماری پایه (`GetClubStatsAsync`, `GetMasterPerformanceAsync`, `GetMyStatsAsync`) — گزارش‌گیری پیشرفته‌تر (Cross-master، فصلی) هنوز نه
- ✅ **فرانت فاز ۲ کامل:** داشبوردهای Master، Owner، Supervisor، Cashier همه ساخته شده‌اند (به `FRONTEND.md` مراجعه شود) — شامل انتخاب کافه (چندکلاب)، مدیریت اعضا، فرم بازی، فلوی پخش نقش
- ⬜ Gamification (فاز ۳) — هنوز شروع نشده
