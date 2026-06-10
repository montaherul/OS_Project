using ERDRR.Scheduling.Models;

namespace ERDRR.Scheduling.Interfaces;

public interface IScheduler
{
    string AlgorithmName { get; }
    SchedulingContext Execute(SchedulingContext context);
}

public interface ISchedulingStrategy
{
    string Name { get; }
    ProcessControlBlock? SelectNextProcess(List<ProcessControlBlock> readyQueue, int currentTime);
}
