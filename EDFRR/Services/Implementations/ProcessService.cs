using EDFRR.Models.DTOs;
using EDFRR.Models.Entities;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;

namespace EDFRR.Services.Implementations;

public class ProcessService : IProcessService
{
    private readonly IProcessRepository _processRepository;

    public ProcessService(IProcessRepository processRepository)
    {
        _processRepository = processRepository;
    }

    public async Task<ProcessDto?> GetByIdAsync(int id)
    {
        var entity = await _processRepository.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDto(entity);
    }

    public async Task<IEnumerable<ProcessDto>> GetBySessionIdAsync(int sessionId)
    {
        var entities = await _processRepository.GetBySessionIdAsync(sessionId);
        return entities.Select(MapToDto);
    }

    public async Task<ProcessListViewModel> GetProcessesPagedAsync(int sessionId, int page, int pageSize, string? searchTerm = null, string? status = null)
    {
        var processes = await _processRepository.GetProcessesPagedAsync(sessionId, page, pageSize, searchTerm, status);
        var totalItems = await _processRepository.CountFilteredAsync(sessionId, searchTerm, status);

        return new ProcessListViewModel
        {
            Processes = processes.Select(MapToDto).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            TotalItems = totalItems,
            SearchTerm = searchTerm,
            FilterStatus = status,
            PageSize = pageSize,
            SessionId = sessionId
        };
    }

    public async Task<ProcessDto> CreateAsync(CreateProcessDto dto, string userId)
    {
        var processId = await GenerateUniqueProcessIdAsync(dto.SchedulingSessionId);

        var entity = new ProcessEntity
        {
            Name = dto.Name,
            ProcessId = processId,
            ArrivalTime = dto.ArrivalTime,
            BurstTime = dto.BurstTime,
            Deadline = dto.Deadline,
            Priority = dto.Priority,
            SchedulingSessionId = dto.SchedulingSessionId,
            UserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _processRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<ProcessDto?> UpdateAsync(int id, ProcessEditViewModel model)
    {
        var entity = await _processRepository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Name = model.Name;
        entity.ArrivalTime = model.ArrivalTime;
        entity.BurstTime = model.BurstTime;
        entity.Deadline = model.Deadline;
        entity.Priority = model.Priority;
        entity.UpdatedAt = DateTime.UtcNow;

        await _processRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _processRepository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _processRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> ExistsAsync(int sessionId, string processId)
    {
        var existing = await _processRepository.GetBySessionAndProcessIdAsync(sessionId, processId);
        return existing != null;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _processRepository.CountAsync(p => !p.IsDeleted);
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        return await _processRepository.CountAsync(p => p.Status == status && !p.IsDeleted);
    }

    public async Task<ProcessDto> GenerateProcessIdAsync(CreateProcessDto dto)
    {
        var processId = await GenerateUniqueProcessIdAsync(dto.SchedulingSessionId);
        dto.Name = $"{dto.Name}";
        return new ProcessDto
        {
            ProcessId = processId,
            Name = dto.Name,
            ArrivalTime = dto.ArrivalTime,
            BurstTime = dto.BurstTime,
            Deadline = dto.Deadline,
            Priority = dto.Priority,
            SchedulingSessionId = dto.SchedulingSessionId
        };
    }

    private async Task<string> GenerateUniqueProcessIdAsync(int sessionId)
    {
        var maxNumber = await _processRepository.GetMaxProcessIdNumberAsync(sessionId);
        return $"P{maxNumber + 1:D3}";
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
}
