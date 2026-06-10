using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Strategies;

public class HybridEDFRRStrategy : ISchedulingStrategy
{
    public string Name => "Hybrid EDF + RR";

    public ProcessControlBlock? SelectNextProcess(List<ProcessControlBlock> readyQueue, int currentTime)
    {
        if (readyQueue.Count == 0)
            return null;

        var eligibleProcesses = readyQueue
            .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0)
            .ToList();

        if (eligibleProcesses.Count == 0)
            return null;

        var earliestDeadline = eligibleProcesses.Min(p => p.ArrivalTime + p.Deadline);

        var deadlineCandidates = eligibleProcesses
            .Where(p => p.ArrivalTime + p.Deadline == earliestDeadline)
            .ToList();

        if (deadlineCandidates.Count == 1)
            return deadlineCandidates[0];

        return deadlineCandidates
            .OrderBy(p => p.ArrivalTime)
            .ThenBy(p => p.ProcessId)
            .FirstOrDefault();
    }
}
