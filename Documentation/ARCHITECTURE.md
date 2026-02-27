## BiUrSite — Architecture Overview

This document is the updated, repository-aligned architecture overview for the BiUrSite project. It records the current stack, component responsibilities, deployment recommendations, class diagrams, and MongoDB schema examples as implemented in the workspace.

---

## Goals

- Present a current architecture for running the backend (ASP.NET Core) with MongoDB and Redis, and the frontend (Next.js).
- Include SignalR usage for realtime notifications and deployment and workflow guidance (Git, DockerHub, Render, Netlify).
- Provide concrete MongoDB schema examples consistent with the C# domain model in `backend/Domain`.

---

## High-level components

- Frontend: Next.js (React + TypeScript)
- Backend API: .NET (ASP.NET Core) — GraphQL (HotChocolate) + SignalR Hubs
- Database: MongoDB (document DB; collections for users, posts, comments, notifications, images)
- Cache / Session / Rate limiting: Redis
- Realtime: SignalR (hosted by the backend; clients connect from Next.js frontend)
- CI/CD & Code: Git (GitHub), GitHub Actions for CI
- Container Registry: Docker Hub (build and push images)
- Hosting: Backend can run on Render / Azure / Container Apps; Frontend on Netlify or Vercel
- Image storage: Cloudinary or S3 (recommended for production)

---

## Component interaction (flow)

graph TD
A[Next.js UI] -->|GraphQL / WS| B[ASP.NET Core API]
B -->|reads/writes| C[(MongoDB)]
B -->|cache| D[(Redis)]
B -->|push| E[SignalR Hub]
E -->|push| A
B -->|stores images| F[Image Storage (Cloudinary / S3)]
subgraph DevOps
G[GitHub] --> H[Docker Hub]
H --> I[Render / Azure]
G --> J[Netlify / Vercel]
end

---

## .NET (Backend) — responsibilities & notes

- Project: ASP.NET Core Web API using GraphQL (HotChocolate) and MediatR (CQRS) in the `Application` layer.
- Responsibilities:
  - Authentication (JWT / OAuth)
  - Business logic (MediatR handlers)
  - Validation & global error handling (middleware)
  - SignalR Hub for realtime notifications and presence
  - Integration with MongoDB via repositories/UnitOfWork
  - Use Redis for caching and rate limiting
- Configuration: `appsettings.json` / environment variables in containers

Implementation tips:

- Keep SignalR payloads minimal and validate messages on server.
- Use pagination and projections for feed queries against MongoDB.
- Keep clean separation: API / Application / Infrastructure.

---

## Reconciled MongoDB design notes (match domain)

Important: the codebase domain model currently expresses these shapes:

- `Post`: uses `Text` (not `Title`) as the main content field and contains a single `Image? Image` on the domain object.
- `Comment` instances are modelled as embedded objects inside `Post` in the domain (`private List<Comment> _comments`) in many domain classes.
- `Notification` objects are embedded inside `User` (user-scoped notifications) in the domain.

Two options exist — keep domain as-is (embedded) or change repository mapping to use separate collections for `comments` / `notifications` if you need large-scale querying. This document follows the repository's current implementation (embedded), while noting trade-offs.

### When to embed vs reference (short)

- Embed: small data read together with parent (author snapshot inside `posts`, recent comments array)
- Reference: large or independently queried children (full comments when posts have many comments)

---

## Recommended document shape (examples aligned with code)

### posts (example document matching domain)

```json
{
  "_id": "...",
  "authorId": "...",
  "author": { "_id": "...", "displayName": "Alice", "avatarUrl": "..." },
  "text": "# Hello\nThis is my post.",
  "image": { "_id": "...", "url": "https://..." },
  "likesCount": 12,
  "comments": [
    /* small embedded list */
  ],
  "createdAt": "2025-10-23T09:00:00Z"
}
```

Notes: use `text` (domain uses `Text`), and a single `image` object when following current domain POCOs. If you prefer multiple images, consider updating `backend/Domain/Posts/Post.cs` to support a list.

---

## Embedding strategies & update flows

- Keep embedded author snapshots up-to-date via a background reconciliation job (update recent posts when a user changes displayName/avatar).
- Keep only the latest N comments embedded (`comments` array trimmed to a small number) and store the full comment history in a separate collection if needed — optional migration if load increases.

Example push to keep `recentComments` trimmed (if you adopt hybrid approach):

```js
db.posts.updateOne(
  { _id: ObjectId("...") },
  { $push: { comments: { $each: [newComment], $slice: -3 } } },
);
```

---

## Redis — usage patterns

- Short-term caching (computed counts, profile caches)
- Rate-limiting counters (sliding window or token bucket)
- Pub/Sub for SignalR backplane when scaling multiple backend instances

---

## SignalR

- Host SignalR inside the ASP.NET backend and secure connections with short-lived access tokens.
- When scaling, use Redis backplane so hubs across instances see all messages.

---

## Frontend (Next.js)

- Responsibilities: UI, routing, GraphQL client, SignalR client, file uploads to CDN or backend proxy.
- Build & Deploy: static or server-side rendering; host on Vercel or Netlify.

---

## Git workflow & CI (short)

- Branching: `main` (protected), `develop`, `feature/*` branches, PR-based merges.
- CI: add GitHub Actions to run `dotnet restore/build/test` for backend and `npm ci && npm run build` for frontend.

---

## Docker / Local development

Add a `mongo` service to `docker-compose.yml` for local development and a small healthcheck for the backend. Example additions are documented in the repository's `Documentation/` folder and the `README`.

Example local `mongo` service (developer convenience):

```yaml
  mongo:
    image: mongo:6.0
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    restart: unless-stopped

volumes:
  mongo-data:
  redis-data:
```

Healthcheck example (backend):

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
  interval: 30s
  timeout: 5s
  retries: 3
```

---

## Observability & logging

- Add structured logging (Serilog) to write logs to stdout for platform compatibility.
- Consider OpenTelemetry or Prometheus + Grafana for metrics in production.

---

## Repo-specific findings (summary)

- Frontend: current codebase uses Next.js (updated here).
- Domain vs example docs: updated `posts` samples to use `text` and a single `image` to match `backend/Domain`.
- Comments & notifications are embedded in domain—this doc now documents that choice and trade-offs.

---

## Actionable next changes (I can implement)

1. Add `mongo` and health endpoints to `backend/API` and add `mongo` to `docker-compose.yml` for local dev.
2. Create `scripts/mongo/` with JSON Schema validators and a `seed` script for local development data.
3. Add a GitHub Actions workflow to run `dotnet build/test` and `npm build` on PRs.

Tell me which of the above you'd like me to implement next; I can add them in this repository.

---

## File location

This file replaces the older `docs/ARCHITECTURE.md` and now lives in `Documentation/ARCHITECTURE.md` to group all deliverables in one place.
