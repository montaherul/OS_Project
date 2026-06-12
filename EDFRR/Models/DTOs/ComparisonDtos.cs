namespace EDFRR.Models.DTOs;

public class ComparisonResultDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int ProcessCount { get; set; }
    public int TimeQuantum { get; set; }
    public bool IsPreemptive { get; set; }
    public DateTime CreatedAt { get; set; }

    public AlgorithmMetricsDto EDF { get; set; } = new();
    public AlgorithmMetricsDto RR { get; set; } = new();
    public AlgorithmMetricsDto Hybrid { get; set; } = new();

    public string RecommendedAlgorithm { get; set; } = string.Empty;
    public string? RecommendationReason { get; set; }
    public double BestScore { get; set; }
}

public class AlgorithmMetricsDto
{
    public double WaitingTime { get; set; }
    public double TurnaroundTime { get; set; }
    public double ResponseTime { get; set; }
    public double CpuUtilization { get; set; }
    public double Throughput { get; set; }
    public int ContextSwitches { get; set; }
    public double DeadlineMissRatio { get; set; }
    public int ExecutionTime { get; set; }
}

public class ComparisonChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<ChartDatasetDto> Datasets { get; set; } = new();
}

public class ChartDatasetDto
{
    public string Label { get; set; } = string.Empty;
    public List<double> Data { get; set; } = new();
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
}

public class ComparisonExportDto
{
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int ProcessCount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<MetricComparisonRowDto> Metrics { get; set; } = new();
    public string RecommendedAlgorithm { get; set; } = string.Empty;
    public string? RecommendationReason { get; set; }
}

public class MetricComparisonRowDto
{
    public string MetricName { get; set; } = string.Empty;
    public double EDFValue { get; set; }
    public double RRValue { get; set; }
    public double HybridValue { get; set; }
    public string BestAlgorithm { get; set; } = string.Empty;
}
