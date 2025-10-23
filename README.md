# BiUrSite - A Web Application for Sharing Ideas and Seeking Advice

BiUrSite is a platform where users can express their ideas, feelings, and challenges in life. Users can post anonymously or create verified accounts to share their thoughts, comment on others’ posts, and receive advice. Admins can manage the platform and ensure a safe environment for everyone.

## Features

### User Features:

- **Anonymous Users**:
  - Can create/register on the platform.
  - Can view posts (only view, no interaction allowed).
- **Verified Users**:
  - Can upload posts.
  - Can comment on posts.
  - Can report issues, provide feedback, and suggest improvements.
  - Must verify their email to ensure authenticity.

### Admin Features:

- **Admin Dashboard**:
  - View reports on total posts, active users, and more.
  - Can manage user behavior by banning users based on their actions.
  - Can make announcements visible to all users.
- **User Management**:
  - Can ban users for a set period based on their behavior.

### Additional Features:

- **Email Verification**:
  - Users must verify their email addresses through a confirmation link before posting or commenting.
- **Ratings & Feedback**:
  - Users can rate posts and provide feedback on advice given.

---

## Development Stack

### Backend:

- **Technology**: ASP.NET Core Web API (MediatR for application layer, Domain-driven structure)
- **Database**: MongoDB (document store) — the project is wired to MongoDB in `appsettings.json` and infrastructure code
- **Caching / Rate limiting / Backplane**: Redis (StackExchange.Redis)
- **Realtime**: SignalR (Notification/Feed hubs)
- **Messaging**: Rebus (used for background/message handlers)
- **Image storage**: GitHubContents-backed implementation exists for small projects; consider Cloudinary/S3 for production
- **Email Service**: SMTP (configured via appsettings / env vars)
- **Testing**: Tests project present (unit/integration tests)
- **Authentication/Authorization**: JWT + OAuth providers supported (Google/Facebook) and role-based checks

### Frontend:

- **Framework**: Angular
- **Realtime client**: @microsoft/signalr for SignalR connections
- **Build**: Standard Angular CLI build (`npm run build`)

### Tools:

- **Version Control**: Git
- **Admin Portal**: Backend contains controllers for admin functionality (Razor admin may be present in other branches)
- **Containerization**: Docker / docker-compose for local development; Docker images are produced using the provided Dockerfiles
- **Cloud**: Render / Netlify / Docker Hub are referenced in docs as quick hosting options; the repo is cloud-agnostic
- **Environment Management**:
  - Use `appsettings.json` for defaults and environment variables for secrets. A `.env` (or `example.env`) is used by `docker-compose` for local runs.

### Code Principles:

- Follow **Clean Code** principles.
- Sensitive information should **never be hardcoded** in the source code.

---

## Project Phases

### Phase 1: Initial Setup

- Design the project structure.
- Build a working API with OpenAPI documentation.
- Version the API for future changes.
- Implement basic **CRUD** operations.
- Write **unit tests** for various test cases.

### Phase 2: Authentication & Authorization

- Implement **Authentication** (AuthN) and **Authorization** (AuthZ).
- Modify and update unit tests for the new changes.

### Phase 3: Security Enhancements

- Eliminate security vulnerabilities (e.g., DDOS, XSS, CSRF attacks).

### Phase 4: Frontend Implementation

- Work with **static data.json** for frontend pages.
- Implement and design all necessary pages.

### Phase 5: Integration

- Integrate backend and frontend functionality.
- Test API calls and frontend data handling.

### Phase 6: Deployment

- Prepare and deploy the app using **Docker** and **Kubernetes**.
- Deploy to cloud platforms like **AWS** or **Azure**.

---

## Docker Setup

This project uses **Docker** to simplify setup and deployment. The services required for the application are defined in the `docker-compose.yml` file.

### Services:

- **sql_server_container**: A SQL Server container that stores the application's data.
- **backend**: The .NET API backend, which interacts with the SQL Server.
- **frontend**: The Angular frontend that communicates with the backend API.

### Setting up and running locally (Docker)

Prerequisites: Docker Desktop (or Docker + Compose) installed.

1. Copy or create a `.env` based on `example.env` or `example.env` in the repo (the repo includes `example.env`). Ensure you set at least the MongoDB and JWT secrets for local runs.

2. Build and start with docker-compose (recommended):

```powershell
docker-compose up -d --build
```

What this does:

- Builds backend & frontend images using the provided Dockerfiles.
- Starts the containers defined in `docker-compose.yml` (backend, frontend, redis). If you want a local MongoDB for development, add a `mongo` service to your compose file or point `MONGODB_CONNECTION_STRING` to a local/Atlas instance.

Accessing the app locally:

- Frontend: http://localhost (port 80 mapped by compose)
- Backend API: http://localhost:5000 (compose maps 5000 -> container 8080)

If you prefer to run only the backend from your machine, run the API project with `dotnet run` from `backend/API` and configure environment variables locally.

---

## Database Setup:

This project uses MongoDB as the primary data store. The connection is configured via `MONGODB_CONNECTION_STRING` and `MONGODB_NAME` (see `backend/API/appsettings.json`).

For local development:

- Option A (recommended): add a `mongo` service to `docker-compose.yml` (image `mongo:6.0`) or run a local MongoDB instance and point `MONGODB_CONNECTION_STRING` at it.
- Option B: use a managed Atlas cluster and set the connection string in your `.env`.

The repo includes JSON Schema validators and sample documents in `docs/ARCHITECTURE.md` that you can use to create collections and indexes during local setup.

---

## Contribution

We welcome contributions. A few suggestions to speed up contributions:

- Open an issue describing the change first for non-trivial work.
- Create small, focused PRs that include tests when possible.
- Use the existing DDD folder layout (API / Application / Domain / Infrastructure) and add tests under `Tests/`.

If you want me to, I can also add starter GitHub Actions workflows for PR checks (dotnet build/test and angular build).

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
