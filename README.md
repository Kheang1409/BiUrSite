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
- **Technology**: .NET API (Latest version)
- **Database**: MSSQL
- **Caching**: Redis (Distributed Cache)
- **Email Service**: SMTP
- **Testing**: xUnit (Unit tests)
- **Authentication/Authorization**: Role-based and policy-based

### Frontend:
- **Framework**: Angular
- **Frontend Data**: Static data initially (data.json)
  
### Tools:
- **Version Control**: Git
- **Admin Portal**: Built with Razor + API
- **Containerization**: Docker (final deployment)
- **Cloud**: AWS/Azure/Kubernetes (for deployment)
- **Environment Management**: 
  - Use `appsettings.json` (for .NET) or `environment.variable.ts` (for Angular) for configuration management.
  
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

### Setting Up Docker

Ensure that **Docker** and **Docker Compose** are installed on your machine.

- Build and start the services:

```bash
docker-compose up --build
```

This command will:
- Build the Docker images for the backend and frontend.
- Start the SQL Server container.
- Start the backend and frontend services.

### Accessing the Application
- **Frontend**: Open your browser and go to [http://localhost:8080](http://localhost:8080).
- **Backend API**: The backend API will be available at [http://localhost:5000](http://localhost:5000).

---

## Database Setup:
The database is automatically created and configured by the SQL Server container. The backend will connect to the database using the connection string defined in the Docker Compose file.

---

## Contribution

We welcome contributions from the community! If you have any suggestions or improvements, feel free to fork the repository, create a pull request, or open an issue.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.