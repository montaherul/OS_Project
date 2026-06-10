using ERDRR.Models.DTOs;

namespace ERDRR.Models.ViewModels;

public class SchedulerComparisonViewModel
{
    public List<SessionDto> Sessions { get; set; } = new();
    public int? SelectedSessionId { get; set; }
    public ComparisonResultDto? Result { get; set; }
    public bool IsLoading { get; set; }
}

public class ComparisonDetailViewModel
{
    public ComparisonResultDto Result { get; set; } = new();
    public string ChartJson { get; set; } = string.Empty;
}
