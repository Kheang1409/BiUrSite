# BiUrSite – High-Level System Architecture (Lab 3)

**Document Version:** 1.0  
**Date:** February 28, 2026  
**Project:** BiUrSite – Anonymous Idea & Advice Sharing Platform  
**Phase:** Inception → Elaboration (Architectural Analysis)  
**Status:** Initial Draft – First Iteration

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Architectural Goals & Constraints](#2-architectural-goals--constraints)
3. [High-Level System Architecture Diagram](#3-high-level-system-architecture-diagram)
4. [Architectural Views](#4-architectural-views)
5. [Layer Descriptions](#5-layer-descriptions)
6. [Component Interaction Flows](#6-component-interaction-flows)
7. [Technology Stack Justification](#7-technology-stack-justification)
8. [Deployment Architecture](#8-deployment-architecture)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [Architectural Decisions & Trade-Offs](#10-architectural-decisions--trade-offs)
11. [Mapping Use Cases to Architecture](#11-mapping-use-cases-to-architecture)
12. [Risk Assessment](#12-risk-assessment)
13. [References](#13-references)

---

## 1. Introduction

### 1.1 Purpose

This document presents the **initial high-level system architecture** for BiUrSite, produced as part of the first iteration of Architectural Analysis (Lab 3). It translates the requirements captured in the Vision document, the SRS, and the Use-Case Model into a concrete solution architecture that addresses all identified functional and non-functional requirements.

### 1.2 Scope

This architecture covers:

- The overall system structure and its major components
- The interaction between layers and external services
- Technology choices and their rationale
- Deployment topology and containerization strategy
- How the architecture satisfies the requirements from the Use-Case Model and Supplementary Specifications

### 1.3 Context

BiUrSite is a cloud-native web platform for anonymous idea-sharing and community engagement. The architectural analysis considers:

- **23 use cases** organized into 6 packages (Authentication, Content, Comments, Notifications, Profile, Administration)
- **3 primary actors** (Anonymous User, Verified User, Admin) and **3 external actors** (Email Service, OAuth Provider, Image Storage)
- **Non-functional requirements** for performance (< 500ms API response), scalability (1,000+ concurrent users), security (JWT, RBAC, rate limiting), and reliability (99.5% uptime)

### 1.4 Definitions

See the [Glossary in the Use-Case Model](02_USE_CASES_AND_DESCRIPTIONS.md#glossary) for complete terminology definitions.

---

## 2. Architectural Goals & Constraints

### 2.1 Architectural Goals

| Goal                        | Description                                                       | Driven By                    |
| --------------------------- | ----------------------------------------------------------------- | ---------------------------- |
| **Separation of Concerns**  | Isolate domain logic from infrastructure and presentation         | Maintainability, Testability |
| **Scalability**             | Support horizontal scaling of API and real-time services          | SS-SC1 through SS-SC5        |
| **Real-Time Communication** | Deliver instant notifications to connected users                  | UC16 (Receive Notifications) |
| **Security**                | Protect user data, prevent common web attacks                     | SS-S1 through SS-S10         |
| **Developer Productivity**  | Clean architecture with CQRS enables parallel feature development | Team velocity                |
| **Cloud-Native Deployment** | Containerized services deployable to any cloud provider           | SS-DC5                       |
| **API-First Design**        | Single GraphQL endpoint for all data operations                   | SS-DC3, Frontend flexibility |

### 2.2 Constraints

| Constraint                 | Type           | Impact                                                                             |
| -------------------------- | -------------- | ---------------------------------------------------------------------------------- |
| **.NET 10.0 Runtime**      | Technology     | Backend must target .NET 10.0; determines NuGet ecosystem                          |
| **MongoDB (NoSQL)**        | Technology     | Document-oriented storage; no relational joins; embedded subdocument patterns      |
| **GraphQL Only (No REST)** | Design         | All client-server data exchange through `/graphql`; no traditional REST endpoints  |
| **JWT Stateless Auth**     | Design         | No server-side session store; token contains all claims; 24-hour expiry            |
| **Docker Deployment**      | Infrastructure | Services must be containerizable; environment configuration via env vars           |
| **Budget**                 | Resource       | Use free/low-cost hosting tiers (Render, Netlify); limit paid service dependencies |
| **Team Size**              | Resource       | Small development team; architecture must enable independent module development    |

### 2.3 Guiding Principles

1. **Clean Architecture (Onion Model):** Dependencies point inward; domain layer has zero external dependencies
2. **CQRS:** Separate read and write paths for clarity and optimization
3. **Event-Driven:** Domain events trigger asynchronous side effects (email, image upload, notifications)
4. **Fail Gracefully:** External service failures (SMTP, image storage) must not crash core API
5. **Security by Default:** All write operations require authentication; rate limiting on all endpoints

---

## 3. High-Level System Architecture Diagram

### 3.1 System Context Diagram

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│                              EXTERNAL ACTORS                                          │
│                                                                                       │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌───────────┐    ┌──────────────┐   │
│  │ Anonymous │    │ Verified │    │  Admin   │    │  OAuth    │    │ Email (SMTP) │   │
│  │   User   │    │   User   │    │          │    │ Provider  │    │   Service    │   │
│  └────┬─────┘    └────┬─────┘    └────┬─────┘    │(Google/FB)│    └──────────────┘   │
│       │               │               │          └───────────┘                        │
└───────┼───────────────┼───────────────┼──────────────────────────────────────────────┘
        │               │               │
        ▼               ▼               ▼
┌──────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                       │
│                          BiUrSite SYSTEM BOUNDARY                                     │
│                                                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                    PRESENTATION TIER                                              │  │
│  │                                                                                   │  │
│  │  ┌─────────────────────────────────────────────────────────────────────────────┐  │  │
│  │  │              Next.js Frontend (React + TypeScript)                           │  │  │
│  │  │                                                                              │  │  │
│  │  │  ┌──────────┐  ┌───────────┐  ┌──────────┐  ┌────────────┐  ┌───────────┐  │  │  │
│  │  │  │  Pages/  │  │Components/│  │  Hooks/  │  │  Services/ │  │   Lib/    │  │  │  │
│  │  │  │  Routes  │  │  UI Layer │  │  State   │  │ GraphQL API│  │ JWT/Theme │  │  │  │
│  │  │  └──────────┘  └───────────┘  └──────────┘  └────────────┘  └───────────┘  │  │  │
│  │  │                                                                              │  │  │
│  │  │  Communication: Apollo Client (GraphQL) + SignalR Client (WebSocket)         │  │  │
│  │  └─────────────────────────────────────────────────────────────────────────────┘  │  │
│  │                         │ GraphQL (HTTPS)          │ WebSocket (WSS)              │  │
│  └─────────────────────────┼─────────────────────────┼──────────────────────────────┘  │
│                             │                         │                                 │
│  ┌──────────────────────────▼─────────────────────────▼──────────────────────────────┐  │
│  │                    APPLICATION TIER                                                │  │
│  │                                                                                    │  │
│  │  ┌──────────────────── ASP.NET Core Web Host ──────────────────────────────────┐  │  │
│  │  │                                                                              │  │  │
│  │  │  ┌── API Layer (Entry Point) ──────────────────────────────────────────────┐ │  │  │
│  │  │  │  /graphql ──► HotChocolate (Query + Mutation resolvers)                 │ │  │  │
│  │  │  │  /notificationHub ──► SignalR Hub (real-time notifications)              │ │  │  │
│  │  │  │  /feedHub ──► SignalR Hub (real-time feed updates)                       │ │  │  │
│  │  │  │  /health ──► Health Check endpoint                                      │ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  Middleware Pipeline:                                                    │ │  │  │
│  │  │  │  ForwardedHeaders → SecurityHeaders → CorrelationId →                   │ │  │  │
│  │  │  │  GlobalException → RateLimiting → RequestDiagnostics                    │ │  │  │
│  │  │  └──────────────────────────────────────────────────────────────────────────┘ │  │  │
│  │  │                              │ MediatR Dispatch                               │  │  │
│  │  │  ┌── Application Layer ─────▼──────────────────────────────────────────────┐ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │ │  │  │
│  │  │  │  │  Commands   │  │   Queries    │  │  Validators  │  │    DTOs     │  │ │  │  │
│  │  │  │  │(CreatePost, │  │(GetPosts,    │  │(FluentValid.)│  │(PostDto,    │  │ │  │  │
│  │  │  │  │ Register,   │  │ GetUser,     │  │              │  │ UserDto,    │  │ │  │  │
│  │  │  │  │ Login, etc.)│  │ GetComments) │  │              │  │ CommentDto) │  │ │  │  │
│  │  │  │  └─────────────┘  └──────────────┘  └──────────────┘  └─────────────┘  │ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  MediatR Pipeline Behaviors:                                             │ │  │  │
│  │  │  │  [Logging] → [Idempotency] → [Validation] → [Handler]                   │ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  Messaging Handlers (Rebus / Async):                                     │ │  │  │
│  │  │  │  SendVerificationEmail, UploadImage, SendToFeed,                         │ │  │  │
│  │  │  │  SendOTP, NotifyPostOwner, DeleteImage                                   │ │  │  │
│  │  │  └──────────────────────────────────────────────────────────────────────────┘ │  │  │
│  │  │                              │ Repository Interfaces                          │  │  │
│  │  │  ┌── Domain Layer ──────────▼──────────────────────────────────────────────┐ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │ │  │  │
│  │  │  │  │  Entities   │  │ Value Objects│  │Domain Events │  │ Repository  │  │ │  │  │
│  │  │  │  │(User, Post, │  │(PostId,Token,│  │(PostCreated, │  │ Interfaces  │  │ │  │  │
│  │  │  │  │ Comment,    │  │ Otp, ImageId)│  │ UserCreated, │  │(IUserRepo,  │  │ │  │  │
│  │  │  │  │ Notification│  │              │  │ CommentAdded)│  │ IPostRepo)  │  │ │  │  │
│  │  │  │  │ Image)      │  │              │  │              │  │             │  │ │  │  │
│  │  │  │  └─────────────┘  └──────────────┘  └──────────────┘  └─────────────┘  │ │  │  │
│  │  │  │  Enums: Role (User, Admin), Status (Unverified, Active, Banned, etc.)   │ │  │  │
│  │  │  └──────────────────────────────────────────────────────────────────────────┘ │  │  │
│  │  │                                                                              │  │  │
│  │  │  ┌── Infrastructure Layer ─────────────────────────────────────────────────┐ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  ┌─────────────────┐  ┌──────────────┐  ┌──────────────────────────┐    │ │  │  │
│  │  │  │  │  Persistence    │  │Authentication│  │  External Integrations   │    │ │  │  │
│  │  │  │  │  ─────────────  │  │──────────────│  │  ──────────────────────  │    │ │  │  │
│  │  │  │  │  MongoDbContext  │  │JwtTokenSvc   │  │  GitHubImageStorage     │    │ │  │  │
│  │  │  │  │  UserRepository │  │Sha512Hasher  │  │  SMTP EmailService      │    │ │  │  │
│  │  │  │  │  PostRepository │  │GoogleOAuth   │  │  Rebus (MongoDB MQ)     │    │ │  │  │
│  │  │  │  │  CommentRepo    │  │FacebookOAuth │  │  OutboxProcessor        │    │ │  │  │
│  │  │  │  │  NotifRepo      │  │SignalRUserId │  │  RedisRateLimiter       │    │ │  │  │
│  │  │  │  │  IndexInit      │  │              │  │  RedisIdempotencyStore  │    │ │  │  │
│  │  │  │  │  TransactionScp │  │              │  │                          │    │ │  │  │
│  │  │  │  └─────────────────┘  └──────────────┘  └──────────────────────────┘    │ │  │  │
│  │  │  │                                                                          │ │  │  │
│  │  │  │  ┌──────────────────┐  ┌──────────────────────────────────────────────┐  │ │  │  │
│  │  │  │  │  SignalR Hubs    │  │  Resilience & Configuration                  │  │ │  │  │
│  │  │  │  │  ──────────────  │  │  ──────────────────────────                  │  │ │  │  │
│  │  │  │  │  FeedHub         │  │  RetryPolicy, CorsConfig, Swagger,           │  │ │  │  │
│  │  │  │  │  NotificationHub │  │  StronglyTypedIdSerializer                   │  │ │  │  │
│  │  │  │  └──────────────────┘  └──────────────────────────────────────────────┘  │ │  │  │
│  │  │  └──────────────────────────────────────────────────────────────────────────┘ │  │  │
│  │  └──────────────────────────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────────────────────────┘  │
│                             │                          │                                 │
│  ┌──────────────────────────▼──────────────────────────▼──────────────────────────────┐  │
│  │                    DATA TIER                                                       │  │
│  │                                                                                    │  │
│  │  ┌────────────────────────┐   ┌────────────────────────┐                          │  │
│  │  │       MongoDB          │   │        Redis            │                          │  │
│  │  │  ──────────────────    │   │  ──────────────────     │                          │  │
│  │  │  • users collection    │   │  • Rate limit counters  │                          │  │
│  │  │  • posts collection    │   │  • Idempotency keys     │                          │  │
│  │  │  • outbox collection   │   │  • Cache (profiles,     │                          │  │
│  │  │  • rebus queues/subs   │   │    feed listings)       │                          │  │
│  │  │                        │   │  • SignalR backplane     │                          │  │
│  │  │  Indexes:              │   │    (multi-instance)      │                          │  │
│  │  │  • email (unique)      │   │                          │                          │  │
│  │  │  • createdAt (desc)    │   └────────────────────────┘                          │  │
│  │  │  • status + createdAt  │                                                        │  │
│  │  │  • text (search index) │                                                        │  │
│  │  └────────────────────────┘                                                        │  │
│  └────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                          │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Architecture Style

BiUrSite follows a **3-Tier Architecture** combined with **Clean Architecture (Onion Model)** internally:

```
                    ┌─────────────────────────────┐
                    │      Presentation Tier       │
                    │    (Next.js SPA / SSR)        │
                    └──────────────┬──────────────┘
                                   │
                          GraphQL + WebSocket
                                   │
                    ┌──────────────▼──────────────┐
                    │      Application Tier        │
                    │    (ASP.NET Core Host)        │
                    │                               │
                    │  ┌── API ──────────────────┐  │
                    │  │ GraphQL │ SignalR │ MW   │  │
                    │  └────────┬────────────────┘  │
                    │           │                    │
                    │  ┌────────▼────────────────┐  │
                    │  │    Application Layer     │  │
                    │  │  (CQRS + MediatR)       │  │
                    │  └────────┬────────────────┘  │
                    │           │                    │
                    │  ┌────────▼────────────────┐  │
                    │  │     Domain Layer         │  │
                    │  │ (Entities, Events, IFs)  │  │
                    │  └────────┬────────────────┘  │
                    │           │                    │
                    │  ┌────────▼────────────────┐  │
                    │  │  Infrastructure Layer    │  │
                    │  │ (MongoDB, Redis, SMTP)   │  │
                    │  └─────────────────────────┘  │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │        Data Tier             │
                    │  (MongoDB + Redis)            │
                    └─────────────────────────────┘
```

---

## 4. Architectural Views

### 4.1 Logical View (Package Diagram)

The backend is organized into **5 .NET projects** with strict dependency rules:

```
┌─────────────────────────────────────────────────────────┐
│                                                           │
│   ┌───────────┐         ┌──────────────────┐             │
│   │    API    │────────►│   Application    │             │
│   │ (Entry)   │         │  (Use Cases)     │             │
│   └─────┬─────┘         └────────┬─────────┘             │
│         │                        │                        │
│         │                        ▼                        │
│         │               ┌──────────────────┐             │
│         │               │     Domain       │             │
│         │               │ (Core Entities)  │             │
│         │               └──────────────────┘             │
│         │                        ▲                        │
│         │                        │                        │
│         ▼                        │                        │
│   ┌──────────────────────────────┘                       │
│   │  Infrastructure                                       │
│   │  (Persistence, Auth, Hubs, Storage, Messaging)        │
│   └──────────────────────────────────────────────────────│
│                                                           │
│   ┌───────────────┐                                       │
│   │ SharedKernel   │ (Cross-cutting exceptions)           │
│   └───────────────┘                                       │
└─────────────────────────────────────────────────────────┘

Dependency Rules:
  ✓ API → Application, Infrastructure
  ✓ Application → Domain, SharedKernel
  ✓ Infrastructure → Domain, Application
  ✗ Domain → NOTHING (zero external dependencies)
```

### 4.2 Process View (Runtime Behavior)

**Request Processing Pipeline:**

```
Client Request
      │
      ▼
[ForwardedHeaders Middleware]
      │
      ▼
[SecurityHeaders Middleware] ──► Adds X-Content-Type-Options, X-Frame-Options, etc.
      │
      ▼
[CorrelationId Middleware] ──► Generates/propagates X-Correlation-Id
      │
      ▼
[GlobalException Middleware] ──► Catches all exceptions → ProblemDetails JSON
      │
      ▼
[RateLimiting Middleware] ──► Redis sliding window check (100 req/min)
      │                          │
      │                    [429 Too Many Requests] (if exceeded)
      ▼
[HotChocolate GraphQL Engine]
      │
      ▼
[Authorization Check] ──► JWT validation, role check
      │
      ▼
[MediatR Dispatch]
      │
      ├──► [LoggingBehavior] ──► Log request/response
      ├──► [IdempotencyBehavior] ──► Check Redis for duplicate command
      ├──► [ValidationBehavior] ──► FluentValidation rules
      │
      ▼
[Command/Query Handler]
      │
      ├──► Repository (MongoDB read/write)
      ├──► Domain Event raised
      │       │
      │       ▼
      │    [Outbox] ──► [OutboxProcessor] ──► [Rebus Message Bus]
      │                                            │
      │                         ┌──────────────────┼──────────────────┐
      │                         ▼                  ▼                  ▼
      │                   [Send Email]      [Upload Image]    [SignalR Push]
      │
      ▼
[GraphQL Response] ──► Client
```

### 4.3 Physical / Deployment View

See [Section 8 – Deployment Architecture](#8-deployment-architecture).

### 4.4 Data View

```
┌─ MongoDB ──────────────────────────────────────────────────┐
│                                                             │
│  ┌─ users ─────────────────┐  ┌─ posts ──────────────────┐ │
│  │ _id: ObjectId           │  │ _id: ObjectId            │ │
│  │ id: UUID (UserId)       │  │ id: UUID (PostId)        │ │
│  │ username: string        │  │ userId: UUID             │ │
│  │ email: string (unique)  │  │ user: { embedded author }│ │
│  │ password: string (hash) │  │ text: string             │ │
│  │ profile: { Image }      │  │ image: { Image }         │ │
│  │ bio: string             │  │ status: enum             │ │
│  │ phone: string?          │  │ comments: [              │ │
│  │ status: enum            │  │   { Comment embedded }   │ │
│  │ role: enum              │  │ ]                        │ │
│  │ authProvider: string    │  │ createdDate: DateTime    │ │
│  │ otp: { Otp }            │  │ modifiedDate: DateTime?  │ │
│  │ token: { Token }        │  │ deletedDate: DateTime?   │ │
│  │ notifications: [        │  └──────────────────────────┘ │
│  │   { Notification emb }  │                               │
│  │ ]                       │  ┌─ outbox ─────────────────┐ │
│  │ hasNewNotification:bool │  │ _id: ObjectId            │ │
│  │ createdDate: DateTime   │  │ eventType: string        │ │
│  │ modifiedDate: DateTime? │  │ payload: BSON            │ │
│  │ deletedDate: DateTime?  │  │ processedAt: DateTime?   │ │
│  └─────────────────────────┘  │ createdAt: DateTime      │ │
│                                └──────────────────────────┘ │
│  ┌─ rebus_subscriptions ──┐  ┌─ rebus_messages ──────────┐ │
│  │ (Rebus internal)       │  │ (Rebus internal)          │ │
│  └────────────────────────┘  └───────────────────────────┘ │
│                                                             │
│  Indexes:                                                   │
│  • users: email (unique), token.value, otp.value            │
│  • posts: status + createdDate (compound), text (search)    │
│  • outbox: processedAt (partial sparse)                     │
└─────────────────────────────────────────────────────────────┘

┌─ Redis ────────────────────────────────────────────────────┐
│  • rate_limit:{userId|IP}:{path} → counter (TTL window)   │
│  • idempotency:{commandId} → result (TTL)                  │
│  • cache:user:{userId} → serialized user (TTL 1hr)         │
│  • cache:feed:page:{n} → serialized posts (TTL 30min)      │
│  • signalr:backplane:* → SignalR internal pub/sub           │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Layer Descriptions

### 5.1 Presentation Tier — Next.js Frontend

| Aspect                   | Detail                                                                                       |
| ------------------------ | -------------------------------------------------------------------------------------------- |
| **Framework**            | Next.js 15 (App Router) with React 19 and TypeScript                                         |
| **Styling**              | Tailwind CSS with dark/light theme support                                                   |
| **API Client**           | Apollo Client for GraphQL; `@microsoft/signalr` for real-time                                |
| **State Management**     | Apollo InMemoryCache with custom merge policies; React hooks                                 |
| **Authentication**       | JWT stored in localStorage; auto-expiry detection; Bearer header injection                   |
| **Architecture Pattern** | Atomic Design (atoms → molecules → organisms) + feature-based page components                |
| **Key Routes**           | `/` (Feed), `/login`, `/register`, `/people`, `/post/[id]`, `/profile`, `/profile/user/[id]` |

**Responsibilities:**

- Render UI with responsive, accessible design (WCAG 2.1 AA)
- Manage client-side routing and page transitions
- Execute GraphQL queries/mutations via Apollo Client
- Maintain real-time SignalR connection for notifications
- Handle optimistic UI updates for instant user feedback
- Implement infinite scroll with IntersectionObserver

### 5.2 Application Tier — ASP.NET Core Backend

#### 5.2.1 API Layer (Entry Point)

**Responsibilities:**

- Host the HTTP pipeline with middleware stack
- Expose GraphQL endpoint via HotChocolate (8 queries, 13+ mutations)
- Host SignalR hubs for real-time communication
- Apply cross-cutting concerns (security headers, correlation IDs, rate limiting, exception handling)

**Endpoints:**

| Endpoint           | Protocol | Purpose                          |
| ------------------ | -------- | -------------------------------- |
| `/graphql`         | HTTPS    | All data queries and mutations   |
| `/notificationHub` | WSS      | Per-user real-time notifications |
| `/feedHub`         | WSS      | Global real-time feed updates    |
| `/health`          | HTTPS    | Health check for load balancers  |

#### 5.2.2 Application Layer (Use Cases / CQRS)

**Responsibilities:**

- Implement all use cases as MediatR commands and queries
- Enforce validation rules via FluentValidation
- Coordinate domain operations and repository calls
- Publish domain events for async processing
- Map domain entities to DTOs for API response

**Feature Folders:**

| Feature           | Commands                                                                                | Queries                           |
| ----------------- | --------------------------------------------------------------------------------------- | --------------------------------- |
| **Posts**         | CreatePost, EditPost, DeletePost                                                        | GetPosts, GetPostById, GetMyPosts |
| **Users**         | CreateUser, Login, VerifyUser, ForgotPassword, ResetPassword, UpdateProfile, DeleteUser | GetUserById, GetUsers             |
| **Comments**      | CreateComment, EditComment, DeleteComment                                               | GetComments                       |
| **Notifications** | (event-driven creation)                                                                 | GetNotifications                  |

**MediatR Pipeline Behaviors (executed in order):**

1. `LoggingBehavior` — Structured logging of all requests/responses
2. `IdempotencyBehavior` — Prevents duplicate command processing (Redis-backed)
3. `ValidationBehavior` — Executes FluentValidation rules before handler

#### 5.2.3 Domain Layer (Core Business Logic)

**Responsibilities:**

- Define entities with rich behavior (not anemic models)
- Express business rules and invariants
- Define repository interfaces (contracts)
- Raise domain events for cross-cutting side effects
- Maintain zero dependency on external frameworks

**Domain Model:**

| Entity           | Key Behaviors                                                                  | Domain Events                                       |
| ---------------- | ------------------------------------------------------------------------------ | --------------------------------------------------- |
| **User**         | Create, Verify, ResetPassword, ForgotPassword, Update, MarkNotificationsAsRead | UserCreatedDomainEvent                              |
| **Post**         | Create, UpdateContent, SetImage, RemoveImage, Delete, AddComment               | PostCreated, PostUpdated, PostDeleted, CommentAdded |
| **Comment**      | Create, UpdateContent, Delete, SetUser                                         | (triggered via Post)                                |
| **Notification** | Create (via event handler)                                                     | —                                                   |
| **Image**        | Value object (Id + URL)                                                        | —                                                   |

**Value Objects:** `PostId`, `UserId`, `CommentId`, `NotificationId`, `ImageId`, `Token`, `Otp`

#### 5.2.4 Infrastructure Layer (External Integration)

**Responsibilities:**

- Implement repository interfaces with MongoDB persistence
- Provide authentication services (JWT, OAuth, password hashing)
- Manage real-time hubs (SignalR)
- Handle external storage (GitHub image API)
- Process asynchronous domain events via Rebus message bus
- Provide resilience (retry policies, fallback strategies)

**Key Components:**

| Component                     | Implementation                                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------------- |
| **MongoDbContext**            | Unit of Work pattern; manages users and posts collections; serializes domain events to outbox |
| **UserRepository**            | CRUD for users; queries by ID, email, token, OTP                                              |
| **PostRepository**            | CRUD for posts; text search, pagination, status filtering                                     |
| **CommentRepository**         | Embedded subdocument operations within posts                                                  |
| **NotificationRepository**    | Embedded subdocument operations within users                                                  |
| **JwtTokenService**           | JWT generation with HS256 signing                                                             |
| **Sha512PasswordHasher**      | SHA-512 with salt for password hashing                                                        |
| **FeedHub / NotificationHub** | SignalR hubs for real-time WebSocket communication                                            |
| **GitHubImageStorageService** | Image upload/delete via GitHub API                                                            |
| **OutboxProcessor**           | Background hosted service processing domain events                                            |
| **RedisRateLimiter**          | Sliding window rate limiting with Redis backend                                               |
| **RetryPolicy**               | Configurable retry logic for MongoDB operations                                               |

### 5.3 Data Tier

| Store                    | Technology | Purpose                                                                                        |
| ------------------------ | ---------- | ---------------------------------------------------------------------------------------------- |
| **Primary Database**     | MongoDB 6+ | Document storage for users, posts (with embedded comments/notifications), outbox, Rebus queues |
| **Cache / Rate Limiter** | Redis 7    | Rate limiting counters, idempotency store, profile/feed caching, SignalR backplane             |

---

## 6. Component Interaction Flows

### 6.1 Flow: User Registration (UC1)

```
Anonymous User → Next.js (RegisterPage)
  │
  ├─► Apollo Client → POST /graphql (Register mutation)
  │
  ├─► [Middleware Pipeline] → JWT not required (public endpoint)
  │
  ├─► MediatR → CreateUserCommand
  │     ├─► [ValidationBehavior] → Validate email, username, password
  │     ├─► [Handler] → Hash password (SHA-512 + salt)
  │     │     ├─► UserRepository.Create() → MongoDB (users collection)
  │     │     └─► Raise UserCreatedDomainEvent
  │     │           └─► Outbox → OutboxProcessor → Rebus
  │     │                 └─► SendUserVerificationEmailHandler
  │     │                       └─► EmailService.Send() → SMTP
  │     └─► Return UserDto
  │
  └─► Next.js → Display "Check your email for verification"
```

### 6.2 Flow: Create Post with Notification (UC9 + UC16)

```
Verified User → Next.js (CreatePostCard)
  │
  ├─► Apollo Client → POST /graphql (CreatePost mutation, JWT Bearer)
  │
  ├─► [Middleware Pipeline] → RateLimiting check → Auth check (JWT valid)
  │
  ├─► MediatR → CreatePostCommand
  │     ├─► [IdempotencyBehavior] → Check Redis for duplicate
  │     ├─► [ValidationBehavior] → Validate text length, image size
  │     ├─► [Handler]
  │     │     ├─► Post.Create() → Domain entity with PostCreatedDomainEvent
  │     │     ├─► PostRepository.Add() → MongoDB (posts collection)
  │     │     ├─► UnitOfWork.SaveChanges() → Persist + serialize events to outbox
  │     │     └─► Return PostDetailDto
  │     │
  │     └─► [Outbox Processing] (async, background)
  │           ├─► UploadImageHandler → GitHubImageStorageService
  │           └─► SendPostToFeedHandler → FeedHub.BroadcastPost()
  │                 └─► SignalR → All connected clients receive new post
  │
  └─► Next.js → Post appears in feed (optimistic UI + SignalR confirmation)
```

### 6.3 Flow: Comment triggers Notification (UC12 → UC16)

```
Verified User → Next.js (PostDetailModal, comment form)
  │
  ├─► Apollo Client → POST /graphql (CreateComment mutation)
  │
  ├─► MediatR → CreateCommentCommand
  │     ├─► [Handler]
  │     │     ├─► Post.AddComment() → Raises CommentAddedDomainEvent
  │     │     ├─► CommentRepository (embedded in post document)
  │     │     └─► UnitOfWork.SaveChanges()
  │     │
  │     └─► [Outbox Processing] (async)
  │           └─► SendNotificationPostOwnerHandler
  │                 ├─► Create Notification entity
  │                 ├─► NotificationRepository → Embed in post author's user doc
  │                 └─► NotificationHub.NotifyUser(postAuthorId)
  │                       └─► SignalR → "ReceiveCommentNotification"
  │                             └─► Post author's browser: red dot + notification
  │
  └─► Next.js → Comment appears immediately (Apollo cache update)
```

---

## 7. Technology Stack Justification

| Technology           | Choice                  | Justification                                                                                                |
| -------------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Next.js 15**       | Frontend framework      | Server-side rendering for SEO + static generation + App Router for modern React patterns                     |
| **React 19**         | UI library              | Component-based, vast ecosystem, optimistic UI capabilities                                                  |
| **TypeScript**       | Language (frontend)     | Type safety reduces runtime errors; better IDE support                                                       |
| **Tailwind CSS**     | Styling                 | Utility-first CSS enables rapid UI development; tree-shakeable for small bundles                             |
| **Apollo Client**    | GraphQL client          | Powerful cache management, optimistic UI, automatic re-fetching                                              |
| **.NET 10.0**        | Backend runtime         | High-performance, cross-platform, rich middleware ecosystem                                                  |
| **HotChocolate**     | GraphQL server          | Best-in-class .NET GraphQL library; schema-first + code-first support                                        |
| **MediatR**          | CQRS mediator           | Decouples request handling from API layer; enables pipeline behaviors                                        |
| **FluentValidation** | Input validation        | Strongly-typed validation rules; integrates with MediatR pipeline                                            |
| **MongoDB**          | Primary database        | Flexible schema for evolving domain model; embedded documents for comments/notifications; native text search |
| **Redis**            | Cache / rate limiter    | Sub-millisecond operations for rate limiting, idempotency, caching; SignalR backplane                        |
| **SignalR**          | Real-time communication | Native .NET WebSocket library; auto-negotiation (WebSocket → SSE → Long Polling)                             |
| **Rebus**            | Message bus             | Lightweight .NET async messaging; MongoDB transport avoids additional infrastructure                         |
| **Docker**           | Containerization        | Consistent dev/prod environments; easy deployment to any cloud provider                                      |
| **JWT**              | Authentication          | Stateless; no session server needed; scales horizontally                                                     |
| **xUnit**            | Testing framework       | Industry standard for .NET; supports integration and unit tests                                              |

---

## 8. Deployment Architecture

### 8.1 Docker Compose (Local Development)

```
┌──────────────────────────────────────────────────────────────────┐
│                     Docker Compose Network                        │
│                                                                   │
│  ┌───────────────┐   ┌───────────────┐   ┌───────────────────┐  │
│  │   frontend    │   │    backend    │   │      redis        │  │
│  │   (Next.js)   │   │  (ASP.NET)   │   │   (redis:7-alpine)│  │
│  │   Port: 3000  │──►│   Port: 8080  │──►│   Port: 6379      │  │
│  │               │   │   (ext: 5000) │   │   Volume:          │  │
│  │  Build:       │   │               │   │   redis-data (AOF) │  │
│  │  ./frontend/  │   │  depends_on:  │   │                    │  │
│  │  Dockerfile   │   │   redis       │   └───────────────────┘  │
│  │               │   │               │                           │
│  │  depends_on:  │   │  Build:       │   ┌───────────────────┐  │
│  │   backend     │   │  ./backend/   │   │    MongoDB         │  │
│  └───────────────┘   │  Dockerfile   │──►│  (External/Cloud)  │  │
│                       │               │   │  MongoDB Atlas or  │  │
│                       │  Env:         │   │  local mongo:6.0   │  │
│                       │  .env file    │   └───────────────────┘  │
│                       └───────────────┘                           │
└──────────────────────────────────────────────────────────────────┘
```

### 8.2 Production Architecture

```
┌────────────────────────────────────────────────────────────────────────┐
│                          INTERNET                                       │
│                                                                         │
│  Users (Browsers) ──────────────────────────────────────────────────── │
│         │                                                               │
│         ▼                                                               │
│  ┌─────────────┐            ┌──────────────────────────────────┐       │
│  │   CDN       │            │          Cloud Provider          │       │
│  │ (Vercel/    │            │      (Render / Azure / AWS)      │       │
│  │  Netlify)   │            │                                  │       │
│  │             │            │  ┌─────────────────────────────┐ │       │
│  │  Next.js    │            │  │    Load Balancer / Proxy    │ │       │
│  │  Static +   │            │  └──────────┬──────────────────┘ │       │
│  │  SSR        │◄──────────►│             │                    │       │
│  │             │  GraphQL   │  ┌──────────▼──────────────────┐ │       │
│  └─────────────┘  + WSS     │  │   Backend Container (x N)   │ │       │
│                              │  │   ASP.NET Core              │ │       │
│                              │  │   /graphql                  │ │       │
│                              │  │   /notificationHub          │ │       │
│                              │  │   /feedHub                  │ │       │
│                              │  └──────────┬──────────────────┘ │       │
│                              │             │                    │       │
│                              │  ┌──────────▼───────┐ ┌───────┐ │       │
│                              │  │  MongoDB Atlas   │ │ Redis │ │       │
│                              │  │  (Managed)       │ │ Cloud │ │       │
│                              │  └──────────────────┘ └───────┘ │       │
│                              └──────────────────────────────────┘       │
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │ Google OAuth  │  │Facebook OAuth│  │  SMTP Server │                  │
│  │ (External)   │  │ (External)   │  │  (External)  │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
└────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Environment Configuration

All services are configured via environment variables (12-factor app):

| Category          | Variables                                                                            |
| ----------------- | ------------------------------------------------------------------------------------ |
| **Database**      | `MONGODB_CONNECTION_STRING`, `MONGODB_NAME`                                          |
| **Cache**         | `REDIS_CONNECTION_STRING`                                                            |
| **Auth**          | `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`                 |
| **OAuth**         | `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `FACEBOOK_APP_ID`, `FACEBOOK_APP_SECRET` |
| **Email**         | `SMTP_SERVER`, `SMTP_PORT`, `SMTP_SENDER_EMAIL`, `SMTP_SENDER_PASSWORD`              |
| **Storage**       | `GIT_TOKEN`, `GIT_USERNAME`, `GIT_REPO`, `GIT_BRANCH`                                |
| **Rate Limiting** | `RATE_LIMIT_REQUESTS`, `RATE_LIMIT_WINDOW_SECONDS`                                   |
| **Frontend**      | `NEXT_PUBLIC_API_URL`                                                                |
| **Messaging**     | `REBUS_QUEUE_NAME`                                                                   |
| **CORS**          | `ALLOWED_CORS`, `FRONTEND`                                                           |

---

## 9. Cross-Cutting Concerns

### 9.1 Security Architecture

```
                              ┌─────────────────────────────┐
                              │     Security Layers          │
                              │                              │
Internet ──► [HTTPS/TLS] ──► │ ┌── SecurityHeaders MW ───┐ │
                              │ │ X-Content-Type-Options   │ │
                              │ │ X-Frame-Options          │ │
                              │ │ X-XSS-Protection         │ │
                              │ └─────────────────────────┘ │
                              │ ┌── RateLimiting MW ──────┐ │
                              │ │ Redis sliding window     │ │
                              │ │ 100 req/min per user/IP  │ │
                              │ └─────────────────────────┘ │
                              │ ┌── JWT Authentication ───┐ │
                              │ │ Bearer token validation  │ │
                              │ │ Claims: userId, role     │ │
                              │ └─────────────────────────┘ │
                              │ ┌── RBAC Authorization ───┐ │
                              │ │ [Authorize] on resolvers │ │
                              │ │ Data-level: own content  │ │
                              │ └─────────────────────────┘ │
                              │ ┌── Input Validation ─────┐ │
                              │ │ FluentValidation rules   │ │
                              │ │ Server-side sanitization │ │
                              │ └─────────────────────────┘ │
                              │ ┌── CORS Configuration ───┐ │
                              │ │ Whitelist-based origins  │ │
                              │ └─────────────────────────┘ │
                              └─────────────────────────────┘
```

### 9.2 Error Handling Strategy

The `GlobalExceptionMiddleware` uses a **Chain of Responsibility** pattern:

```
Exception Thrown
      │
      ▼
ValidationExceptionHandler ──► 400 Bad Request (validation errors)
      │ (not handled)
      ▼
UnauthorizedAccessExceptionHandler ──► 401 Unauthorized
      │ (not handled)
      ▼
ConflictExceptionHandler ──► 409 Conflict (duplicate entity)
      │ (not handled)
      ▼
NotFoundExceptionHandler ──► 404 Not Found
      │ (not handled)
      ▼
ForbiddenHandler ──► 403 Forbidden (insufficient permissions)
      │ (not handled)
      ▼
DefaultExceptionHandler ──► 500 Internal Server Error (generic)
```

All responses follow RFC 7807 `ProblemDetails` format with correlation ID.

### 9.3 Event-Driven Architecture

```
Domain Entity ──► raises DomainEvent
                       │
                       ▼
              UnitOfWork.SaveChanges()
                       │
                ┌──────┴──────┐
                │   Outbox    │ (MongoDB collection)
                │   Table     │
                └──────┬──────┘
                       │
                       ▼
              OutboxProcessor (Background Service)
                       │
                       ▼
              Rebus Message Bus (MongoDB Transport)
                       │
         ┌─────────────┼─────────────────┐──────────────┐
         ▼             ▼                  ▼              ▼
   [Send Email]  [Upload Image]  [Push Feed via  [Notify Post
                                  FeedHub]         Owner via
                                                   NotifHub]
```

### 9.4 Observability

| Concern                  | Implementation                                                              |
| ------------------------ | --------------------------------------------------------------------------- |
| **Correlation Tracking** | `X-Correlation-Id` header on every request (CorrelationIdMiddleware)        |
| **Request Logging**      | RequestDiagnosticMiddleware logs request/response for GraphQL and SignalR   |
| **Structured Logging**   | All handlers use `ILogger<T>` with structured output                        |
| **Health Checks**        | `/health` endpoint for load balancer probes                                 |
| **Error Logging**        | GlobalExceptionMiddleware logs all unhandled exceptions with correlation ID |

---

## 10. Architectural Decisions & Trade-Offs

### ADR-1: GraphQL over REST

| Aspect           | Decision                                                                                                                         |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **Decision**     | Use GraphQL (HotChocolate) as the sole API protocol                                                                              |
| **Rationale**    | Clients request exactly the fields they need; reduces over-fetching for mobile/low-bandwidth; single endpoint simplifies routing |
| **Trade-Off**    | More complex server setup; caching is harder than REST (no native HTTP caching); requires client-side library (Apollo)           |
| **Mitigated By** | Apollo's InMemoryCache; standardized query patterns in `queries.ts`                                                              |

### ADR-2: MongoDB with Embedded Documents

| Aspect           | Decision                                                                                                                           |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| **Decision**     | Use MongoDB with comments embedded in posts and notifications embedded in users                                                    |
| **Rationale**    | Read-optimized: fetching a post with its comments is a single document read; matches the domain model's aggregate pattern          |
| **Trade-Off**    | Document size limit (16 MB); embedded arrays grow unbounded; complex updates on nested arrays                                      |
| **Mitigated By** | Pagination limits on comments; potential future migration to reference-based pattern for high-volume posts (see Design Issue DI-1) |

### ADR-3: Event-Driven with Outbox Pattern

| Aspect           | Decision                                                                                                                                                   |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Decision**     | Use Outbox Pattern + Rebus message bus for async event processing                                                                                          |
| **Rationale**    | Ensures domain events are reliably published even if external services fail; decouples command processing from side effects (email, images, notifications) |
| **Trade-Off**    | Eventual consistency; side effects have slight delay; additional complexity in outbox processing                                                           |
| **Mitigated By** | OutboxProcessor runs as a background hosted service; Rebus provides at-least-once delivery                                                                 |

### ADR-4: Stateless JWT Authentication

| Aspect           | Decision                                                                                                   |
| ---------------- | ---------------------------------------------------------------------------------------------------------- |
| **Decision**     | Use stateless JWT tokens (no server-side session store)                                                    |
| **Rationale**    | Enables horizontal scaling; no session affinity required; works seamlessly with load balancers             |
| **Trade-Off**    | Cannot revoke tokens (logout is client-side only); 24-hour window of vulnerability if token is compromised |
| **Mitigated By** | Short token expiry (24 hours); future refresh token mechanism (Design Issue DI-2); HTTPS-only transport    |

### ADR-5: SignalR for Real-Time

| Aspect           | Decision                                                                                                              |
| ---------------- | --------------------------------------------------------------------------------------------------------------------- |
| **Decision**     | Use SignalR with Redis backplane for real-time notifications and feed updates                                         |
| **Rationale**    | Native .NET integration; automatic transport negotiation (WebSocket → SSE → Long Polling); scales via Redis backplane |
| **Trade-Off**    | Requires persistent WebSocket connections; more server resources than pure polling                                    |
| **Mitigated By** | Minimal payloads; connection lifecycle management in `useNotificationHub` hook; auto-reconnect                        |

### ADR-6: Clean Architecture with CQRS

| Aspect           | Decision                                                                                                                                                                             |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Decision**     | Strict Clean Architecture layers with CQRS via MediatR                                                                                                                               |
| **Rationale**    | Domain layer isolated from framework dependencies; commands and queries have independent optimization paths; pipeline behaviors (logging, validation, idempotency) applied uniformly |
| **Trade-Off**    | More boilerplate (command/query classes, handlers, validators per feature); learning curve for new developers                                                                        |
| **Mitigated By** | Consistent patterns; feature-folder organization; code generation possible                                                                                                           |

---

## 11. Mapping Use Cases to Architecture

This table demonstrates how each use case package maps to architectural components:

### 11.1 Authentication & Account (UC1–UC6)

| Use Case               | Frontend                | API Layer                                | Application                                 | Domain                                      | Infrastructure                        |
| ---------------------- | ----------------------- | ---------------------------------------- | ------------------------------------------- | ------------------------------------------- | ------------------------------------- |
| **UC1** Register Email | RegisterPage → Apollo   | Register mutation                        | CreateUserCommand                           | User.Create(), UserCreatedDomainEvent       | UserRepository, EmailService (SMTP)   |
| **UC2** Register OAuth | RegisterPage → Redirect | — (OAuth flow)                           | CreateUserCommand                           | User.Create()                               | GoogleOAuth / FacebookOAuth providers |
| **UC3** Verify Email   | (Email link)            | VerifyUser mutation                      | VerifyUserCommand                           | User.Verify()                               | UserRepository                        |
| **UC4** Login          | LoginPage → Apollo      | Login mutation                           | LoginCommand                                | —                                           | JwtTokenService, Sha512PasswordHasher |
| **UC5** Logout         | useAuth.logout()        | — (client-side)                          | —                                           | —                                           | —                                     |
| **UC6** Reset Password | (Forgot password page)  | ForgotPassword + ResetPassword mutations | ForgotPasswordCommand, ResetPasswordCommand | User.ForgotPassword(), User.ResetPassword() | EmailService (OTP), UserRepository    |

### 11.2 Content Management (UC7–UC11)

| Use Case             | Frontend                | API Layer              | Application       | Domain                                | Infrastructure                               |
| -------------------- | ----------------------- | ---------------------- | ----------------- | ------------------------------------- | -------------------------------------------- |
| **UC7** View Feed    | Feed → usePosts         | Posts query            | GetPostsQuery     | —                                     | PostRepository (pagination, text search)     |
| **UC8** Search Posts | Feed (search bar)       | Posts query (keywords) | GetPostsQuery     | —                                     | PostRepository (MongoDB text index)          |
| **UC9** Create Post  | CreatePostCard → Apollo | CreatePost mutation    | CreatePostCommand | Post.Create(), PostCreatedDomainEvent | PostRepository, GitHubImageStorage, FeedHub  |
| **UC10** Edit Post   | PostEditForm → Apollo   | EditPost mutation      | EditPostCommand   | Post.UpdateContent()                  | PostRepository, GitHubImageStorage           |
| **UC11** Delete Post | PostMenu → Apollo       | DeletePost mutation    | DeletePostCommand | Post.Delete(), PostDeletedDomainEvent | PostRepository, GitHubImageStorage (cleanup) |

### 11.3 Comments & Engagement (UC12–UC15)

| Use Case                 | Frontend                   | API Layer              | Application          | Domain                                     | Infrastructure                        |
| ------------------------ | -------------------------- | ---------------------- | -------------------- | ------------------------------------------ | ------------------------------------- |
| **UC12** Comment on Post | PostDetailModal → Apollo   | CreateComment mutation | CreateCommentCommand | Post.AddComment(), CommentAddedDomainEvent | CommentRepository, NotificationHub    |
| **UC13** Edit Comment    | Comment component → Apollo | EditComment mutation   | EditCommentCommand   | Comment.UpdateContent()                    | CommentRepository                     |
| **UC14** Delete Comment  | Comment component → Apollo | DeleteComment mutation | DeleteCommentCommand | Comment.Delete()                           | CommentRepository                     |
| **UC15** View Comments   | PostDetailPage → Apollo    | Comments query         | GetCommentsQuery     | —                                          | CommentRepository (embedded in posts) |

### 11.4 Notifications (UC16, UC17, UC19)

| Use Case                       | Frontend                         | API Layer           | Application                      | Domain                | Infrastructure                          |
| ------------------------------ | -------------------------------- | ------------------- | -------------------------------- | --------------------- | --------------------------------------- |
| **UC16** Receive Notifications | useNotificationHub (SignalR)     | NotificationHub     | SendNotificationPostOwnerHandler | Notification.Create() | NotificationRepository, NotificationHub |
| **UC17** View Notifications    | Header NotificationList → Apollo | Notifications query | GetNotificationsQuery            | —                     | NotificationRepository                  |
| **UC19** Update Preferences    | ProfilePage → Apollo             | UpdateMe mutation   | UpdateProfileCommand             | —                     | UserRepository                          |

### 11.5 User Profile (UC18, UC20)

| Use Case                   | Frontend                 | API Layer         | Application          | Domain        | Infrastructure                     |
| -------------------------- | ------------------------ | ----------------- | -------------------- | ------------- | ---------------------------------- |
| **UC18** Update Profile    | ProfilePage → Apollo     | UpdateMe mutation | UpdateProfileCommand | User.Update() | UserRepository, GitHubImageStorage |
| **UC20** View User Profile | UserProfilePage → Apollo | User query        | GetUserByIdQuery     | —             | UserRepository                     |

### 11.6 Administration (UC21–UC23)

| Use Case                | Frontend                   | API Layer   | Application | Domain                 | Infrastructure                    |
| ----------------------- | -------------------------- | ----------- | ----------- | ---------------------- | --------------------------------- |
| **UC21** Ban User       | (Admin dashboard, Phase 2) | — (Phase 2) | —           | User status → Banned   | UserRepository, EmailService      |
| **UC22** Remove Content | (Admin dashboard, Phase 2) | — (Phase 2) | —           | Post/Comment → Deleted | PostRepository, CommentRepository |
| **UC23** View Dashboard | (Admin page, Phase 2)      | — (Phase 2) | —           | —                      | MongoDB aggregation queries       |

---

## 12. Risk Assessment

| Risk                                                                         | Probability | Impact   | Mitigation                                                                     |
| ---------------------------------------------------------------------------- | ----------- | -------- | ------------------------------------------------------------------------------ |
| **MongoDB document size limit (16 MB)** reached for posts with many comments | Medium      | High     | Monitor comment counts; implement reference-based pattern at threshold (DI-1)  |
| **JWT token compromise** within 24-hour window                               | Low         | High     | HTTPS-only; implement refresh tokens (DI-2); monitor for anomalous activity    |
| **GitHub image storage API limits** reached                                  | Medium      | Medium   | Migrate to Cloudinary/S3 for production (DI-7)                                 |
| **Redis unavailability** breaks rate limiting and idempotency                | Low         | Medium   | Fallback to NoopRateLimiter and InMemoryIdempotencyStore (already implemented) |
| **SignalR connection scalability** under 1,000+ concurrent users             | Medium      | Medium   | Redis backplane for multi-instance; monitor connection counts                  |
| **MongoDB single-node failure**                                              | Low         | Critical | Use MongoDB Atlas with replica set; enable automated failover                  |
| **Email service downtime** blocks registration flow                          | Medium      | Medium   | Graceful degradation: account created but unverified; retry queue for email    |

---

## 13. References

| Document                              | Location                                                                           | Relevance                                                                     |
| ------------------------------------- | ---------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| **System Requirements Specification** | [01_SYSTEM_REQUIREMENTS_SPECIFICATION.md](01_SYSTEM_REQUIREMENTS_SPECIFICATION.md) | Functional and non-functional requirements                                    |
| **Use Case Model & Descriptions**     | [02_USE_CASES_AND_DESCRIPTIONS.md](02_USE_CASES_AND_DESCRIPTIONS.md)               | 23 use cases with flows, supplementary specs, glossary                        |
| **Repository Setup & GitHub**         | [03_REPOSITORY_SETUP_AND_GITHUB.md](03_REPOSITORY_SETUP_AND_GITHUB.md)             | Development workflow and CI/CD guidance                                       |
| **Executive Summary**                 | [00_EXECUTIVE_SUMMARY_AND_INDEX.md](00_EXECUTIVE_SUMMARY_AND_INDEX.md)             | Project overview and document index                                           |
| **Architecture Overview**             | [ARCHITECTURE.md](ARCHITECTURE.md)                                                 | Detailed technical architecture (MongoDB schemas, Redis patterns, deployment) |

---

## Version History

| Version | Date       | Author                 | Changes                                                         |
| ------- | ---------- | ---------------------- | --------------------------------------------------------------- |
| 1.0     | 2026-02-28 | Architectural Analysis | Initial high-level system architecture (Lab 3, first iteration) |

---

**Document End**
