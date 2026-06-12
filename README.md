# EDFRR - Adaptive Hybrid CPU Scheduling Framework

## Abstract

EDFRR (Earliest Deadline First + Round Robin) is a web-based platform that simulates, analyzes, and visualizes real-time CPU scheduling using a hybrid algorithm that combines Earliest Deadline First (EDF) and Round Robin (RR) algorithms. The system allows users to create processes, run scheduling simulations, visualize execution using Gantt Charts, compare performance metrics, generate reports, and analyze scheduling efficiency.

---

## Table of Contents

1. [Introduction](#introduction)
2. [Problem Statement](#problem-statement)
3. [Objectives](#objectives)
4. [Scope](#scope)
5. [Technology Stack](#technology-stack)
6. [Architecture](#architecture)
7. [Database Design](#database-design)
8. [Module Description](#module-description)
9. [Scheduling Algorithms](#scheduling-algorithms)
10. [Installation](#installation)
11. [Usage](#usage)
12. [Testing](#testing)
13. [Future Enhancements](#future-enhancements)

---

## Introduction

Real-time operating systems require efficient CPU scheduling algorithms that can handle time-critical processes with strict deadlines. Traditional algorithms like Round Robin provide fairness but ignore deadlines, while EDF prioritizes deadlines but may lead to starvation. This project implements a hybrid approach that combines the strengths of both algorithms.

---

## Problem Statement

Existing CPU scheduling implementations often focus on a single algorithm, lacking:
- Comparative analysis between different scheduling strategies
- Real-time visualization of scheduling decisions
- Interactive simulation with step-by-step execution
- Performance metrics comparison
- Deadline miss detection and reporting

---

## Objectives

1. Implement EDF, Round Robin, and Hybrid EDF+RR scheduling algorithms
2. Create an interactive simulation environment with step-by-step execution
3. Visualize scheduling results using Gantt charts
4. Calculate and display performance metrics (waiting time, turnaround time, CPU utilization, throughput)
5. Generate PDF and Excel reports
6. Support both preemptive and non-preemptive scheduling modes
7. Detect and report missed deadlines

---

## Scope

### In Scope
- Process creation and management
- Scheduling session configuration
- Algorithm execution (EDF, RR, Hybrid)
- Gantt chart visualization
- Performance metrics calculation
- Report generation (PDF, Excel)
- User authentication and authorization
- Dark mode support

### Out Scope
- Multi-core processor scheduling
- Memory management
- I/O scheduling
- Network scheduling

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| Backend | ASP.NET Core MVC (.NET 8) |
| Language | C# |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Frontend | HTML5, CSS3, Bootstrap 5, JavaScript |
| Charts | Chart.js |
| PDF Export | iTextSharp |
| Excel Export | ClosedXML |
| Testing | xUnit, Moq, FluentAssertions |
| Authentication | ASP.NET Core Identity |

---

## Architecture

The project follows **Clean Architecture** with the following layers:

```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚        Presentation Layer       â”‚
â”‚   (Controllers, Views, JS)      â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚         Service Layer           â”‚
â”‚  (Business Logic, DTOs)         â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚      Repository Layer           â”‚
â”‚  (Data Access, EF Core)         â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚      Scheduling Engine          â”‚
â”‚  (EDF, RR, Hybrid Algorithms)   â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚       Database Layer            â”‚
â”‚     (SQL Server, Migrations)    â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Design Patterns Used
- **Repository Pattern** - Data access abstraction
- **Service Layer Pattern** - Business logic encapsulation
- **Strategy Pattern** - Interchangeable scheduling algorithms
- **Dependency Injection** - Loose coupling
- **Unit of Work** - Transaction management

---

## Database Design

### Entity Relationship Diagram

```
Users â”€â”€â”€â”€â”€â”€< SchedulingSessions â”€â”€â”€â”€â”€â”€< Processes
                    â”‚
                    â”œâ”€â”€â”€â”€< SchedulingResults
                    â”‚
                    â””â”€â”€â”€â”€< ExecutionLogs
```

### Tables

| Table | Description |
|-------|-------------|
| AspNetUsers | User accounts (extends IdentityUser) |
| SchedulingSessions | Scheduling configurations |
| Processes | Process definitions per session |
| SchedulingResults | Simulation results per process |
| ExecutionLogs | Step-by-step execution history |

---

## Module Description

### 1. Authentication Module
- User registration and login
- Role-based access control (Admin, User)
- Session management

### 2. Dashboard Module
- Process statistics (total, active, completed, missed deadlines)
- CPU utilization and throughput metrics
- Process and session performance charts

### 3. Process Configuration Module
- CRUD operations for processes
- Bulk process generation
- Search, filter, and pagination

### 4. Scheduling Session Module
- Create/edit/delete sessions
- Configure algorithm type (EDF, RR, Hybrid)
- Set time quantum and preemption mode

### 5. Simulation Module
- Full simulation execution
- Step-by-step execution
- Real-time Gantt chart visualization
- Live process state tracking

### 6. Performance Analysis Module
- Waiting time, turnaround time, response time
- CPU utilization, throughput
- Context switch count
- Deadline miss ratio

### 7. Reporting Module
- PDF report generation
- Excel report generation
- Export options per session

---

## Scheduling Algorithms

### Earliest Deadline First (EDF)
- Selects the process with the nearest deadline
- Supports preemptive and non-preemptive modes
- Optimal for dynamic priority systems

### Round Robin (RR)
- Time-quantum based fair scheduling
- Processes are executed in circular order
- Configurable time quantum

### Hybrid EDF + RR
- Combines EDF priority with RR fairness
- When deadlines are identical, applies Round Robin
- Best of both worlds for real-time systems

### Algorithm Flowchart

```
Start
  â”‚
  â–¼
Arrive processes to Ready Queue
  â”‚
  â–¼
Select process by EDF (earliest deadline)
  â”‚
  â”œâ”€â”€ If unique deadline â”€â”€â–º Execute until completion or preemption
  â”‚
  â””â”€â”€ If multiple same deadlines â”€â”€â–º Apply Round Robin
  â”‚
  â–¼
Check completion
  â”‚
  â”œâ”€â”€ Completed â”€â”€â–º Calculate metrics, terminate
  â”‚
  â””â”€â”€ Not completed â”€â”€â–º Back to Ready Queue
  â”‚
  â–¼
Check for missed deadlines
  â”‚
  â–¼
Repeat until all processes complete
```

---

## Installation

### Prerequisites
- .NET 8 SDK
- SQL Server (or LocalDB)
- Visual Studio 2022 or VS Code

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/EDFRR.git
   cd EDFRR
   ```

2. **Update connection string**
   Edit `EDFRR/appsettings.json` and update the `DefaultConnection` string.

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

6. **Access the application**
   Open `https://localhost:5001` in your browser.

### Default Credentials
- **Admin:** admin@edfrr.com
- **User:** user@edfrr.com

---

## Usage

1. **Register/Login** to access the application
2. **Create a Scheduling Session** with algorithm type and time quantum
3. **Add Processes** with arrival time, burst time, and deadline
4. **Run Simulation** to execute the scheduling algorithm
5. **View Results** including Gantt chart and performance metrics
6. **Generate Reports** in PDF or Excel format

---

## Testing

### Run Unit Tests
```bash
dotnet test EDFRR.Tests
```

### Test Coverage
- Scheduling algorithm tests (EDF, RR, Hybrid)
- Repository tests
- Service tests
- Integration tests with InMemory database

---

## Project Structure

```
EDFRR/
â”œâ”€â”€ EDFRR/                          # Main Web Project
â”‚   â”œâ”€â”€ Controllers/                # MVC Controllers
â”‚   â”œâ”€â”€ Models/
â”‚   â”‚   â”œâ”€â”€ Entities/               # Database Entities
â”‚   â”‚   â”œâ”€â”€ ViewModels/             # View Models
â”‚   â”‚   â””â”€â”€ DTOs/                   # Data Transfer Objects
â”‚   â”œâ”€â”€ Data/                       # DbContext & Migrations
â”‚   â”œâ”€â”€ Repositories/               # Data Access Layer
â”‚   â”œâ”€â”€ Services/                   # Business Logic Layer
â”‚   â”œâ”€â”€ Scheduling/                 # Scheduling Algorithms
â”‚   â”‚   â”œâ”€â”€ Models/                 # PCB, GanttEntry, Metrics
â”‚   â”‚   â”œâ”€â”€ Strategies/             # EDF, RR, Hybrid Implementations
â”‚   â”‚   â””â”€â”€ Engine/                 # Scheduling Engine
â”‚   â”œâ”€â”€ Views/                      # Razor Views
â”‚   â”œâ”€â”€ Areas/Identity/             # Identity Pages
â”‚   â””â”€â”€ wwwroot/                    # Static Files
â”œâ”€â”€ EDFRR.Tests/                    # Unit Tests
â””â”€â”€ README.md
```

---

## Future Enhancements

1. **SignalR Integration** for real-time simulation updates
2. **Multi-core Scheduling** support
3. **Machine Learning** based priority prediction
4. **Custom Algorithm** creation interface
5. **API Endpoints** for external integrations
6. **Docker Containerization**
7. **CI/CD Pipeline** setup
8. **Load Testing** and performance optimization

---

## References

1. Silberschatz, A., Galvin, P. B., & Gagne, G. (2018). *Operating System Concepts* (10th ed.).
2. Tanenbaum, A. S., & Bos, H. (2014). *Modern Operating Systems* (4th ed.).
3. Microsoft. (2024). ASP.NET Core Documentation.
4. Entity Framework Core Documentation.

---

## License

This project is developed as a Final Year Project for academic purposes.

---

**Developed using ASP.NET Core MVC (.NET 8) with Clean Architecture**
