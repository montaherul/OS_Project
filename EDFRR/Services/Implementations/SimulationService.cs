using EDFRR.Models.DTOs;
using EDFRR.Models.Entities;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Scheduling.Engine;
using EDFRR.Scheduling.Models;
using EDFRR.Services.Interfaces;

namespace EDFRR.Services.Implementations;

public class SimulationService : ISimulationService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IProcessRepository _processRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IExecutionLogRepository _executionLogRepository;
    private readonly SchedulingEngine _engine;

    public SimulationService(
        ISessionRepository sessionRepository,
        IProcessRepository processRepository,
        IResultRepository resultRepository,
        IExecutionLogRepository executionLogRepository,
        SchedulingEngine engine)
    {
        _sessionRepository = sessionRepository;
        _processRepository = processRepository;
        _resultRepository = resultRepository;
        _executionLogRepository = executionLogRepository;
        _engine = engine;
    }

    public async Task<SimulationViewModel> InitializeSimulationAsync(int sessionId)
    {
        var session = await _sessionRepository.GetWithProcessesAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found.");

        var processes = await _processRepository.GetBySessionIdAsync(sessionId);

        return new SimulationViewModel
        {
            SessionId = session.Id,
            SessionName = session.Name,
            AlgorithmType = session.AlgorithmType,
            TimeQuantum = session.TimeQuantum,
            IsPreemptive = session.IsPreemptive,
            Processes = processes.Select(MapToDto).ToList(),
            IsRunning = false,
            IsPaused = false,
            IsComplete = false
        };
    }

    public async Task<SimulationStepDto> ExecuteStepAsync(int sessionId, int currentTimeStep)
    {
        var session = await _sessionRepository.GetWithProcessesAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found.");

        var processes = await _processRepository.GetBySessionIdAsync(sessionId);
        var pcbList = processes.Select(MapToPcb).ToList();

        var context = new SchedulingContext
        {
            Processes = pcbList,
            AlgorithmType = session.AlgorithmType,
            TimeQuantum = session.TimeQuantum,
            IsPreemptive = session.IsPreemptive,
            CurrentTime = currentTimeStep
        };

        var result = _engine.RunSimulation(pcbList, session.AlgorithmType, session.TimeQuantum, session.IsPreemptive);
        var metrics = result.Metrics;
        var ganttChart = result.Context.GanttChart.Select(g => new GanttChartDto
        {
            ProcessId = g.ProcessId,
            ProcessName = g.ProcessName,
            StartTime = g.StartTime,
            EndTime = g.EndTime,
            IsContextSwitch = g.IsContextSwitch
        }).ToList();

        var currentGantt = ganttChart.Where(g => g.StartTime <= currentTimeStep && g.EndTime > currentTimeStep).ToList();

        var processStates = result.Context.Processes.Select(p => new ProcessStateDto
        {
            ProcessId = p.ProcessId,
            ProcessName = p.ProcessName,
            State = p.IsCompleted ? "Completed" : (p.HasStarted ? "Ready" : "New"),
            RemainingTime = p.RemainingTime,
            Deadline = p.Deadline,
            MissedDeadline = p.MissedDeadline
        }).ToList();

        var step = new SimulationStepDto
        {
            TimeStep = currentTimeStep,
            ExecutingProcess = result.Context.ExecutionSteps.FirstOrDefault(s => s.TimeStep == currentTimeStep)?.ExecutingProcessName,
            ReadyQueue = result.Context.ExecutionSteps.FirstOrDefault(s => s.TimeStep == currentTimeStep)?.ReadyQueueSnapshot ?? new List<string>(),
            GanttEntries = currentGantt,
            CurrentMetrics = MapMetrics(metrics),
            IsComplete = result.Context.Processes.All(p => p.IsCompleted),
            ProcessStates = processStates
        };

        return step;
    }

    public async Task<SimulationStepDto> ExecuteFullSimulationAsync(int sessionId)
    {
        var session = await _sessionRepository.GetWithProcessesAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found.");

        var processes = await _processRepository.GetBySessionIdAsync(sessionId);
        var pcbList = processes.Select(MapToPcb).ToList();

        var result = _engine.RunSimulation(pcbList, session.AlgorithmType, session.TimeQuantum, session.IsPreemptive);

        await SaveResultsAsync(sessionId, result.Context, result.Metrics);

        var ganttChart = result.Context.GanttChart.Select(g => new GanttChartDto
        {
            ProcessId = g.ProcessId,
            ProcessName = g.ProcessName,
            StartTime = g.StartTime,
            EndTime = g.EndTime,
            IsContextSwitch = g.IsContextSwitch
        }).ToList();

        var processStates = result.Context.Processes.Select(p => new ProcessStateDto
        {
            ProcessId = p.ProcessId,
            ProcessName = p.ProcessName,
            State = p.IsCompleted ? "Completed" : (p.HasStarted ? "Ready" : "New"),
            RemainingTime = p.RemainingTime,
            Deadline = p.Deadline,
            MissedDeadline = p.MissedDeadline
        }).ToList();

        var executionLogs = result.Context.ExecutionSteps.Select(s => new ExecutionLogDto
        {
            TimeStep = s.TimeStep,
            ProcessId = s.ExecutingProcessId ?? "IDLE",
            ProcessName = s.ExecutingProcessName ?? "Idle",
            Action = s.Action,
            Details = s.Details,
            ReadyQueue = string.Join(", ", s.ReadyQueueSnapshot)
        }).ToList();

        return new SimulationStepDto
        {
            TimeStep = result.Context.CurrentTime,
            GanttEntries = ganttChart,
            CurrentMetrics = MapMetrics(result.Metrics),
            IsComplete = true,
            ProcessStates = processStates
        };
    }

    public async Task<SchedulingMetrics> RunAlgorithmAsync(int sessionId, string algorithmType, int timeQuantum, bool isPreemptive)
    {
        var processes = await _processRepository.GetBySessionIdAsync(sessionId);
        var pcbList = processes.Select(MapToPcb).ToList();

        var result = _engine.RunSimulation(pcbList, algorithmType, timeQuantum, isPreemptive);
        await SaveResultsAsync(sessionId, result.Context, result.Metrics);

        return result.Metrics;
    }

    public async Task SaveResultsAsync(int sessionId, SchedulingContext context, SchedulingMetrics metrics)
    {
        await _resultRepository.ClearResultsForSessionAsync(sessionId);
        await _executionLogRepository.ClearLogsForSessionAsync(sessionId);

        foreach (var process in context.Processes)
        {
            var result = new Models.Entities.SchedulingResult
            {
                SchedulingSessionId = sessionId,
                ProcessId = process.ProcessId,
                ProcessName = process.ProcessName,
                ArrivalTime = process.ArrivalTime,
                BurstTime = process.BurstTime,
                Deadline = process.Deadline,
                CompletionTime = process.CompletionTime,
                TurnaroundTime = process.TurnaroundTime,
                WaitingTime = process.WaitingTime,
                ResponseTime = process.ResponseTime,
                IsMissedDeadline = process.MissedDeadline,
                StartTime = process.StartTime,
                EndTime = process.CompletionTime,
                GanttChartData = System.Text.Json.JsonSerializer.Serialize(
                    context.GanttChart.Where(g => g.ProcessId == process.ProcessId).ToList()),
                CpuUtilization = metrics.CpuUtilization,
                Throughput = metrics.Throughput,
                ContextSwitchCount = metrics.ContextSwitchCount,
                DeadlineMissRatio = metrics.DeadlineMissRatio,
                CreatedAt = DateTime.UtcNow
            };

            await _resultRepository.AddAsync(result);
        }

        foreach (var step in context.ExecutionSteps)
        {
            var log = new ExecutionLog
            {
                SchedulingSessionId = sessionId,
                TimeStep = step.TimeStep,
                ExecutingProcessId = step.ExecutingProcessId ?? "IDLE",
                ExecutingProcessName = step.ExecutingProcessName ?? "Idle",
                Action = step.Action,
                Details = step.Details,
                QueueState = step.ReadyQueueSnapshot.Count,
                ReadyQueueSnapshot = System.Text.Json.JsonSerializer.Serialize(step.ReadyQueueSnapshot),
                CreatedAt = DateTime.UtcNow
            };

            await _executionLogRepository.AddAsync(log);
        }

        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session != null)
        {
            session.Status = "Completed";
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);
        }
    }

    private static ProcessDto MapToDto(ProcessEntity entity)
    {
        return new ProcessDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ProcessId = entity.ProcessId,
            ArrivalTime = entity.ArrivalTime,
            BurstTime = entity.BurstTime,
            Deadline = entity.Deadline,
            Priority = entity.Priority,
            Status = entity.Status,
            SchedulingSessionId = entity.SchedulingSessionId
        };
    }

    private static ProcessControlBlock MapToPcb(ProcessEntity entity)
    {
        return new ProcessControlBlock
        {
            ProcessId = entity.ProcessId,
            ProcessName = entity.Name,
            ArrivalTime = entity.ArrivalTime,
            BurstTime = entity.BurstTime,
            RemainingTime = entity.BurstTime,
            Deadline = entity.Deadline,
            Priority = entity.Priority,
            State = "New"
        };
    }

    private static MetricsDto MapMetrics(SchedulingMetrics metrics)
    {
        return new MetricsDto
        {
            AverageWaitingTime = metrics.AverageWaitingTime,
            AverageTurnaroundTime = metrics.AverageTurnaroundTime,
            AverageResponseTime = metrics.AverageResponseTime,
            CpuUtilization = metrics.CpuUtilization,
            Throughput = metrics.Throughput,
            ContextSwitchCount = metrics.ContextSwitchCount,
            MissedDeadlines = metrics.MissedDeadlines,
            DeadlineMissRatio = metrics.DeadlineMissRatio,
            TotalProcesses = metrics.TotalProcesses,
            CompletedProcesses = metrics.CompletedProcesses
        };
    }

    public async Task<List<ExecutionLogDto>> GetExecutionLogsAsync(int sessionId)
    {
        var logs = await _executionLogRepository.GetBySessionIdAsync(sessionId);
        return logs.Select(l => new ExecutionLogDto
        {
            TimeStep = l.TimeStep,
            ProcessId = l.ExecutingProcessId,
            ProcessName = l.ExecutingProcessName,
            Action = l.Action,
            Details = l.Details,
            ReadyQueue = l.ReadyQueueSnapshot
        }).ToList();
    }
}
