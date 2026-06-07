# CLAUDE.md — MafiaPedia Project Intelligence File

> این فایل برای هوش مصنوعی‌ها (Claude، Copilot، Cursor، Windsurf و غیره) طراحی شده
> تا درک کاملی از ساختار، معماری، و منطق پروژه داشته باشند.
> هر بار که روی این پروژه کار می‌کنی، این فایل را اول بخوان.

---

## 1. معرفی پروژه

**MafiaPedia** یک پلتفرم آمار و رتبه‌بندی برای بازیکنان حرفه‌ای بازی مافیا است.
بازی‌های مافیای حرفه‌ای در ایران به صورت ویدیویی روی YouTube پخش می‌شوند و داده‌های آن‌ها
به صورت دستی در این سیستم ثبت می‌شود.

**هدف اصلی:** نمایش رتبه‌بندی، آمار بازیکنان، و پروفایل حرفه‌ای هر بازیکن بر اساس
تاریخچه بازی‌هایشان.

**مخاطب:** بازیکنان حرفه‌ای مافیا، کلاب‌های مافیا، و طرفداران این ورزش فکری.

**زبان رابط کاربری:** فارسی (RTL)

**تم طراحی:** Dark cinematic — سبک ESPN/ورزشی تیره با رنگ‌بندی حرفه‌ای

---

## 2. ساختار کلی ریپازیتوری

```
MafiaPedia/
├── MafiaPedia.Api/          ← Backend: ASP.NET Core Web API
│   ├── Controllers/         ← API Endpoints
│   ├── Services/            ← Business Logic (Interface + Implementation)
│   ├── Entities/            ← EF Core database models (DB-first)
│   ├── DTOs/                ← Data Transfer Objects
│   ├── Data/                ← DbContext
│   ├── Program.cs           ← App startup & DI registration
│   └── appsettings.json     ← Config (connection string, etc.)
│
├── MafiaPediaFront/         ← Frontend: Vue 3 + TypeScript
│   └── src/
│       ├── api/             ← Axios API layer
│       ├── components/      ← Reusable Vue components
│       ├── pages/           ← Page-level Vue components (route targets)
│       ├── layouts/         ← Layout wrappers
│       ├── stores/          ← Pinia state management
│       ├── types/           ← TypeScript type definitions
│       └── router/          ← Vue Router config
│
└── CLAUDE.md                ← این فایل
```

---

## 3. Backend — MafiaPedia.Api

### تکنولوژی‌ها

| ابزار | نقش |
|---|---|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core | ORM — DB-first approach |
| MySQL | دیتابیس اصلی (utf8mb4) |
| Swagger/OpenAPI | مستندسازی API |

### اجرا (Development)

```bash
cd MafiaPedia.Api
dotnet run
# API در http://localhost:5272 اجرا می‌شود
# Swagger UI: http://localhost:5272/swagger
```

### Connection String

در `appsettings.json` یا `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=mafia;User=...;Password=..."
  }
}
```

### CORS

Frontend روی `http://localhost:5173` اجرا می‌شود.
Origins مجاز در `Program.cs`:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:5272`
- `https://localhost:7097`

### Static Files

عکس‌های بازیکنان در `wwwroot/uploads/players/` ذخیره می‌شوند.
مسیر نسبی در فیلد `Player.Picture` در دیتابیس ذخیره می‌شود.

---

## 4. مدل داده‌ای (Entities)

> توجه: کد از DB-first تولید شده. تغییرات schema باید در دیتابیس اعمال شوند.

### موجودیت‌های اصلی

#### `Player` — بازیکن
```
Id, Name, Mobile, Birthday, Code, Picture (path), Desc
→ دارای: Playplayers (ICollection)
```

#### `Play` — یک بازی/دست
```
Id, Title, DateTime, PlayersCount, GuestCount, Desc, Link (YouTube URL)
→ FK: RoomId, SenarioId, MasterId, WinnersideId, UserId, EventId
→ دارای: Playplayers (ICollection)
```

#### `Playplayer` — جدول واسط بازیکن-بازی (جدول اصلی آمار)
```
Id, PlayerId, PlayId, RoleId, Rank, Action
→ این جدول مرکز تمام محاسبات آماری است
→ برای هر بازیکن در هر بازی یک رکورد وجود دارد
```

