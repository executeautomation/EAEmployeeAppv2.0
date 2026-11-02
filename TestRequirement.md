# Test Requirements: EAEmployee

| Item | Value |
| --- | --- |
| Application | EAEmployee — Employee Management Application |
| Document version | 1.0 |
| Prepared | 2 November 2026 |
| Product baseline | [productrequirement.md](productrequirement.md), version 1.0 |
| Scope reviewed | Current working-tree requirements, controllers, models, startup configuration, and key Razor views |
| Status | Proposed test baseline; pending product-owner review |
| Execution status | Not run; this document defines required testing and does not report passed tests |

## 1. Purpose and interpretation

Define verifiable test requirements for the application's employee management, authentication, contributions, analytics, exports, and read-only API. Include positive, negative, boundary, authorization, persistence, and nonfunctional coverage, with traceability to every functional and nonfunctional product requirement.

The product requirements distinguish existing source behavior from proposed completion work. Tests must preserve that distinction:

| Basis | Meaning |
| --- | --- |
| **B — Baseline** | Expected behavior described by the product baseline and supported by current source. Still requires execution; source inspection is not a pass. |
| **T — Target** | Proposed completion or quality requirement. May fail against the current application and must not be weakened to match a known defect. |
| **D — Decision** | Expected outcome needs an approved business or operational decision. Record current behavior separately; do not claim acceptance until the decision is resolved. |

Priorities follow the PRD: **P0** blocks operability or release integrity, **P1** covers the intended baseline, and **P2** covers subsequent enhancements. Priorities and targets remain proposed until approved. Where a row contains multiple inputs or roles, execute and record each variant separately under the row's stable ID.

The PRD reports 18 build errors involving onboarding `DisplayFormat` annotations, an incompatible Create view model, and missing onboarding database columns. The declarations and view mismatch remain visible in the reviewed source. No build or runtime tests were executed while preparing this file; recheck these findings against the exact build under test. Do not substitute previously published binaries for the current source without recording that different scope.

## 2. Scope and test approach

In scope:

- Public pages, navigation, role-dependent controls, registration, login, logout, account summary, and initial local-password setup.
- Employee list, combined filters, pagination, administrator CRUD, duplicate prevention, onboarding selections, and persistence.
- Contribution directory, PF/company calculations, displayed explanations, and missing-record handling.
- Dashboard KPIs, age/grade distributions, retirement indicators, leaderboards, and refresh.
- CSV/XLSX exports, API authentication and contracts, Swagger, and cross-channel consistency.
- Build/startup, schema upgrades, data integrity, sessions, accessibility, responsive behavior, performance, recovery, and error reporting.

Out of scope unless separately approved: payroll execution or statutory compliance, real email delivery/PF provisioning/equipment allocation, attendance, leave, recruitment, API writes, role-management UI, multi-tenancy, third-party course enrollment, and employee self-service editing. Password recovery completion and employee-specific Details navigation are conditional enhancements rather than existing capabilities.

Use calculation-level checks for numeric rules, HTTP/database integration checks for validation and persistence, and browser checks for user journeys and feedback. Use SQLite for database integration coverage so provider-specific filtering, constraints, and migrations are exercised. Read-only API calls alone cannot verify administrator mutation rules. Browser validation alone cannot verify server enforcement.

## 3. Environment, entry criteria, and evidence

### 3.1 Test environment

- Use an isolated test database and synthetic employee/Identity data; record the commit or working-tree snapshot, .NET SDK, OS, database/migration version, environment name, culture, base URL, and browser versions.
- Restore/build the .NET 8 application from the selected source. Use its configured URL; local profiles describe HTTP port 5114 and HTTPS port 7002. Do not assume those ports for another deployment.
- Configure a disposable API key through environment-specific configuration, for example the `ApiSettings__ApiKey` environment variable. Keep the value out of this file, test reports, screenshots, and saved request traces.
- Prepare separate Administrator, User, second User, and passwordless test accounts. A passwordless authenticated account requires a controlled fixture or test harness because registration creates a password. Do not add a production bypass for testing.
- Test both a fresh database and an upgrade copy containing employees and Identity data from the prior supported schema. Snapshot and restore fixtures between suites.
- Proposed browser matrix: Chrome, Edge, Firefox, and Safari at recorded versions. Proposed layout widths: 375, 768, and 1440 CSS pixels. Confirm supported platforms before release sign-off.
- Use a production-like configuration for error/HSTS checks and representative hardware for performance checks. External learning sites require link verification only.

### 3.2 Entry criteria

Documentation review and test design may proceed before a working build. Runtime acceptance requires a successful build, successful fresh/upgrade migrations, reachable application, seeded roles, test accounts, and configured API key. If a prerequisite fails, mark dependent cases **Blocked** with the prerequisite defect; do not mark them passed or silently omit them.

Decision-dependent cases may be characterized before approval, but their acceptance result remains **Blocked — decision pending**. Record the approved decision and update this document before asserting a final expected result.

### 3.3 Required execution record

For each case and parameter variant, capture: test ID, linked FR/NFR, build/environment, preconditions and fixture, actions or request, expected result, actual result, status, evidence, defect reference, tester, and timestamp. Status values are **Not run**, **Pass**, **Fail**, **Blocked**, or **Not applicable with approved rationale**. All cases initially have status **Not run**; basis and priority are not execution statuses.

