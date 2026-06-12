using EDFRR.Scheduling.Interfaces;
using EDFRR.Scheduling.Models;

namespace EDFRR.Scheduling.Strategies;

public class EDFStrategy : ISchedulingStrategy
{
    public string Name => "Earliest Deadline First";

    public ProcessControlBlock? SelectNextProcess(List<ProcessControlBlock> readyQueue, int currentTime)
    {
        if (readyQueue.Count == 0)
            return null;

        return readyQueue
            .Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0)
            .OrderBy(p => p.ArrivalTime + p.Deadline)
            .ThenBy(p => p.ArrivalTime)
            .FirstOrDefault();
    }
}
