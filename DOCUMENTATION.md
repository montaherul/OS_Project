# EDFRR — Full Project Documentation

**Version:** 1.0.0  
**Framework:** ASP.NET Core 8.0 MVC  
**Database:** SQL Server  
**Project Type:** Academic Final Year Project

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
3. [Setup Guide](#3-setup-guide)
4. [Project Structure](#4-project-structure)
5. [Data Models](#5-data-models)
6. [Repository Layer](#6-repository-layer)
7. [Service Layer](#7-service-layer)
8. [Scheduling Engine](#8-scheduling-engine)
9. [Controllers & Routes](#9-controllers--routes)
10. [View Layer](#10-view-layer)
11. [Admin Panel](#11-admin-panel)
12. [API Endpoints](#12-api-endpoints)
13. [Database Schema](#13-database-schema)
14. [Stored Procedures](#14-stored-procedures)
15. [Configuration](#15-configuration)
16. [Security & Authentication](#16-security--authentication)
17. [Dependencies](#17-dependencies)
18. [Browser Support](#18-browser-support)

---

## 1. Project Overview

EDFRR (Earliest Deadline First + Round Robin) is a web-based platform for simulating, analyzing, and visualizing real-time CPU scheduling. It implements three scheduling algorithms — EDF, Round Robin, and a hybrid EDF+RR — and provides interactive Gantt chart visualization, performance metrics, algorithm comparison, and report generation.

### Core Capabilities

- **Scheduling Algorithms:** EDF, Round Robin, Hybrid EDF+RR (preemptive & non-preemptive)
- **Simulation:** Full-run or step-by-step execution with live Gantt chart updates
- **Algorithm Comparison:** Side-by-side execution of all 3 algorithms with recommendation scoring
- **Metrics:** Waiting time, turnaround time, response time, CPU utilization, throughput, context switches, deadline miss ratio
- **Reporting:** PDF (iTextSharp) and Excel (ClosedXML) export
- **Import:** CSV and Excel process bulk import with validation
- **Admin Panel:** User management, session/process management, audit logs, analytics dashboard
- **Authentication:** ASP.NET Core Identity with role-based access (Admin / User)

---

## 2. Architecture

The project follows a layered architecture with clear separation of concerns.

```
+------------------------------------------+
|         Presentation Layer                |
|   Controllers / Views / JS / CSS          |
+------------------------------------------+
|          Service Layer                    |
|   IProcessService   IReportService        |
|   ISimulationService  IComparisonService  |
|   IDashboardService  IAdminService        |
+------------------------------------------+
|         Repository Layer                  |
|   IProcessRepository  ISessionRepository  |
|   IResultRepository  IAdminRepository     |
+------------------------------------------+
|         Scheduling Engine                 |
|   SchedulingEngine -> IScheduler          |
|     +-- EDFScheduler                      |
|     +-- RRScheduler                       |
|     +-- HybridEDFRRScheduler              |
+------------------------------------------+
|          Data Layer                       |
|   ApplicationDbContext / SQL Server       |
+------------------------------------------+
```

### Design Patterns

| Pattern | Usage |
|---------|-------|
| **Repository** | Generic `IRepository<T>` with domain-specific extensions |
| **Service Layer** | Business logic encapsulated in service classes |
| **Strategy** | Interchangeable scheduling algorithms via `IScheduler` |
| **Dependency Injection** | All layers injected via ASP.NET Core DI container |
| **Unit of Work** | EF Core DbContext manages transactions |

---

## 3. Setup Guide

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| SQL Server | 2019+ (or LocalDB) |
| IDE | Visual Studio 2022 / VS Code |

### Installation Steps

1. **Clone the repository**

```bash
git clone https://github.com/yourusername/EDFRR.git
cd EDFRR
```

2. **Configure connection string**

Edit `EDFRR/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=EDFRR;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

3. **Restore packages**

```bash
dotnet restore
```

4. **Apply migrations**

```bash
dotnet ef database update --project EDFRR
```

5. **Run the application**

```bash
dotnet run --project EDFRR
```

6. **Access at** `https://localhost:5001` (or the port shown in the console output)

### Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@edfrr.com | Admin@123 |
| User | user@edfrr.com | User@123 |

### Database Initialization

On first startup, the application:
- Auto-migrates the database to the latest migration
- Seeds admin and user accounts with roles
- Executes all stored procedures from `Data/Sql/StoredProcedures.sql`

---

## 4. Project Structure

```
EDFRR/
+-- EDFRR/                             # Main web application project
|   +-- Areas/
|   |   +-- Admin/
|   |   |   +-- Controllers/           # Admin-only controllers
|   |   |   |   +-- AdminDashboardController.cs
|   |   |   |   +-- UserManagementController.cs
|   |   |   |   +-- ProcessManagementController.cs
|   |   |   |   +-- SessionManagementController.cs
|   |   |   |   +-- ActivityLogsController.cs
|   |   |   |   +-- ProcessController.cs        # Redirects to main area
|   |   |   |   +-- SchedulerComparisonRedirectController.cs
|   |   |   +-- Views/
|   |   +-- Identity/
|   |       +-- Pages/Account/         # Login, Register, Logout, AccessDenied
|   |
|   +-- Controllers/                   # Main area MVC controllers
|   |   +-- HomeController.cs
|   |   +-- DashboardController.cs
|   |   +-- SessionController.cs
|   |   +-- ProcessController.cs
|   |   +-- SimulationController.cs
|   |   +-- ReportController.cs
|   |   +-- SchedulerComparisonController.cs
|   |   +-- ApiController.cs
|   |
|   +-- Data/
|   |   +-- ApplicationDbContext.cs
|   |   +-- DataSeeder.cs
|   |   +-- Sql/StoredProcedures.sql
|   |
|   +-- Migrations/
|   |
|   +-- Models/
|   |   +-- Entities/                  # Database entity classes
|   |   |   +-- BaseEntity.cs          # Id, CreatedAt, UpdatedAt, IsDeleted
|   |   |   +-- ApplicationUser.cs     # Extends IdentityUser
|   |   |   +-- SchedulingSession.cs
|   |   |   +-- ProcessEntity.cs
|   |   |   +-- SchedulingResult.cs
|   |   |   +-- ExecutionLog.cs
|   |   |   +-- AlgorithmComparison.cs
|   |   |   +-- ActivityLog.cs
|   |   +-- DTOs/                      # Data Transfer Objects
|   |   |   +-- SchedulingDtos.cs
|   |   |   +-- ComparisonDtos.cs
|   |   |   +-- AdminDtos.cs
|   |   |   +-- AdminOperationsDtos.cs
|   |   +-- ViewModels/               # View-specific models
|   |       +-- ViewModels.cs
|   |       +-- AdminViewModels.cs
|   |       +-- ComparisonViewModels.cs
|   |       +-- ImportProcessViewModels.cs
|   |
|   +-- Repositories/
|   |   +-- Interfaces/
|   |   +-- Implementations/
|   |
|   +-- Services/
|   |   +-- Interfaces/
|   |   +-- Implementations/
|   |
|   +-- Scheduling/
|   |   +-- Engine/                    # SchedulingEngine.cs
|   |   +-- Interfaces/                # IScheduler, ISchedulingStrategy
|   |   +-- Models/                    # PCB, GanttEntry, SchedulingMetrics, etc.
|   |   +-- Strategies/               # EDF, RR, Hybrid implementations
|   |
|   +-- Views/
|   |   +-- Shared/                    # Layout, partial views
|   |   +-- Dashboard/
|   |   +-- Session/
|   |   +-- Process/
|   |   +-- Simulation/
|   |   +-- Report/
|   |   +-- SchedulerComparison/
|   |
|   +-- wwwroot/
|       +-- css/                       # 5 custom CSS files
|       +-- js/                        # 6 custom JS files
|       +-- lib/                       # Bootstrap, jQuery
|
+-- EDFRR.Tests/                       # Unit tests (xUnit)
+-- README.md
+-- DOCUMENTATION.md                   # This file
```

---

## 5. Data Models

### Entity Relationship Diagram

```
Users (ApplicationUser)
  |--< SchedulingSessions (1:N via UserId, SetNull on delete)
  |--< Processes (1:N via UserId, SetNull on delete)
  |--< AlgorithmComparisons (1:N via UserId)
  
SchedulingSessions
  |--< Processes (1:N via SchedulingSessionId, Cascade)
  |--< SchedulingResults (1:N via SchedulingSessionId, Cascade)
  |--< ExecutionLogs (1:N via SchedulingSessionId, Cascade)
  |--< AlgorithmComparisons (1:N via SchedulingSessionId, Cascade)

Identity: Roles, UserRoles, UserClaims, UserLogins, UserTokens, RoleClaims

ActivityLogs (standalone audit log)
```

### BaseEntity (abstract)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK, auto-increment) | |
| CreatedAt | DateTime | Default: UtcNow |
| UpdatedAt | DateTime? | Nullable |
| IsDeleted | bool | Soft delete flag |

### SchedulingSession (table: `SchedulingSessions`)

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int (PK) | Inherited from BaseEntity |
| Name | string(200) | Required |
| Description | string(1000)? | |
| AlgorithmType | string(50) | "Hybrid", "EDF", "RR" |
| TimeQuantum | int | Range 1-100, default 4 |
| Status | string(20) | "Created" / "Completed" |
| IsPreemptive | bool | Default true |
| UserId | string(450)? | FK -> Users |
| User | ApplicationUser? | Navigation |

### ProcessEntity (table: `Processes`)

| Column | Type | Constraints |
|--------|------|-------------|
| Id | int (PK) | Inherited from BaseEntity |
| Name | string(100) | Required |
| ProcessId | string(50) | Auto-generated "P001", "P002"... |
| ArrivalTime | int | >= 0 |
| BurstTime | int | > 0 |
| Deadline | int | > 0 |
| Priority | int | 0-10 |
| Status | string(20) | Default "Pending" |
| SchedulingSessionId | int | FK -> SchedulingSessions (Cascade) |
| UserId | string(450)? | FK -> Users (SetNull) |

**Unique index:** (SchedulingSessionId, ProcessId)

### SchedulingResult (table: `SchedulingResults`)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Inherited from BaseEntity |
| SchedulingSessionId | int | FK -> SchedulingSessions (Cascade) |
| ProcessId | string(100) | e.g., "P001" |
| ProcessName | string(100) | |
| ArrivalTime | int | |
| BurstTime | int | |
| Deadline | int | |
| CompletionTime | int | |
| TurnaroundTime | int | |
| WaitingTime | int | |
| ResponseTime | int | |
| StartTime | int | |
| EndTime | int | |
| IsMissedDeadline | bool | |
| GanttChartData | string(2000)? | JSON array |
| CpuUtilization | double | |
| Throughput | double | |
| ContextSwitchCount | int | |
| DeadlineMissRatio | double | |

### ExecutionLog (table: `ExecutionLogs`)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Inherited from BaseEntity |
| SchedulingSessionId | int | FK -> SchedulingSessions (Cascade) |
| TimeStep | int | |
| ExecutingProcessId | string(100) | |
| ExecutingProcessName | string(100) | |
| Action | string(50) | "Execute", "Preempt", "Complete", "Idle" |
| Details | string(500)? | |
| QueueState | int | Ready queue length |
| ReadyQueueSnapshot | string(2000)? | JSON |

### AlgorithmComparison (table: `AlgorithmComparisons`)

Stores side-by-side metrics for EDF, RR, and Hybrid for comparison results.

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| SessionId | int | FK -> SchedulingSessions |
| UserId | string(450)? | FK -> Users |
| All EDF_* metrics | double | 7 metric columns |
| All RR_* metrics | double | 7 metric columns |
| All Hybrid_* metrics | double | 7 metric columns |
| RecommendedAlgorithm | string(50) | Best algorithm name |
| RecommendationReason | string(500)? | Explanation |
| BestScore | double | Weighted score |

### ActivityLog (table: `ActivityLogs`)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string(450)? | FK -> Users |
| Action | string(100) | e.g., "UserLogin", "SessionCreated" |
| Description | string(1000) | |
| IPAddress | string(50)? | |
| CreatedAt | DateTime | Default: UtcNow |

### ApplicationUser (extends IdentityUser)

| Column | Type | Notes |
|--------|------|-------|
| FirstName | string(100) | Required |
| LastName | string(100) | Required |
| CreatedAt | DateTime | UtcNow |
| LastLoginAt | DateTime? | |
| IsActive | bool | Default true |
| ProfilePictureUrl | string(500)? | |
| Processes | ICollection | Nav |
| SchedulingSessions | ICollection | Nav |

---

## 6. Repository Layer

### Generic Repository

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
```

### Domain Repositories

| Repository | Key Additional Methods |
|------------|----------------------|
| **IProcessRepository** | `GetBySessionIdAsync`, `GetByUserIdAsync`, `GetBySessionAndProcessIdAsync`, `GetProcessesPagedAsync`, `CountFilteredAsync`, `GetMaxProcessIdNumberAsync` |
| **ISessionRepository** | `GetByUserIdAsync`, `GetWithProcessesAsync`, `GetWithResultsAsync`, `GetFullAsync`, `GetSessionsPagedAsync`, `CountFilteredAsync` |
| **IResultRepository** | `GetBySessionIdAsync`, `ClearResultsForSessionAsync` |
| **IExecutionLogRepository** | `GetBySessionIdAsync`, `ClearLogsForSessionAsync` |
| **IComparisonRepository** | `GetBySessionIdAsync`, `GetByUserIdAsync`, `ClearForSessionAsync` |
| **IActivityLogRepository** | `LogAsync`, `GetRecentAsync` |
| **IAdminRepository** | Uses 23 stored procedures for admin operations (user/process/session CRUD, dashboard stats) |

---

## 7. Service Layer

| Service | Key Methods | Purpose |
|---------|------------|---------|
| **IProcessService** | Create, GetPaged, Update, Delete, BulkCreate, GenerateProcessId | Process CRUD + ID generation |
| **ISessionService** | Create, GetPaged, Update, Delete | Session CRUD |
| **ISimulationService** | ExecuteStep, ExecuteFullSimulation, SaveResults, GetExecutionLogs | Simulation engine integration |
| **IDashboardService** | GetDashboardData, GetDashboardDataForSession, GetAdminDashboardData | Dashboard aggregation |
| **IReportService** | GenerateReport, GeneratePdfReport, GenerateExcelReport | Report generation |
| **IProcessImportService** | ParseCsv, ParseExcel, ValidateRows, SaveValidRows, GenerateSampleCsv/Excel | File import |
| **IComparisonService** | CompareAlgorithms, GetComparison, GetChartData, GetExportData | Algorithm comparison with scoring |
| **IAdminService** | User CRUD, role management, bulk actions, export | Admin operations |
| **IAdminDashboardService** | GetFullDashboard, GetDashboardStats, RecentUsers, Trends | Admin analytics |
| **IActivityLogService** | LogAsync, GetRecentAsync | Activity audit logging |

### Comparison Scoring Formula

```
Score = (20% × WaitingTime) + (20% × TurnaroundTime) + (15% × ResponseTime)
      + (20% × CPUUtilization) + (10% × Throughput) + (15% × DeadlineSuccess)
```

Lower is better for time metrics; higher is better for CPU utilization, throughput, and deadline success.

---

## 8. Scheduling Engine

### Architecture

```
SchedulingEngine (Singleton)
  |-- CreateScheduler(algorithmType, timeQuantum, isPreemptive) -> IScheduler
  |-- RunSimulation(processes, algorithm, quantum, preemptive) -> SchedulingResult
  |-- CalculateMetrics(context) -> SchedulingMetrics
  
IScheduler (interface)
  +-- Execute(context) -> SchedulingContext (populated with Gantt chart, steps, metrics)

ISchedulingStrategy (interface)
  +-- SelectNextProcess(readyQueue, currentTime, isPreemptive) -> PCB?
  
Implementations:
  EDFScheduler          -> uses EDFStrategy (earliest absolute deadline)
  RRScheduler           -> uses RRStrategy (FCFS queue with time quantum)
  HybridEDFRRScheduler  -> uses HybridEDFRRStrategy (EDF + RR per deadline group)
```

### ProcessControlBlock (PCB)

| Property | Type | Description |
|----------|------|-------------|
| ProcessId | string | "P001" format |
| ProcessName | string | Display name |
| ArrivalTime | int | Time process enters system |
| BurstTime | int | Total CPU time needed |
| RemainingTime | int | Remaining CPU time |
| Deadline | int | Relative deadline |
| Priority | int | 0-10 priority level |
| CompletionTime | int | Time of completion |
| FirstExecutionTime | int | First time slice start |
| State | string | New -> Ready -> Running -> Terminated |
| IsCompleted | bool | |
| MissedDeadline | bool | |
| TurnaroundTime | int | Computed: CompletionTime - ArrivalTime |
| WaitingTime | int | Computed: TurnaroundTime - BurstTime |
| ResponseTime | int | Computed: FirstExecutionTime - ArrivalTime |
| AbsoluteDeadline | int | Computed: ArrivalTime + Deadline |

### EDF Algorithm

1. At each time step, add newly arrived processes to the ready queue
2. Select the process with the earliest **absolute deadline** (`ArrivalTime + Deadline`)
3. **Preemptive mode:** If a newly arrived process has an earlier deadline than the currently running process, preempt the current process
4. **Non-preemptive mode:** Run to completion or until blocked
5. Mark process as missed if `CompletionTime > AbsoluteDeadline`

### Round Robin Algorithm

1. Maintain a FIFO ready queue
2. Execute each process for a fixed **time quantum** (configurable)
3. If a process does not complete within its quantum, move it to the back of the queue
4. New arrivals are added at the back of the queue
5. Repeat until all processes complete

### Hybrid EDF+RR Algorithm

1. Group ready processes by their absolute deadline
2. **Single-process deadline group:** Schedule by EDF (earliest deadline first)
3. **Multi-process deadline group:** Apply Round Robin within the group for fairness
4. Track RR rotation per deadline group to ensure equal CPU time
5. Preempt when a new process with an earlier deadline arrives
6. Combines EDF's optimal deadline handling with RR's fairness

### Metrics Calculation

| Metric | Formula |
|--------|---------|
| Avg Waiting Time | Sum of all processes' waiting times / Total processes |
| Avg Turnaround Time | Sum of turnaround times / Total processes |
| Avg Response Time | Sum of response times / Total processes |
| CPU Utilization | (Busy time / Total time) × 100 |
| Throughput | Completed processes / Total time |
| Context Switch Count | Total number of context switches |
| Missed Deadlines | Count of processes where CompletionTime > AbsoluteDeadline |
| Deadline Miss Ratio | (Missed / Total) × 100 |

---

## 9. Controllers & Routes

### Main Area (no area prefix)

| Controller | Route | Auth | Actions |
|------------|-------|------|---------|
| Home | `/` or `/Home/{action}` | None | Index, Privacy, About |
| Dashboard | `/Dashboard/{action}` | `[Authorize]` | Index(sessionId?) |
| Session | `/Session/{action}` | `[Authorize]` | Index, Create, Edit, Delete |
| Process | `/Process/{action}` | `[Authorize]` | Index, Create, Edit, Delete, BulkCreate, Import, ImportResults, DownloadSampleCsv/Excel |
| Simulation | `/Simulation/{action}` | `[Authorize]` | Index, RunFull, RunStep, Reset |
| SchedulerComparison | `/SchedulerComparison/{action}` | `[Authorize]` | Index, RunComparison, GetComparisonChartData, ExportComparisonReport, Delete |
| Report | `/Report/{action}` | `[Authorize]` | Index(sessionId?), Export(sessionId, format) |
| Api | `/api/Api/{action}` | `[Authorize]` | GetDashboard, GetSession, GetProcesses, RunSimulation, RunStep |

### Admin Area (`/Admin/{controller}/{action}`)

| Controller | Route | Auth | Actions |
|------------|-------|------|---------|
| AdminDashboard | `/Admin/AdminDashboard` | Admin | Index + AJAX stat endpoints |
| UserManagement | `/Admin/UserManagement` | Admin | Index, Create, Details, Edit, Delete, Lock/Unlock, ResetPassword, BulkAction, ExportExcel/Pdf |
| ProcessManagement | `/Admin/ProcessManagement` | Admin | Index, GetFilterOptions, Details, Delete, BulkAction, ExportExcel/Pdf |
| SessionManagement | `/Admin/SessionManagement` | Admin | Index, GetFilterOptions, Details, Delete, BulkAction, ExportExcel/Pdf |
| ActivityLogs | `/Admin/ActivityLogs` | Admin | Index, GetRecent |
| Process (redirector) | `/Admin/Process` | `[Authorize]` | Import, ImportResults — redirects to main area |
| SchedulerComparison (redirector) | `/Admin/SchedulerComparison` | `[Authorize]` | Index — redirects to main area |

---

## 10. View Layer

### Layout & Theme

- **Layout:** `Views/Shared/_Layout.cshtml` — full sidebar + topnav layout
- **Theme system:** CSS custom properties with light/dark mode toggle (`edfrr-theme.js`)
- **Dark mode:** Persisted in localStorage, applied via `data-theme` attribute on `<html>`

### Shared Partials

| Partial | Purpose |
|---------|---------|
| `_AlgorithmSelector` | Pill-style 3-way algorithm toggle (Hybrid / EDF / RR) |
| `_GanttChart` | Canvas-based Gantt chart rendering |
| `_MetricCard` | Label + value + trend indicator |
| `_StatusBadge` | Color-coded process status badge |
| `_Toast` | Notification toast with auto-dismiss |
| `_ValidationScriptsPartial` | jQuery Validation + Unobtrusive |

### Custom CSS Files (wwwroot/css/)

| File | Purpose |
|------|---------|
| `edfrr-theme.css` | CSS custom properties, light/dark theme variables |
| `edfrr-layout.css` | Topnav, sidebar, page layout |
| `edfrr-components.css` | Cards, buttons, tables, badges, forms, modals |
| `edfrr-overrides.css` | Bootstrap overrides, DataTables, SweetAlert2 |
| `site.css` | Legacy styles |

### Custom JS Files (wwwroot/js/)

| File | Purpose |
|------|---------|
| `edfrr-theme.js` | Theme toggle, prefers-color-scheme detection |
| `edfrr-ui.js` | Sidebar interaction, toast notifications, confirmation dialogs |
| `edfrr-charts.js` | Chart.js initialization for dashboard charts |
| `edfrr-simulation.js` | Simulation step execution, polling, Gantt chart updates |
| `gantt-chart.js` | Canvas-based Gantt chart rendering |
| `site.js` | Legacy JS |

### CDN Dependencies

- **Google Fonts:** Inter (UI), JetBrains Mono (code)
- **Bootstrap 5.3** (CSS + JS bundle)
- **Font Awesome 6.5** (Free)
- **Chart.js 4.4**
- **DataTables 2.0** (with Bootstrap 5 integration)
- **SweetAlert2 11**

---

## 11. Admin Panel

Accessible at `/Admin/AdminDashboard` for users in the **Admin** role.

### Sections

#### Dashboard
- System-wide totals (users, sessions, processes, comparisons)
- Monthly registration trend (bar chart)
- Algorithm usage distribution (pie chart)
- 30-day session creation trend (line chart)
- Recent users and sessions tables

#### User Management
- DataTable-driven user list with search, role/status filters
- Create/edit user with profile fields + role assignment
- User detail view with activity log
- Lock/unlock, password reset
- Bulk actions (delete, lock, unlock)
- Export to Excel/PDF

#### Process Management
- DataTable-driven process list with multi-filter
- Process detail with scheduling result metrics
- Soft delete (single/bulk)
- Export to Excel/PDF

#### Session Management
- DataTable-driven session list with filters
- Session detail with aggregate metrics + process results + Gantt chart
- Soft delete (single/bulk)
- Export to Excel/PDF

#### Activity Logs
- Audit trail of all admin actions
- Recent activity view with user names, actions, timestamps

### DataTables Configuration
- Server-side processing (pagination, sorting, searching)
- AJAX endpoints returning JSON with draw/recordsTotal/recordsFiltered/data format
- 23 stored procedures power the admin data operations

---

## 12. API Endpoints

### REST API (`/api/Api/{action}`)

All endpoints require `[Authorize]` authentication cookie.

| Endpoint | Method | Parameters | Returns |
|----------|--------|------------|---------|
| GetDashboard | GET | — | DashboardDto with metrics + charts |
| GetSession | GET | id | SessionDto with process count |
| GetProcesses | GET | sessionId | List of ProcessDto |
| RunSimulation | POST | sessionId | SimulationStepDto (final) |
| RunStep | POST | sessionId, timeStep, isFullRun? | SimulationStepDto |

---

## 13. Database Schema

### Tables

| Table | Schema | Description |
|-------|--------|-------------|
| AspNetUsers | dbo | Identity users (extends with custom fields) |
| AspNetRoles | dbo | Identity roles |
| AspNetUserRoles | dbo | User-role assignments |
| AspNetRoleClaims | dbo | Role claims |
| AspNetUserClaims | dbo | User claims |
| AspNetUserLogins | dbo | External login providers |
| AspNetUserTokens | dbo | Authentication tokens |
| SchedulingSessions | dbo | Simulation configurations |
| Processes | dbo | Process definitions |
| SchedulingResults | dbo | Per-process simulation results |
| ExecutionLogs | dbo | Step-by-step execution history |
| AlgorithmComparisons | dbo | Algorithm comparison results |
| ActivityLogs | dbo | Audit log entries |
| __EFMigrationsHistory | dbo | EF Core migrations |

### Foreign Key Relationships

```
Processes.SchedulingSessionId -> SchedulingSessions.Id (CASCADE)
Processes.UserId -> AspNetUsers.Id (SET NULL)

SchedulingSessions.UserId -> AspNetUsers.Id (SET NULL)

SchedulingResults.SchedulingSessionId -> SchedulingSessions.Id (CASCADE)

ExecutionLogs.SchedulingSessionId -> SchedulingSessions.Id (CASCADE)

AlgorithmComparisons.SessionId -> SchedulingSessions.Id (CASCADE)
AlgorithmComparisons.UserId -> AspNetUsers.Id (NO ACTION)
```

---

## 14. Stored Procedures

The `Data/Sql/StoredProcedures.sql` file contains 23 stored procedures used primarily by the admin panel via `AdminRepository`.

### User Management (9 SPs)

| Procedure | Purpose |
|-----------|---------|
| `sp_GetUsersPaged` | Paginated user listing with sort, search, role/status filters |
| `sp_GetUserDetails` | Full user detail with counts + recent activities |
| `sp_UpdateUser` | Update user first/last name, email, phone |
| `sp_DeleteUser` | Soft delete (IsActive=0, lockout 100yr) |
| `sp_LockUser` | Lock user account |
| `sp_UnlockUser` | Unlock user account |
| `sp_BulkDeleteUsers` | Bulk soft delete via STRING_SPLIT |
| `sp_BulkLockUsers` | Bulk lock multiple users |
| `sp_BulkUnlockUsers` | Bulk unlock multiple users |

### Process Management (3 SPs)

| Procedure | Purpose |
|-----------|---------|
| `sp_GetProcessesPaged` | Paginated process listing with multiple filters |
| `sp_GetProcessDetails` | Process detail with scheduling result metrics |
| `sp_DeleteProcess` / `sp_BulkDeleteProcesses` | Single/bulk soft delete |

### Session Management (4 SPs)

| Procedure | Purpose |
|-----------|---------|
| `sp_GetSessionsPaged` | Paginated session listing with filters |
| `sp_GetSessionDetails` | Full session detail with aggregate metrics + process results + Gantt chart |
| `sp_DeleteSession` / `sp_BulkDeleteSessions` | Single/bulk soft delete |

### Dashboard & Activity (7 SPs)

| Procedure | Purpose |
|-----------|---------|
| `sp_GetDashboardStats` | Aggregate totals, monthly registrations, algorithm usage, 30-day trends |
| `sp_GetRecentUsers` | Latest N users with roles |
| `sp_GetRecentSessions` | Latest N sessions with process count + creator |
| `sp_GetAlgorithmUsage` | Algorithm distribution stats |
| `sp_GetSessionTrend` | Session creation trend (last 30 days) |
| `sp_GetRecentActivities` | Latest N activity logs with user names |

---

## 15. Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=EDFRR;..."
  },
  "ApplicationSettings": {
    "CompanyName": "EDFRR",
    "ItemsPerPage": 10,
    "DefaultTimeQuantum": 4
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "...",
          "tableName": "Logs",
          "autoCreateSqlTable": true
        }
      }
    ]
  }
}
```

### Program.cs Pipeline Order

1. Exception handler (non-dev) / Developer exception page (dev)
2. HSTS (non-dev)
3. HTTPS redirection
4. Static files
5. Routing
6. Authentication
7. Authorization
8. Map area routes (`Admin/{controller=AdminDashboard}/{action=Index}/{id?}`)
9. Map default routes (`{controller=Home}/{action=Index}/{id?}`)

### Identity Configuration

- Password: 6+ chars, 1 digit, 1 uppercase, 1 lowercase
- Lockout: 5 failed attempts = 5 min lockout
- Cookie: Login path = `/Identity/Account/Login`
- Access denied path = `/Identity/Account/AccessDenied`

---

## 16. Security & Authentication

### Authentication Flow

1. User navigates to a protected page
2. Unauthenticated users are redirected to `/Identity/Account/Login`
3. After login, Identity cookie is issued
4. Role-based checks use `[Authorize(Roles = "Admin")]` on admin controllers
5. Admin-only views are conditionally rendered in the sidebar

### Authorization Rules

| Resource | Access |
|----------|--------|
| Home, About, Privacy | Public (no auth) |
| Dashboard, Session, Process, Simulation, Compare, Report | Authenticated users |
| Admin area (`/Admin/*`) | Authenticated users with "Admin" role |
| API endpoints | Authenticated users |

### Seeded Data

| User | Role | Details |
|------|------|---------|
| admin@edfrr.com | Admin | Full system access |
| user@edfrr.com | User | Standard user access |

---

## 17. Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.6 | Identity with EF Core |
| Microsoft.AspNetCore.Identity.UI | 8.0.6 | Identity Razor UI |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.6 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.6 | CLI tooling |
| Microsoft.EntityFrameworkCore.Design | 8.0.6 | Design-time support |
| Microsoft.Extensions.Logging.Debug | 8.0.0 | Debug logging |
| iTextSharp.LGPLv2.Core | 3.4.22 | PDF generation |
| ClosedXML | 0.102.3 | Excel generation |
| CsvHelper | 33.1.0 | CSV parsing |
| Serilog.AspNetCore | 8.0.1 | Structured logging |
| Serilog.Sinks.MSSqlServer | 6.6.0 | DB log sink |

### Client-Side Libraries (CDN)

| Library | Version |
|---------|---------|
| Bootstrap | 5.3.x |
| Font Awesome | 6.5.x |
| Chart.js | 4.4.x |
| DataTables | 2.0.x |
| SweetAlert2 | 11.x |
| jQuery | 3.x |
| jQuery Validation | 1.x |

---

## 18. Browser Support

- Chrome 90+
- Firefox 90+
- Edge 90+
- Safari 15+ (macOS/iOS)

The dark mode toggle relies on `prefers-color-scheme` media query and CSS custom properties.