Verify persisted mutations through a fresh read or database assertion, rather than trusting a success banner or redirect. Record row counts and relevant field values before and after rejected requests. Redact credentials, API keys, session cookies, anti-forgery tokens, and personal data from evidence.

## 4. Test data and independent expected results

### 4.1 Deterministic employee fixture

Create these 12 records in the listed order with generated IDs, recording each assigned ID. Set all onboarding flags false unless a test explicitly overrides them. `Months` means `DurationWorked`; salary is monthly. Treat `A01`–`A12` as fixture labels, not database IDs.

| Label | Name | Email | Age | Salary | Months | Grade |
| --- | --- | --- | --- | --- | --- | --- |
| A01 | Ann Alpha | ann.alpha@team-a.example | 18 | 1000.00 | 6 | 1 |
| A02 | Anna Beta | anna.beta@team-a.example | 29 | 2000.00 | 12 | 2 |
| A03 | Anne Gamma | anne.gamma@team-b.example | 30 | 3000.00 | 18 | 3 |
| A04 | Jo Ann | jo.ann@team-a.example | 39 | 4000.00 | 24 | 4 |
| A05 | Ben Delta | ben.delta@team-a.example | 40 | 5000.00 | 30 | 1 |
| A06 | Cara Echo | cara.echo@team-b.example | 49 | 6000.00 | 36 | 2 |
| A07 | Dev Foxtrot | dev.foxtrot@team-a.example | 50 | 7000.00 | 42 | 3 |
| A08 | Eli Golf | eli.golf@team-b.example | 54 | 8000.00 | 48 | 4 |
| A09 | Fay Hotel | fay.hotel@team-a.example | 55 | 9000.00 | 54 | 1 |
| A10 | Gus India | gus.india@team-b.example | 59 | 10000.00 | 60 | 2 |
| A11 | Hana Juliet | hana.juliet@team-a.example | 60 | 11000.00 | 66 | 3 |
| A12 | Ivo Kilo | ivo.kilo@team-b.example | 100 | 12000.00 | 72 | 4 |

Independent expectations for this exact fixture:

- Total employees: **12**; salary sum: **78,000.00**; average salary: **6,500.00**; average age: **583 / 12 = 48.5833…**, displayed as **48.6**. Dashboard salary KPIs show zero decimal places.
- Age bands under 30 / 30–39 / 40–49 / 50–59 / 60+: **2 / 2 / 2 / 4 / 2**. Each grade count: **3**. Near retirement (55–59): **2**; retirement age (60+): **2**.
- Top earners and longest tenure: **A12, A11, A10, A09, A08**, in that order.
- Name prefix `Ann`: **A01, A02, A03**; `Jo Ann` must not match that prefix. Email fragment `team-a`: **7** matches. Combined name `Ann`, email `team-a`, grade `2`: **A02** only.
- Web list: **3 pages**, with counts **5 / 5 / 2**; assert membership by ID without assuming an unspecified web sort. Default API: **2 pages**, with counts **10 / 2**, ordered by ID ascending.
- Unfiltered CSV/XLSX: **12 data rows** plus headers, regardless of the current directory page.

### 4.2 Additional boundary fixtures

Use fresh unique emails for valid mutations. Test empty and single-record databases, exactly 5 and 6 records for web pagination, and 51 records for API size limits. Add separate records with Unicode names, apostrophes, commas, double quotes, and permitted line breaks for rendering/export checks. Include formula-like text beginning with `=`, `+`, `-`, and `@` for spreadsheet-safety checks without executing any formula payload.

Test age 17/18/100/101; salary -0.01/0/0.01; tenure -1/0/1; grades 0/1/4/5; blank/whitespace-only names; blank/malformed emails; missing values; nonnumeric, fractional integer, and overflowing numeric input. Zero salary/tenure and unspecified upper bounds need the decisions in Section 10. Test email case/whitespace variants and concurrent duplicate requests separately from exact duplicates.

For contribution examples use a separate employee with salary **5,000.00**, months **24**, and grade **2**. Employee PF is **14,400.00**. Current company-detail logic gives **21,800.00**, while onboarding's employer calculation gives **14,600.00**. These company values characterize the documented discrepancy; neither establishes the approved employer rule.

## 5. Functional test requirements

