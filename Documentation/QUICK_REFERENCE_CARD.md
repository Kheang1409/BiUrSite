# BiUrSite – Quick Reference Card

**Print-Friendly Cheat Sheet**  
**Date:** February 22, 2026

---

## At a Glance

| Aspect        | Detail                                            |
| ------------- | ------------------------------------------------- |
| **Project**   | BiUrSite – Anonymous idea/advice sharing platform |
| **Type**      | SaaS web application (3-tier architecture)        |
| **Status**    | In Development – Analysis Phase Complete          |
| **Users**     | Anonymous, Verified, Admin                        |
| **Use Cases** | 23 major features mapped                          |

---

## System Layers

```
┌─────────────────────────────────────────────────┐
│ Frontend: Next.js + React + TypeScript           │
│ (http://localhost:3000)                         │
└─────────────────┬───────────────────────────────┘
                  │ GraphQL / WebSocket (SignalR)
┌─────────────────▼───────────────────────────────┐
│ API: ASP.NET Core + GraphQL                     │
│ (http://localhost:8080/graphql)                 │
└─────────────────┬───────────────────────────────┘
                  │ Queries / Events
┌─────────────────▼───────────────────────────────┐
│ Data: MongoDB + Redis                           │
│ (mongodb:27017 / redis:6379)                    │
└─────────────────────────────────────────────────┘
```

---

## Key Use Cases (Quick Summary)

### Authentication (UC1-6)

- **UC1:** Email/password registration → verification email sent
- **UC2:** OAuth (Google/Facebook) → auto-verified account
- **UC3:** Verify email → account activated
- **UC4:** Login → JWT token returned
- **UC5:** Logout → token cleared
- **UC6:** Forgot password → OTP sent, password reset

### Content (UC7-11)

- **UC7:** View feed → paginated, public posts
- **UC8:** Search posts → keyword filtering
- **UC9:** Create post → text + optional image
- **UC10:** Edit post → author only
- **UC11:** Delete post → soft delete (preserve audit trail)

### Engagement (UC12-15)

- **UC12:** Comment on post → triggers notification
- **UC13:** Edit comment → author only
- **UC14:** Delete comment → soft delete
- **UC15:** View comments → paginated list with sorting

### Notifications (UC16-17) ⭐ REAL-TIME

- **UC16:** Receive notification → instant push via SignalR (if online)
- **UC17:** View notifications → history in dashboard

### User (UC18-20)

- **UC18:** Update profile → bio, avatar, phone
- **UC19:** Update preferences → notification settings
- **UC20:** View profile → public user information

### Admin (UC21-23)

- **UC21:** Ban user → prevent login/posting
- **UC22:** Remove content → soft delete with reason
- **UC23:** View dashboard → analytics & metrics

---

## Technology Stack

| Component              | Technology       | Version      | Notes                       |
| ---------------------- | ---------------- | ------------ | --------------------------- |
| **Frontend Runtime**   | Node.js          | 18+          | JavaScript/TypeScript       |
| **Frontend Framework** | Next.js          | 15           | React-based, SSR support    |
| **Frontend UI**        | React            | 19           | Component library           |
| **Frontend Styling**   | Tailwind CSS     | 3.4          | Utility-first CSS           |
| **State Management**   | Zustand          | 4.4          | Lightweight, React hooks    |
| **GraphQL Client**     | Apollo Client    | 3.10         | Query/subscription support  |
| **Real-Time Client**   | SignalR          | 8.0.7        | WebSocket for notifications |
| **Backend Runtime**    | .NET             | 8.0          | C# language                 |
| **Backend Framework**  | ASP.NET Core     | 8.0          | Web framework               |
| **API**                | GraphQL          | HotChocolate | Query language              |
| **CQRS**               | MediatR          | Latest       | Command/Query separation    |
| **Validation**         | FluentValidation | Latest       | Input validation            |
| **Database**           | MongoDB          | 6+           | Document NoSQL              |
| **Cache**              | Redis            | 7            | In-memory data store        |
| **Real-Time**          | SignalR          | Native .NET  | WebSocket hub               |
| **Auth**               | JWT + OAuth2     | Standard     | Google, Facebook providers  |
| **Email**              | SMTP             | Configurable | Email notifications         |
| **Testing**            | xUnit            | Latest       | .NET test framework         |
| **Container**          | Docker           | Latest       | Containerization            |
| **Orchestration**      | Docker Compose   | Latest       | Multi-service setup         |

