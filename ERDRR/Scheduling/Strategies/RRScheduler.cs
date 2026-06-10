using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Strategies;

public class RRScheduler : IScheduler
{
    public string AlgorithmName => "RR";
    private readonly int _timeQuantum;

    public RRScheduler(int timeQuantum = 4)
    {
        _timeQuantum = timeQuantum;
    }

    public SchedulingContext Execute(SchedulingContext context)
    {
        var processes = context.Processes.Select(p => p.Clone()).ToList();
        var ganttChart = new List<GanttEntry>();
        var executionSteps = new List<ExecutionStep>();
        var readyQueue = new Queue<ProcessControlBlock>();

        int currentTime = 0;
        int completedCount = 0;
        int totalProcesses = processes.Count;
        int contextSwitches = 0;
        var inQueue = new HashSet<string>();

        var sortedProcesses = processes.OrderBy(p => p.ArrivalTime).ToList();

        foreach (var p in sortedProcesses.Where(p => p.ArrivalTime <= currentTime))
        {
            readyQueue.Enqueue(p);
            inQueue.Add(p.ProcessId);
        }

        while (completedCount < totalProcesses)
        {
            if (readyQueue.Count == 0)
            {
                var notArrived = processes.Where(p => !p.IsCompleted && p.State == "New").ToList();
                if (notArrived.Count == 0) break;

                int nextArrival = notArrived.Min(p => p.ArrivalTime);
                ganttChart.Add(new GanttEntry
                {
                    ProcessId = "IDLE",
                    ProcessName = "Idle",
                    StartTime = currentTime,
                    EndTime = nextArrival,
                    IsIdle = true
                });
                executionSteps.Add(new ExecutionStep
                {
                    TimeStep = currentTime,
                    Action = "Idle",
                    Details = $"No process ready. Jumping to time {nextArrival}",
                    ReadyQueueSnapshot = new List<string>()
                });
                currentTime = nextArrival;

                foreach (var p in sortedProcesses.Where(p => p.ArrivalTime <= currentTime && !inQueue.Contains(p.ProcessId) && !p.IsCompleted))
                {
                    readyQueue.Enqueue(p);
                    inQueue.Add(p.ProcessId);
                }
                continue;
            }

            var currentProcess = readyQueue.Dequeue();
            inQueue.Remove(currentProcess.ProcessId);

            foreach (var p in sortedProcesses.Where(p => p.ArrivalTime <= currentTime && !inQueue.Contains(p.ProcessId) && !p.IsCompleted && p != currentProcess))
            {
                readyQueue.Enqueue(p);
                inQueue.Add(p.ProcessId);
            }

            if (currentProcess.State == "New" || currentProcess.State == "Ready")
            {
                if (currentProcess.FirstExecutionTime < 0)
                {
                    currentProcess.FirstExecutionTime = currentTime;
                }
                currentProcess.State = "Running";
            }

            int executionTime = Math.Min(_timeQuantum, currentProcess.RemainingTime);
            int startTime = currentTime;
            int endTime = currentTime + executionTime;

            ganttChart.Add(new GanttEntry
            {
                ProcessId = currentProcess.ProcessId,
                ProcessName = currentProcess.ProcessName,
                StartTime = startTime,
                EndTime = endTime
            });

            executionSteps.Add(new ExecutionStep
            {
                TimeStep = currentTime,
                ExecutingProcessId = currentProcess.ProcessId,
                ExecutingProcessName = currentProcess.ProcessName,
                Action = "Execute",
                Details = $"Executing {currentProcess.ProcessId} for {executionTime} units (Quantum: {_timeQuantum})",
                ReadyQueueSnapshot = readyQueue.Select(p => p.ProcessId).ToList()
            });

            currentProcess.RemainingTime -= executionTime;
            currentTime = endTime;

            foreach (var p in sortedProcesses.Where(p => p.ArrivalTime <= currentTime && !inQueue.Contains(p.ProcessId) && !p.IsCompleted && p != currentProcess))
            {
                readyQueue.Enqueue(p);
                inQueue.Add(p.ProcessId);
            }

            if (currentProcess.RemainingTime <= 0)
            {
                currentProcess.CompletionTime = currentTime;
                currentProcess.State = "Terminated";
                currentProcess.IsCompleted = true;
                completedCount++;

                if (currentTime > currentProcess.ArrivalTime + currentProcess.Deadline)
                {
                    currentProcess.MissedDeadline = true;
                }
            }
            else
            {
                currentProcess.State = "Ready";
                readyQueue.Enqueue(currentProcess);
                inQueue.Add(currentProcess.ProcessId);
                contextSwitches++;
            }
        }

        context.Processes = processes;
        context.GanttChart = ganttChart;
        context.ExecutionSteps = executionSteps;
        context.CurrentTime = currentTime;
        context.ContextSwitchCount = contextSwitches;

        return context;
    }
}
