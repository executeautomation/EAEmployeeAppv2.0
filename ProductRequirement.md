# Product Requirement Document (PRD)
## EAEmployee — ASP.NET Core 8 Employee Management System

**Version:** 2.0  
**Owner:** Karthik KK — ExecuteAutomation  
**Last Updated:** May 2026  
**Status:** Active

---

## 1. Overview

EAEmployee is a web-based Human Resources management application designed to manage employee records and calculate Provident Fund (PF) and employer contributions. It is a full migration and redesign of the original ExecuteAutoEmployee application (built on .NET Framework) to **ASP.NET Core 8 MVC**, replacing legacy dependencies with modern equivalents.

The application serves as both a functional HR tool and a reference/educational project for the ExecuteAutomation community, demonstrating real-world ASP.NET Core 8 patterns including role-based access control, Entity Framework Core, and Identity.

---

## 2. Goals & Objectives

| Goal | Description |
|------|-------------|
| **Employee Management** | Provide administrators a full CRUD interface for employee records |
| **PF Calculations** | Compute employee and employer Provident Fund contributions inline without external services |
| **Access Control** | Enforce role-based permissions so only authorised users can perform sensitive operations |
| **Modern Stack** | Run on .NET 8 with SQLite, removing SQL Server and WCF dependencies |
| **Educational Reference** | Serve as a teaching tool for ASP.NET Core 8 MVC patterns at ExecuteAutomation |

---

## 3. Stakeholders

| Role | Description |
|------|-------------|
| **Administrator** | HR/system admin with full access to create, read, update, and delete employee records |
| **User** | Regular employee or read-only staff who can view employee details and PF information |
| **Anonymous Visitor** | Unauthenticated user; can view the public employee list but cannot access details or manage records |
| **Developer / Student** | ExecuteAutomation audience using this as a reference implementation |

---

## 4. User Roles & Permissions

| Feature / Action | Administrator | User | Anonymous |
|---|:---:|:---:|:---:|
| View Employee List | ✅ | ✅ | ✅ |
| Search Employees by Name | ✅ | ✅ | ✅ |
| Create Employee | ✅ | ❌ | ❌ |
| Edit Employee | ✅ | ❌ | ❌ |
| Delete Employee | ✅ | ❌ | ❌ |
| View Employee Details (PF) | ✅ | ✅ | ❌ |
| View Employer Contribution | ✅ | ✅ | ❌ |
| Register an Account | N/A | N/A | ✅ |
| Login / Logout | ✅ | ✅ | ✅ (login only) |
| Change / Set Password | ✅ | ✅ | ❌ |
| Forgot Password Flow | ✅ | ✅ | ✅ |

> New users who self-register via the UI are automatically assigned the **User** role.  
> The default **Administrator** account is seeded at startup.

---

## 5. Functional Requirements

### 5.1 Authentication & Account Management

| ID | Requirement |
|----|-------------|
| AUTH-01 | Users must be able to log in with a username and password via a custom login page |
| AUTH-02 | Users must be able to register a new account with username, email, and password (min 6 characters) |
| AUTH-03 | Registration must confirm that the password and confirmation password match |
| AUTH-04 | Newly registered users are automatically assigned the **User** role |
| AUTH-05 | Users must be able to log out securely (POST with CSRF protection) |
| AUTH-06 | A "Remember Me" option must be available on the login page |
| AUTH-07 | Unauthenticated access to restricted pages must redirect to `/Account/Login` |
| AUTH-08 | Access denied attempts must redirect to `/Account/AccessDenied` |
| AUTH-09 | A Forgot Password page must accept an email address (email delivery is a stub / future feature) |
| AUTH-10 | Authenticated users must be able to set or change their account password from the Manage section |
| AUTH-11 | Account lockout is disabled (lockoutOnFailure: false); can be enabled in future |

### 5.2 Employee Management (CRUD)

