# Product Requirements Document: EAEmployee

| Item | Value |
| --- | --- |
| Product | EAEmployee — Employee Management Application |
| Repository | EAEmployeeAppv2.0 |
| Document version | 1.0 |
| Review date | 2 November 2026 |
| Status | Source-derived baseline and proposed completion requirements; pending product-owner review |
| Review scope | Current working tree, including existing uncommitted changes |

## 1. Purpose and evidence

This document describes the application’s purpose, users, workflows, functional requirements, business rules, and known implementation gaps. It provides a baseline for product planning and acceptance testing.

The review covered all controllers, Razor views, models, database context and migrations, startup configuration, API authorization, seed data, frontend scripts, and existing project documentation. Source code takes precedence where `README.md` or `KNOWLEDGEBASE.md` disagrees with the implementation.

**Verification limitation:** `dotnet build --no-restore --nologo` failed with 18 compiler errors in onboarding-related `DisplayFormat` attributes in `Models/Employee.cs` and `Models/AccountViewModels.cs`. The current source could not be launched for browser verification. Statements about existing features describe source behavior, not successful end-to-end execution. Previously published binaries were not used to validate the current source.

Throughout this document:

- **Present** means the implementation exists in source, subject to the build limitation above.
- **Partial** means a workflow exists but has identified gaps or inconsistencies.
- **Proposed** means a requirement or target recommended here, not an implemented or approved capability.
- Priorities are proposed: **P0** blocks basic operability or release integrity; **P1** is required for the intended baseline; **P2** is a subsequent enhancement.

## 2. Product overview

EAEmployee is a browser-based employee management application built with ASP.NET Core 8 MVC. It combines an employee directory, administrator-controlled record maintenance, contribution estimates, workforce analytics, data exports, and a read-only integration API.

The product addresses the need to maintain employee information in one place and make salary, tenure, grade, age distribution, and contribution information easy to review. Its ExecuteAutomation branding and educational links also support use as a demonstration application for software testing and automation training.

The intended business outcomes inferred from the application are:

1. Reduce the effort needed to create, find, update, and remove employee records.
2. Give authorized users a consistent view of employee and company contributions.
3. Make workforce composition and retirement-planning indicators visible at a glance.
4. Support reporting through filtered downloads and programmatic read access.
5. Provide repeatable role-based workflows suitable for demonstrations and testing.

Contribution calculations and retirement thresholds are application business rules. This document does not establish them as statutory payroll rules.

## 3. Users and access

### 3.1 Personas

| Persona | Main needs |
| --- | --- |
| Administrator / HR operator | Maintain employee records, avoid duplicates, select onboarding tasks, export data, and inspect workforce metrics. |
| Registered User | Browse employees, view contribution details, and inspect their own account information. |
| Anonymous visitor | Explore the public application, view currently public employee information, and register or sign in. |
| API consumer | Retrieve employee records through a configured API key for reporting or automated checks. |
| Training participant / tester | Exercise observable CRUD, validation, authentication, filtering, analytics, and API workflows. |

### 3.2 Current authorization matrix

This matrix records server-side authorization as implemented. Public access to compensation and contact information requires an explicit product decision before use with real employee data.

| Capability | Anonymous | User | Administrator | API key alone |
| --- | --- | --- | --- | --- |
| Home, About, Contact | Yes | Yes | Yes | Public pages |
| Employee list and filters | Yes | Yes | Yes | Public page |
| Workforce dashboard | Yes | Yes | Yes | Public page |
| CSV and Excel exports | Yes | Yes | Yes | Public endpoints |
| Create, edit, delete employees | No | No | Yes | No |
| Contribution directory and calculations | No | Yes | Yes | No |
| Manage own account / set initial local password | No | Yes | Yes | No |
| Employee REST endpoints | Valid key required | Valid key required | Valid key required | Yes |
| Swagger UI and API schema | Yes | Yes | Yes | Public documentation |

Registration assigns the `User` role and signs the new account in. Administrators are seeded at startup; there is no role-management UI. A user account is not linked to an employee record, so contribution access is to the directory rather than only the signed-in user’s record. An API key does not grant an Identity role or a browser session.

## 4. Scope

### 4.1 Baseline capabilities

- Public landing, About, and Contact pages with ExecuteAutomation information and external learning links.
- Registration, login, persistent sign-in option, logout, access-denied handling, and account summary.
- Employee directory with combined name, email, and grade filters; five records per page.
- Administrator-only employee creation, editing, and confirmed deletion.
- Duplicate-email handling during employee creation.
- Onboarding task selections for welcome email, PF setup, and IT equipment; currently incomplete and simulated.
- Authenticated employee PF and company-contribution views.
- Workforce KPIs, age and grade distributions, retirement alerts, and ranked employee lists.
- Filtered CSV and Excel downloads.
- API-key-protected employee list and single-record endpoints, with Swagger documentation.

