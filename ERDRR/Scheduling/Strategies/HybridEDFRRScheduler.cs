using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Strategies;

public class HybridEDFRRScheduler : IScheduler
{
    public string AlgorithmName => "HybridEDFRR";
    private readonly int _timeQuantum;
    private readonly bool _isPreemptive;

    public HybridEDFRRScheduler(int timeQuantum = 4, bool isPreemptive = true)
    {
        _timeQuantum = timeQuantum;
        _isPreemptive = isPreemptive;
    }

    public SchedulingContext Execute(SchedulingContext context)
    {
        var processes = context.Processes.Select(p => p.Clone()).ToList();
        var ganttChart = new List<GanttEntry>();
        var executionSteps = new List<ExecutionStep>();
        var readyQueue = new List<ProcessControlBlock>();

        int currentTime = 0;
        int completedCount = 0;
        int totalProcesses = processes.Count;
        int contextSwitches = 0;

        // Round-robin tracker: tracks which process was last executed in each deadline group
        var rrRotation = new Dictionary<int, int>();

        while (completedCount < totalProcesses)
        {
            // Admit newly arrived processes
            foreach (var p in processes.Where(p => p.State == "New" && p.ArrivalTime <= currentTime && !p.IsCompleted))
            {
                p.State = "Ready";
                if (!readyQueue.Contains(p))
                    readyQueue.Add(p);
            }

            var selectedProcess = SelectByHybridEDFRR(readyQueue, currentTime, rrRotation);

            if (selectedProcess == null)
            {
                if (completedCount < totalProcesses)
                {
                    var notCompleted = processes.Where(p => !p.IsCompleted).ToList();
                    if (notCompleted.Count == 0) break;

                    int nextArrival = notCompleted.Min(p => p.ArrivalTime);
                    if (nextArrival <= currentTime)
                    {
                        currentTime++;
                        continue;
                    }

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
                        Details = $"CPU Idle. Fast-forwarding to time {nextArrival}",
                        ReadyQueueSnapshot = new List<string>()
                    });

                    currentTime = nextArrival;
                }
                continue;
            }

            // Set first execution time (for response time calculation)
            if (selectedProcess.FirstExecutionTime < 0)
            {
                selectedProcess.FirstExecutionTime = currentTime;
                if (selectedProcess.StartTime < 0)
                    selectedProcess.StartTime = currentTime;
            }

            selectedProcess.State = "Running";
            selectedProcess.HasStarted = true;

            // Determine if this process is in a same-deadline group (RR mode)
            int selectedDeadline = selectedProcess.ArrivalTime + selectedProcess.Deadline;
            var sameDeadlineGroup = readyQueue
                .Where(p => !p.IsCompleted && p.RemainingTime > 0 && p.ArrivalTime <= currentTime)
                .Where(p => p.ArrivalTime + p.Deadline == selectedDeadline)
                .ToList();

            bool useRoundRobin = sameDeadlineGroup.Count > 1;

            // Calculate execution time
            int executionTime;
            if (useRoundRobin)
            {
                // Round Robin: execute for quantum or remaining, whichever is smaller
                executionTime = Math.Min(_timeQuantum, selectedProcess.RemainingTime);
            }
            else if (_isPreemptive)
            {
                // EDF: execute until next deadline change or completion
                var nextDeadlineChange = GetNextDeadlineChangeTime(processes, currentTime);
                int maxByDeadline = nextDeadlineChange - currentTime;
                executionTime = Math.Min(selectedProcess.RemainingTime, maxByDeadline > 0 ? maxByDeadline : selectedProcess.RemainingTime);
            }
            else
            {
                executionTime = selectedProcess.RemainingTime;
            }

            int startTime = currentTime;
            int endTime = currentTime + executionTime;
            bool wasPreempted = selectedProcess.RemainingTime > executionTime;

            ganttChart.Add(new GanttEntry
            {
                ProcessId = selectedProcess.ProcessId,
                ProcessName = selectedProcess.ProcessName,
                StartTime = startTime,
                EndTime = endTime
            });

            string mode = useRoundRobin ? "RR" : "EDF";
            executionSteps.Add(new ExecutionStep
            {
                TimeStep = currentTime,
                ExecutingProcessId = selectedProcess.ProcessId,
                ExecutingProcessName = selectedProcess.ProcessName,
                Action = wasPreempted ? "Preempt" : "Execute",
                Details = $"[{mode}] {selectedProcess.ProcessId} (Deadline:{selectedDeadline}) for {executionTime} units" +
                          (wasPreempted ? $" | Preempted (remaining:{selectedProcess.RemainingTime - executionTime})" : " | Completed"),
                ReadyQueueSnapshot = readyQueue
                    .Where(p => p != selectedProcess && !p.IsCompleted && p.RemainingTime > 0)
                    .OrderBy(p => p.ArrivalTime + p.Deadline)
                    .Select(p => $"{p.ProcessId}(D:{p.ArrivalTime + p.Deadline})")
                    .ToList()
            });