### 5.1 Public pages, authentication, accounts, and permissions

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-NAV-01 | FR-01 | P1 / B | Open Home, Employees, Dashboard, About, and `/Home/Contact`. Each intended page renders with usable navigation; visible learning/contact links have the intended destinations. Exercise navigation at desktop and mobile widths. |
| TR-NAV-02 | FR-01, FR-04 | P1 / B | Compare anonymous, User, and Administrator navigation. Signed-out users see Register/Login; signed-in users see account/logout; contribution Details navigation is available to User and Administrator; employee mutation controls are Administrator-only. |
| TR-AUTH-01 | FR-02 | P1 / B | Register a valid unique username/email with matching password. Exactly one Identity account is created, assigned User rather than Administrator, signed in, and redirected Home. No employee record is implicitly created or linked. |
| TR-AUTH-02 | FR-02 | P1 / B | Parameterize username and password lengths 5/6/100/101, missing username/email/password, malformed email, confirmation mismatch, and an existing username. Reject invalid submissions with actionable validation and no extra account; boundary lengths 6 and 100 pass when other Identity constraints are satisfied. Do not assume Identity email uniqueness without an approved rule. |
| TR-AUTH-03 | FR-03 | P1 / B | Login with valid credentials, wrong password, unknown username, and missing fields. Success authenticates; failures retain the form and show generic invalid-login feedback or required-field errors without issuing an authenticated session. |
| TR-AUTH-04 | FR-03 | P1 / B | Login with a local return URL, no URL, an external absolute URL, and a protocol-relative URL. Only a valid local return URL is followed; other successful logins return Home. |
| TR-AUTH-05 | FR-03 | P1 / B | Compare remember-me true/false authentication cookie persistence in isolated browser profiles. After a valid logout POST, a fresh protected-page request requires authentication. A GET request must not perform logout. |
| TR-AUTH-06 | FR-04 | P1 / B | Execute every role/route combination in Section 6 through direct HTTP requests, including mutation POSTs with valid anti-forgery context. Unauthorized requests make no data changes; hiding a control alone is insufficient. |
| TR-ACC-01 | FR-18 | P1 / B | Login as each of two Users and open `/Manage`. Each sees their own username/email only. Anonymous requests require login; crafted query/form IDs must not select another account. |
| TR-ACC-02 | FR-18 | P1 / B | With an authenticated passwordless fixture, set a matching 6–100 character password. Verify password persistence, refreshed sign-in, success message, and subsequent login using the new password. |
| TR-ACC-03 | FR-18 | P1 / B | Submit initial-password lengths 5/6/100/101, blank input, and mismatched confirmation; also call SetPassword for an account already having a password. Invalid attempts display validation/Identity errors and preserve the existing credentials; this route does not change an existing password. |
| TR-REC-01 | FR-19, FR-20 | P2 / B+T | Characterize the current ForgotPassword form: malformed input stays on the form; a syntactically valid email leads to confirmation without delivery. Target: demo wording must accurately describe this limitation and must not promise a completed recovery action. |
| TR-REC-02 | FR-19 | P2 / D | If real recovery is approved, verify generic responses for known/unknown emails, configured delivery, valid reset, invalid/expired/reused token rejection, and login with the new password. Keep this conditional suite blocked until its delivery and token rules are approved and implemented. |

### 5.2 Employee directory, search, and pagination

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-LIST-01 | FR-05 | P1 / B | Load the 12-record fixture. Verify displayed name, age, salary, months, grade label, email, filtered total, and role-dependent actions against the stored records. Grades 1–4 display Junior/Middle/Senior/C-Level. |
| TR-LIST-02 | FR-06 | P1 / B | Test name prefix, email substring, exact grade, and all combined filters using Section 4's expected IDs. Filters combine with AND; no unrelated record appears. Blank filters return all records. |
| TR-LIST-03 | FR-06, FR-07 | P1 / B | Search, navigate Next/Previous/numbered pages, and download both export types. Active filter values remain in controls/links/requests. Clear removes every filter and returns to the first unfiltered page. |
| TR-LIST-04 | FR-05, FR-07 | P1 / B | Test 0, 1, 5, 6, and 12 records and a no-match filter. Show at most five rows, correct total/page controls, and “No employees found” for an empty directory or filtered set. |
| TR-LIST-05 | FR-07 | P1 / T | Request page 0, negative pages, an out-of-range positive page, and very large page values. Enforce a documented normalization/rejection policy without 500 errors; repeated paging over unchanged data has stable order and no duplicates or omissions. Exact boundary policy and sort need approval. |
| TR-LIST-06 | FR-06 | P1 / D | Characterize case variants and surrounding whitespace for name/email filters using the deployed SQLite behavior. Final acceptance follows the approved normalization/case policy; do not assume case-insensitive matching across all channels. |
| TR-LIST-07 | FR-05, FR-15 | P1 / B | Check ages 49/50/54/55/59/60. Verify green/no extra status below 50, blue “Senior Tenure” at 50–54, purple “Near Retirement” at 55–59, and red “Retirement Age” at 60+. Status does not modify or remove the employee. |