### 4.2 Outside the current baseline

There is no implemented payroll execution, tax processing, attendance, leave, recruitment, performance management, department hierarchy, benefits administration, employee import, API writes, role administration, or employee self-service record editing. There is no real email delivery, external PF provisioning, asset inventory integration, audit history, soft-delete recovery, multi-tenancy, or multi-currency conversion. External course enrollment happens on third-party websites.

Password recovery and onboarding are visible in the interface but must not be represented as completed external actions in their current form.

## 5. Primary user journeys

### 5.1 Find an employee and export matching records

1. Open **Employees** from the main navigation.
2. Enter a name prefix, email fragment, and/or grade, then select **Search**.
3. Review matching records and navigate pages while retaining the filters.
4. Select **Clear** to remove all filters.
5. Choose CSV or Excel from **Export** to download all matching records, including those outside the current page.

An empty result displays “No employees found.” Only administrators see the create, edit, and delete controls.

### 5.2 Create an employee

1. Sign in as an Administrator and select **New Employee**.
2. Enter name, age, monthly salary, months worked, grade, and email.
3. Select onboarding tasks, if required.
4. Submit the form.
5. If the email already exists, display the duplicate employee and offer **Edit Existing** or **Close**.
6. Otherwise, save the employee and return to the directory.

The current journey is blocked by build and Razor model issues described in Section 12. Onboarding selections currently set flags and write console messages; they do not deliver or provision anything.

### 5.3 Maintain an employee

An Administrator opens **Edit**, changes the six employee information fields, saves, and returns to the list. Invalid input should remain visible with validation messages. **Delete** first opens a confirmation page showing the employee’s information; confirming permanently removes the record. Canceling either form returns to the directory without the requested mutation.

### 5.4 Review contributions

A User or Administrator opens **Details**, selects **PF Contribution** or **Company Contribution**, and reviews the employee’s name, salary, months worked, grade, formula, and calculated amount. Unknown employee IDs return 404. Anonymous visitors are directed to login. The per-row **Details** link currently opens the whole contribution directory because its supplied employee ID is not used by that action.

### 5.5 Review workforce health

Open **Dashboard** to see total employees, average salary, average age, monthly salary bill, age and grade distributions, retirement alerts, top earners, and longest tenure. **Refresh** reloads the page and recalculates the metrics. With no employee records, show an empty state and an administrator-only add-employee action.

### 5.6 Create and use an account

Register with username, email, password, and matching confirmation. Successful registration grants the User role and signs the user in. Existing users sign in with username and password and may choose **Keep me signed in**. The account page shows username and email; logout ends the session. A passwordless account may set an initial local password. Existing-password change and working password recovery are not implemented.

### 5.7 Retrieve records through the API

Configure an API key outside the source code, send it in the `X-Api-Key` header, and request the list or an employee by ID. The list supports the same three filters as the web directory, with independent pagination settings. Missing or invalid keys return 401.

## 6. Functional requirements and acceptance criteria

Requirements describe the intended baseline. The status column identifies how closely current source supports that baseline; a requirement is not evidence that its acceptance checks have passed.

