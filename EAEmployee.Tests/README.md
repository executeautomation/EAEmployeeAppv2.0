# EAEmployee.Tests

End-to-end **Playwright UI tests** + fast **NUnit unit tests** for the
`EAEmployeeAppv2.0` ASP.NET Core 8 MVC app.

The test project assumes the application is **already running** — it never
spins up an in-process `WebApplicationFactory`. Two terminal sessions is the
intended workflow.

---

## Quick start

```bash
# Terminal 1 — start the app in the Test environment
# (disables bot detection, isolates the test DB to app.test.db)
cd /Users/karthikkk/tryout/GitHub/EAEmployeeAppv2.0
dotnet run --project EAEmployee.Net8.csproj --environment Test

# Terminal 2 — run the full suite
dotnet test EAEmployee.Tests/EAEmployee.Tests.csproj
```

> **Why `--environment Test`?** The default `Development` env enables
> `BotDetection:Enabled=true` and the full reCAPTCHA pipeline. That makes
> Playwright login flaky. The `Test` environment (see
> `appsettings.Test.json` at the project root) disables bot detection
> entirely and points the DB at `app.test.db`.

The seeded admin user (`admin / password`) is created by `SeedData` on
startup.

---

## Project layout

```
EAEmployee.Tests/
├── EAEmployee.Tests.csproj        # NUnit 4 + Playwright 1.59 + FluentAssertions
├── Properties/AssemblyInfo.cs     # Parallelism = 2 fixtures
├── Fixtures/
│   ├── AppFixture.cs              # BaseUrl, admin creds, UniqueSuffix()
│   ├── PlaywrightTestBase.cs      # Creates Chromium + fresh context per test
│   └── AuthenticatedPageBase.cs   # LoginAsAdmin / RegisterFreshUser helpers
├── Pages/                         # Page Object Model — one class per screen
│   ├── HomePage.cs
│   ├── LoginPage.cs
│   ├── RegisterPage.cs
│   ├── EmployeeListPage.cs
│   ├── EmployeeCreatePage.cs
│   ├── EmployeeEditPage.cs
│   ├── EmployeeDeletePage.cs
│   └── EmployeeDetailsPage.cs
└── Tests/
    ├── Unit/                      # Pure in-process tests (no HTTP, no DB)
    │   ├── PfCalculationTests.cs       # PF/employer math via reflection
    │   ├── EmployeeValidationTests.cs  # DataAnnotations on Employee model
    │   ├── AccountViewModelTests.cs    # Login/Register/Forgot VM rules
    │   └── BotDetectionServiceTests.cs # All 5 bot signals + rate limit
    └── Ui/                        # Playwright-driven browser tests
        ├── HomePageTests.cs
        ├── AuthenticationTests.cs
        ├── EmployeeListTests.cs
        ├── EmployeeCrudTests.cs
        ├── AccessControlTests.cs
        ├── EmployeeDetailsTests.cs
        └── FormValidationTests.cs
```

---

## Running subsets

```bash
# Unit tests only — fast, no browser, no network
dotnet test --filter "Category=Unit"

# UI tests only — needs the app running
dotnet test --filter "Category=UI"

# A single test class
dotnet test --filter "FullyQualifiedName~EmployeeCrudTests"

# A single test method
dotnet test --filter "Name=PF_Page_Displays_The_Calculated_Contribution_For_Known_Inputs"
```

---

## Configuration

| Setting | Default | Override |
|---|---|---|
| Base URL | `http://localhost:5114` | `EATEST_BASEURL` env var (e.g. `https://localhost:7002`) |
| Admin user | `admin / password` | `SeedData.InitializeAsync` |
| Default user password | `Test123!` | `AppFixture.DefaultUserPassword` |
| Test DB | `app.test.db` | `appsettings.Test.json` → `ConnectionStrings:DefaultConnection` |

The base URL is read once per test process by `AppFixture.BaseUrl`.
HTTPS is auto-handled by Playwright's `IgnoreHTTPSErrors = true`.

---

## Test design

- **Page Object Model** keeps locators and flows next to the screen they
  describe. Tests should only call high-level methods like
  `loginPage.LoginAsync(...)` or `listPage.SearchByNameAsync(...)`.
- **Fresh `IBrowserContext` per test** — sign in inside the test, not once
  per class. This keeps state isolated and parallel-safe.
- **Unique test data** — `AppFixture.UniqueSuffix()` returns
  `yyyyMMddHHmmssfff`; every test that creates an employee or user
  appends this to names and emails to avoid collisions.
- **Mutating tests use `[Parallelizable(ParallelScope.None)]`** — create,
  edit, delete, and registration tests can collide on shared DB state, so
  they serialize against each other. Read-only tests still parallelize.