---

## Database Collections (MongoDB)

### users

```json
{
  "_id": ObjectId,
  "id": UUID,
  "email": "user@example.com",
  "username": "alice",
  "password": "bcrypt_hash",
  "status": "Active|Unverified|Banned|Deleted",
  "role": "User|Admin",
  "profile": {
    "avatar": "url",
    "bio": "...",
    "phone": "..."
  },
  "notifications": [
    { "id": UUID, "message": "...", "createdAt": Date }
  ],
  "createdAt": Date,
  "modifiedAt": Date,
  "deletedAt": Date
}
```

### posts

```json
{
  "_id": ObjectId,
  "id": UUID,
  "userId": UUID,
  "author": {
    "id": UUID,
    "username": "alice",
    "avatar": "url"
  },
  "text": "Post content...",
  "image": "url",
  "status": "Active|Deleted",
  "comments": [
    { "id": UUID, "userId": UUID, "text": "...", "createdAt": Date }
  ],
  "recentComments": [...],
  "commentsCount": 5,
  "createdAt": Date,
  "modifiedAt": Date,
  "deletedAt": Date
}
```

### comments

```json
{
  "_id": ObjectId,
  "id": UUID,
  "postId": UUID,
  "userId": UUID,
  "text": "Comment text...",
  "status": "Active|Deleted",
  "createdAt": Date,
  "modifiedAt": Date,
  "deletedAt": Date
}
```

### notifications

```json
{
  "_id": ObjectId,
  "id": UUID,
  "userId": UUID,
  "postId": UUID,
  "message": "Alice commented on your post",
  "status": "Active|Deleted",
  "createdAt": Date,
  "deletedAt": Date
}
```

---

## API Endpoints (GraphQL)

### Queries (Read)

```graphql
# Get posts (feed)
query GetPosts($keywords: String, $pageNumber: Int!) {
  posts(keywords: $keywords, pageNumber: $pageNumber) {
    id
    text
    image
    author
    createdAt
  }
}

# Get single post
query GetPost($id: ID!) {
  post(id: $id) {
    id
    text
    author
    comments
  }
}

# Get notifications
query GetNotifications($pageNumber: Int!) {
  notifications(pageNumber: $pageNumber) {
    id
    message
    postId
    createdAt
  }
}

# Get current user
query Me {
  me {
    id
    email
    username
    role
  }
}
```

### Mutations (Write)

```graphql
# Register
mutation Register($email: String!, $username: String!, $password: String!) {
  register(input: {email, username, password}) {
    id, email, username
  }
}

# Login
mutation Login($email: String!, $password: String!) {
  login(email: $email, password: $password) {
    token
  }
}

# Create post
mutation CreatePost($text: String!, $data: [Byte!]) {
  createPost(text: $text, data: $data) {
    id, text, createdAt
  }
}

# Comment
mutation CreateComment($postId: ID!, $text: String!) {
  createComment(postId: $postId, text: $text) {
    id, text, author, createdAt
  }
}
```

### Real-Time (SignalR)

```javascript
// Client code
const connection = new HubConnectionBuilder()
  .withUrl("/notificationHub?access_token=" + token)
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveCommentNotification", (message) => {
  console.log(`Notification: ${message}`);
});

await connection.start();
```

---

## Authentication Flow

### Email/Password

```
1. Register → email sent
2. Verify email → account active
3. Login (email + password)
4. Validate password (bcrypt)
5. Return JWT token (24h expiry)
6. Client stores token in LocalStorage
7. Include in Authorization header: Bearer <token>
```

### OAuth (Google/Facebook)

```
1. Click "Sign with Google"
2. Redirect to provider consent screen
3. User grants permission
4. Provider redirects back with code
5. Backend exchanges code for user profile
6. Create/lookup user in MongoDB
7. Return JWT token (auto-verified)
```

---

## Real-Time Notification Flow

