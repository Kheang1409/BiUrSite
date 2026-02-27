# BiUrSite – Repository & GitHub Setup Guide

**Document Version:** 1.0  
**Date:** February 22, 2026

---

## Table of Contents

1. [Repository Structure Analysis](#1-repository-structure-analysis)
2. [Branching Strategy](#2-branching-strategy)
3. [Folder Organization Improvements](#3-folder-organization-improvements)
4. [Naming Conventions](#4-naming-conventions)
5. [Commit Conventions](#5-commit-conventions)
6. [Professional README Template](#6-professional-readme-template)
7. [GitHub Configuration](#7-github-configuration)

---

## 1. Repository Structure Analysis

### Current State

```
BiUrSite/
├── docker-compose.yml          ✓ Good – infrastructure as code
├── example.env                 ✓ Good – environment template
├── LICENSE                     ✓ Required for open source
├── README.md                   ~ Needs improvement
├── backend/                    ✓ Well-organized clean architecture
│   ├── backend.sln
│   ├── Dockerfile
│   ├── API/                    ✓ GraphQL endpoints, middleware
│   ├── Application/            ✓ Business logic, handlers, DTOs
│   ├── Domain/                 ✓ Entities, enums, value objects
│   ├── Infrastructure/         ✓ Repositories, services, external integrations
│   ├── SharedKernel/           ✓ Common exceptions, primitives
│   └── Tests/                  ✓ Unit tests
├── frontend/                   ✓ Well-organized Next.js project
│   ├── package.json
│   ├── next.config.ts
│   ├── tsconfig.json
│   ├── tailwind.config.ts
│   ├── src/
│   │   ├── app/               ✓ Next.js pages/layout
│   │   ├── components/         ✓ React components
│   │   ├── hooks/             ~ Consider organizing by feature
│   │   ├── lib/               ~ Could separate utilities
│   │   ├── types/             ✓ TypeScript interfaces
│   │   └── utils/             ✓ Helper functions
│   ├── public/                ✓ Static assets
│   └── Dockerfile
└── docs/                       ✓ Documentation (enhance)
    └── ARCHITECTURE.md
```

### Strengths ✓

1. **Clear separation of concerns** (backend: API, Application, Domain, Infrastructure)
2. **Infrastructure as code** (docker-compose.yml)
3. **Environment management** (example.env template)
4. **Appropriate folder nesting** (organized by feature/responsibility)
5. **Licensed** (identifies ownership)

### Gaps to Address ⚠️

1. **Missing GitHub-specific files:**
   - `.github/` directory for workflows, issue templates, PR templates
   - `CONTRIBUTING.md` – contribution guidelines
   - `.gitignore` – prevent committing sensitive files
   - `.editorconfig` – consistent formatting

2. **Documentation gaps:**
   - No API documentation (should auto-generate from GraphQL)
   - No deployment guide
   - No local development setup
   - No troubleshooting guide

3. **Frontend organization:**
   - Components could be organized by feature
   - Hooks scattered; could group by feature
   - Utilities could be more granular

4. **Backend:**
   - No migrations folder (if using database schema versioning)
   - Could benefit from separate Services folder

5. **CI/CD:**
   - No GitHub Actions workflows
   - No automated testing on PR
   - No deployment automation

---

## 2. Branching Strategy

### Recommended: Git Flow

```
main                           (production-ready releases)
  ├── release/1.0.0           (release candidate)
  ├── release/1.1.0

develop                        (integration branch)
  ├── feature/auth-oauth       (new features)
  ├── feature/notifications
  ├── bugfix/login-validation
  ├── hotfix/security-patch    (urgent production fixes)

main hotfix branches:
  ├── hotfix/1.0.1            (critical production bug)
```

### Branch Naming Conventions

| Type     | Format                       | Example                                         |
| -------- | ---------------------------- | ----------------------------------------------- |
| Feature  | `feature/<feature-name>`     | `feature/post-creation`, `feature/user-profile` |
| Bug Fix  | `bugfix/<issue-description>` | `bugfix/login-failure`                          |
| Hot Fix  | `hotfix/<issue-description>` | `hotfix/sql-injection-patch`                    |
| Refactor | `refactor/<area>`            | `refactor/authentication-service`               |
| Docs     | `docs/<topic>`               | `docs/api-specification`                        |
| Release  | `release/<version>`          | `release/1.0.0`                                 |

### Merge Requirements

- **All branches** require pull request review (no direct commits to main/develop)
- **Minimum 1 approval** before merge
- **All CI checks** must pass (tests, linting, build)
- **Auto-delete branch** after merge
- **Squash commits** on feature branches for clean history

---

## 3. Folder Organization Improvements

### Backend Structure – Recommended New Layout

```
backend/
├── backend.sln
├── src/                               (actual source code)
│   ├── API/                          (HTTP/GraphQL endpoints)
│   │   ├── API.csproj
│   │   ├── Program.cs               (thin startup; delegates DB initialization and endpoint/middleware mapping to extension methods such as `InitializeDatabaseAsync`, `UseDiagnostics`, and `MapEndpoints`)
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlerMiddleware.cs
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── LoggingMiddleware.cs   (new)
│   │   ├── GraphQL/                 (GraphQL queries & mutations)
│   │   │   ├── Query.cs
│   │   │   ├── Mutation.cs
│   │   │   ├── Subscriptions.cs      (future: real-time subscriptions)
│   │   │   ├── Types/               (GraphQL types)
│   │   │   └── Filters/
│   │   ├── Controllers/              (if REST endpoints added later)
│   │   └── Extensions/
│   │
│   ├── Application/                  (Business logic, CQRS)
│   │   ├── Application.csproj
│   │   ├── DependencyInjection.cs   (service registration)
│   │   ├── Common/                  (shared application logic)
│   │   │   ├── Behaviors/           (MediatR pipeline behaviors)
│   │   │   ├── Exceptions/
│   │   │   └── Models/
│   │   ├── Users/
│   │   │   ├── Commands/            (write operations)
│   │   │   │   ├── Register/
│   │   │   │   ├── Login/
│   │   │   │   ├── Update/
│   │   │   │   └── Delete/
│   │   │   └── Queries/             (read operations)
│   │   │       ├── GetUser/
│   │   │       └── GetUsers/
│   │   ├── Posts/
│   │   │   ├── Commands/
│   │   │   │   ├── Create/
│   │   │   │   ├── Edit/
│   │   │   │   └── Delete/
│   │   │   └── Queries/
│   │   │       ├── GetPosts/
│   │   │       └── GetPostById/
│   │   ├── Comments/               (similar structure)
│   │   ├── Notifications/
│   │   ├── DTOs/                   (Data Transfer Objects)
│   │   ├── Services/               (application services)
│   │   └── Events/                 (domain events)
│   │
│   ├── Domain/                      (Business entities, rules)
│   │   ├── Domain.csproj
│   │   ├── Users/
│   │   │   ├── User.cs             (entity)
│   │   │   ├── UserId.cs           (value object)
│   │   │   └── IUserRepository.cs  (interface)
│   │   ├── Posts/
│   │   ├── Comments/
│   │   ├── Notifications/
│   │   ├── Enums/
│   │   │   ├── Role.cs
│   │   │   └── Status.cs
│   │   ├── Events/                 (domain events)
│   │   └── Exceptions/             (domain exceptions)
│   │
│   ├── Infrastructure/              (External integrations, repositories)
│   │   ├── Infrastructure.csproj
│   │   ├── DependencyInjection.cs
│   │   ├── Persistence/            (database layer)
│   │   │   ├── MongoDB/
│   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   ├── Repositories/   (repository implementations)
│   │   │   │   └── Migrations/     (schema versions)
│   │   ├── Authentication/         (JWT, OAuth providers)
│   │   ├── Email/                  (SMTP service)
│   │   ├── Storage/                (image upload service)
│   │   ├── Cache/                  (Redis cache layer)
│   │   ├── Hubs/                   (SignalR hubs)
│   │   ├── Configurations/
│   │   ├── Extensions/
│   │   └── Services/               (infrastructure services)
│   │
│   ├── SharedKernel/               (Shared primitives)
│   │   ├── SharedKernel.csproj
│   │   ├── Entity.cs
│   │   ├── ValueObject.cs
│   │   ├── Exceptions/
│   │   └── Primitives/
│   │
│   └── Tests/                      (Unit & integration tests)
│       ├── Tests.csproj
│       ├── Unit/
│       │   ├── Domain/             (domain logic tests)
│       │   ├── Application/        (handler tests)
│       │   └── Infrastructure/     (repository tests)
│       ├── Integration/            (future: API integration tests)
│       ├── TestFixtures/
│       └── Common/
│
├── Dockerfile
├── .dockerignore
└── .env.example                    (environment template)
```

### Frontend Structure – Recommended New Layout

```
frontend/
├── src/
│   ├── app/                        (Next.js App Router)
│   │   ├── (auth)/                 (auth route group)
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   └── reset-password/
│   │   ├── (main)/                 (authenticated routes)
│   │   │   ├── feed/
│   │   │   ├── post/[id]/
│   │   │   ├── profile/[id]/
│   │   │   └── settings/
│   │   ├── (admin)/                (admin routes)
│   │   │   └── dashboard/
│   │   ├── layout.tsx
│   │   ├── page.tsx
│   │   ├── globals.css
│   │   └── error.tsx               (error boundary)
│   │
│   ├── components/
│   │   ├── layouts/
│   │   │   ├── MainLayout.tsx
│   │   │   └── AuthLayout.tsx
│   │   ├── features/               (feature-based organization)
│   │   │   ├── auth/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   └── AuthGuard.tsx
│   │   │   ├── posts/
│   │   │   │   ├── PostCard.tsx
│   │   │   │   ├── PostForm.tsx
│   │   │   │   ├── PostDetail.tsx
│   │   │   │   └── PostsGrid.tsx
│   │   │   ├── comments/
│   │   │   │   ├── CommentForm.tsx
│   │   │   │   ├── CommentCard.tsx
│   │   │   │   └── CommentList.tsx
│   │   │   ├── notifications/
│   │   │   │   ├── NotificationBell.tsx
│   │   │   │   ├── NotificationPanel.tsx
│   │   │   │   └── NotificationProvider.tsx
│   │   │   └── user/
│   │   │       ├── UserCard.tsx
│   │   │       ├── UserProfile.tsx
│   │   │       └── ProfileForm.tsx
│   │   ├── common/                 (reusable UI components)
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── Input.tsx
│   │   │   └── LoadingSpinner.tsx
│   │   └── Header.tsx              (top-level component)
│   │
│   ├── hooks/
│   │   ├── auth/                   (auth-related hooks)
│   │   │   ├── useAuth.ts
│   │   │   └── useLogin.ts
│   │   ├── posts/                  (post-related hooks)
│   │   │   ├── usePosts.ts
│   │   │   └── useCreatePost.ts
│   │   ├── notifications/
│   │   │   ├── useNotifications.ts
│   │   │   └── useSignalR.ts
│   │   └── useLocalStorage.ts      (general hooks)
│   │
│   ├── lib/
│   │   ├── api/                    (API client)
│   │   │   ├── graphqlClient.ts
│   │   │   └── queryClient.ts
│   │   ├── auth/                   (auth utilities)
│   │   │   └── tokenManager.ts
│   │   └── signalr/                (SignalR utilities)
│   │       └── connectionManager.ts
│   │
│   ├── types/                      (TypeScript interfaces)
│   │   ├── auth.ts
│   │   ├── post.ts
│   │   ├── user.ts
│   │   └── index.ts
│   │
│   ├── utils/                      (helper functions)
│   │   ├── formatting.ts           (date, text formatting)
│   │   ├── validation.ts           (input validation)
│   │   └── constants.ts            (app constants)
│   │
│   ├── styles/                     (global styles)
│   │   ├── globals.css
│   │   ├── variables.css
│   │   └── animations.css
│   │
│   └── store/                      (Zustand state management)
│       ├── auth.store.ts           (authentication state)
│       ├── post.store.ts           (post state)
│       └── notification.store.ts   (notification state)
│
├── .github/
│   └── workflows/
│       ├── frontend-ci.yml         (tests on PR)
│       └── frontend-deploy.yml     (deploy on merge)
│
├── public/                         (static assets)
│   └── images/
│
├── Dockerfile
├── .dockerignore
├── .env.example
├── eslintrc.json                   (linting config)
├── package.json
├── tsconfig.json
├── next.config.ts
└── tailwind.config.ts
```

### Root Level – Recommended Structure

```
BiUrSite/
├── .github/
│   ├── workflows/
│   │   ├── backend-ci.yml          (backend tests, build)
│   │   ├── frontend-ci.yml         (frontend tests, lint)
│   │   ├── docker-build.yml        (build docker images)
│   │   └── deploy.yml              (automated deployment)
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.md
│   │   ├── feature_request.md
│   │   └── question.md
│   └── pull_request_template.md    (PR guidelines)
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API.md                      (GraphQL schema docs)
│   ├── SETUP.md                    (local development)
│   ├── DEPLOYMENT.md               (deployment guide)
│   ├── CONTRIBUTING.md             (contribution guidelines)
│   └── TROUBLESHOOTING.md
│
├── backend/
│   ├── src/
│   └── ...
│
├── frontend/
│   ├── src/
│   └── ...
│
├── docker-compose.yml
├── docker-compose.prod.yml         (production config)
├── .gitignore                      (ignore node_modules, bin, obj, .env)
├── .env.example                    (template for .env)
├── .editorconfig                   (editor settings)
├── LICENSE
├── README.md                        (root-level overview)
└── CHANGELOG.md                    (release notes)
```

---

## 4. Naming Conventions

### C# (Backend)

| Element             | Convention             | Example                                           |
| ------------------- | ---------------------- | ------------------------------------------------- |
| **Classes**         | PascalCase             | `PostRepository`, `CreateUserCommand`             |
| **Interfaces**      | PascalCase + I prefix  | `IUserRepository`, `IEmailService`                |
| **Methods**         | PascalCase             | `CreateUser()`, `IsEmailVerified()`               |
| **Properties**      | PascalCase             | `Email`, `CreatedDate`                            |
| **Parameters**      | camelCase              | `userId`, `emailAddress`                          |
| **Local Variables** | camelCase              | `userCount`, `isValid`                            |
| **Constants**       | UPPER_SNAKE_CASE       | `MAX_POST_LENGTH = 5000`, `DEFAULT_ROLE = "User"` |
| **Async Methods**   | Suffix with Async      | `GetUserAsync()`, `SendEmailAsync()`              |
| **Booleans**        | Prefix with Is/Has/Can | `IsActive`, `HasVerified`, `CanDelete`            |

### TypeScript/JavaScript (Frontend)

| Element              | Convention                          | Example                                         |
| -------------------- | ----------------------------------- | ----------------------------------------------- |
| **Components**       | PascalCase                          | `PostCard`, `UserProfile`, `AuthGuard`          |
| **Hooks**            | camelCase + use prefix              | `useAuth()`, `usePosts()`, `useNotifications()` |
| **Utilities**        | camelCase                           | `formatDate()`, `validateEmail()`               |
| **Constants**        | UPPER_SNAKE_CASE                    | `API_BASE_URL`, `MAX_IMAGE_SIZE`                |
| **Types/Interfaces** | PascalCase                          | `User`, `Post`, `Comment`                       |
| **Files**            | kebab-case (components: PascalCase) | `user-profile.ts`, `UserProfile.tsx`            |
| **Variables**        | camelCase                           | `isPending`, `userList`, `selectedPost`         |
| **Enums**            | PascalCase                          | `UserRole`, `NotificationStatus`                |

### Database (MongoDB)

| Element         | Convention            | Example                                       |
| --------------- | --------------------- | --------------------------------------------- |
| **Collections** | lowercase, singular   | `user`, `post`, `comment`                     |
| **Fields**      | camelCase             | `userId`, `createdAt`, `comments`             |
| **IDs**         | Suffix with Id        | `userId`, `postId`, `commentId`               |
| **Booleans**    | Prefix is prefix      | `isVerified`, `isDeleted`, `hasImage`         |
| **Timestamps**  | Suffix with Date\*\*  | `createdAt`, `modifiedAt`, `deletedAt`        |
| **Arrays**      | Plural or descriptive | `comments`, `recentComments`, `notifications` |

### Git Commits

See section 5 below.

---

## 5. Commit Conventions

### Format: Conventional Commits

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Examples

```
feat(auth): implement OAuth2 Google login

- Add Google OAuth2 provider configuration
- Create OAuth callback handler
- Add user auto-verification on OAuth
- Update authentication service

Closes #42
```

```
fix(posts): prevent XSS in post text

Sanitize user input using DOMPurify before rendering.
Escapes HTML tags and prevents script injection.

Fixes #125
```

```
docs(api): add GraphQL schema documentation

Generate schema documentation from GraphQL introspection.
Include examples for common queries and mutations.
```

### Type Reference

| Type         | Meaning                               | Example                                      |
| ------------ | ------------------------------------- | -------------------------------------------- |
| **feat**     | New feature                           | `feat(posts): add image upload`              |
| **fix**      | Bug fix                               | `fix(auth): correct JWT expiry`              |
| **docs**     | Documentation                         | `docs(setup): add local development guide`   |
| **style**    | Formatting (no logic change)          | `style(format): align indentation`           |
| **refactor** | Code restructure (no behavior change) | `refactor(auth): extract OAuth logic`        |
| **perf**     | Performance improvement               | `perf(db): add index on created_at`          |
| **test**     | Add or modify tests                   | `test(posts): add create post handler tests` |
| **chore**    | Maintenance, dependencies             | `chore(deps): upgrade graphql to v16`        |
| **ci**       | CI/CD configuration                   | `ci: add GitHub Actions workflow`            |

### Commit Best Practices

1. **Atomic commits:** One logical change per commit
2. **Frequent commits:** Small, reviewable changes
3. **Descriptive subject:** < 50 characters, imperative mood ("add", not "added")
4. **Detailed body:** Explain _why_, not _what_
5. **Reference issues:** Use `Closes #123` in footer
6. **No merge commits:** Rebase and squash for clean history

---

## 6. Professional README Template

````markdown
# BiUrSite

> A modern web platform for sharing ideas, seeking advice, and building community through anonymous and verified interactions.

[![Build Status](https://github.com/YourOrg/BiUrSite/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/YourOrg/BiUrSite/actions)
[![Frontend Tests](https://github.com/YourOrg/BiUrSite/actions/workflows/frontend-ci.yml/badge.svg)](https://github.com/YourOrg/BiUrSite/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Contributors](https://img.shields.io/github/contributors/YourOrg/BiUrSite)](https://github.com/YourOrg/BiUrSite/graphs/contributors)

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Features

- ✅ **User Authentication:**
  - Email/password registration with email verification
  - OAuth2 (Google, Facebook) sign-in
  - JWT-based stateless authentication
- ✅ **Post Management:**
  - Create, edit, delete posts (text + images)
  - Public feed with pagination and search
  - Post details with inline comments

- ✅ **Comments & Discussions:**
  - Comment on posts with @mentions (future)
  - Edit/delete own comments
  - Real-time comment notifications

- ✅ **Notifications:**
  - Real-time notifications via SignalR
  - Notification preferences per user
  - Offline notification storage

- ✅ **Admin Moderation:**
  - User management (ban users)
  - Content moderation (remove posts/comments)
  - Moderation dashboard with analytics

### Planned Features (Phase 2+)

- Post ratings & voting
- User reputation system
- Advanced search & filtering
- In-app messaging
- Mobile applications

## Tech Stack

### Backend

- **Runtime:** .NET 8.0
- **API:** ASP.NET Core (GraphQL + REST)
- **Database:** MongoDB
- **Cache:** Redis
- **Real-Time:** SignalR
- **Email:** SMTP
- **Auth:** JWT + OAuth2
- **Testing:** xUnit, Moq

### Frontend

- **Framework:** Next.js 15
- **Language:** TypeScript
- **Styling:** Tailwind CSS
- **State:** Zustand
- **API Client:** Apollo Client + GraphQL
- **Real-Time:** SignalR client
- **UI Components:** Headless (custom + shadcn/ui)

### DevOps

- **Containerization:** Docker & Docker Compose
- **Hosting:** Render (backend), Netlify/Vercel (frontend)
- **CI/CD:** GitHub Actions
- **Image Storage:** Cloudinary / AWS S3
- **Secrets Management:** Environment variables

## Quick Start

### Prerequisites

- Node.js 18+ (frontend)
- .NET 8.0+ SDK (backend)
- Docker & Docker Compose
- Git

### Local Development

1. **Clone the repository:**
   ```bash
   git clone https://github.com/YourOrg/BiUrSite.git
   cd BiUrSite
   ```
````

2. **Set up environment:**

   ```bash
   cp .env.example .env
   # Edit .env with your local config
   ```

3. **Start Docker services (MongoDB, Redis):**

   ```bash
   docker-compose up -d
   ```

4. **Backend setup:**

   ```bash
   cd backend
   dotnet restore
   dotnet build
   dotnet run --project API/Api.csproj
   # API available at http://localhost:8080
   # GraphQL at http://localhost:8080/graphql
   ```

5. **Frontend setup:**

   ```bash
   cd frontend
   npm install
   npm run dev
   # UI available at http://localhost:3000
   ```

6. **Open application:**
   - Frontend: http://localhost:3000
   - GraphQL Playground: http://localhost:8080/graphql

For detailed setup, see [SETUP.md](docs/SETUP.md)

## Project Structure

```
BiUrSite/
├── backend/          # .NET API (GraphQL + REST)
├── frontend/         # Next.js web application
├── docs/            # Documentation
└── docker-compose.yml
```

See [PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) for detailed breakdown.

## Development

### Code Style

We follow clean code principles and standard conventions:

- **C#:** PascalCase for classes/methods, camelCase for variables
- **TypeScript:** camelCase for functions, PascalCase for components
- **MongoDB:** camelCase for fields, singular collection names

### Linting & Formatting

```bash
# Backend (C#)
dotnet format

# Frontend (TS/JS)
npm run lint
npm run lint:fix
```

### Pre-commit Hooks

Install git hooks (husky) to lint before commit:

```bash
npm run prepare
```

## Testing

### Backend

```bash
cd backend
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test file
dotnet test Tests/Unit/Domain/UserTests.cs
```

### Frontend

```bash
cd frontend
# Run Jest tests
npm test

# Run with coverage
npm test -- --coverage

# Watch mode
npm test -- --watch
```

### Integration Testing

```bash
# Run docker-compose, run tests, clean up
npm run test:integration
```

## Deployment

### Architecture Diagram

[Insert ASCII diagram or Mermaid diagram of architecture]

### Backend Deployment (Render)

1. Push to `main` branch
2. GitHub Actions builds Docker image
3. Push to Docker Hub
4. Render detects and deploys automatically

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed instructions.

### Frontend Deployment (Netlify/Vercel)

```bash
npm run build
npm run export  # (if using static export)
```

Automatic deployment on git push to `main`.

### Environment Variables

| Variable                    | Backend | Frontend | Example                     |
| --------------------------- | ------- | -------- | --------------------------- |
| `NEXT_PUBLIC_API_URL`       | -       | ✓        | `https://api.biursite.com`  |
| `MONGODB_CONNECTION_STRING` | ✓       | -        | `mongodb://localhost:27017` |
| `JWT_SECRET_KEY`            | ✓       | -        | (generate random)           |
| `REDIS_CONNECTION_STRING`   | ✓       | -        | `redis://localhost:6379`    |

See `.env.example` for complete list.

## API Documentation

### GraphQL Schema

Auto-generated documentation available at:

- Local: http://localhost:8080/graphql
- Production: https://api.biursite.com/graphql

[Or link to external docs](docs/API.md)

### Example Queries

```graphql
# Fetch recent posts
query GetPosts($pageNumber: Int!) {
  posts(pageNumber: $pageNumber) {
    id
    text
    author {
      username
      avatar
    }
    createdAt
  }
}

# Create a post
mutation CreatePost($text: String!) {
  createPost(text: $text) {
    id
    text
    createdAt
  }
}
```

## Architecture Decisions

Key architectural choices are documented as ADRs (Architecture Decision Records):

- [ADR-001: Use GraphQL over REST](docs/adr/001-graphql.md)
- [ADR-002: MongoDB for document storage](docs/adr/002-mongodb.md)
- [ADR-003: SignalR for real-time notifications](docs/adr/003-signalr.md)

See [docs/adr/](docs/adr/) for all ADRs.

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines on:

- Code of conduct
- Pull request process
- Development workflow
- Commit message format
- Issue reporting

### Quick Contribution Steps

1. Fork and clone the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m "feat: add amazing feature"`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a pull request

## Roadmap

### Phase 1 (Current) ✅

- Basic CRUD operations
- User authentication
- Notifications

### Phase 2 (Q1 2026)

- Admin moderation dashboard
- Post ratings
- Advanced search
- Email notifications

### Phase 3 (Q2 2026)

- Mobile applications
- Reputation system
- User recommendations
- API rate limiting enhancements

See [ROADMAP.md](docs/ROADMAP.md) for detailed timeline.

## Security

We take security seriously:

- All passwords hashed with bcrypt
- JWT tokens for stateless auth
- HTTPS/TLS in production
- Rate limiting on all endpoints
- Regular dependency updates
- Security policy: [SECURITY.md](SECURITY.md)

Report security vulnerabilities responsibly:
📧 [security@biursite.example.com](mailto:security@biursite.example.com)

## Performance

### Key Metrics

- API response time: < 500ms (p95)
- Page load time: < 3s
- Real-time notification latency: < 100ms
- Database query time: < 50ms (p95)

### Optimization Strategies

- MongoDB indexes on frequently queried fields
- Redis caching for user profiles & feeds
- Image optimization & CDN
- GraphQL query optimization

See [PERFORMANCE.md](docs/PERFORMANCE.md) for benchmarks.

## Troubleshooting

Common issues and solutions:

- Port already in use
- Database connection failures
- Docker build errors
- GraphQL query fails

See [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) for solutions.

## Monitoring & Logging

### Application Logs

```bash
# Backend
docker logs biursite-backend

# Frontend (build logs)
npm run build 2>&1 | tee build.log
```

### Metrics

- Application Performance Monitoring (APM): [Add service]
- Error tracking: Sentry (future)
- Analytics: Google Analytics / Segment (future)

## FAQ

**Q: Can I self-host BiUrSite?**
A: Yes! See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for self-hosting guide.

**Q: How do I report a bug?**
A: Open an issue using the [bug report template](.github/ISSUE_TEMPLATE/bug_report.md).

**Q: Is there a public demo?**
A: Yes, at https://biursite.vercel.app (staging environment).

See [FAQ.md](docs/FAQ.md) for more questions.

## License

This project is licensed under the MIT License – see [LICENSE](LICENSE) file for details.

## Support

- 📖 **Documentation:** [docs/](docs/)
- 🐛 **Issues:** [GitHub Issues](https://github.com/YourOrg/BiUrSite/issues)
- 💬 **Discussions:** [GitHub Discussions](https://github.com/YourOrg/BiUrSite/discussions)
- 📧 **Email:** support@biursite.example.com

## Acknowledgments

- Built with [.NET](https://dot.net)
- Frontend powered by [Next.js](https://nextjs.org)
- Real-time features via [SignalR](https://aspnet.core/signalr)
- Database by [MongoDB](https://mongodb.com)
- Styling with [Tailwind CSS](https://tailwindcss.com)

---

**Last Updated:** February 22, 2026  
**Status:** Active Development

````

---

## 7. GitHub Configuration

### .github/workflows/backend-ci.yml

```yaml
name: Backend CI

on:
  push:
    branches: [main, develop]
    paths:
      - 'backend/**'
      - .github/workflows/backend-ci.yml
  pull_request:
    branches: [main, develop]
    paths:
      - 'backend/**'

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    services:
      mongodb:
        image: mongo:6
        options: >-
          --health-cmd mongosh
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 27017:27017

      redis:
        image: redis:7-alpine
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore backend/

      - name: Build
        run: dotnet build backend/ --no-restore --configuration Release

      - name: Run unit tests
        run: dotnet test backend/Tests/ --no-build --configuration Release --logger "xunit;LogFileName=test-results.xml"

      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: failure()
        with:
          name: test-results
          path: backend/Tests/test-results.xml

      - name: Code coverage
        run: dotnet test backend/Tests/ /p:CollectCoverage=true /p:CoverageFormat=cobertura

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./backend/Tests/coverage.cobertura.xml
````

### .github/pull_request_template.md

```markdown
## Description

Brief description of changes. Link to related issue:
Closes #ISSUE_NUMBER

## Changes Made

- [ ] Item 1
- [ ] Item 2
- [ ] Item 3

## Type of Change

- [ ] Bug fix (non-breaking)
- [ ] New feature (non-breaking)
- [ ] Breaking change
- [ ] Documentation update
- [ ] Refactoring (no feature change)

## Testing

- [ ] Unit tests added/updated
- [ ] Integration tests added
- [ ] Manual testing performed

**Test Coverage:**

- New code coverage: XX%
- Overall coverage: XX%

## Screenshots (if UI change)

[Add screenshots if applicable]

## Checklist

- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests pass locally
- [ ] Commits follow conventional format

## Reviewer Notes

Any special instructions or areas needing review.
```

### .gitignore (Root)

```
# Local environment
.env
.env.local
*.local

# IDE
.vscode/
.idea/
*.swp
*.swo
*~
.DS_Store

# Build artifacts
bin/
obj/
dist/
build/

# Dependencies
node_modules/
.npm
package-lock.json
yarn.lock

# Logs
logs/
*.log
npm-debug.log*

# Temporary files
temp/
tmp/
*.tmp

# Docker
.docker/
docker-compose.override.yml
```

### .editorconfig

```ini
root = true

# All files
[*]
charset = utf-8
indent_style = space
indent_size = 2
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

# C# files
[*.cs]
indent_size = 4

# JSON files
[*.json]
indent_size = 2

# Markdown files
[*.md]
trim_trailing_whitespace = false
```

---

## Summary

| Area                  | Recommendation                            | Priority |
| --------------------- | ----------------------------------------- | -------- |
| **Branch Strategy**   | Implement Git Flow                        | High     |
| **CI/CD**             | Add GitHub Actions workflows              | High     |
| **Documentation**     | Create SETUP.md, CONTRIBUTING.md, etc.    | High     |
| **Code Organization** | Refactor frontend by features             | Medium   |
| **Testing**           | Increase test coverage target to 80%+     | High     |
| **Security**          | Add SECURITY.md, enable branch protection | High     |
| **Error Handling**    | Add logging/monitoring dashboards         | Medium   |
| **Commit Standards**  | Enforce conventional commits              | Medium   |

---

**Document End**