| ID | Requirement |
|----|-------------|
| EMP-01 | The Employee List page must display all employees in a tabular format, accessible to all visitors |
| EMP-02 | The Employee List must support searching/filtering by employee name (prefix match) |
| EMP-03 | Only **Administrators** can access the Create Employee form |
| EMP-04 | Creating an employee must validate all required fields: Name, Salary, Age, Duration Worked, Grade, Email |
| EMP-05 | Employee age must be validated to be between 18 and 100 |
| EMP-06 | Employee email must be a valid email address format |
| EMP-07 | Duplicate email detection: if an employee with the same email already exists, the system must return a JSON response indicating the duplicate along with the existing record details (instead of saving a duplicate) |
| EMP-08 | Only **Administrators** can edit an existing employee record |
| EMP-09 | Only **Administrators** can delete an employee record |
| EMP-10 | Delete must show a confirmation page before permanently removing the record |
| EMP-11 | All write operations must be protected with anti-forgery (CSRF) tokens |

#### Employee Data Model

| Field | Type | Constraints |
|-------|------|-------------|
| `Id` | `int` | Auto-generated primary key |
| `Name` | `string` | Required |
| `Salary` | `float` | Required (monthly salary) |
| `Age` | `int` | Required; must be 18–100 |
| `DurationWorked` | `int` | Required (months worked) |
| `Grade` | `int` | Required; 1 = Junior, 2 = Middle, 3 = Senior, 4 = C-Level |
| `Email` | `string` | Required; valid email format; unique |

### 5.3 Employee Details & PF Calculations

| ID | Requirement |
|----|-------------|
| DET-01 | An Employee Details list page must be accessible to authenticated users (Administrator or User roles only) |
| DET-02 | The system must calculate and display the **Employee PF Contribution** for a selected employee |
| DET-03 | The system must calculate and display the **Employer (Company) Contribution** for a selected employee |
| DET-04 | PF calculations must be performed inline without calling any external service |

#### PF Calculation Formulas

| Contribution | Formula |
|---|---|
| **Employee PF** | `Salary × 12% × DurationWorked` |
| **Employer PF** | `(Salary × 18% × DurationWorked) + (Grade × Salary × 2%)` |

> The employer contribution includes a grade-based bonus allowance on top of the standard PF rate.

### 5.4 Home / Navigation

| ID | Requirement |
|----|-------------|
| HOME-01 | A public home page must be available with a hero section, EA branding, and navigation links |
| HOME-02 | Navigation must reflect authentication state (show login/register when anonymous; show logout/manage when authenticated) |
| HOME-03 | An About page must be available from the home navigation |

---

## 6. Non-Functional Requirements

### 6.1 Security

| ID | Requirement |
|----|-------------|
| SEC-01 | All form submissions must include CSRF (anti-forgery) token validation |
| SEC-02 | Role-based authorization must be enforced via `[Authorize(Roles = "...")]` attributes |
| SEC-03 | Passwords must be at minimum 6 characters; additional complexity rules are configurable |
| SEC-04 | Authentication cookies must redirect to `/Account/Login` on unauthenticated access |
| SEC-05 | HTTPS must be enforced in production environments via `UseHttpsRedirection` and HSTS |
| SEC-06 | The default admin password (`password`) must be changed before deploying to production |

### 6.2 Performance

| ID | Requirement |
|----|-------------|
| PERF-01 | All database queries must be asynchronous (using `async/await` with Entity Framework Core) |
| PERF-02 | PF calculations are in-memory (O(1)) and require no external round-trips |

### 6.3 Reliability & Maintainability

| ID | Requirement |
|----|-------------|
| REL-01 | The application must auto-apply pending Entity Framework migrations on startup |
| REL-02 | Seed data (roles + default admin) must be applied idempotently on every startup |
| REL-03 | Database errors during startup must be logged and re-thrown to prevent silent failures |
| REL-04 | The production error handler must display a generic error page without exposing stack traces |

### 6.4 UI / UX

| ID | Requirement |
|----|-------------|
| UI-01 | The UI must be built with Bootstrap 5 and custom CSS, responsive across device widths |
| UI-02 | Employee grades must be displayed as labelled pills/badges in the list view |
| UI-03 | The application must use custom login, register, and forgot-password pages (no scaffolded Identity UI) |

---

## 7. Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | ASP.NET Core MVC | .NET 8 |
| ORM | Entity Framework Core | 8.0.6 |
| Database | SQLite (`app.db`) | — |
| Auth | ASP.NET Core Identity with Roles | 8.0.6 |
| Frontend | Bootstrap 5 + custom CSS | — |
| Runtime | .NET 8 SDK | 8.x |

