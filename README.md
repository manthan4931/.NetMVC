# ERP Project Manager — ASP.NET Core MVC Backend

A full **ASP.NET Core MVC 8** backend migrated from Python/Django REST Framework, serving a React SPA frontend.

---

## Project Structure

```
ERP.Backend/
├── Controllers/          # MVC Controllers (inheriting Controller, return Json())
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── DepartmentsTeamsController.cs
│   ├── ProjectsController.cs
│   ├── TasksController.cs
│   ├── TimeEntriesController.cs
│   ├── ActivitiesController.cs
│   └── HomeController.cs          # SPA fallback
├── Models/
│   ├── Entities.cs               # All 11 EF Core entity classes
│   └── Enums/Enums.cs            # All domain enums
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core context + SaveChanges signal override
│   └── Migrations/               # EF Core SQLite migrations
├── Services/
│   ├── Interfaces/IServices.cs   # All service interfaces
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── DepartmentTeamService.cs
│   ├── ProjectService.cs
│   ├── MilestoneTaskService.cs
│   ├── TimeTrackingService.cs
│   └── ActivityService.cs
├── ViewModels/ViewModels.cs      # All request/response DTOs
├── Helpers/ClaimsHelper.cs       # JWT claims extraction
├── Views/Home/Index.cshtml       # React SPA bootstrap view
├── wwwroot/                      # Static assets + React build output
├── Program.cs                    # Full pipeline: JWT + EF + MVC + SPA fallback
└── appsettings.json
```

---

## Tech Stack

| Layer        | Technology                          |
|-------------|-------------------------------------|
| Framework   | ASP.NET Core MVC 8                  |
| ORM         | Entity Framework Core 8 + SQLite    |
| Auth        | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| Password    | BCrypt.Net-Next                     |
| Frontend    | React (SPA, served via MVC fallback)|

---

## API Endpoints

### Authentication — `/api/auth`
| Method | Endpoint                    | Auth | Description              |
|--------|-----------------------------|------|--------------------------|
| POST   | `/api/auth/login`           | No   | Login, returns JWT token |
| POST   | `/api/auth/register`        | No   | Register new user        |
| POST   | `/api/auth/token/refresh`   | No   | Refresh JWT token        |
| GET    | `/api/auth/me`              | Yes  | Get current user profile |
| POST   | `/api/auth/change-password` | Yes  | Change password          |
| POST   | `/api/auth/logout`          | Yes  | Logout (stateless)       |

### Users — `/api/users`
| Method | Endpoint                    | Description                |
|--------|-----------------------------|----------------------------|
| GET    | `/api/users`                | List all users (paginated) |
| GET    | `/api/users/{id}`           | Get user by ID             |
| GET    | `/api/users/me`             | Get current user           |
| PUT    | `/api/users/{id}`           | Update user profile        |
| PUT    | `/api/users/me`             | Update own profile         |
| POST   | `/api/users/upload-avatar`  | Upload avatar image        |
| DELETE | `/api/users/{id}`           | Deactivate user (admin)    |

### Projects — `/api/projects`
| Method | Endpoint                                  | Description            |
|--------|-------------------------------------------|------------------------|
| GET    | `/api/projects`                           | List projects          |
| POST   | `/api/projects`                           | Create project         |
| GET    | `/api/projects/{id}`                      | Get project detail     |
| PUT    | `/api/projects/{id}`                      | Update project         |
| DELETE | `/api/projects/{id}`                      | Delete project         |
| GET    | `/api/projects/{id}/stats`                | Project statistics     |
| GET    | `/api/projects/{id}/members`              | List members           |
| POST   | `/api/projects/{id}/members`              | Add member             |
| DELETE | `/api/projects/{id}/members/{memberId}`   | Remove member          |
| GET    | `/api/projects/{id}/milestones`           | List milestones        |
| POST   | `/api/projects/{id}/milestones`           | Create milestone       |
| PUT    | `/api/projects/{id}/milestones/{msId}`    | Update milestone       |
| DELETE | `/api/projects/{id}/milestones/{msId}`    | Delete milestone       |