### 5.3 Employee creation, validation, duplicates, and onboarding

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-CREATE-01 | FR-08 | P0 / T | As Administrator, open Create and submit a valid unique record. The form renders without Razor/model errors, saves exactly one employee, and returns to the directory. A fresh read matches all six supplied employee fields. |
| TR-VAL-01 | FR-08, FR-11 | P1 / T | Run every invalid employee input in Section 4.2 through both Create and Edit, including raw POSTs bypassing browser constraints. Reject blank/whitespace name, invalid email, age outside 18–100, negative salary/months, unsupported grade, missing required values, and invalid numeric binding without any persistence. |
| TR-VAL-02 | FR-08, FR-11, FR-20 | P1 / T | On invalid submission, preserve valid nonsecret field values and onboarding selections, associate validation with the relevant fields, and keep a usable form visible. Client and server rules agree; no validation response is treated as successful creation by AJAX. |
| TR-VAL-03 | FR-08, FR-11 | P1 / T+D | Accept ages 18/100 and grades 1/4 with valid remaining fields. Verify 0.01 salary and one month persist correctly. Test zero salary/months, monetary precision, and large values against approved bounds/rounding; decision-dependent expectations remain blocked. |
| TR-DUP-01 | FR-09 | P1 / B | Create with an exact existing email. No record is added; the duplicate dialog shows the correct existing employee. Edit Existing opens that employee; Close preserves the attempted form without creating or changing a record. |
| TR-DUP-02 | FR-09, FR-11 | P1 / T+D | Test case/whitespace email variants, editing to another employee's email, and editing while keeping one's own email. Apply approved normalization consistently; reject collisions without mutation and allow a nonconflicting unchanged email. |
| TR-DUP-03 | FR-09 | P1 / T | Submit the same new email concurrently from two valid Administrator sessions. Exactly one new record exists; the competing request gets controlled duplicate feedback rather than an unhandled error. Verify database uniqueness enforcement, using the approved normalization policy. |
| TR-ONB-01 | FR-10 | P1 / T | Test all eight combinations of welcome-email/PF-setup/IT-equipment selections. Persist exactly the selected flags, then restart/reload and verify them. Check initial selections against the intended defaults; the Create model currently defaults welcome email true and the other selections false. |
| TR-ONB-02 | FR-10, FR-20 | P1 / T | Labels and feedback distinguish selected/simulated onboarding work from verified delivery/provisioning. No test expects an external email, PF account, or asset allocation from current simulation code. Ordinary logs must not expose employee salary/contact data. |
| TR-CREATE-02 | FR-08, FR-20 | P1 / T | Exercise create validation responses, duplicate JSON, successful redirect, HTTP failure, network interruption, and expired authentication. Show accurate feedback; do not redirect as success on validation/login/error HTML. After an ambiguous network result, determine persistence before retrying so no accidental duplicate is introduced. |
| TR-CREATE-03 | FR-08 | P1 / B | Enter valid data and select Cancel/Back to List without submitting. Return to the directory with no new employee. |

### 5.4 Employee editing and deletion

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-EDIT-01 | FR-11 | P1 / B+T | Load an existing record as Administrator, change all six editable fields to valid values, and save. Exactly the intended record changes and a fresh read agrees; employee count remains unchanged. Cancel/Back to List makes no requested change. |
| TR-EDIT-02 | FR-10, FR-11 | P1 / T | Edit a record with all onboarding flags true, then a mixed-flag record. Ordinary field edits preserve every flag omitted from the form. Crafted submissions cannot change fields outside the approved edit contract. |
| TR-EDIT-03 | FR-11, FR-20 | P1 / B+T | Unknown Edit GET returns 404; a route/model ID mismatch is rejected with no mutation. Delete a record in another session before saving its open edit form: show controlled not-found/conflict feedback without recreating it or returning an unhandled error. |
| TR-DEL-01 | FR-12 | P1 / B | Delete GET shows the selected employee without changing data. Cancel preserves it; a valid Administrator confirmation POST removes only that record and returns to the list. |
| TR-DEL-02 | FR-12 | P1 / B | Unknown Delete GET returns 404. Confirming an already-missing employee safely redirects to the list. Repeat a completed deletion and verify no other records are affected. |

### 5.5 Contributions and analytics

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-PF-01 | FR-13 | P1 / B | User and Administrator open the contribution directory and its name-prefix query. Verify matching employees, salary/grade, PF/company links, and empty state. Characterize the per-row Details link as opening the whole directory; employee-specific navigation needs a separate approved change. |
| TR-PF-02 | FR-14 | P1 / B | With salary 5,000, months 24, grade 2, employee PF is 14,400.00. Verify rendered name/salary/months/grade and the explanation agree with the selected employee and `round(salary × 0.12 × months, 2)`. |
| TR-PF-03 | FR-14 | P1 / D | Record company detail 21,800.00 for the same example and the conflicting 18% controller, 15% label, and 12% onboarding/documentation rates. After approval, require calculation, explanation, onboarding, documentation, allowance period, and currency to agree with one rule. Do not pass the requirement solely because current code is reproduced. |
| TR-PF-04 | FR-14 | P1 / B+T+D | Unknown PF/company IDs return 404. Test fractional salaries, midpoint rounding, all grades, zero salary, and zero months. Apply approved monetary/zero rules; explicitly check whether a grade allowance remains when months are zero. Compare numeric output to an independent calculation and show two decimals. |
| TR-DASH-01 | FR-15 | P1 / B | On the 12-record fixture, reconcile all KPIs, five age bands, four grade counts, retirement counts, and both top-five lists with Section 4.1. Directory filters must not restrict dashboard aggregates. |
| TR-DASH-02 | FR-15 | P1 / B | Test ages 29/30, 39/40, 49/50, 54/55, and 59/60. Every employee belongs to exactly one age band; alert appears only when at least one age is 55+ and distinguishes 55–59 from 60+. |
| TR-DASH-03 | FR-15 | P1 / B | With zero records, show the dashboard empty state without division/rendering errors and offer add-employee only to Administrator. With one to four records, show no more leaderboard rows than exist. Ties need no specific secondary order until one is defined. |
| TR-DASH-04 | FR-15 | P1 / B | Create, update, and delete a fixture record, refreshing after each action. Dashboard metrics and rankings recompute from current persisted data; no stale total is shown after a successful refresh. |