### NuGet Dependencies

| Package | Version |
|---------|---------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.6 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.6 |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.6 |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.6 |

---

## 8. System Architecture

```
Browser (Bootstrap 5 UI)
        │
        ▼
ASP.NET Core 8 MVC
├── HomeController          → Public landing and about pages
├── AccountController       → Login, Register, Logout, ForgotPassword
├── ManageController        → SetPassword, account profile
├── EmployeeController      → CRUD (Administrator-only writes)
└── EmployeeDetailsController → PF & employer contribution views (authenticated)
        │
        ▼
ApplicationDbContext (EF Core 8)
        │
        ▼
SQLite (app.db)
├── AspNetUsers             → Identity user store
├── AspNetRoles             → "Administrator" | "User"
├── AspNetUserRoles         → User ↔ Role mapping
└── Employees               → Employee records
```

---

## 9. Project Structure

```
EAEmployee.Net8/
├── Controllers/
│   ├── AccountController.cs        # Auth flows
│   ├── EmployeeController.cs       # Employee CRUD
│   ├── EmployeeDetailsController.cs # PF calculations
│   ├── HomeController.cs           # Public pages
│   └── ManageController.cs         # Account management
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext
│   ├── Migrations/                 # EF migration history
│   └── SeedData.cs                 # Roles + default admin seeder
├── Models/
│   ├── ApplicationUser.cs          # Custom IdentityUser
│   ├── Employee.cs                 # Employee entity + validation
│   ├── AccountViewModels.cs        # Login, Register, ForgotPassword VMs
│   └── ManageViewModels.cs         # ChangePassword, SetPassword VMs
├── Views/
│   ├── Account/                    # Login, Register, ForgotPassword, AccessDenied
│   ├── Employee/                   # Index, Create, Edit, Delete
│   ├── EmployeeDetails/            # Index, EmployeePF, EmployeeBonus
│   ├── Home/                       # Index, About
│   ├── Manage/                     # Index, SetPassword
│   └── Shared/                     # _Layout, _LoginPartial, Error
├── wwwroot/
│   ├── css/site.css                # Custom styles
│   └── images/                     # Branding logos
├── EAEmployee.Tests/               # Test project
├── appsettings.json
├── appsettings.Development.json
└── Program.cs                      # DI setup, Identity config, middleware pipeline
```

---

## 10. Default Credentials

| Username | Password | Role |
|----------|----------|------|
| `admin` | `password` | Administrator |

> ⚠️ **Security Notice:** Change the default admin password before any production deployment.

---

## 11. Out of Scope (Intentionally Excluded)

The following features from the original .NET Framework version were deliberately excluded from v2.0:

| Excluded Feature | Reason |
|---|---|
| `PFServiceClient` (WCF service) | Replaced by inline calculation; WCF not supported in .NET Core |
| Benefits management module | Deferred to a future release |
| Role management UI | Admin role assignment handled via seed data and code |
| SQL Server database | Replaced by SQLite for portability and simplicity |
| Email delivery for Forgot Password | Stubbed; redirects to a confirmation page without sending email |

---

## 12. Future Enhancements

| Priority | Feature |
|----------|---------|
| High | Real email delivery for Forgot Password (SMTP / SendGrid integration) |
| High | Change Password flow (currently only Set Password is implemented) |
| Medium | Admin UI for user and role management |
| Medium | Benefits and bonus management module |
| Medium | Pagination for the employee list |
| Low | Export employees to CSV / Excel |
| Low | Audit log of create/edit/delete operations |
| Low | Docker/container deployment configuration |

---

## 13. Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the Application

```bash
cd EAEmployee.Net8
dotnet run
```

The application starts at **http://localhost:5114**.  
The SQLite database is automatically created and seeded on first run.

### Database Migrations

```bash
# Install EF tools (once)
dotnet tool install --global dotnet-ef

# Apply existing migrations
dotnet ef database update

# Add a new migration
dotnet ef migrations add <MigrationName>
```

---

## 14. References

- [ExecuteAutomation](https://executeautomation.com) — parent organisation
- [ASP.NET Core 8 Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Bootstrap 5](https://getbootstrap.com/docs/5.0/)