### Tasks — `/api/tasks`
| Method | Endpoint                                        | Description           |
|--------|-------------------------------------------------|-----------------------|
| GET    | `/api/tasks`                                    | List tasks (filtered) |
| POST   | `/api/tasks`                                    | Create task           |
| GET    | `/api/tasks/{id}`                               | Get task detail       |
| PUT    | `/api/tasks/{id}`                               | Update task           |
| DELETE | `/api/tasks/{id}`                               | Delete task           |
| GET    | `/api/tasks/{id}/comments`                      | List comments         |
| POST   | `/api/tasks/{id}/comments`                      | Add comment           |
| PUT    | `/api/tasks/{taskId}/comments/{commentId}`      | Update comment        |
| DELETE | `/api/tasks/{taskId}/comments/{commentId}`      | Delete comment        |
| POST   | `/api/tasks/{id}/attachments`                   | Upload attachment     |
| DELETE | `/api/tasks/{taskId}/attachments/{attachmentId}`| Delete attachment     |

### Time Tracking — `/api/timeentries`
| Method | Endpoint                        | Description                    |
|--------|---------------------------------|--------------------------------|
| GET    | `/api/timeentries`              | List entries (filterable)      |
| POST   | `/api/timeentries`              | Create manual entry            |
| GET    | `/api/timeentries/{id}`         | Get entry                      |
| PUT    | `/api/timeentries/{id}`         | Update entry                   |
| DELETE | `/api/timeentries/{id}`         | Delete entry                   |
| POST   | `/api/timeentries/start`        | **Start stopwatch timer**      |
| POST   | `/api/timeentries/stop`         | **Stop running timer**         |
| GET    | `/api/timeentries/running`      | Get current running timer      |
| GET    | `/api/timeentries/export_csv`   | Export entries as CSV download |

### Today Dashboard — `/api/today`
| Method | Endpoint    | Description                        |
|--------|-------------|------------------------------------|
| GET    | `/api/today`| Active timers, stats, dev summary  |

### Departments & Teams
| Method | Endpoint                | Description       |
|--------|-------------------------|-------------------|
| GET    | `/api/departments`      | List departments  |
| POST   | `/api/departments`      | Create department |
| PUT    | `/api/departments/{id}` | Update department |
| DELETE | `/api/departments/{id}` | Delete department |
| GET    | `/api/teams`            | List teams        |
| POST   | `/api/teams`            | Create team       |
| PUT    | `/api/teams/{id}`       | Update team       |
| DELETE | `/api/teams/{id}`       | Delete team       |

### Activities — `/api/activities`
| Method | Endpoint           | Description             |
|--------|--------------------|-------------------------|
| GET    | `/api/activities`  | List activity logs      |
| GET    | `/api/activities/my`| My activity feed       |

---

## Running the Backend

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Steps

```bash
cd ERP.Backend

# Restore packages
dotnet restore

# Apply database migrations (creates erp.db)
dotnet ef database update

# Run the server
dotnet run
```

Server starts on **http://localhost:5000**

---

## Deploying the React Frontend

```bash
# 1. Build React app (from the frontend project root)
npm run build

# 2. Copy build output into .NET wwwroot
cp -r dist/* ERP.Backend/wwwroot/

# 3. Update Views/Home/Index.cshtml with the Vite-generated index.html content
```

The React app will be served at `/` and all `/api/*` requests will be handled by the MVC controllers.

---

## Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=erp.db"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "ERP.Backend",
    "Audience": "ERP.Frontend"
  }
}
```

---

## Key Architecture Decisions

1. **MVC Controllers** inherit from `Controller` (not `ControllerBase`) and return `Json(data)` for all API responses — matching Django REST Framework's camelCase JSON output.
2. **JWT Auth** — stateless Bearer token authentication; the `HomeController.Index()` SPA fallback is the only unauthenticated view.
3. **EF Core SaveChanges Override** — automatically recalculates `actualHours` on any `TimeEntry` change, replicating Django's post-save signals.
4. **SPA Routing** — `app.MapFallbackToController("Index", "Home")` ensures React's client-side router handles all non-API URL paths.
5. **SQLite** — zero-config database for development; swap connection string for PostgreSQL/SQL Server in production.