### 5.6 CSV and Excel exports

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-EXP-01 | FR-16 | P1 / B | Export CSV and XLSX unfiltered and with each combined filter, including from web page 2. Match the complete filtered ID set, all seven core fields, numeric grades, and persisted values. Twelve unfiltered employees produce twelve data rows; no matches produces headers only. Onboarding flags are excluded. |
| TR-EXP-02 | FR-16 | P1 / B | Verify filename pattern `employees_yyyyMMdd_HHmmss.csv` / `.xlsx`; CSV MIME `text/csv`; XLSX MIME `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`. CSV headers are `Id,Name,Email,Age,Salary,DurationWorked,Grade`; XLSX replaces the tenure header with `Duration Worked (months)`, uses sheet `Employees`, and styles its header row. |
| TR-EXP-03 | FR-16 | P1 / B+T | Round-trip Unicode, commas, quotes, and permitted line breaks through a CSV parser and representative spreadsheet software. Embedded quotes are escaped and values/row boundaries preserved. XLSX numeric cells remain numeric; locale changes must not corrupt CSV columns or monetary values under the approved format policy. |
| TR-EXP-04 | FR-16 | P1 / T+D | Export user text starting with formula markers and inspect cell interpretation without executing payloads. Apply an approved CSV/XLSX neutralization policy that prevents formulas while preserving understandable text. Record the chosen policy before final acceptance. |

### 5.7 Employee API and Swagger

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-API-01 | FR-17 | P1 / B | Call both GET endpoints with missing, incorrect, case-altered key values, and the correct configured `X-Api-Key`. Missing/incorrect values return 401 JSON errors; the correct key allows the request. A browser Identity session alone does not bypass the key requirement. |
| TR-API-02 | FR-17 | P1 / B | Request `GET /api/employees` against the fixture with no paging parameters. Return 200 JSON containing `totalCount: 12`, `page: 1`, `pageSize: 10`, `totalPages: 2`, and ten `data` entries in ascending ID order. Page 2 contains the remaining two entries. |
| TR-API-03 | FR-17 | P1 / B | Parameterize page 0/-1/1 and pageSize -1/0/1/50/51. Page is at least 1; size clamps to 1–50; returned metadata reflects normalized values. Use 51 records to verify the maximum and last page. |
| TR-API-04 | FR-17 | P1 / B+T | No-match filters return `totalCount: 0`, `totalPages: 0`, `data: []`. An ordinary positive page beyond the last returns empty `data` with the matching total unchanged. Very large bound integers must not cause arithmetic-overflow pagination errors or a 500 response. |
| TR-API-05 | FR-17 | P1 / B | Test independent/combined `searchTerm`, `emailTerm`, and `gradeFilter` using Section 4.1. Compare exact matching IDs/totals; pagination applies after filtering. Well-typed unsupported grades yield no matches; malformed integer query parameters return framework validation errors (400) with a valid API key. |
| TR-API-06 | FR-17 | P1 / B | Existing integer ID returns 200 with correct employee values. Unknown integer ID returns 404 with an `error` message; a noninteger route does not resolve to an employee. Verify camel-case fields: `id`, `name`, `salary`, `age`, `durationWorked`, `grade`, `email`, `isWelcomeEmailSent`, `isPFAccountSetupCompleted`, `isITEquipmentAssigned`. No Identity credentials or unrelated data are serialized. |
| TR-API-07 | FR-17 | P1 / B | Exercise unsupported POST/PUT/PATCH/DELETE requests against employee API paths. No employee mutation occurs; no write operation is advertised in the schema. Record route/method rejection without assuming every unmatched route returns the same status. |
| TR-API-08 | FR-17 | P1 / B | Open Swagger UI and schema in development and production-like environments. Both employee GET operations, parameters, list envelope, response model, and `X-Api-Key` authorization are discoverable. Interactive requests obey the same key checks as direct requests; public Swagger access is current behavior pending policy review. |
| TR-API-09 | FR-17 | P1 / T | Start an isolated instance without a configured key and with an empty configured key; try absent, empty, and arbitrary supplied values. No employee data is returned. Record the operational failure clearly; do not accept a blank key as authorization. |

### 5.8 Error handling and cross-channel consistency

| Test ID | PRD | Priority / basis | Scenario and required result |
| --- | --- | --- | --- |
| TR-ERR-01 | FR-20 | P1 / T | In a controlled environment, trigger a persistence failure and an unexpected exception. No mutation is falsely reported as successful, no partial record is saved, and production responses expose a request identifier without stack traces or secrets. Logs support diagnosis. |
| TR-INT-01 | FR-05, FR-11, FR-12, FR-14, FR-15, FR-16, FR-17 | P1 / B+T | Create an employee, compare directory/API/export/contribution/dashboard values, edit it and repeat, then delete it. Every fresh read reflects the same persisted state; deleted detail/API IDs return 404 and totals/exports no longer contain the record. |

## 6. Authorization and session test matrix

Apply this matrix to TR-AUTH-06 and the security cases below. It records the current server policy from the PRD, not approval to publish real employee data. Run fresh requests in isolated sessions; API-key-only means no Identity cookie.

