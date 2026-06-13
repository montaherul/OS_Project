# EDFRR — Adaptive Hybrid CPU Scheduling Framework

**ASP.NET Core 8 MVC** | **SQL Server** | **Strategy Pattern** | **Clean Architecture**

A web-based platform for simulating, analyzing, and visualizing real-time CPU scheduling using a hybrid EDF+RR algorithm with interactive Gantt charts, performance metrics, algorithm comparison, and reporting.

---

## Quick Start

```bash
# 1. Clone
git clone https://github.com/yourusername/EDFRR.git
cd EDFRR

# 2. Update connection string in EDFRR/appsettings.json

# 3. Restore & run
dotnet restore
dotnet ef database update --project EDFRR
dotnet run --project EDFRR
```

**Default credentials:**  
- Admin: `admin@edfrr.com` / `Admin@123`  
- User: `user@edfrr.com` / `User@123`

---

## Features

- **3 scheduling algorithms** — EDF, Round Robin, Hybrid EDF+RR (preemptive/non-preemptive)
- **Interactive simulation** — Step-by-step or full-run with live Gantt chart
- **Algorithm comparison** — Side-by-side execution with weighted scoring & recommendation
- **Performance metrics** — Wait/turnaround/response time, CPU utilization, throughput, deadline miss ratio
- **Reporting** — PDF (iTextSharp) and Excel (ClosedXML) export
- **Process import** — CSV and Excel bulk import with validation
- **Admin panel** — User/process/session management, audit logs, analytics dashboard
- **Dark mode** — Theme toggle persisted in localStorage

---

## Architecture

```
Presentation Layer  →  Service Layer  →  Repository Layer
                         ↕
                    Scheduling Engine
                    (Strategy Pattern)
```

| Pattern | Usage |
|---------|-------|
| Repository | Generic `IRepository<T>` + domain-specific extensions |
| Service Layer | Business logic encapsulation |
| Strategy | Interchangeable schedulers (EDF / RR / Hybrid) |
| DI | All layers injected via ASP.NET Core container |

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| Backend | ASP.NET Core 8 MVC (C#) |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Frontend | Bootstrap 5, Chart.js 4, DataTables 2, SweetAlert2 |
| PDF | iTextSharp (LGPL) |
| Excel | ClosedXML |
| Logging | Serilog (Console + SQL Server) |
| Auth | ASP.NET Core Identity |
| CSV | CsvHelper |

---

## Project Structure

```
EDFRR/
+-- Controllers/                  # Main area MVC controllers
+-- Areas/Admin/Controllers/      # Admin-only controllers
+-- Areas/Identity/Pages/         # Login, Register, Logout
+-- Models/
|   +-- Entities/                 # Database entities (7 classes)
|   +-- DTOs/                     # Data transfer objects
|   +-- ViewModels/               # View-specific models
+-- Data/
|   +-- ApplicationDbContext.cs   # EF Core context
|   +-- DataSeeder.cs             # Seed users & roles
|   +-- Sql/StoredProcedures.sql  # 23 admin SPs
+-- Repositories/                 # Generic + domain repositories
+-- Services/                     # Business logic (10 services)
+-- Scheduling/                   # Core engine + strategies
|   +-- Engine/SchedulingEngine.cs
|   +-- Strategies/ (EDF, RR, Hybrid)
|   +-- Models/ (PCB, GanttEntry, Metrics, Context)
+-- Views/                        # Razor views + partials
+-- wwwroot/css/                  # 5 custom CSS files
+-- wwwroot/js/                   # 6 custom JS files
```

---

## Scheduling Algorithms

| Algorithm | Strategy | Key Behavior |
|-----------|----------|-------------|
| **EDF** | Earliest deadline first | Preemptive: preempt on earlier-deadline arrival |
| **Round Robin** | FCFS + time quantum | Fair time-slicing, configurable quantum |
| **Hybrid EDF+RR** | EDF + RR per deadline group | EDF for unique deadlines, RR for ties |

**Scoring formula for comparisons:**

```
Score = 20% Wait + 20% Turnaround + 15% Response
      + 20% CPU Util + 10% Throughput + 15% Deadline Success
```

---

## Reports & Data Flow

1. **Create session** → choose algorithm, time quantum, preemptive mode
2. **Add processes** — single, bulk generate, or CSV/Excel import
3. **Run simulation** — step-by-step or full-run with real-time Gantt chart
4. **Compare algorithms** → runs all 3, recommends best
5. **Export** — PDF or Excel with metrics, process table, execution log

---

## See Also

- **[Full Documentation](DOCUMENTATION.md)** — Complete reference: entities, routes, services, scheduling engine, stored procedures, admin panel, API
- **`Services/Implementations/ReportService.cs`** — PDF/Excel export implementation
- **`Scheduling/`** — Scheduling engine with strategy pattern

---

## License

Academic project — Final Year Project.