```
Trigger: User2 comments on User1's post
   ↓
CommentCreatedEvent published
   ↓
SendNotificationPostOwnerHandler executes
   ↓
Create Notification in DB
   ↓
Emit via SignalR: Clients.User(user1).SendAsync(...)
   ↓
User1 receives real-time toast notification (if online)
   ↓
Fallback: Notification stored in DB for offline retrieval
```

---

## Common GraphQL Patterns

### Pagination

```graphql
posts(pageNumber: 1) {
  # Returns items 0-9
  # pageNumber=2 returns 10-19
}
```

### Soft Delete

```graphql
# Delete post (set status = Deleted)
deletePost(id: "...")

# Query excludes deleted posts (filter: status = Active)
posts { ... }  # Never shows deleted
```

### Embedded Data

```graphql
# Author embedded in post
posts {
  author {  # No separate query needed
    id, username, avatar
  }
}

# Recent comments embedded in post
posts {
  recentComments {  # Last 3 for feed preview
    id, text, author
  }
}
```

---

## Environment Variables

### Backend (.env)

```bash
ASPNETCORE_ENVIRONMENT=Development
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
MONGODB_NAME=BiUrSite
JWT_SECRET_KEY=<generate-random-key>
JWT_ISSUER=BiUrSite
JWT_AUDIENCE=BiUrSiteApp
REDIS_CONNECTION_STRING=redis://localhost:6379
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
GOOGLE_CLIENT_ID=<from Google Console>
GOOGLE_CLIENT_SECRET=<from Google Console>
FACEBOOK_APP_ID=<from Facebook Developer>
FACEBOOK_APP_SECRET=<from Facebook Developer>
```

### Frontend (.env.local)

```bash
NEXT_PUBLIC_API_URL=http://localhost:8080
NEXT_PUBLIC_GRAPHQL_ENDPOINT=http://localhost:8080/graphql
NEXT_PUBLIC_SIGNALR_URL=http://localhost:8080
```

---

## Naming Conventions (Quick)

### C# (Backend)

- Classes: `PascalCase` → `UserRepository`, `CreatePostCommand`
- Methods: `PascalCase` → `GetUserAsync()`, `IsEmailVerified()`
- Variables: `camelCase` → `userId`, `isValid`
- Constants: `UPPER_SNAKE` → `MAX_POST_LENGTH`
- Async: Suffix `Async` → `GetUserAsync()`
- Booleans: Prefix `Is/Has/Can` → `IsActive`, `HasVerified`

### TypeScript/React (Frontend)

- Components: `PascalCase` → `PostCard`, `CommentForm`
- Hooks: Prefix `use` → `useAuth()`, `usePosts()`
- Variables: `camelCase` → `isLoading`, `userList`
- Constants: `UPPER_SNAKE` → `API_BASE_URL`
- Files: `kebab-case` → `user-profile.tsx`

### MongoDB

- Collections: `lowercase` → `users`, `posts`
- Fields: `camelCase` → `userId`, `createdAt`
- IDs: Suffix `Id` → `userId`, `postId`

---

## Git Commit Format

```
<type>(<scope>): <subject>

<body>

Closes #<issue>
```

**Types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`

**Example:**

```
feat(auth): implement OAuth2 Google login

Add Google OAuth2 provider configuration.
Create OAuth callback handler.
Auto-verify user on OAuth registration.

Closes #42
```

---

## Performance Targets

| Metric           | Target        | How to Achieve                    |
| ---------------- | ------------- | --------------------------------- |
| API Response     | < 500ms (p95) | Indexes, caching, pagination      |
| Feed Load        | < 200ms       | Redis cache, projection           |
| Notification     | < 100ms       | SignalR, WebSocket                |
| Page Load        | < 3 sec       | CDN, code splitting, images       |
| Concurrent Users | 1,000+        | Stateless API, horizontal scaling |

---

## Security Checklist

- [ ] HTTPS/TLS in production
- [ ] JWT validation on every request
- [ ] Password hash (bcrypt)
- [ ] Rate limiting (100 req/min)
- [ ] CORS configured (whitelist)
- [ ] Input validation (server-side)
- [ ] SQL/NoSQL injection prevention
- [ ] XSS prevention (auto via React)
- [ ] CSRF protection (OAuth state param)
- [ ] No hardcoded secrets (.env only)
- [ ] Error messages (generic, no leaks)
- [ ] Logging (PII masked)

---

## Deployment

### Environments

```
Local       → docker-compose up
Staging     → Deploy to Render (backend), Netlify (frontend)
Production  → Deploy to production infrastructure
```

### Ports (Local)

```
Frontend    → 3000
Backend     → 8080
MongoDB     → 27017
Redis       → 6379
```

### Docker Commands

```bash
# Build
docker-compose build