| Resource / operation | Anonymous | User | Administrator | API key only |
| --- | --- | --- | --- | --- |
| Home, About, Contact | Allowed | Allowed | Allowed | Public access |
| Directory, dashboard, CSV/XLSX | Allowed today | Allowed | Allowed | Public access |
| Create/Edit/Delete GET and POST | Login required | Access denied | Allowed with valid input/token | Login required |
| Contribution directory/PF/company | Login required | Allowed | Allowed | Login required |
| Manage/SetPassword | Login required | Own account | Own account | Login required |
| Employee API GETs | 401 without key | 401 without key | 401 without key | Allowed with valid key |
| Swagger UI/schema | Allowed today | Allowed | Allowed | Public access |

For cookie-protected MVC routes, check the initial authentication/access-denied redirect and destination rather than only the eventual 200 login/error page. For API-key failures, require 401 without treating a followed redirect as equivalent. For every denied mutation, verify unchanged data. A valid API key must never grant Administrator or User membership.

## 7. Nonfunctional and operational test requirements

| Test ID | PRD | Priority / basis | Verification and acceptance requirement |
| --- | --- | --- | --- |
| TR-OPS-01 | NFR-01 | P0 / T | Restore/build the chosen source, then start it using a fresh database created solely by committed migrations. All employee/Identity tables and onboarding columns exist; roles/admin seed once; representative directory/Create/API requests succeed without schema/Razor errors. |
| TR-OPS-02 | NFR-01, NFR-08 | P0 / T | Upgrade a prior-schema database containing employees, users, and role membership, then restart twice. Preserve existing data and credentials; apply migrations once; seed no duplicate roles/admin. Validate old age-0 records are identified/remediated under an approved cleanup policy rather than silently treated as valid ages. |
| TR-SEC-01 | NFR-02 | P1 / B+T | Submit every state-changing browser form with missing/invalid anti-forgery tokens: Register, Login, Logout, ForgotPassword, SetPassword, Create, Edit, Delete. Reject requests without mutations/session changes. Valid authenticated/token variants still work. |
| TR-SEC-02 | NFR-02 | P1 / B+D | Verify logout and an expired/absent session prevent fresh protected requests, local-return enforcement, and the complete Section 6 matrix. Repeat public read/export/Swagger checks against the approved release policy; a policy change requires updated expectations. |
| TR-DATA-01 | NFR-03 | P1 / T+D | Reconcile server validation, normalized database uniqueness, concurrency behavior, preserved onboarding flags, and stable pagination using the CRUD cases. Verify stored and displayed monetary precision under the approved decimal/rounding policy; current float storage is a known limitation. |
| TR-SEC-03 | NFR-04 | P1 / T+D | Inspect deployment configuration and sanitized logs for secrets or salary/contact leakage. API key comes from environment-specific secret configuration; operational deployments replace demonstration admin credentials. Public read policy and key-rotation ownership are recorded. |
| TR-SEC-04 | NFR-02, NFR-04 | P1 / T | Enter HTML/script-like text in permitted name fields and search inputs, including duplicate-modal rendering. Content is safely encoded, scripts do not execute, and query-like text does not alter database/query scope. Verify authorized direct requests separately from UI visibility. |
| TR-UX-01 | NFR-05 | P1 / T | Review against proposed WCAG 2.2 AA: keyboard navigation, visible focus, associated labels/errors, reading order, contrast, modal focus trap/return/Escape, table headers, and accessible control names. Status meaning remains clear without color. Combine automated checks with manual keyboard/screen-reader inspection. |
| TR-UX-02 | NFR-06 | P1 / T | At each Section 3 browser/width combination, exercise navigation, Create/Edit, duplicate dialog, directory/paging/exports, contribution cards, dashboard, and account forms. No overlapping controls, inaccessible actions, unreadable errors, or clipped essential information; table overflow remains usable. Include 200% zoom. |
| TR-PERF-01 | NFR-07 | P1 / D | Proposed profile: 1,000 employees and 10 concurrent readers, p95 below 2 seconds for directory/API reads and below 3 seconds for dashboard requests, excluding external network latency. Approve hardware, warm-up, test duration/request mix, measurement boundary, and acceptable error rate first; capture p50/p95, errors, throughput, CPU, memory, and DB contention. |
| TR-PERF-02 | NFR-07 | P1 / D | Measure filtered/unfiltered CSV/XLSX generation time, row completeness, memory, and concurrent-read impact using approved dataset sizes and limits. Define timing/capacity thresholds before pass/fail; no export SLA is currently approved. |
| TR-REL-01 | NFR-08 | P1 / T+D | Back up a populated SQLite database using a documented consistent method, restore to an isolated instance, and compare employee values, flags, roles, login, API, and metrics. Record recovery duration/data loss against approved RTO/RPO; these targets remain undecided. |
| TR-REL-02 | NFR-08 | P0 / T | Trigger a migration/seeding failure using a controlled fixture/configuration. Startup fails visibly with diagnostic logs rather than serving a partially initialized application. Restore the fixture and verify normal startup resumes. |
| TR-OBS-01 | NFR-09 | P1 / T+D | Trigger a controlled production request failure and correlate its request ID to logs without exposing stack traces/secrets to the client. If operational employee auditing is approved, separately verify actor/time/change history; current code has no audit trail to certify. |
| TR-EXP-05 | NFR-10 | P1 / T+D | Execute TR-EXP-03/04 with recorded culture and spreadsheet versions. Prove value preservation, correct field boundaries, and approved formula neutralization. Record unsupported or undecided locale behavior explicitly. |