- **2 fixtures in parallel** — set in `Properties/AssemblyInfo.cs` via
  `[assembly: LevelOfParallelism(2)]` so a single Playwright run doesn't
  fight itself for the local browser.

---

## One-time setup: Playwright browsers

The first time you run the UI tests, you need the headless Chromium
binary. The test project bundles `Microsoft.Playwright` but the
browser must be downloaded separately:

```bash
# 1. Build the test project once so playwright.ps1 is generated
dotnet build EAEmployee.Tests/EAEmployee.Tests.csproj

# 2. Download the version-pinned headless Chromium into the Playwright cache
node EAEmployee.Tests/bin/Debug/net10.0/.playwright/package/cli.js install chromium-headless-shell
```

The browser is cached at
`~/Library/Caches/ms-playwright/chromium_headless_shell-<version>/`
and reused across runs.

---

## What's covered

### Unit tests (51 tests, ~20 ms total)

- **`PfCalculationTests`** — the private static
  `EmployeeDetailsController.CalculatePFContribution(salary, months)`
  and `CalculateEmployerContribution(salary, months, grade)` are
  exercised via reflection. Locks in the 12% / 18%+2% rules plus
  zero/edge cases.
- **`EmployeeValidationTests`** — every `[Required]`, `[EmailAddress]`,
  and `[Range]` attribute on `Employee` is asserted through
  `Validator.TryValidateObject`. Also guards the `[Display(Name="…")]`
  labels so the Razor labels don't accidentally regress.
- **`AccountViewModelTests`** — `LoginViewModel`, `RegisterViewModel`,
  and the honeypot/captcha fields. `[Compare]` password confirmation is
  verified.
- **`BotDetectionServiceTests`** — every branch of `Analyze`:
  disabled toggle, known-bot UA strings (curl, GPTBot, …), honeypot
  filled, missing/invalid JS token, too-fast timing (≥800 ms
  threshold), 10-attempt rate limit, `X-Forwarded-For` precedence.
  Uses a real `MemoryCache` and `DefaultHttpContext`.

### UI tests (39 tests, ~6 min for the full set)

- **HomePage** — public marketing page renders, employee card CTA,
  login/register links for anonymous users, dashboard empty state or
  KPI cards, About and Contact pages.
- **Authentication** — admin login, wrong-password error, forgot-password
  link, honeypot is off-screen, registration flow, duplicate
  username, logout returns to anonymous nav, AccessDenied page.
- **EmployeeList** — anonymous user can browse, admin sees Create
  button, name/email search, grade filter, pagination, clear-filters.
- **EmployeeCrud** — admin can create/edit/delete; age range
  validation message; duplicate email triggers the modal.
- **AccessControl** — anonymous redirected to login on /Manage and
  /EmployeeDetails; non-admin cannot reach /Employee/Create; non-admin
  can read /EmployeeDetails.
- **EmployeeDetails** — PF page shows `12% × salary × months`, bonus
  page shows `18% × salary × months + 2% × grade × salary`, missing
  employee returns 404.
- **FormValidation** — required-field errors on Create, Register, and
  Login; invalid email rejected on Create.

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| `Executable doesn't exist at …/chromium_headless_shell-1217/…` | Run the one-time install command above. |
| All UI tests time out on first locator | The app is not running, or `EATEST_BASEURL` is wrong. Check `lsof -i :5114`. |
| Login fails with bot detection | You forgot `--environment Test`. Restart with it. |
| "Address already in use" on `dotnet run` | A previous app instance is still running: `pkill -f EAEmployee.Net8`. |
| Tests collide on duplicate email | `AppFixture.UniqueSuffix()` should be appended; if you wrote a new test, copy the pattern from `EmployeeCrudTests`. |
| `dotnet test` builds but skips tests | The test app might be running under a different `bin` directory; ensure you are inside the repo root. |

---

## Adding a new test

1. **UI test** — find the matching page object in `Pages/`. If the
   screen doesn't exist, add one following the pattern (constructor
   takes `IPage`, expose `ILocator` properties, expose high-level
   methods that return `Task`). Then add a `[Test]` method to the
   matching fixture in `Tests/Ui/`. Use `AppFixture.UniqueSuffix()` for
   any new data. Mark mutating tests `[Parallelizable(ParallelScope.None)]`.
2. **Unit test** — if the production code exposes a private method
   worth testing, use `BindingFlags.NonPublic | Static` and the
   `MethodInfo.Invoke` pattern from `PfCalculationTests`. For public
   services, construct the service directly with the smallest real
   dependencies (real `MemoryCache`, real `DefaultHttpContext`,
   `NullLogger`, `ConfigurationBuilder` with `AddInMemoryCollection`).
3. Run `dotnet test --filter "FullyQualifiedName~YourNewTests"` to
   verify locally before committing.
