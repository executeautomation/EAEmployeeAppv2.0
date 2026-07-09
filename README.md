# EAEmployee — ASP.NET Core 8 Employee Management System

> A modern, role-aware HR management application built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**. Migrated and redesigned from the original .NET Framework demo at [ExecuteAutomation.com](https://executeautomation.com).

---

## Screenshots

| Home | Employee List | Create Employee |
|------|--------------|-----------------|
| Dark gradient hero with EA logo | Shadowed table with grade pills & search | Two-column card form with dropdown grade |


---

## Features

- **Employee CRUD** — Create, read, update, and delete employee records (Administrators only)
- **Search** — Filter employees by name from the list view
- **PF Contributions** — View employee and employer Provident Fund contributions calculated inline (no external service)
- **Role-Based Access Control** — Two roles enforced via ASP.NET Core Identity:
  - `Administrator` — full CRUD on employees
  - `User` — read-only access to Employee Details (PF & company contributions)
- **Authentication** — Custom login, register, forgot password, change password pages (no scaffolded Identity UI)
- **Modern UI** — Fully redesigned with Bootstrap 5, custom CSS; responsive across pages

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQLite (`app.db`) |
| Auth | ASP.NET Core Identity with Roles |
| Frontend | Bootstrap 5 + custom CSS |
| Runtime | .NET 8 |

### NuGet Packages

| Package | Version |
|---------|---------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.6 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.6 |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.6 |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.6 |

---

## Project Structure

```
EAEmployee.Net8/
├── Controllers/
│   ├── AccountController.cs        # Login, Register, Logout, ForgotPassword
│   ├── EmployeeController.cs       # CRUD for employees
│   ├── EmployeeDetailsController.cs # PF & Company contribution views
│   ├── HomeController.cs           # Home, About, Contact
│   └── ManageController.cs         # Change/Set password
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext (Identity + Employee)
│   └── SeedData.cs                 # Seeds roles and default admin on startup
├── Models/
│   ├── ApplicationUser.cs          # Custom IdentityUser
│   ├── Employee.cs                 # Employee entity
│   ├── AccountViewModels.cs        # Login, Register, ForgotPassword VMs
│   └── ManageViewModels.cs         # ChangePassword, SetPassword VMs
├── Migrations/                     # EF Core migration history
├── Views/
│   ├── Account/                    # Login, Register, ForgotPassword (standalone pages)
│   ├── Employee/                   # Index, Create, Edit, Delete
│   ├── EmployeeDetails/            # Index, EmployeePF, EmployeeBonus
│   ├── Home/                       # Index (hero + courses), About
│   ├── Manage/                     # Index, ChangePassword, SetPassword
│   └── Shared/                     # _Layout, _LoginPartial, Error
├── wwwroot/
│   ├── css/site.css                # Custom styles (hero, cards, badges)
│   └── images/                     # Logo files (logo.png, EA-Icon.png, EA-Icon-green.png)
├── app.db                          # SQLite database (auto-created)
├── appsettings.json
└── Program.cs                      # DI, Identity config, middleware pipeline
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the Application

```bash
cd EAEmployee.Net8
dotnet run
```

The app starts at **http://localhost:5114** (port defined in `Properties/launchSettings.json`).

The database is automatically created and seeded on first run.

### Database Migrations

If you need to re-apply or add migrations:

```bash
# Install EF tools (once)
dotnet tool install --global dotnet-ef

# Apply existing migrations
dotnet ef database update

# Add a new migration
dotnet ef migrations add <MigrationName>
```

---

## Default Credentials

| Username | Password | Role |
|----------|----------|------|
| `admin` | `password` | Administrator |

> New users who register through the UI are automatically assigned the `User` role.

---

## Employee Model

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Full name |
| `Salary` | `float` | Monthly salary |
| `DurationWorked` | `int` | Months worked |
| `Grade` | `int` | 1 = Junior, 2 = Middle, 3 = Senior, 4 = C-Level |
| `Email` | `string` | Email address |

### PF Contribution Formulas

```
Employee PF  = Salary × 12% × DurationWorked
Employer PF  = Salary × 12% × DurationWorked + (Grade × Salary × 2%)
```

---

## Roles & Permissions

| Action | Administrator | User | Anonymous |
|--------|:---:|:---:|:---:|
| View Employee List | ✅ | ✅ | ✅ |
| Create / Edit / Delete Employee | ✅ | ❌ | ❌ |
| View Employee Details (PF) | ✅ | ✅ | ❌ |
| Manage own account | ✅ | ✅ | ❌ |

---

## Origin

This project is a full migration of the original **ExecuteAutoEmployee** ASP.NET MVC (.NET Framework) application originally built by [Karthik KK](https://executeautomation.com/about) at ExecuteAutomation. The following were intentionally excluded from the migration:

- `PFServiceClient` WCF service (replaced by inline calculation)
- `Benefits` and `Role` management modules
- SQL Server dependency (replaced by SQLite)

---

## About ExecuteAutomation

ExecuteAutomation specialises in **AI and Generative AI testing education** with 370,000+ students across 198 countries.

- 🌐 [executeautomation.com](https://executeautomation.com)
- 🎓 [All Courses](https://executeautomation.com/courses)
- 💼 [LinkedIn — Karthik KK](https://www.linkedin.com/in/karthikkk/)
- 📺 [YouTube](https://youtube.com/executeautomation)