| ID | Priority | Requirement and acceptance criteria | Source status |
| --- | --- | --- | --- |
| FR-01 | P1 | **Public navigation:** Home, Employees, Dashboard, and About are reachable from the shared navigation. Contact is reachable at `/Home/Contact`. Signed-out users see registration/login; signed-in users see account/logout. The Details navigation item appears for User and Administrator roles. | Present |
| FR-02 | P1 | **Registration:** Accept a username of 6–100 characters, valid email, password of 6–100 characters, and matching confirmation. Display Identity validation errors without creating an invalid account. Success creates a User, signs them in, and returns Home. | Present |
| FR-03 | P1 | **Login and logout:** Require username/password, show a generic invalid-login message on failure, honor the remember-me choice, and return successful logins only to a local return URL or Home. Logout uses an anti-forgery-protected POST. | Present |
| FR-04 | P1 | **Authorization:** Protect employee mutations on both GET and POST with the Administrator role. Protect contribution pages with User or Administrator roles and account pages with authentication. Direct URL requests must enforce the same permissions as UI actions. | Present; public read policy pending |
| FR-05 | P1 | **Employee directory:** Show name, age, salary, months worked, grade label, email, and actions. Show the filtered total and a useful empty state. Retirement badges follow Section 8. | Present |
| FR-06 | P1 | **Combined filtering:** Name uses prefix matching, email uses substring matching, and grade uses exact matching. Multiple filters combine with AND. Retain filters in page links and exports; clearing resets all filters and starts from the first page. | Present; case/whitespace policy unspecified |
| FR-07 | P1 | **Web pagination:** Display at most five matching records per page. Previous/next and numbered links reflect the current page. Proposed completion: apply a stable order and handle zero, negative, and out-of-range page requests consistently without server errors. | Partial; no explicit order or page normalization |
| FR-08 | P0 | **Employee creation:** An Administrator can save a valid employee and return to the directory. Enforce the field rules in Section 7 on the server and client. Invalid submissions retain entered values, display errors, and save nothing. | Partial; build, view-model, validation, and migration gaps |
| FR-09 | P1 | **Duplicate prevention:** Creating an employee with an existing email must create no additional record. Display the existing employee and an edit link. Proposed completion: normalize email consistently, enforce uniqueness during edit and concurrent submissions, and provide a database constraint. | Partial; exact equality check on Create only |
| FR-10 | P1 | **Onboarding selections:** Capture welcome-email, PF-setup, and IT-equipment selections with employee creation. Persist selected values. Clearly identify simulated actions. Proposed completion: ordinary employee edits preserve these flags unless explicitly changed. | Partial; no real integrations or complete persistence support |
| FR-11 | P1 | **Employee editing:** Load the selected employee, reject a route/model ID mismatch, save valid changes, and return to the directory. Missing records must produce a controlled not-found response. Apply the same validation and uniqueness rules as Create. | Partial; GET not-found handling exists; edit integrity gaps remain |
| FR-12 | P1 | **Confirmed deletion:** GET displays employee information without deleting it. A valid Administrator POST deletes the record and redirects to the list. Cancel makes no change. A missing confirmation-page record returns 404; confirming an already-missing record safely returns to the list. | Present; permanent deletion |
| FR-13 | P1 | **Contribution directory:** Users and Administrators can browse employees with name, salary, grade, and two calculation links. The controller accepts name-prefix filtering; a visible filter control and employee-specific navigation are proposed improvements. | Partial; all-record directory is present |
| FR-14 | P1 | **Contribution calculations:** Employee PF uses salary × 12% × months worked, rounded to two decimals. Company contribution must use one approved formula consistently in calculation, displayed explanation, onboarding, and documentation. Missing IDs return 404. | Partial; company rates disagree |
| FR-15 | P1 | **Workforce analytics:** Calculate metrics from all employees, independent of directory filters. Show the four KPIs, five age bands, four grade counts, retirement alert, and up to five employees in each leaderboard. Recalculate on page load/refresh; show an empty state for zero employees. | Present |
| FR-16 | P1 | **Exports:** CSV and XLSX include every filtered record and seven core employee fields, without limiting output to the current page. Return a timestamped filename and appropriate content type. CSV escapes embedded quotes in text; XLSX has a named sheet and styled headers. Proposed completion: protect spreadsheet consumers against formula injection and define culture-independent numeric output. | Partial; basic exports present |
| FR-17 | P1 | **Read-only API:** Require the configured API key for both endpoints. Return the documented list envelope and employee schema, filter correctly, clamp page to at least 1 and page size to 1–50, and return 404 for an unknown ID. Expose discoverable Swagger documentation. | Present; deployed key configuration unverified |
| FR-18 | P1 | **Account summary:** Show only the signed-in account’s username and email. For an account without a local password, support matching new-password entry, display Identity errors, refresh sign-in on success, and show confirmation. | Present; no existing-password change |
| FR-19 | P2 | **Password recovery:** Proposed completion: send a time-limited recovery link through a configured delivery service and allow a valid token to reset the password. Handle invalid/expired tokens and give a generic request confirmation. Until implemented, clearly describe the current page as a demonstration. | Partial; request/confirmation pages only |
| FR-20 | P1 | **Error and feedback states:** Invalid input is actionable; failed requests do not appear successful or lose input. Missing records return 404, access-denied requests show the appropriate page, and unexpected production errors expose a request ID without a stack trace. | Partial; create AJAX lacks reliable validation/error handling |

## 7. Data requirements

### 7.1 Employee

