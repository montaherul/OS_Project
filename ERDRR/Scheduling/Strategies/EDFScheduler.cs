using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Strategies;

public class EDFScheduler : IScheduler
{
    public string AlgorithmName => "EDF";
    private readonly bool _isPreemptive;

    public EDFScheduler(bool isPreemptive = true)
    {
        _isPreemptive = isPreemptive;
    }

    public SchedulingContext Execute(SchedulingContext context)
    {
        var processes = context.Processes.Select(p => p.Clone()).ToList();
        var ganttChart = new List<GanttEntry>();
        var executionSteps = new List<ExecutionStep>();
        var readyQueue = new List<ProcessControlBlock>();
        var timeQuantum = context.TimeQuantum;

        int currentTime = 0;
        int completedCount = 0;
        int totalProcesses = processes.Count;
        int contextSwitches = 0;

        while (completedCount < totalProcesses)
        {
            foreach (var p in processes.Where(p => p.State == "New" && p.ArrivalTime <= currentTime))
            {
                p.State = "Ready";
                readyQueue.Add(p);
            }

            if (_isPreemptive)
            {
                var arrivingProcesses = processes
                    .Where(p => p.State == "Running" && p.ArrivalTime <= currentTime && !readyQueue.Contains(p) && !p.IsCompleted)
                    .ToList();

                foreach (var p in arrivingProcesses)
                {
                    if (p.State == "Running")
                    {
                        p.State = "Ready";
                    }
                }
            }

            var selectedProcess = SelectByEDF(readyQueue, currentTime);

            if (selectedProcess == null)
            {
                if (readyQueue.Count == 0 && completedCount < totalProcesses)
                {
                    var nextArrival = processes
                        .Where(p => !p.IsCompleted && p.State == "New")
                        .Min(p => (int?)p.ArrivalTime);

                    if (nextArrival.HasValue)
                    {
                        ganttChart.Add(new GanttEntry
                        {
                            ProcessId = "IDLE",
                            ProcessName = "Idle",
                            StartTime = currentTime,
                            EndTime = nextArrival.Value,
                            IsIdle = true
                        });
                        executionSteps.Add(new ExecutionStep
                        {
                            TimeStep = currentTime,
                            Action = "Idle",
                            Details = $"No process available. Advancing to time {nextArrival.Value}",
                            ReadyQueueSnapshot = readyQueue.Select(p => p.ProcessId).ToList()
                        });
                        currentTime = nextArrival.Value;
                    }
                    else
                    {
                        break;
                    }
                }
                continue;
            }

            if (selectedProcess.State == "Ready" && selectedProcess.FirstExecutionTime < 0)
            {
                selectedProcess.FirstExecutionTime = currentTime;
                selectedProcess.StartTime = currentTime;
            }

            selectedProcess.State = "Running";
            selectedProcess.HasStarted = true;

            int executionTime;
            if (_isPreemptive)
            {
                var nextDeadline = GetNextDeadlineTime(processes, currentTime);
                var maxExecute = nextDeadline - currentTime;
                executionTime = Math.Min(selectedProcess.RemainingTime, Math.Min(timeQuantum, maxExecute > 0 ? maxExecute : timeQuantum));
            }
            else
            {
                executionTime = selectedProcess.RemainingTime;
            }

            int startTime = currentTime;
            int endTime = currentTime + executionTime;

            ganttChart.Add(new GanttEntry
            {
                ProcessId = selectedProcess.ProcessId,
                ProcessName = selectedProcess.ProcessName,
                StartTime = startTime,
                EndTime = endTime
            });

            executionSteps.Add(new ExecutionStep
            {
                TimeStep = currentTime,
                ExecutingProcessId = selectedProcess.ProcessId,
                ExecutingProcessName = selectedProcess.ProcessName,
                Action = "Execute",
                Details = $"Executing {selectedProcess.ProcessId} for {executionTime} units",
                ReadyQueueSnapshot = readyQueue.Where(p => p != selectedProcess).Select(p => p.ProcessId).ToList()
            });

            selectedProcess.RemainingTime -= executionTime;
            currentTime = endTime;

            if (selectedProcess.RemainingTime <= 0)
            {
                selectedProcess.CompletionTime = currentTime;
                selectedProcess.State = "Terminated";
                selectedProcess.IsCompleted = true;
                completedCount++;

                if (currentTime > selectedProcess.ArrivalTime + selectedProcess.Deadline)
                {
                    selectedProcess.MissedDeadline = true;
                }

                readyQueue.Remove(selectedProcess);
            }
            else
            {
                selectedProcess.State = "Ready";
                context.ContextSwitchCount++;
                contextSwitches++;
            }

            foreach (var p in processes.Where(p => p.State == "New" && p.ArrivalTime <= currentTime && !p.IsCompleted))
            {
                if (!readyQueue.Contains(p))
                {
                    p.State = "Ready";
                    readyQueue.Add(p);
                }
            }
        }

        context.Processes = processes;
        context.GanttChart = ganttChart;
        context.ExecutionSteps = executionSteps;
        context.CurrentTime = currentTime;
        context.ContextSwitchCount = contextSwitches;

        return context;
    }

    private ProcessControlBlock? SelectByEDF(List<ProcessControlBlock> readyQueue, int currentTime)
    {
        return readyQueue
            .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0)
            .OrderBy(p => p.ArrivalTime + p.Deadline)
            .ThenBy(p => p.ArrivalTime)
            .FirstOrDefault();
    }

    private int GetNextDeadlineTime(List<ProcessControlBlock> processes, int currentTime)
    {
        var upcomingDeadlines = processes
            .Where(p => !p.IsCompleted && p.ArrivalTime > currentTime)
            .Select(p => p.ArrivalTime + p.Deadline)
            .Where(d => d > currentTime)
            .ToList();

        return upcomingDeadlines.Count > 0 ? upcomingDeadlines.Min() : currentTime + 1000;
    }
}