# Run
docker-compose up

# Stop
docker-compose down

# View logs
docker-compose logs -f backend
docker-compose logs -f redis
```

---

## Monitoring & Troubleshooting

### Common Issues

| Issue                              | Solution                                      |
| ---------------------------------- | --------------------------------------------- |
| Port already in use                | `lsof -i :8080` (find process) or change port |
| Database connection fail           | Check `MONGODB_CONNECTION_STRING`             |
| GraphQL query fails                | Check query syntax, run in GraphQL playground |
| Real-time notification not working | Check SignalR connection, token expiry        |
| Build fails                        | Clear `node_modules`, `bin/`, `obj/` folders  |
| CORS error                         | Check `ALLOWED_CORS` env var                  |

### Debug Mode

```bash
# Backend
set ASPNETCORE_ENVIRONMENT=Development

# Frontend
NODE_ENV=development npm run dev
```

---

## Key Files & Folders

| Path                              | Purpose                                       |
| --------------------------------- | --------------------------------------------- |
| `backend/API/Program.cs`          | Thin startup configuration (delegates DB init and middleware mapping to extension methods: `InitializeDatabaseAsync`, `UseDiagnostics`, `MapEndpoints`) |
| `backend/API/GraphQL/Query.cs`    | GraphQL queries                               |
| `backend/API/GraphQL/Mutation.cs` | GraphQL mutations                             |
| `backend/Application/`            | Business logic (handlers, validators)         |
| `backend/Domain/`                 | Entities, value objects, rules                |
| `backend/Infrastructure/`         | Repositories, services, external integrations |
| `frontend/src/app/`               | Next.js pages & layouts                       |
| `frontend/src/components/`        | React components                              |
| `frontend/src/lib/`               | API clients, utilities                        |
| `frontend/src/store/`             | Zustand state                                 |
| `docker-compose.yml`              | Multi-container setup                         |
| `.env.example`                    | Environment template                          |
| `docs/`                           | Documentation (SRS, use cases, etc.)          |

---

## Documentation Files (This Analysis)

| File                                      | Purpose               | Read Time |
| ----------------------------------------- | --------------------- | --------- |
| `00_EXECUTIVE_SUMMARY_AND_INDEX.md`       | Overview & navigation | 10 min    |
| `01_SYSTEM_REQUIREMENTS_SPECIFICATION.md` | Technical spec        | 45 min    |
| `02_USE_CASES_AND_DESCRIPTIONS.md`        | Business requirements | 90 min    |
| `03_REPOSITORY_SETUP_AND_GITHUB.md`       | Setup & CI/CD         | 60 min    |
| `04_PRESENTATION_OUTLINE_10_SLIDES.md`    | Stakeholder pitch     | 20 min    |

---

## Quick Links

- **Local:** http://localhost:3000 (frontend), http://localhost:8080/graphql (API)
- **GitHub:** [Your repo URL]
- **Issues:** [GitHub Issues]
- **Architecture:** See `docs/ARCHITECTURE.md`

---

## Team Checklist (On Day 1)

- [ ] Clone repo
- [ ] Copy `.env.example` → `.env`
- [ ] Install Node.js 18+, .NET 8.0
- [ ] `docker-compose up`
- [ ] `cd backend && dotnet run`
- [ ] `cd frontend && npm install && npm run dev`
- [ ] Visit http://localhost:3000 (should load)
- [ ] Visit http://localhost:8080/graphql (GraphQL UI)
- [ ] Read `00_EXECUTIVE_SUMMARY_AND_INDEX.md` (this folder)

---

**Print this card and keep it by your desk!** 📌

**Last Updated:** February 22, 2026