#### `Role` — نقش در بازی
```
Id, Name, SenarioId, SideId
→ مثال: شهروند ساده، دکتر، کارآگاه، دن مافیا، مافیا ساده
```

#### `Side` — طرف/تیم
```
Id, Name
→ مقادیر ثابت: 1=Mafia, 2=Citizen
→ این IDs در کد RankingService به صورت const هاردکد شده‌اند
```

#### `Senario` — سناریوی بازی
```
Id, Name
→ مثال: سناریوی استاندارد، سناریوی ویژه
```

#### `Event` — رویداد/مسابقه
```
Id, Name, ClubId
→ یک Event زیرمجموعه یک Club است
```

#### `Club` — کلاب مافیا
```
Id, Name
```

#### `Room` — اتاق بازی
```
Id, Name
```

#### `Master` — گرداننده بازی
```
Id, Name
```

#### `Comment` — سیستم کامنت
```
Id, Content, EntityType, EntityId, UserId, ParentCommentId, CreatedAt, UpdatedAt
→ سیستم کامنت عمومی (threaded/reply support)
→ EntityType = "player" و EntityId = PlayerId برای کامنت روی پروفایل
```

#### `User` — کاربر سیستم
```
Id, Username, Password
→ احراز هویت هنوز پیاده‌سازی نشده (بدون JWT)
```

### دیاگرام روابط (متنی)

```
Club ──< Event ──< Play >── Master
                     │
                     ├── Room
                     ├── Senario ──< Role >── Side
                     └── Playplayer >── Player
                              └── Role
```

---

## 5. API Endpoints

### Players — `/api/players`

| Method | Route | توضیح |
|---|---|---|
| GET | `/api/players/{playerId}` | پروفایل کامل بازیکن + آمار |
| GET | `/api/players/{playerId}/comments` | لیست کامنت‌های پروفایل |
| POST | `/api/players/{playerId}/comments` | ثبت کامنت جدید |
| GET | `/api/players/search?query=&limit=10` | جستجوی بازیکن (حداقل ۲ کاراکتر) |

### Rankings — `/api/rankings`

| Method | Route | توضیح |
|---|---|---|
| GET | `/api/rankings/overall?clubId=` | رتبه‌بندی کلی (فیلتر اختیاری بر اساس کلاب) |
| GET | `/api/rankings/side?sideId=&clubId=&eventId=&scenarioId=&minimumGames=` | رتبه‌بندی بر اساس ساید |

**منطق رتبه‌بندی:**
- حداقل ۱۰ بازی برای ظاهر شدن در رتبه‌بندی کلی
- مرتب‌سازی: win rate نزولی، سپس تعداد بازی نزولی
- `SideId=1` → مافیا، `SideId=2` → شهروند

### Plays — `/api/plays`

| Method | Route | توضیح |
|---|---|---|
| POST | `/api/plays` | ثبت بازی جدید به همراه لیست بازیکنان |

**ساختار `CreatePlayDto`:**
```json
{
  "title": "...",
  "dateTime": "2024-01-01T00:00:00",
  "playersCount": 12,
  "guestCount": 0,
  "desc": "...",
  "link": "https://youtube.com/...",
  "senarioId": 1,
  "winnersideId": 1,
  "eventId": 1,
  "roomId": 1,
  "masterId": 1,
  "userId": 1,
  "players": [
    { "playerId": 5, "roleId": 3, "rank": 1, "action": null }
  ]
}
```

### Dropdown — `/api/dropdown`

Dropdownهای مورد نیاز فرم‌ها: لیست کلاب‌ها، رویدادها، سناریوها، نقش‌ها، طرف‌ها.

### Health — `/api/health`

بررسی سلامت API.

---

## 6. Frontend — MafiaPediaFront

### تکنولوژی‌ها

| ابزار | نقش |
|---|---|
| Vue 3 | UI framework (Composition API) |
| TypeScript | Type safety |
| Vue Router 4 | Client-side routing |
| Pinia | State management |
| Axios | HTTP client |
| Tailwind CSS 3 | Utility-first styling |
| Vite | Build tool و dev server |

### اجرا (Development)

```bash
cd MafiaPediaFront
npm install
npm run dev
# در http://localhost:5173 اجرا می‌شود
```

### Environment Variables

فایل `.env` یا `.env.local` در ریشه `MafiaPediaFront/`:
```env
VITE_API_BASE_URL=http://localhost:5272/api
```

