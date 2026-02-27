# BiUrSite — 10‑Slide Presentation (Markdown)

> Presenter: Hang Kheang Taing · Date: February 22, 2026

---

# BiUrSite: A Platform for Sharing Ideas & Seeking Advice

**Subtitle:** Building a moderated, real‑time community for trustworthy feedback

![branding](./presentation/images/branding.png)

<!-- Speaker Notes: Quick greeting, personal intro, one‑line project pitch. -->

---

## Problem Statement

- People lack safe spaces to ask for advice without fear of harassment.
- Anonymous posting often means poor moderation and low signal‑to‑noise.
- Communities are fragmented across many platforms (forums, Reddit, Discord).

![problem](./presentation/images/problem_gap.png)

<!-- Speaker Notes: Describe user pain; give 1–2 short examples of typical scenarios. -->

---

## Our Solution

- Moderation-first platform with optional anonymous or verified accounts.
- Real‑time engagement: instant notifications when someone comments.
- Simple, mobile‑friendly UI and scalable backend (ASP.NET Core, MongoDB, Redis).

![solution](./presentation/images/solution_flow.png)

<!-- Speaker Notes: Emphasize safety + immediacy; value proposition for users and moderators. -->

---

## System Architecture (Three‑Tier)

- Frontend: Next.js (React, TypeScript) — SSR + client UX
- API: ASP.NET Core 8 · GraphQL (HotChocolate) · SignalR hubs
- Data: MongoDB (documents) + Redis (cache, rate limiting)

![architecture](./presentation/images/architecture_diagram.png)

<!-- Speaker Notes: Walk through a request lifecycle: client → GraphQL → DB/Redis → SignalR push. -->

---

## Use‑Case Model (Summary)

- 23 major interactions covering users and admins (register, post, comment, notify, moderate).
- Primary actors: Anonymous user, Verified user, Admin.

![usecase](./presentation/images/usecase_diagram.png)

<!-- Speaker Notes: Highlight the most important use cases you implemented and why they matter. -->

---

## Key Use Case: Registration → First Post

1. Register (email or OAuth) → verification email sent
2. Email verified → login → land on feed
3. Create post (text + optional image) → optimistic update in feed

<!-- Speaker Notes: Explain acceptance criteria and how tests will validate each step. -->

---

## Key Use Case: Engagement Loop (Real‑Time)

1. User A comments on User B's post
2. Server pushes notification via SignalR to User B (if online)
3. User B clicks notification → navigates to thread → replies

<!-- Speaker Notes: Mention performance targets (notification latency <100ms) and fallback behavior when offline. -->

---

## Technology Stack & Rationale

- Frontend: Next.js, React, TypeScript — fast DX, SSR for SEO
- Backend: .NET 8, MediatR (CQRS), GraphQL (HotChocolate)
- Data & infra: MongoDB, Redis, Cloudinary (images), Docker, GitHub Actions

<!-- Speaker Notes: Short justification for each major choice (1–2 sentences each). -->

---

## Non‑Functional Requirements (Selected)

- Performance: API p95 < 500ms; feed pagination < 200ms; notifications < 100ms
- Security: JWT auth, OAuth options, rate limiting, input validation
- Scalability: Stateless API, Redis backplane for SignalR, MongoDB replication

<!-- Speaker Notes: Mention monitoring (health checks, Serilog/OpenTelemetry) and backups. -->

---

## Demo Flow (Screenshots / Short Recording)

1. Login → view feed (show feed screenshot)
2. Create post → optimistic update (screenshot)
3. Another browser comments → author receives toast notification (screenshot)

![demo](./presentation/images/demo_flow.png)

<!-- Speaker Notes: Explain what to show in the live/demo recording and the expected outcomes. -->

---

## Roadmap & Next Steps

- Phase 2 (Q1 2026): Advanced moderation tools, reputation system, post ratings
- Phase 3 (Q2 2026): Mobile apps, discovery/recommendations
- Phase 4 (Q3+ 2026): Monetization and creator tools

<!-- Speaker Notes: Close with ask: beta testers, feedback, and where to find the repo. -->

---

## Appendix & Links

- SRS: `Documentation/01_SYSTEM_REQUIREMENTS_SPECIFICATION.md`
- Use‑cases: `Documentation/02_USE_CASES_AND_DESCRIPTIONS.md`
- Architecture: `Documentation/ARCHITECTURE.md`
- Repo: https://github.com/Kheang1409/BiUrSite

<!-- Speaker Notes: Provide commit hash or branch name if you want graders to check a specific snapshot. -->