            selectedProcess.RemainingTime -= executionTime;
            currentTime = endTime;

            // Admit processes that arrived during execution
            foreach (var p in processes.Where(p => p.State == "New" && p.ArrivalTime <= currentTime && !p.IsCompleted))
            {
                p.State = "Ready";
                if (!readyQueue.Contains(p))
                    readyQueue.Add(p);
            }

            if (selectedProcess.RemainingTime <= 0)
            {
                // Process completed
                selectedProcess.CompletionTime = currentTime;
                selectedProcess.State = "Terminated";
                selectedProcess.IsCompleted = true;
                completedCount++;

                if (currentTime > selectedProcess.ArrivalTime + selectedProcess.Deadline)
                {
                    selectedProcess.MissedDeadline = true;
                }

                readyQueue.Remove(selectedProcess);

                // Remove from RR rotation
                if (rrRotation.ContainsKey(selectedDeadline))
                    rrRotation.Remove(selectedDeadline);

                executionSteps.Add(new ExecutionStep
                {
                    TimeStep = currentTime,
                    ExecutingProcessId = selectedProcess.ProcessId,
                    ExecutingProcessName = selectedProcess.ProcessName,
                    Action = "Complete",
                    Details = $"Completed {selectedProcess.ProcessId} at time {currentTime}" +
                              (selectedProcess.MissedDeadline ? " | MISSED DEADLINE" : ""),
                    ReadyQueueSnapshot = readyQueue.Select(p => p.ProcessId).ToList()
                });
            }
            else
            {
                // Process not finished — count context switch
                contextSwitches++;
                selectedProcess.State = "Ready";

                // Update RR rotation for this deadline group
                if (useRoundRobin)
                {
                    int groupKey = selectedDeadline;
                    if (!rrRotation.ContainsKey(groupKey))
                        rrRotation[groupKey] = 0;

                    int currentIndex = sameDeadlineGroup.IndexOf(selectedProcess);
                    rrRotation[groupKey] = (currentIndex + 1) % sameDeadlineGroup.Count;
                }

                // Check for preemption by a newly arrived process with earlier deadline
                if (_isPreemptive && !useRoundRobin)
                {
                    var bestCandidate = SelectByHybridEDFRR(readyQueue, currentTime, null);
                    if (bestCandidate != null && bestCandidate != selectedProcess)
                    {
                        int bestDeadline = bestCandidate.ArrivalTime + bestCandidate.Deadline;
                        int currentDeadline = selectedProcess.ArrivalTime + selectedProcess.Deadline;
                        if (bestDeadline < currentDeadline)
                        {
                            executionSteps.Add(new ExecutionStep
                            {
                                TimeStep = currentTime,
                                Action = "Preempt",
                                Details = $"Preempting {selectedProcess.ProcessId} (D:{currentDeadline}) for {bestCandidate.ProcessId} (D:{bestDeadline})",
                                ReadyQueueSnapshot = readyQueue.Select(p => p.ProcessId).ToList()
                            });
                        }
                    }
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

    private ProcessControlBlock? SelectByHybridEDFRR(
        List<ProcessControlBlock> readyQueue,
        int currentTime,
        Dictionary<int, int>? rrRotation)
    {
        var eligible = readyQueue
            .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0 && !p.IsCompleted)
            .ToList();

        if (eligible.Count == 0)
            return null;

        // EDF: group by absolute deadline
        int earliestDeadline = eligible.Min(p => p.ArrivalTime + p.Deadline);
        var deadlineGroup = eligible.Where(p => p.ArrivalTime + p.Deadline == earliestDeadline).ToList();

        if (deadlineGroup.Count == 1)
            return deadlineGroup[0];

        // Round Robin within same-deadline group
        int groupKey = earliestDeadline;
        if (rrRotation != null && rrRotation.ContainsKey(groupKey))
        {
            int lastIndex = rrRotation[groupKey];
            if (lastIndex < deadlineGroup.Count)
            {
                return deadlineGroup[lastIndex];
            }
        }

        // Default: first by arrival time
        return deadlineGroup
            .OrderBy(p => p.ArrivalTime)
            .ThenBy(p => p.ProcessId)
            .FirstOrDefault();
    }

    private int GetNextDeadlineChangeTime(List<ProcessControlBlock> processes, int currentTime)
    {
        var deadlines = processes
            .Where(p => !p.IsCompleted)
            .Select(p => p.ArrivalTime + p.Deadline)
            .Where(d => d > currentTime)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return deadlines.Count > 0 ? deadlines[0] : currentTime + 1000;
    }
}