## 8. Product requirement traceability

The mappings below identify coverage; they do not indicate completion or a pass. Detailed test rows above supply the scenarios and expected outcomes.

| Product requirement | Test requirements |
| --- | --- |
| FR-01 Public navigation | TR-NAV-01, TR-NAV-02 |
| FR-02 Registration | TR-AUTH-01, TR-AUTH-02, TR-SEC-01 |
| FR-03 Login/logout | TR-AUTH-03, TR-AUTH-04, TR-AUTH-05, TR-SEC-01, TR-SEC-02 |
| FR-04 Authorization | TR-NAV-02, TR-AUTH-06, Section 6, TR-SEC-02 |
| FR-05 Directory | TR-LIST-01, TR-LIST-04, TR-LIST-07, TR-INT-01 |
| FR-06 Filters | TR-LIST-02, TR-LIST-03, TR-LIST-06 |
| FR-07 Pagination | TR-LIST-03, TR-LIST-04, TR-LIST-05 |
| FR-08 Create/validation | TR-CREATE-01, TR-CREATE-02, TR-CREATE-03, TR-VAL-01, TR-VAL-02, TR-VAL-03 |
| FR-09 Duplicates | TR-DUP-01, TR-DUP-02, TR-DUP-03 |
| FR-10 Onboarding | TR-ONB-01, TR-ONB-02, TR-EDIT-02 |
| FR-11 Editing | TR-EDIT-01, TR-EDIT-02, TR-EDIT-03, TR-VAL-01, TR-VAL-02, TR-VAL-03, TR-DUP-02, TR-INT-01 |
| FR-12 Deletion | TR-DEL-01, TR-DEL-02, TR-INT-01 |
| FR-13 Contribution directory | TR-PF-01 |
| FR-14 Calculations | TR-PF-02, TR-PF-03, TR-PF-04, TR-INT-01 |
| FR-15 Analytics | TR-DASH-01, TR-DASH-02, TR-DASH-03, TR-DASH-04, TR-LIST-07, TR-INT-01 |
| FR-16 Exports | TR-EXP-01, TR-EXP-02, TR-EXP-03, TR-EXP-04, TR-INT-01 |
| FR-17 API | TR-API-01 through TR-API-09, TR-INT-01 |
| FR-18 Account summary/password | TR-ACC-01, TR-ACC-02, TR-ACC-03 |
| FR-19 Recovery | TR-REC-01, TR-REC-02 |
| FR-20 Errors/feedback | TR-VAL-02, TR-CREATE-02, TR-EDIT-03, TR-ONB-02, TR-REC-01, TR-ERR-01 |
| NFR-01 Operability | TR-OPS-01, TR-OPS-02 |
| NFR-02 Authorization/sessions | TR-SEC-01, TR-SEC-02, TR-SEC-04, TR-AUTH-04, TR-AUTH-05, TR-AUTH-06 |
| NFR-03 Data integrity | TR-DATA-01, TR-VAL-01, TR-DUP-03, TR-EDIT-02, TR-LIST-05 |
| NFR-04 Credentials/sensitive data | TR-SEC-03, TR-SEC-04, TR-ONB-02, TR-API-09 |
| NFR-05 Accessibility | TR-UX-01 |
| NFR-06 Responsive layout | TR-UX-02 |
| NFR-07 Performance | TR-PERF-01, TR-PERF-02 |
| NFR-08 Reliability/recovery | TR-OPS-02, TR-REL-01, TR-REL-02, TR-ERR-01 |
| NFR-09 Observability | TR-OBS-01, TR-ERR-01 |
| NFR-10 Export safety | TR-EXP-03, TR-EXP-04, TR-EXP-05 |

## 9. Known-gap regression coverage

These are findings inherited from PRD Section 12, not newly executed test results. Log/recheck each against the build under test and retain its regression case after correction.

| PRD gap | Main risk | Regression requirements |
| --- | --- | --- |
| GAP-01 | Source does not compile | TR-OPS-01 |
| GAP-02 | Create view/action model mismatch | TR-CREATE-01, TR-VAL-02, TR-DUP-01 |
| GAP-03 | Missing onboarding schema columns | TR-OPS-01, TR-OPS-02, TR-ONB-01 |
| GAP-04 | Incomplete server validation | TR-VAL-01, TR-VAL-02, TR-VAL-03 |
| GAP-05 | Inconsistent employer formula/currency | TR-PF-03, TR-PF-04 |
| GAP-06 | Incomplete duplicate enforcement | TR-DUP-01, TR-DUP-02, TR-DUP-03 |
| GAP-07 | Editing resets flags/concurrent deletion errors | TR-EDIT-02, TR-EDIT-03 |
| GAP-08 | AJAX false success/missing errors | TR-CREATE-02, TR-VAL-02 |
| GAP-09 | Public employee data/exports | TR-AUTH-06, TR-SEC-02, Section 6 |
| GAP-10 | Simulated completion/sensitive logs | TR-ONB-02, TR-SEC-03 |
| GAP-11 | Recovery/password-change claims exceed implementation | TR-REC-01, TR-REC-02, TR-ACC-03 |
| GAP-12 | Pagination/export interoperability | TR-LIST-05, TR-EXP-03, TR-EXP-04, TR-EXP-05 |
| GAP-13 | Details link ignores ID/filter lacks visible control | TR-PF-01; add navigation/filter UI acceptance cases if that enhancement is approved |

