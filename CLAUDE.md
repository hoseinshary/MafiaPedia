# README.md — MafiaPedia

> این فایل را اول بخوان. برای جزئیات بیشتر فایل‌های زیر را ببین:
> - `DATABASE.md` — ساختار کامل دیتابیس
> - `API.md` — همه endpoints
> - `FRONTEND.md` — router، components، stores

---

## پروژه

**MafiaPedia** — پلتفرم آمار و رتبه‌بندی بازیکنان حرفه‌ای مافیا در ایران.
بازی‌ها روی YouTube پخش می‌شوند، داده‌ها دستی وارد می‌شوند.
زبان UI: فارسی (RTL) — تم: Dark cinematic

---

## ساختار ریپو

```
MafiaPedia/
├── MafiaPedia.Api/        ← ASP.NET Core 8 + EF Core + MySQL
│   ├── Controllers/
│   ├── Services/Iservices/
│   ├── Entities/          ← DB-first scaffold
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
├── README.md
├── API.md
├── DATABASE.md
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
**عکس بازیکنان:** `wwwroot/uploads/players/` → DB: `/uploads/players/{filename}`

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

## Services

| Interface | مسئولیت |
|---|---|
| `IAuthService` | register, login, refresh, OTP |
| `IPlayerService` | CRUD بازیکن + پروفایل + جستجو |
| `ICommentService` | کامنت + like (جدید) |
| `IPlayWriteService` | ثبت، ویرایش، حذف بازی |
| `IPlayReadService` | خواندن بازی‌ها |
| `IRankingService` | محاسبه رتبه‌بندی |
| `IDropdownService` | داده‌های dropdown |
| `IUserService` | CRUD کاربران (admin) |

---

## قوانین — تغییر نده

- **DB-first** — Entities را دستی تغییر نده، فقط scaffold کن
- **SideId هاردکد** — 1=Mafia, 2=Citizen در RankingService
- **SecretKey** — باید انگلیسی بماند
- **Content-Type** — در apiClient.ts set نشود (axios خودش handle می‌کند)
- **Middleware order** — ترتیب بالا ثابت است

## الگوهای پروژه

- هر Service → Interface + Implementation + AddScoped در Program.cs
- Controllers فقط routing — منطق در Services
- DTOs برای ورودی/خروجی (نه Entity مستقیم)
- multipart/form-data → `[Consumes("multipart/form-data")]` + `[FromForm]`
- Admin endpoints → `[Authorize(Policy = "AdminOnly")]`
- Reply کامنت فقط یک سطح عمق
- حذف Play → Playplayers هم حذف می‌شوند
- حذف Player با Playplayer → 409
- حذف User با Comment → 409
- Admin نمی‌تواند خودش را حذف کند

---

## وضعیت فعلی

**✅ آماده:** Auth کامل، Player CRUD، Play CRUD، Comment+Like، Ranking، User CRUD، Dropdown، Admin Panel پایه

**🚧 مانده:** SMS واقعی OTP، Initials avatar، صفحه 404، Pagination رتبه‌بندی
