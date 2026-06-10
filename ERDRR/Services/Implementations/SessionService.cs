using ERDRR.Models.DTOs;
using ERDRR.Models.Entities;
using ERDRR.Models.ViewModels;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Interfaces;

namespace ERDRR.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IProcessRepository _processRepository;

    public SessionService(ISessionRepository sessionRepository, IProcessRepository processRepository)
    {
        _sessionRepository = sessionRepository;
        _processRepository = processRepository;
    }

    public async Task<SessionDto?> GetByIdAsync(int id)
    {
        var entity = await _sessionRepository.GetWithProcessesAsync(id);
        if (entity == null) return null;
        return MapToDto(entity);
    }

    public async Task<SessionListViewModel> GetSessionsPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var sessions = await _sessionRepository.GetSessionsPagedAsync(page, pageSize, searchTerm);
        var totalItems = await _sessionRepository.CountFilteredAsync(searchTerm);

        var sessionDtos = new List<SessionDto>();
        foreach (var session in sessions)
        {
            var processCount = await _processRepository.CountBySessionIdAsync(session.Id);
            var dto = MapToDto(session);
            dto.ProcessCount = processCount;
            sessionDtos.Add(dto);
        }

        return new SessionListViewModel
        {
            Sessions = sessionDtos,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            TotalItems = totalItems,
            SearchTerm = searchTerm,
            PageSize = pageSize
        };
    }

    public async Task<SessionDto> CreateAsync(CreateSessionDto dto, string userId)
    {
        var entity = new SchedulingSession
        {
            Name = dto.Name,
            Description = dto.Description,
            AlgorithmType = dto.AlgorithmType,
            TimeQuantum = dto.TimeQuantum,
            IsPreemptive = dto.IsPreemptive,
            UserId = userId,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _sessionRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<SessionDto?> UpdateAsync(int id, SessionEditViewModel model)
    {
        var entity = await _sessionRepository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.AlgorithmType = model.AlgorithmType;
        entity.TimeQuantum = model.TimeQuantum;
        entity.IsPreemptive = model.IsPreemptive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _sessionRepository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _sessionRepository.CountAsync(s => !s.IsDeleted);
    }

    private static SessionDto MapToDto(SchedulingSession entity)
    {
        return new SessionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            AlgorithmType = entity.AlgorithmType,
            TimeQuantum = entity.TimeQuantum,
            Status = entity.Status,
            IsPreemptive = entity.IsPreemptive,
            CreatedAt = entity.CreatedAt,
            ProcessCount = entity.Processes?.Count ?? 0
        };
    }
}