## 10. Decisions required before acceptance

| Decision | Affected tests | Suggested owner |
| --- | --- | --- |
| Public demo versus operational HR access; anonymous directory/dashboard/exports, Swagger exposure, open registration, and User contribution scope | TR-AUTH-06, TR-SEC-02, TR-SEC-03, TR-PF-01, TR-API-08 | Product owner / HR / engineering |
| Company rate, allowance period, currency, monetary precision, and rounding | TR-PF-03, TR-PF-04, TR-DATA-01, TR-VAL-03 | Product owner / finance |
| Zero salary/months, upper bounds, name/email lengths, and cleanup of existing age-0 data | TR-VAL-01, TR-VAL-03, TR-OPS-02 | Product owner / HR |
| Email normalization/uniqueness and filter case/whitespace behavior | TR-DUP-02, TR-DUP-03, TR-LIST-06 | Product owner / engineering |
| Stable web sort and invalid/out-of-range page behavior | TR-LIST-05 | Product owner / engineering |
| Onboarding intent versus manual completion/integration; supported defaults | TR-ONB-01, TR-ONB-02 | HR / operations |
| CSV locale/format and spreadsheet formula-neutralization policy | TR-EXP-03, TR-EXP-04, TR-EXP-05 | Product owner / engineering |
| Recovery, existing-password change, employee-specific Details/filter UI, recoverable deletion, and audit scope | TR-REC-02, TR-ACC-03, TR-PF-01, TR-DEL-01, TR-OBS-01 | Product owner / HR |
| Supported browsers, accessibility target, performance profile/export limits, availability, backup retention, and RTO/RPO | TR-UX-01, TR-UX-02, TR-PERF-01, TR-PERF-02, TR-REL-01 | Product owner / operations |
| API key provisioning/rotation and deployment credential ownership | TR-SEC-03, TR-API-01, TR-API-09 | Engineering / operations |

Do not invent business outcomes for undecided cases. Record characterization evidence, obtain the decision in the product baseline, then revise the expected results and release scope accordingly.

## 11. Execution order and exit criteria

Execute in this order:

1. Build, fresh initialization, upgrade, and startup-failure checks. Resolve P0 blockers before dependent runtime testing.
2. Authentication/authorization and a smoke journey: login as Administrator, create, find, edit, verify contribution/dashboard/export/API, confirm-delete, and logout. Verify User/anonymous mutation denial.
3. All approved P1 functional, negative, boundary, integrity, and nonfunctional cases, prioritizing the known gaps.
4. Conditional P2/enhancement cases only when approved and implemented. Retest defects and their affected workflows after fixes.

Proposed release exit criteria:

- All in-scope P0/P1 acceptance variants pass on the identified release build, including fresh and upgrade paths. No unresolved critical/high-severity defect affecting operability, permissions, data integrity, or numeric correctness remains; final severity definitions belong to release governance.
- Every FR-01–FR-20 and NFR-01–NFR-10 has execution evidence or an explicitly approved deferral/rationale. P2 and decision-dependent scope is stated; blocked/untested cases are never counted as passes.
- Known gaps are fixed and regression-tested, or their deferral and release impact are explicitly accepted by the responsible owner. Public-data and contribution decisions are resolved before operational HR use.
- Reporting, exports, API data, and contribution results reconcile with independent fixture expectations. Invalid/unauthorized mutations leave persisted data unchanged.
- Approved accessibility, browser, performance, and recovery checks meet their targets; unresolved thresholds and untested environments remain visible in the release report.

The final execution report must identify coverage totals by status and priority, defects and residual risks, approved deferrals, environment/build details, and links to evidence. This requirements document alone does not authorize a release or certify application quality.

## 12. Source references

- [Product requirements](productrequirement.md): Sections 3, 6–10, 12–13, and 15 provide the access policy, FR/NFR IDs, data/business rules, known gaps, release expectations, and open decisions.
- [Employee controller](Controllers/EmployeeController.cs), [employee model](Models/Employee.cs), [account/create models](Models/AccountViewModels.cs), and [pagination model](Models/PaginatedList.cs): CRUD, validation, duplicate handling, onboarding, search, exports, and paging.
- [Employee views](Views/Employee): directory, create/edit validation, duplicate modal, and deletion confirmation.
- [Account controller](Controllers/AccountController.cs), [Manage controller](Controllers/ManageController.cs), and [Manage models](Models/ManageViewModels.cs): authentication, account summary, initial password, and recovery limitations.
- [Contribution controller](Controllers/EmployeeDetailsController.cs), [contribution views](Views/EmployeeDetails), [Home controller](Controllers/HomeController.cs), and [dashboard view](Views/Home/Dashboard.cshtml): calculations, analytics, indicators, and display rules.
- [Employee API controller](Controllers/Api/EmployeeApiController.cs) and [API-key filter](Filters/ApiKeyAuthFilter.cs): endpoint contracts and authentication.
- [Startup](Program.cs), [seed data](Data/SeedData.cs), [database context](Data/ApplicationDbContext.cs), and [migrations](Data/Migrations): initialization, schema, configuration, roles, and deployment behavior.
