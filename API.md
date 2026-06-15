# API.md — MafiaPedia API Reference

Base URL: `http://localhost:5272/api`
Auth: `Authorization: Bearer {accessToken}`

---

## Auth `/api/auth`

| Method | Route | Auth | توضیح |
|---|---|---|---|
| POST | `/auth/register` | ❌ | `{username, password, mobile, displayName?}` |
| POST | `/auth/login` | ❌ | `{username, password}` → `{accessToken, refreshToken}` |
| POST | `/auth/refresh` | ❌ | `{refreshToken}` → `{accessToken, refreshToken}` |
| POST | `/auth/send-otp` | ❌ | `{mobile}` |
| POST | `/auth/verify-otp` | ❌ | `{mobile, otpCode}` |

**JWT Claims:** `NameIdentifier=userId`, `Name=username`, `Role=role`, `mobile=mobile`

---

## Players `/api/players`

| Method | Route | Auth | توضیح |
|---|---|---|---|
| GET | `/players` | ❌ | `?page&pageSize&search` |
| GET | `/players/search` | ❌ | `?query&limit=10` (حداقل ۲ کاراکتر) |
| GET | `/players/{id}` | ❌ | پروفایل کامل + آمار |
| GET | `/players/{id}/detail` | Admin | اطلاعات فرم ویرایش |
| POST | `/players` | Admin | `multipart/form-data`: Name*, Mobile, Code, Birthday, Desc, Picture |
| PUT | `/players/{id}` | Admin | `multipart/form-data`: همان فیلدها (همه optional) |
| DELETE | `/players/{id}` | Admin | 409 اگه Playplayer داشته باشد |

---

## Plays `/api/plays`

| Method | Route | Auth | توضیح |
|---|---|---|---|
| GET | `/plays` | ❌ | `?page&pageSize&search&clubId&eventId&senarioId&playerId&masterId&winnersideId` |
| GET | `/plays/{id}` | ❌ | جزئیات + بازیکنان + نام‌ها |
| POST | `/plays` | Login | ثبت بازی — userId از JWT |
| PUT | `/plays/{id}` | Admin | ویرایش (فیلدهای optional) |
| DELETE | `/plays/{id}` | Admin | حذف + Playplayers |

**CreatePlayDto:**
```json
{
  "title": "", "dateTime": "", "playersCount": 0, "guestCount": 0,
  "desc": "", "link": "", "senarioId": 0, "winnersideId": 0,
  "eventId": 0, "roomId": 0, "masterId": 0,
  "players": [{"playerId": 0, "roleId": 0, "rank": 0, "action": 0}]
}
```

---

## Rankings `/api/rankings`

| Method | Route | Auth | توضیح |
|---|---|---|---|
| GET | `/rankings/overall` | ❌ | `?clubId` — حداقل ۱۰ بازی، مرتب win rate |
| GET | `/rankings/side` | ❌ | `?sideId&clubId&eventId&scenarioId&minimumGames` |

**SideId:** 1=Mafia, 2=Citizen

---

## Comments `/api/comments`

| Method | Route | Auth | توضیح |
|---|---|---|---|
| GET | `/comments/{entityType}/{entityId}` | ❌* | لیست + replies + LikeCount + IsLikedByCurrentUser |
| POST | `/comments/{entityType}/{entityId}` | Login | `{content, parentCommentId?}` |
| DELETE | `/comments/{id}` | Login | صاحب یا admin |
| POST | `/comments/{id}/like` | Login | toggle — برگشت `{isLiked}` |

*اگه login باشد IsLikedByCurrentUser پر می‌شود
**entityType:** فقط `player` یا `play`
**Reply:** فقط یک سطح — parentCommentId باید root comment باشد

---

## Users `/api/users` (همه Admin)

| Method | Route | توضیح |
|---|---|---|
| GET | `/users` | `?page&pageSize&search` |
| GET | `/users/{id}` | جزئیات |
| POST | `/users` | `{username, password, displayName?, mobile?, role}` |
| PUT | `/users/{id}` | `{displayName?, mobile?, role?, isActive?, password?}` |
| DELETE | `/users/{id}` | 400 اگه خودش باشد، 409 اگه Comment داشته باشد |

**Role values:** `admin` \| `player` \| `master` \| `cafe_owner`

---

## Dropdown `/api/dropdown`

```json
GET /api/dropdown →
{
  "clubs": [{id, name}],
  "events": [{id, name}],
  "eventsWithClub": [{id, name, clubId}],
  "scenarios": [{id, name}],
  "sides": [{id, name}],
  "masters": [{id, name}],
  "rooms": [{id, name}]
}
```
