# BiUrSite – System Requirements Specification (SRS)

**Document Version:** 1.0  
**Date:** February 22, 2026  
**Project:** BiUrSite – Anonymous Idea & Advice Sharing Platform  
**Repository:** https://github.com/Kheang1409/BiUrSite
**Status:** In Development

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Overall Description](#2-overall-description)
3. [System Features (Functional Requirements)](#3-system-features-functional-requirements)
4. [Non-Functional Requirements](#4-non-functional-requirements)
5. [External Interfaces](#5-external-interfaces)
6. [Data Requirements](#6-data-requirements)
7. [Performance](#7-performance)
8. [Security](#8-security)

---

## 1. Introduction

### 1.1 Purpose

BiUrSite is a web-based platform designed to facilitate anonymous and verified user interactions around idea sharing, advice-seeking, and community feedback. The system enables users to express thoughts, receive constructive advice, and build a safe, moderated community. Administrators monitor and enforce community standards.

### 1.2 Scope

**In Scope:**

- User registration, authentication (email/password, OAuth2 via Google & Facebook)
- Post lifecycle management (create, edit, delete)
- Comment system with notifications
- Real-time user notifications (SignalR)
- Admin moderation dashboard
- User profile management
- Notification preferences
- Rate limiting and DDoS protection
- Email verification and password recovery

**Out of Scope (Future phases):**

- Advanced analytics/reporting
- In-app messaging between users
- Post ratings/voting system
- User reputation system
- Mobile-native applications

### 1.3 Definitions, Acronyms, Abbreviations

| Term        | Definition                                                 |
| ----------- | ---------------------------------------------------------- |
| **JWT**     | JSON Web Token – stateless authentication mechanism        |
| **GraphQL** | Query language for API communication                       |
| **OAuth2**  | Open Authorization standard for third-party login          |
| **SignalR** | Real-time, bidirectional communication protocol            |
| **MongoDB** | Document-oriented NoSQL database                           |
| **Redis**   | In-memory data structure store for caching & rate limiting |
| **MediatR** | .NET library for implementing CQRS & event-driven patterns |
| **CORS**    | Cross-Origin Resource Sharing – web security mechanism     |
| **XSS**     | Cross-Site Scripting – web vulnerability                   |
| **CSRF**    | Cross-Site Request Forgery – web vulnerability             |
| **RBAC**    | Role-Based Access Control                                  |

---

## 2. Overall Description

### 2.1 Product Perspective

BiUrSite is a standalone, cloud-ready web platform with a separated backend API and frontend UI. The system follows a **three-tier architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend Layer (Next.js)                  │
│         (Browser-based, responsive, GraphQL client)          │
└─────────────────────────────────────┬───────────────────────┘
                                       │ REST / GraphQL
                                       │ WebSocket (SignalR)
┌─────────────────────────────────────▼───────────────────────┐
│                 Application Layer (.NET Core)                 │
│   (GraphQL API, business logic, authentication, validation)   │
└─────────────────────────────────────┬───────────────────────┘
                                       │ MongoDB / Redis
┌─────────────────────────────────────▼───────────────────────┐
│            Data & Infrastructure Layer                        │
│     (MongoDB, Redis, Email Service, External Storage)        │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 User Classes

| User Class            | Description                      | Key Capabilities                                                         |
| --------------------- | -------------------------------- | ------------------------------------------------------------------------ |
| **Anonymous User**    | New or unverified visitor        | View public posts, browse user profiles, register/login                  |
| **Verified User**     | Registered & email-verified user | Create/edit/delete posts, comment, receive notifications, update profile |
| **Admin User**        | System administrator             | View reports, ban users, remove content, manage platform                 |
| **External Services** | Third-party integrations         | OAuth providers (Google, Facebook), email service, storage               |

### 2.3 Operating Environment

**Backend Infrastructure:**

- Runtime: .NET 8.0+
- Database: MongoDB (document-based, flexible schema)
- Cache/Session: Redis
- Host Environment: Docker container (Linux-based), deployed on Render/Azure/AWS
- Port: 8080 (HTTP), supports HTTPS in production

**Frontend Environment:**

- Runtime: Node.js 18+
- Framework: Next.js 15+
- Browser: Chrome, Firefox, Safari, Edge (latest versions)
- Host Environment: Static hosting (Netlify, Vercel, or CDN)

**External Services:**

- Email Service: SMTP (configurable provider)
- Authentication Providers: Google OAuth2, Facebook SDK
- Image Storage: Cloudinary/S3/Git LFS

### 2.4 Assumptions & Dependencies

**Assumptions:**

- Users have valid email addresses for registration and verification
- Users have JavaScript enabled in their browsers
- Network connectivity is available for all real-time features
- Admins are trusted system operators
- External OAuth providers remain accessible

**Dependencies:**

- MongoDB instance must be running and accessible
- Redis instance required for caching and rate limiting
- SMTP server configured for email notifications
- OAuth credentials (Google, Facebook) must be provisioned
- SignalR requires WebSocket support in production environments

---

## 3. System Features (Functional Requirements)

### 3.1 User Authentication & Account Management

#### FR-3.1.1 User Registration (Email/Password)

- **Description:** New users can create an account using email and password.
- **Inputs:** Email, username, password, confirm password
- **Outputs:** User account created in "Unverified" status, email verification link sent
- **Behavior:**
  - Validate email uniqueness
  - Hash password using secure algorithm (bcrypt or similar)
  - Generate and send email verification token (valid for 24 hours)
  - Store user with `Status = Unverified`
  - Account requires email verification before posting/commenting

#### FR-3.1.2 OAuth Registration (Google/Facebook)

- **Description:** Users can register via Google or Facebook accounts.
- **Inputs:** OAuth provider (Google/Facebook), user consent
- **Outputs:** User account in "Active" status (auto-verified via OAuth), JWT token
- **Behavior:**
  - Redirect to OAuth provider consent screen
  - Extract user profile (email, name, profile picture)
  - Create/lookup user in system
  - Auto-verify status (trust OAuth provider verification)
  - Return JWT token with claims

#### FR-3.1.3 Email Verification

- **Description:** Users must verify ownership of email address to access core features.
- **Inputs:** Verification token (from email link)
- **Outputs:** Account status updated to "Active", notification
- **Behavior:**
  - Validate token expiry (24 hours)
  - Update user `Status` from `Unverified` to `Active`
  - Token becomes invalid after use
  - Resend verification link if expired

#### FR-3.1.4 User Login

- **Description:** Verified users authenticate using email and password.
- **Inputs:** Email, password
- **Outputs:** JWT token (valid for 24 hours), user profile data
- **Behavior:**
  - Check user exists and status is not `Banned` or `Deleted`
  - Verify password hash
  - Generate JWT token with claims (userId, email, role)
  - Return token & user info
  - Log failed login attempts for security auditing

#### FR-3.1.5 Password Recovery

- **Description:** Users can reset forgotten passwords.
- **Inputs:** Email (forgot), Token + new password (reset)
- **Outputs:** OTP sent to email, password reset confirmation
- **Behavior:**
  - Send OTP (One-Time Password) to registered email, valid 30 minutes
  - User enters OTP and new password
  - Validate OTP and update password hash
  - Invalidate OTP after use
  - Force new login with new credentials

#### FR-3.1.6 User Profile Management

- **Description:** Verified users can view and update their profile.
- **Inputs:** Bio, avatar/profile picture, phone (optional), notification preferences
- **Outputs:** Updated user profile, success notification
- **Behavior:**
  - Allow editing of bio, avatar, phone
  - Validate input lengths and formats
  - Store profile updates with last-modified timestamp
  - Support image upload with validation (size, format)
  - Trigger notification preference updates

#### FR-3.1.7 Logout

- **Description:** Users can terminate their session.
- **Inputs:** Token/session identifier
- **Outputs:** Session invalidated
- **Behavior:**
  - Client-side: Clear JWT token from storage
  - No backend session table (stateless JWT)
  - Redirect to public homepage

### 3.2 Post Management

#### FR-3.2.1 Create Post (Verified Users Only)

- **Description:** Verified users compose and publish text/image posts.
- **Inputs:** Post text (required), optional image (file upload), author ID (from JWT)
- **Outputs:** Post created with auto-generated ID, notification to feed subscribers
- **Behavior:**
  - Validate post not empty (min 3 chars, max 5000 chars for text)
  - If image provided: validate format (JPG, PNG, WebP), size (<10 MB), compress
  - Store image using Cloudinary/S3 or inline in MongoDB
  - Create post with `Status = Active`, timestamp
  - Emit `PostCreatedEvent` to trigger notifications
  - Add post to real-time feed via SignalR FeedHub
  - Return post DTO with author embedded snapshot

#### FR-3.2.2 View Feed (Public)

- **Description:** All users (anonymous & verified) view recent posts.
- **Inputs:** Page number (pagination), optional keywords (search filter)
- **Outputs:** List of posts (paginated, 10 per page), sorted by created date DESC
- **Behavior:**
  - Retrieve posts with `Status = Active` only
  - Include author snapshot (ID, username, avatar)
  - Embed recent comments (last 3) for quick preview
  - Apply search filter (text contains keyword)
  - Exclude deleted/soft-deleted posts
  - Include metadata: commentsCount, createdAt

#### FR-3.2.3 View Post Details

- **Description:** Users view a single post with all comments.
- **Inputs:** Post ID
- **Outputs:** Post DTO with full comment list, author details
- **Behavior:**
  - Fetch post by ID
  - Return 404 if post not found or deleted
  - Fetch all associated comments from `comments` collection
  - Include author object (ID, username, avatar)
  - Return with comments sorted by createdAt DESC
  - Update view counter (implicit, no explicit tracking yet)

#### FR-3.2.4 Edit Post (Author Only)

- **Description:** Post author can modify post content and image.
- **Inputs:** Post ID, new text, optional new image, removeImage flag
- **Outputs:** Updated post, last-modified timestamp
- **Behavior:**
  - Verify requester is post author
  - Validate new text (same constraints as create)
  - If new image: validate and store
  - If removeImage=true: delete old image from storage
  - Update post text, image, modifiedDate
  - Publish domain event for downstream handlers
  - Return updated post DTO

#### FR-3.2.5 Delete Post (Author Only)

- **Description:** Post author can remove their post.
- **Inputs:** Post ID, requester ID (from JWT)
- **Outputs:** Post soft-deleted, success notification
- **Behavior:**
  - Verify requester is post author or admin
  - Soft-delete: set `Status = Deleted`, `DeletedDate = now()`
  - Do NOT physically remove from DB
  - Trigger cascade: also soft-delete associated comments if needed
  - Emit `PostDeletedEvent` to clean up images
  - Return success response (no post DTO)
  - Clients receive SignalR notification to remove post from feed

### 3.3 Comment System

#### FR-3.3.1 Create Comment

- **Description:** Verified users comment on a post.
- **Inputs:** Post ID, comment text, author ID (from JWT)
- **Outputs:** Comment created, notification sent to post owner
- **Behavior:**
  - Validate post exists and not deleted
  - Validate comment text (min 1 char, max 2000 chars)
  - Create comment with `Status = Active`
  - Store in separate `comments` collection (reference-based)
  - Increment post's `commentsCount` counter
  - Add comment to post's `recentComments` array (keep last 3)
  - Emit `CommentCreatedEvent` → triggers notification to post author
  - Notify post author via NotificationHub (real-time if online)
  - Return CommentDto with author embedded

#### FR-3.3.2 View Comments

- **Description:** View all comments on a post (paginated).
- **Inputs:** Post ID, page number
- **Outputs:** List of comments (paginated, 20 per page), sorted by newest first
- **Behavior:**
  - Fetch comments with `postId = <ID>` and `Status = Active`
  - Sort by createdAt DESC
  - Include author snapshot (ID, username, avatar)
  - Exclude deleted comments
  - Return with pagination metadata (page, total, hasMore)

#### FR-3.3.3 Edit Comment (Author Only)

- **Description:** Comment author can edit comment text.
- **Inputs:** Comment ID, post ID (for validation), new text, author ID
- **Outputs:** Updated comment, modifiedDate
- **Behavior:**
  - Verify requester is comment author or admin
  - Validate new text (same constraints as create)
  - Update comment text, set modifiedDate
  - Update `recentComments` array in post (if comment is in it)
  - Return updated CommentDto

#### FR-3.3.4 Delete Comment

- **Description:** Comment author or post owner can delete a comment.
- **Inputs:** Comment ID, post ID, requester ID
- **Outputs:** Comment soft-deleted, success notification
- **Behavior:**
  - Verify requester is comment author, post owner, or admin
  - Soft-delete: set `Status = Deleted`, `DeletedDate = now()`
  - Decrement post's `commentsCount`
  - Remove from post's `recentComments` array if present
  - Return success response
  - Notify clients via SignalR of comment removal

### 3.4 Notification System

#### FR-3.4.1 Generate Notifications

- **Description:** System automatically generates notifications for user interactions.
- **Behavior:**
  - **Trigger 1:** When user U2 comments on a post by U1, create notification: "U2 commented on your post"
  - **Trigger 2:** On comment creation, fetch post author and send notification document
  - Store in `notifications` collection: `{userId, postId, message, status, createdAt}`
  - Emit `CommentCreatedEvent` → `SendNotificationPostOwnerHandler`
  - Handler creates Notification document in MongoDB

#### FR-3.4.2 Receive Real-Time Notifications

- **Description:** Verified users receive push notifications in real-time via SignalR.
- **Inputs:** User ID (from JWT), SignalR connection
- **Outputs:** Real-time notification dispatch
- **Behavior:**
  - Client subscribes to NotificationHub on login
  - Backend sends notifications to user via: `Clients.User(userId).SendAsync("ReceiveCommentNotification", message)`
  - Notification appears in real-time to connected user(s)
  - If user offline, notification saved in DB for retrieval on next login
  - NotificationHub uses JWT token extracted from query string for authentication

#### FR-3.4.3 View Notification History

- **Description:** Users view their past notifications (paginated).
- **Inputs:** User ID (from JWT), page number
- **Outputs:** List of notifications (paginated, 10 per page), sorted by newest first
- **Behavior:**
  - Query `notifications` where `userId = <ID>` and `Status = Active`
  - Include post details (ID, text preview)
  - Include commenter details (username, avatar)
  - Sort by createdAt DESC
  - Return with pagination metadata

#### FR-3.4.4 Clear Notification

- **Description:** Users can dismiss/delete individual notifications.
- **Inputs:** Notification ID, user ID
- **Outputs:** Notification soft-deleted
- **Behavior:**
  - Verify requester owns notification
  - Soft-delete: `Status = Deleted`
  - Do NOT remove from DB
  - Return success response

#### FR-3.4.5 Update Notification Preferences

- **Description:** Users can configure notification settings.
- **Inputs:** Notification preferences (e.g., enable/disable email notifications)
- **Outputs:** Preferences saved, confirmation
- **Behavior:**
  - Store preferences on user profile: `profileNotificationStatus`
  - **Options:** All notifications, comments only, none
  - Use in backend to control which notifications to send/store
  - Respect preference when triggering notifications

### 3.5 Admin Moderation

#### FR-3.5.1 View Dashboard Analytics

- **Description:** Admins view platform statistics.
- **Inputs:** None (authorization: Admin role)
- **Outputs:** Dashboard with metrics
- **Behavior:**
  - Total users, posts, comments
  - Active users (last 7 days)
  - Reported content count
  - Banned user count
  - _Note: Full implementation planned for Phase 2_

#### FR-3.5.2 Ban User

- **Description:** Admin can ban a user, preventing access.
- **Inputs:** User ID, reason (optional)
- **Outputs:** User status updated
- **Behavior:**
  - Update user `Status = Banned`
  - User cannot login, comment, or post
  - User's future posts/comments marked as deleted/archived
  - Send email notification to user of ban and reason
  - Log admin action for audit trail
  - _Note: Full implementation planned for Phase 2_

#### FR-3.5.3 Remove Content

- **Description:** Admin can delete posts or comments.
- **Inputs:** Content ID (post or comment), reason
- **Outputs:** Content soft-deleted
- **Behavior:**
  - Soft-delete post or comment (set Status = Deleted)
  - Log admin action with reason and timestamp
  - Notify affected user(s)
  - _Note: Full implementation planned for Phase 2_

#### FR-3.5.4 View Reports

- **Description:** Admins review user-submitted reports on content.
- **Inputs:** Filter (status: pending/resolved)
- **Outputs:** List of reports with details
- **Behavior:**
  - _Planned for Phase 2, not yet implemented_

---

## 4. Non-Functional Requirements

### 4.1 Performance Requirements

| Requirement                 | Target                | Rationale                          |
| --------------------------- | --------------------- | ---------------------------------- |
| **Page Load Time**          | < 3 seconds           | User experience, SEO ranking       |
| **API Response Time (p95)** | < 500 ms              | User responsiveness                |
| **Feed Pagination**         | < 200 ms for 10 posts | Real-time feed interactions        |
| **SignalR Message Latency** | < 100 ms              | Real-time notifications            |
| **Concurrent Users**        | 1,000+ (scalable)     | Cloud deployment, auto-scaling     |
| **Database Query Time**     | < 50 ms (p95)         | Indexed queries, optimized schema  |
| **Cache Hit Rate**          | > 80%                 | Redis for frequently accessed data |

**Implementation:**

- MongoDB indexes on: userId, createdAt, postId, status
- Redis caching: user profiles, post listings, user feed
- GraphQL query optimization: field selection, no N+1 queries
- Image compression: server-side compress before serving
- CDN for static assets (frontend)

### 4.2 Scalability Requirements

| Aspect                 | Strategy                                                 |
| ---------------------- | -------------------------------------------------------- |
| **Horizontal Scaling** | Stateless API (JWT), enables multiple backend instances  |
| **Database Scaling**   | MongoDB replication & eventual sharding by user segment  |
| **Cache Scaling**      | Redis cluster for distributed caching                    |
| **Real-time Scaling**  | SignalR backplane (Message Bus) to sync across instances |
| **Frontend Scaling**   | Static site hosting, CDN distribution                    |

### 4.3 Availability Requirements

| Requirement                           | Target                                   |
| ------------------------------------- | ---------------------------------------- |
| **Uptime SLA**                        | 99.5% (monthly)                          |
| **MTTR** (Mean Time To Repair)        | < 15 minutes                             |
| **MTBF** (Mean Time Between Failures) | > 720 hours                              |
| **Graceful Degradation**              | API available even if email service down |

**Implementation:**

- Health checks on all endpoints
- Automated alerts for downtime
- Database backups (daily incremental, weekly full)
- Blue-green deployment for zero-downtime updates

### 4.4 Security Requirements

#### 4.4.1 Authentication & Authorization

- **JWT Tokens:**
  - Algorithm: HS256 (symmetric key)
  - Expiry: 24 hours
  - Claims: userId (sub), email, username, role
  - Refresh tokens: Implementation deferred to Phase 3
- **Password Security:**
  - Hashing: bcrypt with salt (cost factor 10+)
  - Min length: 8 characters
  - Complexity: enforced at UI level, optional backend validation
  - Expiry policy: None yet; planned for Phase 3

- **OAuth2:**
  - Redirect URI validated (exact match, HTTPS only in production)
  - State parameter to prevent CSRF on OAuth callback
  - Scope minimum: email, profile

- **Role-Based Access Control (RBAC):**
  - Two roles: User, Admin
  - Enforce at GraphQL resolver level via `[Authorize]` attribute
  - Claim: "Role" in JWT token

#### 4.4.2 Data Protection

- **Encryption at Rest:**
  - MongoDB: Enable encryption for sensitive fields (password hash, email)
  - OTP tokens: Short-lived, valid 30 minutes only
- **Encryption in Transit:**
  - HTTPS/TLS 1.2+ (mandatory in production, configured via reverse proxy)
  - WSS (Secure WebSocket) for SignalR
- **Sensitive Data Handling:**
  - Passwords: Never logged, never returned in API responses
  - Email addresses: Indexed for uniqueness, protected from anonymous users
  - OTP: Single-use, auto-deleted after expiry or use

#### 4.4.3 Attack Prevention

| Attack                                | Prevention Strategy                                                                 |
| ------------------------------------- | ----------------------------------------------------------------------------------- |
| **XSS (Cross-Site Scripting)**        | Sanitize user input on server (GraphQL validation), React auto-escapes JSX          |
| **CSRF (Cross-Site Request Forgery)** | CORS whitelist, SameSite cookies, state token in OAuth                              |
| **SQL Injection**                     | N/A (MongoDB doesn't use SQL), but validate all inputs                              |
| **NoSQL Injection**                   | Filter user input, use parameterized queries (MongoDB driver handles)               |
| **DDoS**                              | Rate limiting middleware: 100 requests/minute per IP/user ID                        |
| **Brute Force Login**                 | Failed login tracking (planned Phase 2), eventual IP throttling                     |
| **Broken Authentication**             | JWT validation on every request, secure token storage (localStorage→sessionStorage) |

#### 4.4.4 Authorization & Access Control

- **Endpoint-Level:** GraphQL `[Authorize]` attributes
- **Data-Level:**
  - Users can only edit/delete their own posts/comments
  - Admins can perform any action
  - AnonymousUser cannot post/comment (readonly access)
- **Field-Level:** Exclude sensitive fields from GraphQL schema (e.g., password hash)

### 4.5 Reliability Requirements

- **Data Loss Prevention:**
  - Soft deletes (logical delete) to preserve audit trail
  - Database backups daily
  - Transaction support for critical operations (e.g., post + notification creation)

- **Graceful Error Handling:**
  - Global exception middleware returns standardized error responses
  - GraphQL error filter returns meaningful messages to client
  - Failed external calls (email, storage) do not crash core API

- **Monitoring & Alerting:**
  - Application logs: errors, warnings, info level
  - Database query slow logs
  - Email delivery failures logged
  - Alerts: 500+ errors, uptime loss, latency spikes

### 4.6 Maintainability Requirements

- **Code:**
  - Clean Architecture: Domain, Application, Infrastructure layers
  - CQRS pattern (via MediatR) for command/query separation
  - Domain-Driven Design: entities, value objects, aggregates
  - Unit tests: minimum 70% code coverage for business logic
- **Documentation:**
  - Inline comments for complex logic
  - README with setup instructions
  - API schema auto-generated from GraphQL
  - Architecture decisions documented (ADRs)

- **CI/CD:**
  - Automated tests on each commit
  - Container image build & push to registry
  - Automated deployment to staging on merge to main
  - Manual approval for production release

### 4.7 Usability Requirements

- **Accessibility:**
  - WCAG 2.1 Level AA compliance (headings, labels, color contrast)
  - Keyboard navigation support
  - Screen reader friendly (semantic HTML)

- **Responsiveness:**
  - Mobile-first design
  - Breakpoints: 320px, 768px, 1024px, 1440px
  - Touch-friendly buttons/inputs (min 48px)

- **Localization:**
  - No explicit multi-language support yet
  - Structured for future i18n (strings in constants)

---

## 5. External Interfaces

### 5.1 API Interface (GraphQL)

**Endpoint:** `/graphql`  
**Authentication:** JWT Bearer token in Authorization header or query string (for SignalR)  
**Request Format:** GraphQL query/mutation  
**Response Format:** JSON with `data` and `errors` fields

**Example Query:**

```graphql
query GetPosts($keywords: String! = 0) {
  posts(keywords: $keywords, pageNumber: 1) {
    id
    text
    createdAt
    author {
      id
      username
      profile {
        url
      }
    }
    comments {
      id
      text
      author {
        username
      }
    }
  }
}
```

**Example Mutation:**

```graphql
mutation CreatePost($text: String!, $data: [Byte!]) {
  createPost(text: $text, data: $data) {
    id
    text
    createdAt
    author {
      username
    }
  }
}
```

### 5.2 Real-Time Interface (SignalR)

**Hub Endpoints:**

- `/notificationHub` – Real-time notifications
- `/feedHub` – Real-time feed updates (planned)

**Authentication:** JWT token passed as query string: `?access_token=<JWT>`

**Client Events (Server → Client):**

- `ReceiveCommentNotification(message: string)` – New comment notification
- `ReceivePostAdded(post: PostDto)` – New post added to feed

**Server Methods (Client → Server):**

- None yet (server-only push model)

### 5.3 Email Service Interface

**Provider:** SMTP (configurable)  
**Trigger Points:**

- User registration → verification email
- Password reset → OTP email
- Comment notification → email (based on preferences)
- User banned → notification email

**Email Templates:**

- Welcome email with verification link
- OTP password reset
- Comment notification
- Admin announcements (future)

### 5.4 OAuth2 External Providers

**Google OAuth2:**

- Endpoint: `https://accounts.google.com/o/oauth2/v2/auth`
- Scopes: `openid email profile`
- Redirect URI: `{APP_URL}/auth/google/callback`

**Facebook SDK:**

- App ID & Secret configured in environment
- Endpoint: `https://www.facebook.com/v18.0/dialog/oauth`
- Scopes: `email public_profile`
- Redirect URI: `{APP_URL}/auth/facebook/callback`

### 5.5 Image Storage Interface

**Provider Options:**

1. **Cloudinary (preferred for Phase 1)**
   - Upload: POST to Cloudinary API
   - Retrieval: CDN URL served directly to client
   - Transformations: Resize, compress via URL parameters

2. **AWS S3/Spaces (production)**
   - Upload: Presigned URL or direct API call
   - Retrieval: CloudFront CDN
   - Lifecycle policies: Auto-delete old images after N days

3. **Git LFS (dev/testing)**
   - Commit binary files to Git LFS
   - Not recommended for production due to bandwidth costs

---

## 6. Data Requirements

### 6.1 Data Collection

| Entity           | Fields                                                                        | Purpose                                |
| ---------------- | ----------------------------------------------------------------------------- | -------------------------------------- |
| **User**         | ID, email, username, password hash, profile pic, bio, status, role, createdAt | Identity, authentication, profile mgmt |
| **Post**         | ID, userId, text, image URL, status, createdAt, modifiedAt, deletedAt         | Content storage, versioning            |
| **Comment**      | ID, postId, userId, text, status, createdAt, modifiedAt, deletedAt            | Interaction tracking                   |
| **Notification** | ID, userId, postId, message, status, createdAt, deletedAt                     | User engagement, event log             |

### 6.2 Data Storage & Retention

- **Users:** No deletion (soft-delete with deletedAt timestamp)
- **Posts:** Retained indefinitely (soft-delete, may be archived to cold storage after 2 years)
- **Comments:** Retained indefinitely
- **Notifications:** Retained for 90 days; older notifications purged via scheduled job
- **Logs:** Retained for 30 days; archived to cold storage for compliance

### 6.3 Data Privacy

- PII fields: email, username
- User consent: Required for email verification and marketing emails
- GDPR compliance: Users can request data export (future)
- Passwords: Never stored in plaintext, never logged
- OAuth tokens: Not stored; only user profile data retained

---

## 7. Performance

### 7.1 Load Testing Targets

- **Throughput:** 100 requests/second
- **Concurrent Connections:** 1,000 users
- **Sustained Peak Load:** Handle 10x normal traffic for 10 minutes
- **Recovery Time:** Return to normal response times within 5 minutes post-spike

### 7.2 Optimization Strategies

1. **Database:**
   - Create indexes: `{userId: 1}`, `{createdAt: -1}`, `{postId: 1}`, `{status: 1}`
   - Use aggregation pipeline for complex queries
   - Projection: Return only needed fields

2. **Caching:**
   - Redis TTL: 1 hour for user profiles, 30 min for post listings
   - Cache invalidation: Manual invalidation on write, or TTL expiry
   - Cache warming: Pre-load hot data on startup

3. **API:**
   - Pagination: Cursor-based (keyset) or offset-based (max 1000/10000 limit)
   - Compression: Gzip responses ~ 70% reduction
   - Batch queries: Support batching multiple GraphQL operations

4. **Frontend:**
   - Code splitting (Next.js automatic)
   - Image optimization (Next.js Image component)
   - Lazy loading (Intersection Observer API)
   - CSS-in-JS optimization (Tailwind CSS, automatic purging)

---

## 8. Security

### 8.1 Security Checklist

- [ ] HTTPS/TLS 1.2+ enforced in production
- [ ] CORS whitelist configured (no wildcard in production)
- [ ] JWT validation on every authenticated request
- [ ] Password hashing: bcrypt with salt
- [ ] Rate limiting: 100 requests/min per user/IP
- [ ] Input validation: All user inputs validated server-side
- [ ] Output encoding: Prevent XSS (auto via React)
- [ ] CSRF tokens: Included in OAuth state parameter
- [ ] SQL/NoSQL injection: Not applicable; use safe driver APIs
- [ ] Sensitive data: No hardcoded secrets; use environment variables
- [ ] Error messages: Generic messages in production (no stack traces to client)
- [ ] Logging: No PII in logs; access logs with sensitive fields masked
- [ ] Dependencies: Regular security audits, patch management
- [ ] Secrets management: Use Azure Key Vault or AWS Secrets Manager

### 8.2 Vulnerability Assessment

**Open/Planned:**

- Implement refresh token mechanism (vs. single 24h token)
- Add user device fingerprinting for anomaly detection
- Implement two-factor authentication (2FA) for Admins
- Regular penetration testing (quarterly)

---

## Appendix

### A. Glossary

- **Soft Delete:** Mark record as deleted without physically removing it
- **Denormalization:** Storing redundant data for performance (e.g., author snapshot in post)
- **Aggregation:** MongoDB operation to transform/group documents
- **Event Sourcing:** Storing immutable events instead of final state (future consideration)

### A. Acronyms Reference

- **CQRS:** Command Query Responsibility Segregation
- **DDD:** Domain-Driven Design
- **DTO:** Data Transfer Object
- **JWT:** JSON Web Token
- **OAuth2:** Open Authorization 2.0 Protocol
- **RBAC:** Role-Based Access Control
- **SLA:** Service Level Agreement
- **XSS:** Cross-Site Scripting
- **CSRF:** Cross-Site Request Forgery

---

**Document End**
