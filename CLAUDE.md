# CLAUDE.md — MafiaPedia

> این فایل را اول بخوان. برای جزئیات بیشتر فایل‌های زیر را ببین:
> - `DATABASE.md` — ساختار کامل دیتابیس (فاز ۱ + فاز ۲)
> - `API.md` — همه endpoints فاز ۱
> - `FRONTEND.md` — router، components، stores
> - `ROADMAP.md` — نقشه راه و تصمیمات معماری

---

## پروژه

**MafiaPedia** — پلتفرم دوفازه برای اکوسیستم بازی مافیا در ایران:
- **فاز ۱ (کامل):** آمار و رتبه‌بندی بازیکنان حرفه‌ای — داده‌ها دستی وارد می‌شوند
- **فاز ۲ (در حال توسعه):** سیستم مدیریت کافه مافیا (B2B SaaS)

زبان UI: فارسی (RTL) — تم: Dark cinematic

---

## ساختار ریپو

```
MafiaPedia/
├── MafiaPedia.Api/        ← ASP.NET Core 9 + EF Core + MySQL
│   ├── Controllers/
│   ├── Services/Iservices/
│   ├── Entities/          ← DB-first scaffold (شامل entities فاز ۲)
│   ├── DTOs/
│   ├── Data/MafiaDbContext.cs
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

**CORS origins:** `localhost:5173`, `localhost:5272`, `localhost:7097`
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

## Services — فاز ۲ (در حال توسعه)

| Interface | مسئولیت |
|---|---|
| `IClubPlayerService` | CRUD مشتریان کافه + جستجو + عضویت در کلاب |
| `IClubUserService` | مدیریت نقش‌های per-club (owner/cashier/master/supervisor) |
| `IClubPlayService` | ثبت بازی کافه + فلوی پخش نقش + مدیریت status |
| `IMasterListService` | لیست‌های شخصی گرداننده |
| `IClubReportService` | گزارش‌های آماری کافه |

---

## قوانین — تغییر نده

- **DB-first** — Entities را دستی تغییر نده، فقط scaffold کن
- **SideId هاردکد** — 1=Mafia, 2=Citizen در RankingService (فاز ۱)
- **SecretKey** — باید انگلیسی بماند
- **Content-Type** — در apiClient.ts set نشود (axios خودش handle می‌کند)
- **Middleware order** — ترتیب بالا ثابت است
- **جداسازی فاز ۱ و ۲** — هرگز play/playplayer/player را با clubplay/clubplayplayer/clubplayer مخلوط نکن

## الگوهای پروژه

- هر Service → Interface + Implementation + AddScoped در Program.cs
- Controllers فقط routing — منطق در Services
- DTOs برای ورودی/خروجی (نه Entity مستقیم)
- multipart/form-data → `[Consumes("multipart/form-data")]` + `[FromForm]`
- Admin endpoints → `[Authorize(Policy = "AdminOnly")]`
- Club-scoped endpoints → بررسی `clubuser` برای تأیید دسترسی user به آن club
- Reply کامنت فقط یک سطح عمق
- حذف Play → Playplayers هم حذف می‌شوند
- حذف Player با Playplayer → 409
- حذف User با Comment → 409
- Admin نمی‌تواند خودش را حذف کند

## نکات مهم فاز ۲

- **clubuser constraint باگ:** `userId UNIQUE` و `clubId UNIQUE` جداگانه‌اند (اشتباه) —
  فعلاً هر user فقط در یک club نقش دارد. این در آینده اصلاح می‌شود.
- **clubplay.status چرخه:** `pending → notwinside → notrank → done`
- **پخش نقش:** سرور نقش‌ها را رندوم assign می‌کند (نه فرانت) — برای fairness
- **masterlist.masteId:** تایپو در نام ستون — دستی تغییر نده، همینطور scaffold شده

---

## وضعیت فعلی

**✅ فاز ۱ کامل:** Auth، Player CRUD، Play CRUD، Comment+Like، Ranking، User CRUD، Dropdown، Admin Panel

**🚧 فاز ۱ مانده:** SMS واقعی OTP، Initials avatar، صفحه 404، Pagination رتبه‌بندی

**🔄 فاز ۲ در حال توسعه:**
- ✅ Database scaffold شده (entities جدید موجود)
- ⬜ Club/Room/Master management
- ⬜ ClubPlayer (customer) CRUD
- ⬜ ClubUser access control
- ⬜ ClubPlay + role distribution flow
- ⬜ Reports & statistics
