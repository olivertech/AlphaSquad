# ??? Project Roadmap

This document tracks the evolution and planned features of the AlphaSquad platform.

## ?? Development Phases

### Phase 1: Authentication & Security ?
- [x] Implement Refresh Token rotation.
- [x] Implement Change Password functionality.
- [x] Implement Logout (session revocation).
- [x] Fix database relationships for Refresh Tokens.
- [x] Secure claims parsing for User IDs.

### Phase 2: Tenant, Media & Features ?
- [x] Model `Feature` and `TenantFeature` entities.
- [x] Seed base features and link to `alpha-demo` tenant.
- [x] Implement `GET /api/tenants/current`.
- [x] Implement `PUT /api/tenants/current/logo` with automatic old file deletion.
- [x] Implement `GET /api/tenants/current/features`.
- [x] Implement `GET /api/media/{id}`.

### Phase 3: Workouts & Exercises ??
- [ ] Implement Exercise CRUD.
- [ ] Implement Workout CRUD.
- [ ] Implement Workout-Exercise association (`POST /api/workouts/{id}/exercises`).

### Phase 4: Engagement & Scheduling ??
- [ ] Implement Check-in system (`POST /api/checkins`).
- [ ] Implement Check-in queries (`/me` and `/tenant`).
- [ ] Implement Classes/Agendas CRUD.
- [ ] Implement Class Booking system (`book` / `unbook`).

---

## ?? Goals
- [ ] Full multi-tenant isolation.
- [ ] High performance via Redis caching.
- [ ] Scalable media storage via Cloudflare R2.
- [ ] Complete gym management workflow.