| Field | Current type / representation | Intended rule and implementation notes |
| --- | --- | --- |
| `Id` | Integer, SQLite-generated primary key | System assigned; identifies records across UI and API. |
| `Name` | String | Required, nonblank name. Entity has `[Required]`; Create view model has no equivalent annotations. Maximum length is unspecified. |
| `Email` | String | Required, valid address. Intended unique employee identifier for duplicate detection. No unique database index or edit-time check exists. Normalization policy is pending. |
| `Age` | Integer | 18–100 inclusive. Entity range and browser constraints exist; Create model lacks the range rule. The age migration gives old records a default of 0, requiring data cleanup. |
| `Salary` | Float | Monthly salary. Forms intend a value of at least zero with 0.01 increments; server-side range checks are absent. Proposed: use decimal storage and a defined rounding policy for monetary correctness. |
| `DurationWorked` | Integer | Months worked, not years. Forms intend zero or more months; server-side nonnegative validation is absent. |
| `Grade` | Integer | Only 1 = Junior, 2 = Middle, 3 = Senior, 4 = C-Level. Dropdowns restrict ordinary selection; server-side range enforcement is absent. |
| `IsWelcomeEmailSent` | Boolean | Set from `SendWelcomeEmail` on Create. Represents a simulated choice, not verified delivery. |
| `IsPFAccountSetupCompleted` | Boolean | Set from `SetupPFAccount` on Create. No external account is created. |
| `IsITEquipmentAssigned` | Boolean | Set from `AssignITEquipment` on Create. No asset record or external assignment is created. |

The last three properties exist in the entity but are absent from committed migrations and the model snapshot. Salary, age, and tenure are current values; there are no effective dates or historical records. Employee records have no department, employment status, user-account relationship, created/updated timestamps, or audit trail.

### 7.2 Identity and configuration

Application users extend ASP.NET Core Identity’s user model without additional profile fields. Identity stores users, role membership, credentials, tokens, and related authentication data separately from employees.

Startup applies EF Core migrations and seeds the `Administrator` and `User` roles plus a default demonstration administrator if absent. Confirmed-account sign-in is disabled. Password complexity requirements are disabled except for a minimum length of six; registration and set-password models also impose a maximum length of 100. Failed logins do not increment lockout counters through the login action because `lockoutOnFailure` is false.

The API reads its shared key from `ApiSettings:ApiKey`. Checked-in application settings do not supply that value; environment-specific configuration must provide it. API keys and credentials must not be copied into this document or API examples.

## 8. Business rules and calculations

### 8.1 Filtering and pagination

- Web directory and exports share name-prefix, email-substring, and exact-grade filters.
- The contribution-directory controller accepts only a name-prefix filter, with no corresponding search form in its current view.
- The API orders results by employee ID ascending. Web pagination and exports have no explicit ordering.
- Matching case sensitivity follows the current database/provider behavior; no product-wide normalization is specified.
- Web page size is five. API defaults to ten records and limits page size to fifty.

### 8.2 Contribution rules

Let `S` be monthly salary, `M` months worked, and `G` grade number.

| Calculation | Current source behavior |
| --- | --- |
| Employee PF detail | `round(S × 0.12 × M, 2)` |
| Company contribution detail | `round((S × 0.18 × M) + (G × S × 0.02), 2)` |
| Onboarding PF console calculation | Employee: `S × 0.12 × M`; employer: `(S × 0.12 × M) + (G × S × 0.02)` |
| Company page explanation | States 15% PF, conflicting with the controller’s 18%. |
| Existing README / knowledge base | Describe a 12% employer base, conflicting with the controller’s 18%. |

For an illustrative salary of 5,000, tenure of 24 months, and grade 2, employee PF is 14,400.00 and the current company-detail controller produces 21,800.00. The onboarding employer calculation produces 14,600.00. The controller adds the grade allowance once, rather than once per month.

**Product decision required:** approve the employer rate, whether the grade allowance is one-time or monthly, rounding behavior, and currency. Current employee forms/list/dashboard use `$`, while contribution pages use `₹`; there is no conversion or stored currency field. Until the decision is made, retain these values as documented discrepancies rather than claiming a single approved formula.

### 8.3 Age and retirement indicators

| Age | Employee directory indicator |
| --- | --- |
| Under 50 | Green age badge; no extra status label. |
| 50–54 | Blue badge and “Senior Tenure” label. This is age-based despite its name. |
| 55–59 | Purple badge and “Near Retirement” label. |
| 60+ | Red badge and “Retirement Age” label. |

The dashboard groups ages into under 30, 30–39, 40–49, 50–59, and 60+. It displays a retirement alert when any employee is 55 or older, distinguishing 55–59 from 60+. These thresholds are hardcoded and do not change employment status or remove employees.

