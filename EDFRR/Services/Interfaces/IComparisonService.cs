using EDFRR.Models.DTOs;

namespace EDFRR.Services.Interfaces;

public interface IComparisonService
{
    Task<ComparisonResultDto> CompareAlgorithmsAsync(int sessionId, string userId);
    Task<ComparisonResultDto?> GetComparisonAsync(int comparisonId);
    Task<ComparisonResultDto?> GetLatestComparisonAsync(int sessionId);
    Task<List<ComparisonResultDto>> GetUserComparisonsAsync(string userId);
    Task<ComparisonChartDataDto> GetChartDataAsync(int comparisonId);
    Task<ComparisonExportDto> GetExportDataAsync(int comparisonId);
    Task DeleteComparisonAsync(int id);
}
