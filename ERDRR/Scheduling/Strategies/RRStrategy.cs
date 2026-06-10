using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Strategies;

public class RRStrategy : ISchedulingStrategy
{
    public string Name => "Round Robin";

    public ProcessControlBlock? SelectNextProcess(List<ProcessControlBlock> readyQueue, int currentTime)
    {
        if (readyQueue.Count == 0)
            return null;

        return readyQueue
            .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0)
            .OrderBy(p => p.ArrivalTime)
            .ThenBy(p => p.ProcessId)
            .FirstOrDefault();
    }
}