### 8.4 Analytics

- Total employees: number of employee records.
- Average salary: arithmetic mean of monthly salary; displayed without decimal places.
- Average age: arithmetic mean of age; displayed to one decimal place.
- Monthly salary bill: sum of monthly salaries; displayed without decimal places.
- Grade breakdown: count of each grade from 1 through 4.
- Top earners: up to five employees ordered by salary descending.
- Most experienced: up to five employees ordered by months worked descending.
- Leaderboard tie-breaking is unspecified. Metrics reflect current stored values and refresh on request; no live push updates or historical trends exist.

## 9. Screens and API contract

### 9.1 Screen inventory

| Area | Routes / screens | Key behavior |
| --- | --- | --- |
| Public content | `/`, `/Home/Index`, `/Home/About`, `/Home/Contact` | Product introduction, learning links, organization information, contact email. |
| Employee directory | `/Employee`, `/Employee/Index` | Filters, five-row pages, age/grade badges, role-aware actions, exports. |
| Employee maintenance | `/Employee/Create`, `/Employee/Edit/{id}`, `/Employee/Delete/{id}` | Administrator-only forms and deletion confirmation. |
| Contributions | `/EmployeeDetails`, `/EmployeeDetails/EmployeePF/{id}`, `/EmployeeDetails/EmployeeBonus/{id}` | Role-protected directory and calculation cards. `EmployeeBonus` is the company-contribution screen. |
| Analytics | `/Home/Dashboard` | KPIs, distributions, alerts, leaderboards, manual refresh. |
| Authentication | `/Account/Login`, `/Account/Register`, `/Account/AccessDenied` | Login, registration, permission feedback; shared lockout view also exists. |
| Recovery | `/Account/ForgotPassword`, `/Account/ForgotPasswordConfirmation` | Demonstration request and confirmation; no reset completion route. |
| Account | `/Manage`, `/Manage/Index`, `/Manage/SetPassword` | Identity summary and initial local-password setup. |
| Errors | `/Home/Error` | Production exception page with request identifier. |
| API documentation | `/swagger`, `/swagger/v1/swagger.json` | Swagger UI and schema are enabled in development and production. |

### 9.2 Employee API

Both endpoints require `X-Api-Key: <configured-key>`. Identity login alone does not authorize these requests. The API is read-only.

| Endpoint | Parameters | Successful result | Error behavior |
| --- | --- | --- | --- |
| `GET /api/employees` | Optional `searchTerm`, `emailTerm`, `gradeFilter`, `page` (default 1), `pageSize` (default 10) | JSON envelope with `totalCount`, `page`, `pageSize`, `totalPages`, and `data`. | Missing/invalid key: 401. Invalid bound query types: framework validation error. |
| `GET /api/employees/{id:int}` | Integer employee ID | JSON employee object. | Unknown ID: 404 with an `error` message. Missing/invalid key: 401. |

The list normalizes `page` to at least 1 and clamps `pageSize` to 1–50. An empty result has `totalCount: 0`, `totalPages: 0`, and `data: []`. An out-of-range positive page returns an empty `data` array with the matching total unchanged.

Current serialization returns the entity directly: `id`, `name`, `salary`, `age`, `durationWorked`, `grade`, `email`, `isWelcomeEmailSent`, `isPFAccountSetupCompleted`, and `isITEquipmentAssigned`. There is no separate field-level permission model. API response fields must be reviewed before adding new sensitive properties to the entity.

### 9.3 Export contract

CSV columns are `Id,Name,Email,Age,Salary,DurationWorked,Grade`. Excel uses the same information, with `Duration Worked (months)` as the tenure header and a worksheet named `Employees`. Both filenames follow `employees_yyyyMMdd_HHmmss` with their respective extensions. Grade is exported numerically. Onboarding flags are excluded. No matches produces a header-only export.

## 10. Nonfunctional requirements

These are proposed acceptance expectations, not measured guarantees. Performance and capacity values require owner approval and a representative test environment.

