# Pranav Enterprise .NET Web Application
> **Enterprise-grade ASP.NET Core web platform featuring Identity Authentication, Role-Based Access Control (RBAC), Interactive Analytics Dashboard, and Full Business Management Modules.**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-emerald.svg)]()
[![Database](https://img.shields.io/badge/Database-EF%20Core%20(SQLite%20%2F%20SQL%20Server)-orange.svg)]()
[![Security](https://img.shields.io/badge/Security-Identity%20%2B%20JWT%20%2B%20RBAC-red.svg)]()

> 📖 **Comprehensive Manual**: For an in-depth beginner-to-advanced architectural guide, full API catalog with request/response payloads, ER diagram, and step-by-step business workflows, see [PROJECT_DOCUMENTATION.md](PROJECT_DOCUMENTATION.md).

---

## 🌟 Key Features

1. **Enterprise Clean Architecture**:
   - `PranavEnterprises.Domain`: Entities, Enums, Interfaces, Auditable base classes, and RBAC constants.
   - `PranavEnterprises.Application`: DTOs, Service Contracts, Business validation models, and ViewModels.
   - `PranavEnterprises.Infrastructure`: EF Core DbContext, Repositories, Database Seeder, Identity store, JWT Service, and Audit Logging.
   - `PranavEnterprises.Web`: ASP.NET Core MVC & RESTful Web APIs, Centralized Exception Middleware, Swagger UI, and modern Bootstrap 5 UI.

2. **Dual Authentication & Security**:
   - **Interactive Web App**: ASP.NET Core Identity with secure, HTTP-only, sliding-expiration Cookie authentication and Anti-Forgery token protection.
   - **Headless & External APIs**: JWT (JSON Web Tokens) with HMAC-SHA256 signatures, customizable lifetime, and Bearer token authentication.
   - **Account Management**: Self-registration, login, profile management, and password change with audit logging.
   - **Lockout Protection**: Automatically locks accounts after 5 consecutive failed login attempts.

3. **Role-Based Access Control (RBAC)**:
   - **Super Admin**: Complete administrative oversight, user provisioning, custom role management, system settings, and audit logs.
   - **Admin**: User and employee management, project oversight, task allocation, and operational reports.
   - **Manager**: Project planning, milestone tracking, task delegation, and workforce reviews.
   - **Employee**: Personal dashboard, assigned task tracking, status updates, and profile maintenance.

4. **Interactive Executive Dashboard**:
   - **KPI Cards**: Workforce headcount, active strategic projects, task completion rates, and total managed portfolio budget.
   - **Interactive Charts (Chart.js)**:
     - Monthly Productivity Velocity & Task Delivery Trend (Line chart)
     - Projects by Lifecycle Status (Doughnut chart)
     - Department Headcount Allocation (Horizontal Bar chart)
     - Task Status Breakdown (Doughnut chart)
   - **Operational Feeds**: Real-time audit trail and upcoming urgent task deadlines.

5. **Complete Business CRUD Modules**:
   - **Employees Directory**: Workforce roster, department filtering, salary records, hire dates, status tracking, and active task counters.
   - **Projects Portfolio**: Strategic initiatives, client organizations, budget allocations, start/end dates, managers, and progress bars.
   - **Tasks Management**: Granular work items, estimated vs. logged hours, priority levels, assignees, and quick status transitions.
   - **User & Access Management**: Provisioning users, role assignment, active/inactive toggle, and deletion.
   - **Role Management**: Creating and inspecting custom security roles.
   - **Activity Logs**: Centralized immutable audit trail logging user, role, IP address, timestamp, module, and details.

---

## 🔑 Pre-Configured Demo Accounts

The application automatically seeds 4 test accounts representing each RBAC tier. On the Login screen, click any **"Quick Demo Login"** button to autofill credentials instantly!

| Role | User Name | Email | Password | Access Capabilities |
| :--- | :--- | :--- | :--- | :--- |
| **Super Admin** | Pranav Naik | `superadmin@pranav.com` | `P@ssword123!` | Full system control, Role approval, Role management, User deletion, System audit |
| **Admin** | Amruta Naik | `admin@pranav.com` | `P@ssword123!` | User management, Employee CRUD, Project CRUD, Task CRUD |
| **Manager** | Shubham Naik | `manager@pranav.com` | `P@ssword123!` | Project & Task creation/management, Team tracking |
| **Employee** | Ram Naik | `employee@pranav.com` | `P@ssword123!` | Assigned tasks execution, Profile management |

> 📬 **New User Registration & Approval**: When a new user registers, an automated notification is sent to **`pranavnaik355@gmail.com`**. The account remains pending and inactive until Super Admin (**Pranav Naik**) reviews the candidate and approves their role at `/Users/Pending`.

---

## 🚀 Setup & Quickstart

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- Any modern web browser (Edge, Chrome, Firefox).

### 1. Run the Application
Open PowerShell or your command prompt in the root project folder:

```powershell
# Restore dependencies and build
dotnet build

# Run the web application
dotnet run --project src/PranavEnterprises.Web/PranavEnterprises.Web.csproj
```

The application will start and listen on:
- Web Portal: **http://localhost:5000** (or https://localhost:5001)
- Swagger OpenAPI UI: **http://localhost:5000/swagger**

> **Note**: On first run, the database is created and automatically seeded with roles, users, departments, employees, projects, tasks, and audit logs. No manual SQL scripts required!

---

## 🗄️ Database Configuration (SQLite vs. SQL Server)

The application supports both **SQLite** (default for zero-configuration, instant execution) and **Microsoft SQL Server**.

To switch database providers, update `src/PranavEnterprises.Web/appsettings.json`:

### Option A: SQLite (Default)
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=PranavEnterprises.db"
  }
}
```

### Option B: Microsoft SQL Server
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PranavEnterprisesDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## 📡 RESTful API Documentation & Testing

The system exposes fully protected REST APIs documented with Swagger/OpenAPI.

### Interactive Swagger UI
1. Navigate to **http://localhost:5000/swagger**.
2. Click `POST /api/auth/login`, click **Try it out**, enter:
   ```json
   {
     "email": "superadmin@pranav.com",
     "password": "P@ssword123!",
     "rememberMe": true
   }
   ```
3. Copy the returned `token`.
4. Click the green **Authorize** button at the top of Swagger, enter:
   `Bearer <your_token>`
5. All protected endpoints (`/api/employees`, `/api/projects`, `/api/tasks`, `/api/dashboard/summary`) can now be tested directly in the browser!

### Testing via cURL / PowerShell
```powershell
# 1. Authenticate & Obtain JWT Token
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
  -Method Post `
  -Body (@{ email = "superadmin@pranav.com"; password = "P@ssword123!" } | ConvertTo-Json) `
  -ContentType "application/json"

$token = $response.token

# 2. Call Protected Employees API
Invoke-RestMethod -Uri "http://localhost:5000/api/employees" `
  -Headers @{ Authorization = "Bearer $token" }

# 3. Call Dashboard Summary API
Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/summary" `
  -Headers @{ Authorization = "Bearer $token" }
```

---

## 🏗️ Architecture & Project Structure

```
PranavEnterprises/
├── PranavEnterprises.sln
├── README.md
├── src/
│   ├── PranavEnterprises.Domain/          # Core Domain Entities & Rules
│   │   ├── Common/                       # BaseEntity, IAuditableEntity
│   │   ├── Constants/                    # AppRoles, AppPolicies
│   │   ├── Entities/                     # ApplicationUser, ApplicationRole, Employee, Project, ProjectTask, Department, ActivityLog
│   │   └── Enums/                        # EmployeeStatus, ProjectStatus, PriorityLevel, TaskItemStatus
│   │
│   ├── PranavEnterprises.Application/     # Application Contracts & DTOs
│   │   ├── DTOs/                         # AuthDtos, EmployeeDtos, ProjectDtos, TaskDtos, UserDtos, DashboardDtos, ActivityLogDto
│   │   └── Interfaces/                   # IAuthService, IEmployeeService, IProjectService, ITaskService, IDashboardService, etc.
│   │
│   ├── PranavEnterprises.Infrastructure/  # Persistence & External Services
│   │   ├── Data/                         # ApplicationDbContext, DbInitializer (Seed data)
│   │   └── Services/                     # AuthService, EmployeeService, ProjectService, TaskService, DashboardService, JwtTokenService, UserService, RoleService, ActivityLogService
│   │
│   └── PranavEnterprises.Web/             # Presentation & RESTful APIs
│       ├── Controllers/                  # MVC Controllers (Account, Dashboard, Employees, Projects, Tasks, Users, Roles, ActivityLogs)
│       │   └── Api/                      # REST API Controllers (AuthApiController, EmployeesApiController, ProjectsApiController, TasksApiController, DashboardApiController)
│       ├── Middleware/                   # Centralized ExceptionHandlingMiddleware
│       ├── Views/                        # Razor Views with Bootstrap 5 & Chart.js
│       ├── wwwroot/                      # CSS, JS, Bootstrap, and UI Assets
│       ├── Program.cs                    # Application Dependency Injection, Auth Pipeline, RBAC Policies, Swagger
│       └── appsettings.json              # Provider Configuration, JWT Settings, Connection Strings
```

---

## 🛡️ Enterprise Development Best Practices

- **Centralized Exception Handling**: Custom global middleware catches unhandled exceptions, logs full stack traces, and formats uniform JSON errors for APIs or friendly error views for Web.
- **Audit Logging**: Every create, update, delete, and authentication action records who performed it, their role, the timestamp, and client IP address.
- **Soft Deletion**: Entities inherit `IsDeleted` with EF Core query filters to prevent accidental irreversible data loss.
- **Input Validation**: DataAnnotations and model state validation enforce rigorous enterprise input constraints.
- **Password Security**: Identity hashes passwords using PBKDF2 with SHA-256 and unique per-user cryptographic salts.

---

&copy; 2026 **Pranav Enterprises**. All Rights Reserved.
