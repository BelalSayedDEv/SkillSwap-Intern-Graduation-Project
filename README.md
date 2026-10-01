# SkillSwap — Internship Graduation Project

Skill-exchange platform: users offer skills they can teach, request skills they want to learn,
send swap requests, manage subscriptions/quotas, favorites, availability, and trust (blocks/reports).
Built during a .NET Backend internship at CodePlus (Jul 2026 – Sep 2026).

## Stack

- .NET 9, ASP.NET Core Web API, EF Core, SQL Server (LocalDB for development)
- ASP.NET Core Identity + JWT access tokens + hashed refresh-token rotation (family + reuse detection)
- Clean Architecture: `SkillSwap.API / Application / Infrastructure / Domain`
- Cross-cutting: global exception handler (`ProblemDetails` + `traceId`), per-policy rate limiting,
  L1 cache-aside (`IMemoryCache`) on the skills catalog with explicit invalidation

## Domains & Endpoints

| Domain | Base route | Endpoints |
|---|---|---|
| Auth | `api/auth` | `POST register/login/refresh/logout`, `POST revoke-family` |
| Catalog (public) | `api/catalog` | `GET categories`, `GET skills`, `GET skills/search` |
| My skills | `api/me/skills` | `GET`, `POST`, `DELETE {id}` |
| Moderation (admin) | `api/moderation/skills` | `POST requests`, `GET pending`, `POST {id}/approve`, `POST {id}/reject` |
| Profile | `api/me/profile` | `GET`, `PUT`, `POST complete-onboarding`, `GET/POST availabilities`, `DELETE availabilities/{id}` |
| Favorites | `api/me/favorites` | `GET`, `POST toggle`, `DELETE {id}` |
| Swaps | `api/swaps` | `POST`, `GET mine`, `GET {id}`, `POST {id}/accept|reject|cancel` |
| Subscription | `api/me/subscription` | `GET`, `GET quota`, `POST upgrade` |
| Blocks | `api/me/blocks` | `GET`, `POST`, `DELETE {blockedId}` |
| Reports | `api/reports`, `api/admin/reports` | `POST`, `GET` (admin), `POST {id}/resolve` (admin) |

Auth model: `ActiveUserOnly` policy (active, non-deleted users) + `AdminOnly` role policy.
Accepting a swap consumes one session from the acceptor's monthly quota (Free 5 / Premium 30).

## How to run

Prerequisites: .NET 9 SDK, SQL Server LocalDB.

```powershell
# 1. Dev JWT secret (never committed — validated on startup, min 32 chars)
dotnet user-secrets set "Jwt:SecretKey" "<32+ random chars>" --project SkillSwap.API/SkillSwap.API.csproj

# 2. Create / migrate the database
dotnet ef database update --project SkillSwap.Infrastructure --startup-project SkillSwap.API

# 3. Run (Development)
dotnet run --project SkillSwap.API/SkillSwap.API.csproj --launch-profile http

# Swagger:
http://localhost:5055/swagger/index.html
```

## Caching (skills catalog, L1)

- `skills:categories:all` — 30 min (rarely changes)
- `skills:approved:v{version}:{category}:{page}:{size}` — 2 min, versioned
- `Approve/Reject` bump the version + drop the categories key (explicit invalidation);
  `RequestSkill` (pending) invalidates nothing — pending items never appear in the catalog
- Per-key `SemaphoreSlim` stampede guard: one miss hits SQL, concurrent requests wait and share the result
- Hit/miss lines are logged (`cache.hit` / `cache.miss`) — watch them in the console

## Notes for evaluators

- All packages target .NET 9 (`9.0.8`); `Microsoft.AspNetCore.Authorization` must stay on 9.x
  (v10 breaks startup with `MissingMethodException`)
- `Jwt:SecretKey` comes from user-secrets / `Jwt__SecretKey` env — there is intentionally
  no secret in `appsettings.json`
- Migrations are per-domain (`InitialCreate`, `FavoritesAndEnums`, `MatchesDomain`,
  `SubscriptionsDomain`, `TrustDomain`) — apply with `database update`