| ID | Area | Proposed requirement / verification |
| --- | --- | --- |
| NFR-01 | Operability | A clean checkout restores and builds; migrations initialize a fresh database and upgrade an existing one; startup seeds roles without duplicating them. No entity/schema mismatch prevents normal requests. |
| NFR-02 | Authorization and sessions | Verify the approved matrix with direct requests, including exports and dashboard. All state-changing browser forms use anti-forgery protection. Validate local login return URLs and end sessions on logout. |
| NFR-03 | Data integrity | Enforce the same server-side employee rules on Create and Edit, database-backed email uniqueness, stable pagination, and preservation of fields absent from edit forms. Use a defined monetary precision and rounding policy. |
| NFR-04 | Credentials and sensitive data | Supply deployment credentials and API keys through environment-specific secret configuration. Replace demonstration administrator credentials before use with real data. Avoid salary/contact details in ordinary logs. Decide who may read/export employee data. |
| NFR-05 | Usability and accessibility | Proposed target: WCAG 2.2 AA. Verify keyboard navigation, labels, focus behavior, validation association, modal focus, sufficient contrast, and readable status text independent of color. This review did not certify compliance. |
| NFR-06 | Responsive layout | Verify navigation, forms, dashboard, and tables at representative phone, tablet, and desktop widths. Controls and employee information remain usable without overlap or clipped actions. |
| NFR-07 | Performance | Proposed initial test profile: 1,000 employee records and 10 concurrent readers; target p95 under 2 seconds for directory/API reads and under 3 seconds for dashboard requests, excluding external network latency. Establish export limits and timing separately. No benchmark has been run. |
| NFR-08 | Reliability and recovery | Document database backup/restore and verify recovery in a test environment. Migration or seeding failures must stop startup visibly. Mutations must not falsely report success after failure. Availability, recovery-time, and data-loss targets remain undecided. |
| NFR-09 | Observability | Log failures with request context while keeping production responses free of stack traces. Proposed: record who changed employee data and when before operational HR use. Current implementation has no employee audit trail. |
| NFR-10 | Export safety | Preserve names, emails, embedded quotes, and numeric values on download. Define locale handling and neutralize spreadsheet formulas in user-controlled text. Test CSV/XLSX in representative spreadsheet software. |

## 11. Technical and operational constraints

- Server-rendered ASP.NET Core 8 MVC and Razor; Bootstrap 5, jQuery, and unobtrusive validation on the frontend.
- EF Core 8.0.6 with SQLite, and ASP.NET Core Identity for authentication and roles.
- ClosedXML generates Excel downloads; Swashbuckle exposes the API schema/UI.
- SQLite defaults to `app.db`; startup resolves its relative path against the application content root and applies migrations automatically.
- The local HTTP launch profile uses port 5114; the HTTPS profile also uses port 7002. Deployment URLs are environment-specific.
- The application includes HTTPS redirection, production HSTS, production error handling, and IIS configuration.
- The dashboard loads all employees into memory; export filtering also materializes the whole result set. These implementations have no demonstrated large-dataset capacity.
- SQLite deployment needs a writable database location and a backup strategy. The current design has no tenant boundary or distributed deployment model.
- The application does not depend on the original external PF WCF service. Contributions are calculated locally.

## 12. Known gaps and proposed priorities

| ID | Priority | Finding and impact | Completion requirement |
| --- | --- | --- | --- |
| GAP-01 | P0 | Build fails with 18 errors from `DisplayFormat` declarations using unsupported constructor arguments, `ApplyFormattedTo`, and `DataFormats.Boolean` in both employee models. | Correct annotations; obtain a successful build before runtime acceptance testing. |
| GAP-02 | P0 | Create view declares `Employee` while referencing `SendWelcomeEmail`, `SetupPFAccount`, and `AssignITEquipment`, which belong to `EmployeeCreateViewModel`. Invalid POST also returns the view model to an entity-typed view. | Align view/action models and verify initial rendering, validation redisplay, duplicates, and successful create. |
| GAP-03 | P0 | Onboarding flags are in the entity but absent from migrations and snapshot. A database created solely from current migrations lacks these columns. | Add a migration and verify both fresh initialization and upgrade with existing data. |
| GAP-04 | P1 | Create view model lacks employee validation attributes; salary, tenure, and grade also lack entity range checks. Browser constraints can be bypassed. | Centralize and enforce the Section 7 rules on the server for both mutations. |
| GAP-05 | P1 | Employer contribution is 18% in detail logic, 15% in the view, and 12% in onboarding/documentation. Currency symbols also differ. | Obtain the contribution/currency decision and update every calculation and explanation together. |
| GAP-06 | P1 | Duplicate checks use exact email equality only on Create; no unique database constraint exists. | Define normalization; enforce uniqueness on create/edit and during concurrent requests. |
| GAP-07 | P1 | Edit marks the entire bound entity modified while its form omits onboarding flags, allowing those values to reset to false. | Update permitted fields on the existing entity and preserve unrelated values; handle records removed during edit. |
| GAP-08 | P1 | Create AJAX treats any nonduplicate success response as success, including an HTML validation view, and has no explicit failure feedback. | Define response handling for success, duplicates, validation, authorization expiry, and server/network failures. |
| GAP-09 | P1 decision | Anonymous users can read salary/age/email, view workforce analytics, and download the directory. This is current server behavior. | Decide whether public demonstration access is intentional; implement a revised matrix before using real employee information. |
| GAP-10 | P1 | Onboarding flags and console messages imply completion without actual integrations. Console output includes employee contact/salary information. | Label simulated actions accurately and avoid sensitive logs; real integrations require a separately approved scope. |
| GAP-11 | P2 | Forgot-password confirmation asks users to check email, but no message or reset token is sent. No existing-password change action exists despite README claims. | Implement the workflows if selected, or correct the interface/documentation to reflect their limits. |
| GAP-12 | P1 | Web pagination has no explicit sort or page bounds; exports use current-culture numeric formatting and lack an explicit CSV formula-injection policy. | Define deterministic pagination and safe, interoperable export behavior. |
| GAP-13 | P2 | Per-row Details links pass an ignored ID; contribution filtering exists only in the controller. | Decide whether the link opens the whole directory or an employee-specific view; align navigation and filtering controls. |
| GAP-14 | P1 | Existing migrated employee ages may be 0; float monetary storage can introduce precision differences. | Validate legacy data and select a compatible monetary representation/migration strategy. |
| GAP-15 | P1 deployment | Checked-in settings contain no API key, and startup seeds a fixed demonstration administrator. Swagger is public in all environments. | Configure deployment secrets and review administrator provisioning and documentation exposure for the chosen environment. |