### مسیرهای Router

| Path | Component | توضیح |
|---|---|---|
| `/` | `HomePage.vue` | صفحه اصلی |
| `/ranking/citizen` | `CitizenRankingPage.vue` | رتبه‌بندی شهروندان |
| `/ranking/mafia` | `MafiaRankingPage.vue` | رتبه‌بندی مافیاها |
| `/player/:id` | `PlayerProfilePage.vue` | پروفایل بازیکن |
| `/plays/create` | `CreatePlayPage.vue` | ثبت بازی جدید |

همه صفحات درون `DefaultLayout.vue` رندر می‌شوند.

### لایه API (src/api/)

```
apiClient.ts       ← Axios instance پایه (baseURL + interceptors)
PlayerApi.ts       ← توابع مرتبط با بازیکن
RankingApi.ts      ← توابع مرتبط با رتبه‌بندی
PlaysApi.ts        ← توابع ثبت بازی
LookupApi.ts       ← توابع dropdown
index.ts           ← Export مرکزی
```

### کامپوننت‌ها (src/components/)

| کامپوننت | کاربرد |
|---|---|
| `PlayerSearchAutocomplete.vue` | جستجوی live بازیکن (debounced) |
| `PlayerStatisticsCard.vue` | نمایش آمار کلی بازیکن |
| `RecentGamesTable.vue` | جدول آخرین بازی‌های یک بازیکن |
| `RolesTable.vue` | جدول خلاصه عملکرد بر اساس نقش |

### State Management (Pinia)

```
stores/playerStore.ts   ← State مربوط به بازیکن فعلی
stores/index.ts         ← Export مرکزی
```

---

## 7. قوانین مهم برای AI

### ❌ تغییر ندهید

- ساختار Entities — DB-first است، schema در MySQL تعریف شده
- `SideId` های هاردکد شده در `RankingService.cs` (1=Mafia, 2=Citizen)
- CORS origins در `Program.cs`
- ترتیب middleware در `Program.cs` (CORS باید قبل از `MapControllers` باشد)

### ✅ الگوهای پروژه

- هر Service باید یک Interface داشته باشد (`IXxxService` + `XxxService`)
- همه سرویس‌ها در `Program.cs` با `AddScoped` ثبت می‌شوند
- Controllers فقط dependency injection و routing — منطق در Services
- DTOs برای ورودی/خروجی API استفاده می‌شود، نه Entities مستقیم
- همه API calls در فرانت از `src/api/` عبور می‌کنند، نه مستقیم از component

### 📝 نکات Naming

- فایل‌های Entity: PascalCase ساده (`Player.cs`, `Playplayer.cs`)
- جدول دیتابیس: lowercase (`player`, `playplayer`, `play`)
- DTOs: `XxxDto.cs` یا `CreateXxxDto.cs`
- Vue pages: `XxxPage.vue`
- Vue components: `XxxComponent.vue` یا نام توصیفی

---

## 8. وضعیت فعلی پروژه (Features)

### ✅ پیاده‌سازی شده

- رتبه‌بندی کلی بازیکنان (با فیلتر کلاب)
- رتبه‌بندی بر اساس ساید (مافیا / شهروند) با فیلترهای چندگانه
- پروفایل کامل بازیکن (آمار + نقش‌ها + آخرین بازی‌ها)
- جستجوی autocomplete بازیکنان
- ثبت بازی جدید (با تراکنش atomic)
- سیستم کامنت روی پروفایل بازیکنان
- Static file serving برای عکس بازیکنان

### 🚧 هنوز پیاده‌سازی نشده

- احراز هویت (JWT / Session) — جدول `User` وجود دارد ولی Auth middleware نیست
- Admin panel برای مدیریت داده
- سیستم ثبت بازیکن جدید از UI
- صفحه تاریخچه همه بازی‌ها
- فیلتر پیشرفته در صفحات رتبه‌بندی

---

## 9. نکات محیط توسعه

- **IDE:** Visual Studio (backend) + VSCode یا هر editor برای frontend
- **AI Tools استفاده شده:** OpenCode (با CLAUDE.md)
- **دیتابیس:** MySQL local در development
- **عکس‌های بازیکنان:** باید به صورت دستی در `MafiaPedia.Api/wwwroot/uploads/players/` کپی شوند

---

*این فایل باید با هر تغییر مهم در معماری پروژه به‌روز شود.*
