# EAEmployee App — Knowledge Base

> This document helps AI agents and developers quickly understand the codebase without re-exploring from scratch.

---

## Tech Stack

- **Framework**: ASP.NET Core 8 MVC (server-rendered Razor views, no API controllers or Blazor)
- **Database**: SQLite (`app.db`), EF Core with `IdentityDbContext`
- **Auth**: ASP.NET Core Identity with role-based access
- **Frontend**: Bootstrap 5 + jQuery (loaded via `_Layout.cshtml`)
- **No icon library** — all icons are HTML entities (e.g., `&#128101;`, `&#128200;`)

## Project Structure

```
Controllers/
  HomeController.cs          — Home, About, Dashboard (analytics)
  EmployeeController.cs      — Employee CRUD (writes require Administrator role)
  EmployeeDetailsController.cs — PF & Bonus calculations (requires auth)
  AccountController.cs       — Login, Register, Logout
  ManageController.cs        — User account management

Models/
  Employee.cs                — Core entity
  ApplicationUser.cs         — Identity user model
  ErrorViewModel.cs          — Error page model

Data/
  ApplicationDbContext.cs    — DbContext with single DbSet: Employees
  SeedData.cs               — Seeds roles and default admin user
  Migrations/               — EF Core migrations

Views/
  Employee/                  — Index (grid), Create, Edit, Delete
  EmployeeDetails/           — PF contribution, Bonus/Company contribution
  Home/                      — Index (landing), About, Contact, Dashboard
  Shared/                    — _Layout, _LoginPartial, _ValidationScriptsPartial
```

## Employee Model

| Field | Type | Details |
|-------|------|---------|
| `Id` | int | Primary key, auto-increment |
| `Name` | string | Required |
| `Salary` | float | Required, monthly salary |
| `Age` | int | Required, range 18–100 |
| `DurationWorked` | int | Required, in **months** (not years) |
| `Grade` | int | Required. 1=Junior, 2=Middle, 3=Senior, 4=C-Level |
| `Email` | string | Required, validated as email, used for **duplicate detection** |

## Authentication & Roles

| Role | Access |
|------|--------|
| `Administrator` | Full CRUD on employees, access to Details |
| `User` | Read-only employee list, access to Details (PF/Bonus) |
| Anonymous | Can view employee list and home page only |

**Default admin seed**: username `admin`, email `admin@executeautomation.com`, password `password`

## Key Business Logic

### Duplicate Detection (EmployeeController)
- Uniqueness is checked by **Email** on the Create POST action
- If duplicate found, returns **JSON** (`{ isDuplicate: true, employee: {...} }`) for AJAX handling
- The Create view intercepts form submit with `$.ajax` and shows a Bootstrap modal on duplicate
- On success (no duplicate), returns a standard redirect to Index

### PF Calculations (EmployeeDetailsController)
- **Employee PF**: `12% × Salary × DurationWorked (months)`
- **Employer Contribution**: `PF + (Grade × 2% × Salary)`
- Both are private static methods in `EmployeeDetailsController`

### Retirement Color Coding (Employee/Index.cshtml)
Age-based badges in the employee grid:
| Age | Color | Label |
|-----|-------|-------|
| ≥ 60 | 🔴 Red | "Retirement Age" (with pulse animation) |
| 55–59 | 🟣 Purple | "Near Retirement" |
| 50–54 | 🔵 Blue | "Senior Tenure" |
| < 50 | 🟢 Green | Active (no label) |

> ⚠️ These thresholds are hardcoded in both `Views/Employee/Index.cshtml` and `Views/Home/Dashboard.cshtml` — update both if policy changes.

### Analytics Dashboard (Home/Dashboard)
- KPI cards: Total Employees, Average Salary, Average Age, Monthly Salary Bill
- Retirement alert banner (when 55+ employees exist)
- Age distribution horizontal bar chart
- Grade breakdown horizontal bar chart
- Top 5 Earners leaderboard
- Top 5 Most Experienced leaderboard

## UI Conventions

- **No shared CSS classes** — each `.cshtml` has its own `<style>` block
- **Design language**: Dark gradient headers, white card bodies, rounded corners (14–16px)
- **Icons**: HTML entities only (no FontAwesome, no icon fonts)
- **Forms**: Card-based with gradient header, icon-prefixed inputs, inline validation messages
- **Tables**: Card-wrapped, hover effect, uppercase column headers, badge-styled grades

## Build & Run

```bash
# Build
dotnet build

# Run (port 5114)
dotnet run

# Publish
dotnet publish -c Release -o ./publish

# Run published app
dotnet ./publish/EAEmployee.Net8.dll

# Database migrations
dotnet ef database update
dotnet ef migrations add <MigrationName>
```

## Database

- **Engine**: SQLite, file at `app.db` (copied to output on build)
- **ORM**: EF Core 8.0.6
- **Context**: `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>`
- **Tables**: `Employees` + all ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.)
- **Migrations**: `Data/Migrations/`