## 13. Proposed release sequence and acceptance

### Phase 1: Restore a verifiable baseline

Resolve GAP-01 through GAP-03, build successfully, initialize a fresh database, and upgrade a representative existing database. Launch the current application and verify that every primary screen renders. These steps are prerequisites for claiming runtime support for the reviewed source.

### Phase 2: Complete core behavior

Resolve field validation, duplicate integrity, edit preservation, AJAX feedback, company-contribution consistency, and pagination/export correctness. Obtain decisions on public data access and currency. Complete role-based checks against the approved authorization matrix.

### Phase 3: Optional enhancements

Prioritize real password recovery, existing-password changes, employee-specific Details navigation, operational onboarding integrations, auditing, and additional reporting only after their scope is approved. No delivery dates or implementation estimates are committed by this document.

### Release acceptance checklist

- [ ] Fresh source builds and the application starts without schema errors.
- [ ] Existing database upgrade preserves employee and Identity data.
- [ ] Administrator can create, edit, and confirm-delete an employee; cancel makes no requested change.
- [ ] User and anonymous direct requests cannot perform administrator mutations.
- [ ] Invalid age, negative salary/tenure, unsupported grade, malformed email, and blank name are rejected server-side without losing form input.
- [ ] Duplicate email handling works for create, edit, normalized variants, and concurrent attempts under the agreed policy.
- [ ] Editing employee information preserves onboarding flags unless explicitly changed.
- [ ] Combined filters, retained page filters, empty states, and boundary page requests behave consistently.
- [ ] CSV/XLSX contain all matching records and correct values, with agreed spreadsheet safety behavior.
- [ ] Contribution amounts and displayed formulas match the approved rule, including zero salary/tenure and rounding cases.
- [ ] Dashboard totals and distributions reconcile with known sample records; retirement boundaries at 50, 55, and 60 are correct.
- [ ] Login, registration, logout, local return URLs, account access, and access-denied behavior pass direct and UI checks.
- [ ] API missing/invalid keys return 401; unknown IDs return 404; filters, pagination limits, empty responses, and response fields match Section 9.
- [ ] Simulated or unfinished actions are described accurately in the interface.
- [ ] Public versus restricted reads/exports match the approved policy.
- [ ] Representative responsive, accessibility, performance, and backup/restore checks are completed for the intended release environment.

These checks are proposed future acceptance work. They were not executed as part of this documentation-only change; the build failure prevented runtime validation.

## 14. Success measures

The following measures are proposed for baseline acceptance. No production telemetry or existing measurements were found during review.

