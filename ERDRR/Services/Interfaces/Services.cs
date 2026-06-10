using ERDRR.Models.DTOs;
using ERDRR.Models.ViewModels;
using ERDRR.Scheduling.Engine;
using ERDRR.Scheduling.Models;

namespace ERDRR.Services.Interfaces;

public interface IProcessService
{
    Task<ProcessDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProcessDto>> GetBySessionIdAsync(int sessionId);
    Task<ProcessListViewModel> GetProcessesPagedAsync(int sessionId, int page, int pageSize, string? searchTerm = null, string? status = null);
    Task<ProcessDto> CreateAsync(CreateProcessDto dto, string userId);
    Task<ProcessDto?> UpdateAsync(int id, ProcessEditViewModel model);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int sessionId, string processId);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByStatusAsync(string status);
    Task<ProcessDto> GenerateProcessIdAsync(CreateProcessDto dto);
}

public interface ISessionService
{
    Task<SessionDto?> GetByIdAsync(int id);
    Task<SessionListViewModel> GetSessionsPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<SessionDto> CreateAsync(CreateSessionDto dto, string userId);
    Task<SessionDto?> UpdateAsync(int id, SessionEditViewModel model);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}

public interface ISimulationService
{
    Task<SimulationViewModel> InitializeSimulationAsync(int sessionId);
    Task<SimulationStepDto> ExecuteStepAsync(int sessionId, int currentTimeStep);
    Task<SimulationStepDto> ExecuteFullSimulationAsync(int sessionId);
    Task<SchedulingMetrics> RunAlgorithmAsync(int sessionId, string algorithmType, int timeQuantum, bool isPreemptive);
    Task SaveResultsAsync(int sessionId, SchedulingContext context, SchedulingMetrics metrics);
    Task<List<ExecutionLogDto>> GetExecutionLogsAsync(int sessionId);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync();
    Task<DashboardDto> GetDashboardDataForSessionAsync(int sessionId);
    Task<DashboardDto> GetDashboardDataForUserAsync(string userId);
    Task<DashboardDto> GetAdminDashboardDataAsync();
}

public interface IReportService
{
    Task<ReportDto> GenerateReportAsync(int sessionId);
    Task<byte[]> GeneratePdfReportAsync(int sessionId);
    Task<byte[]> GenerateExcelReportAsync(int sessionId);
}