| Outcome | Proposed measure |
| --- | --- |
| Reliable core workflows | All release-blocking create/read/update/delete and authentication acceptance scenarios pass. |
| Permission correctness | No unauthorized employee mutation succeeds in the role test matrix; approved read/export restrictions are enforced. |
| Accurate reporting | All tested contribution examples, dashboard metrics, and export rows reconcile with agreed source records. |
| Employee data integrity | Invalid and duplicate records are rejected under the approved validation/normalization policy; ordinary edits preserve unrelated data. |
| Understandable onboarding | UI differentiates selected/simulated tasks from externally completed work in every tested onboarding path. |
| Usable response times | Meet the approved performance profile after measurement in a representative environment. |

## 15. Open product decisions

| Decision | Why it matters | Suggested owner |
| --- | --- | --- |
| Public demonstration application or operational HR product? | Determines whether anonymous employee data, exports, analytics, and open registration remain appropriate. | Product owner |
| Should Users view every employee’s contribution details or only their own? | Current accounts are not linked to employees; self-only access requires a new relationship and authorization rules. | Product owner / HR |
| What are the approved company contribution formula, allowance period, rounding rule, and currency? | Current code and labels produce conflicting interpretations and amounts. | Product owner / finance |
| Should zero salary and zero tenure be valid? What upper bounds and name/email lengths apply? | Forms suggest nonnegative values, but complete server rules are undefined. | HR / product owner |
| How are employee email addresses normalized and made unique? | Determines duplicate handling for case changes, whitespace, updates, and concurrent submissions. | Product owner / engineering |
| Do onboarding checks record intent, manual completion, or trigger integrations? | Current flags and messages cannot prove delivery, provisioning, or equipment assignment. | HR / operations |
| Is password recovery/change required for the next release? | Existing users cannot recover credentials or change a local password through completed workflows. | Product owner |
| Should deletion be permanent, recoverable, or retained in an audit history? | Current deletion is irreversible through the application. | HR / product owner |
| Are the hardcoded age thresholds and “Senior Tenure” label appropriate? | These are age-based indicators with no configurable employment policy. | HR |
| What scale, availability, retention, and recovery targets apply? | SQLite, in-memory analytics, and full-result exports have no tested operating envelope. | Product owner / operations |
| Who owns API key rotation and Swagger exposure? | Current API uses one shared configured key without a management UI. | Engineering / operations |

## 16. Source traceability

All links below are repository-relative and refer to the reviewed working-tree files.

| Requirement area | Primary sources |
| --- | --- |
| Employee CRUD, filters, exports | [EmployeeController](Controllers/EmployeeController.cs), [directory view](Views/Employee/Index.cshtml), [create view](Views/Employee/Create.cshtml), [edit view](Views/Employee/Edit.cshtml), [delete view](Views/Employee/Delete.cshtml) |
| Employee schema, validation, onboarding | [Employee](Models/Employee.cs), [account and create view models](Models/AccountViewModels.cs), [pagination model](Models/PaginatedList.cs) |
| Contribution rules and views | [EmployeeDetailsController](Controllers/EmployeeDetailsController.cs), [directory](Views/EmployeeDetails/Index.cshtml), [PF detail](Views/EmployeeDetails/EmployeePF.cshtml), [company detail](Views/EmployeeDetails/EmployeeBonus.cshtml) |
| Dashboard and public pages | [HomeController](Controllers/HomeController.cs), [dashboard](Views/Home/Dashboard.cshtml), [home](Views/Home/Index.cshtml), [About](Views/Home/About.cshtml), [Contact](Views/Home/Contact.cshtml) |
| Authentication and account management | [AccountController](Controllers/AccountController.cs), [ManageController](Controllers/ManageController.cs), [manage models](Models/ManageViewModels.cs), [account views](Views/Account), [manage views](Views/Manage) |
| Roles, migrations, storage | [startup](Program.cs), [seed data](Data/SeedData.cs), [DbContext](Data/ApplicationDbContext.cs), [migrations](Data/Migrations), [snapshot](Data/Migrations/ApplicationDbContextModelSnapshot.cs) |
| API contract and authorization | [EmployeeApiController](Controllers/Api/EmployeeApiController.cs), [API key filter](Filters/ApiKeyAuthFilter.cs), [startup](Program.cs) |
| Navigation and frontend | [shared layout](Views/Shared/_Layout.cshtml), [login partial](Views/Shared/_LoginPartial.cshtml), [validation scripts](Views/Shared/_ValidationScriptsPartial.cshtml), [site CSS](wwwroot/css/site.css) |
| Runtime and deployment | [project](EAEmployee.Net8.csproj), [application settings](appsettings.json), [launch profiles](Properties/launchSettings.json), [IIS configuration](web.config) |
| Prior documentation, cross-checked against source | [README](README.md), [knowledge base](KNOWLEDGEBASE.md) |
