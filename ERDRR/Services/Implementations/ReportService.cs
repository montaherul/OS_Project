using System.Text;
using ClosedXML.Excel;
using ERDRR.Models.DTOs;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ERDRR.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IProcessRepository _processRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IExecutionLogRepository _executionLogRepository;

    public ReportService(
        ISessionRepository sessionRepository,
        IProcessRepository processRepository,
        IResultRepository resultRepository,
        IExecutionLogRepository executionLogRepository)
    {
        _sessionRepository = sessionRepository;
        _processRepository = processRepository;
        _resultRepository = resultRepository;
        _executionLogRepository = executionLogRepository;
    }

    public async Task<ReportDto> GenerateReportAsync(int sessionId)
    {
        var session = await _sessionRepository.GetWithProcessesAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found.");

        var processes = await _processRepository.GetBySessionIdAsync(sessionId);
        var results = await _resultRepository.GetBySessionIdAsync(sessionId);
        var logs = await _executionLogRepository.GetBySessionIdAsync(sessionId);

        var resultList = results.ToList();
        var avgWaitingTime = resultList.Count > 0 ? resultList.Average(r => r.WaitingTime) : 0;
        var avgTurnaroundTime = resultList.Count > 0 ? resultList.Average(r => r.TurnaroundTime) : 0;
        var avgResponseTime = resultList.Count > 0 ? resultList.Average(r => r.ResponseTime) : 0;
        var avgCpuUtil = resultList.Count > 0 ? resultList.Average(r => r.CpuUtilization) : 0;
        var avgThroughput = resultList.Count > 0 ? resultList.Average(r => r.Throughput) : 0;

        return new ReportDto
        {
            SessionId = sessionId,
            SessionName = session.Name,
            AlgorithmType = session.AlgorithmType,
            GeneratedAt = DateTime.UtcNow,
            Metrics = new MetricsDto
            {
                AverageWaitingTime = Math.Round(avgWaitingTime, 2),
                AverageTurnaroundTime = Math.Round(avgTurnaroundTime, 2),
                AverageResponseTime = Math.Round(avgResponseTime, 2),
                CpuUtilization = Math.Round(avgCpuUtil, 2),
                Throughput = Math.Round(avgThroughput, 4),
                ContextSwitchCount = resultList.FirstOrDefault()?.ContextSwitchCount ?? 0,
                MissedDeadlines = resultList.Count(r => r.IsMissedDeadline),
                DeadlineMissRatio = resultList.Count > 0
                    ? Math.Round((double)resultList.Count(r => r.IsMissedDeadline) / resultList.Count * 100, 2)
                    : 0,
                TotalProcesses = resultList.Count,
                CompletedProcesses = resultList.Count(r => r.CompletionTime > 0)
            },
            Processes = resultList.Select(r => new ProcessDto
            {
                ProcessId = r.ProcessId,
                Name = r.ProcessName,
                ArrivalTime = r.ArrivalTime,
                BurstTime = r.BurstTime,
                Deadline = r.Deadline,
                CompletionTime = r.CompletionTime,
                TurnaroundTime = r.TurnaroundTime,
                WaitingTime = r.WaitingTime,
                ResponseTime = r.ResponseTime,
                MissedDeadline = r.IsMissedDeadline
            }).ToList(),
            ExecutionLogs = logs.Select(l => new ExecutionLogDto
            {
                TimeStep = l.TimeStep,
                ProcessId = l.ExecutingProcessId,
                ProcessName = l.ExecutingProcessName,
                Action = l.Action,
                Details = l.Details,
                ReadyQueue = l.ReadyQueueSnapshot
            }).ToList()
        };
    }

    public async Task<byte[]> GeneratePdfReportAsync(int sessionId)
    {
        var report = await GenerateReportAsync(sessionId);

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4, 25, 25, 25, 25);
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        document.Add(new Paragraph("ERDRR - CPU Scheduling Report", titleFont));
        document.Add(new Paragraph($"Session: {report.SessionName}", normalFont));
        document.Add(new Paragraph($"Algorithm: {report.AlgorithmType}", normalFont));
        document.Add(new Paragraph($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}", normalFont));
        document.Add(new Paragraph(" "));

        document.Add(new Paragraph("Performance Metrics", headerFont));
        var metricsTable = new PdfPTable(2);
        metricsTable.WidthPercentage = 50;
        metricsTable.AddCell(new Phrase("Metric", boldFont));
        metricsTable.AddCell(new Phrase("Value", boldFont));
        metricsTable.AddCell("Average Waiting Time");
        metricsTable.AddCell(report.Metrics.AverageWaitingTime.ToString("F2"));
        metricsTable.AddCell("Average Turnaround Time");
        metricsTable.AddCell(report.Metrics.AverageTurnaroundTime.ToString("F2"));
        metricsTable.AddCell("Average Response Time");
        metricsTable.AddCell(report.Metrics.AverageResponseTime.ToString("F2"));
        metricsTable.AddCell("CPU Utilization");
        metricsTable.AddCell($"{report.Metrics.CpuUtilization:F2}%");
        metricsTable.AddCell("Throughput");
        metricsTable.AddCell(report.Metrics.Throughput.ToString("F4"));
        metricsTable.AddCell("Context Switches");
        metricsTable.AddCell(report.Metrics.ContextSwitchCount.ToString());
        metricsTable.AddCell("Missed Deadlines");
        metricsTable.AddCell(report.Metrics.MissedDeadlines.ToString());
        metricsTable.AddCell("Deadline Miss Ratio");
        metricsTable.AddCell($"{report.Metrics.DeadlineMissRatio:F2}%");
        document.Add(metricsTable);
        document.Add(new Paragraph(" "));

        document.Add(new Paragraph("Process Results", headerFont));
        var processTable = new PdfPTable(7);
        processTable.WidthPercentage = 100;
        processTable.AddCell(new Phrase("PID", boldFont));
        processTable.AddCell(new Phrase("Name", boldFont));
        processTable.AddCell(new Phrase("Arrival", boldFont));
        processTable.AddCell(new Phrase("Burst", boldFont));
        processTable.AddCell(new Phrase("Completion", boldFont));
        processTable.AddCell(new Phrase("Turnaround", boldFont));
        processTable.AddCell(new Phrase("Waiting", boldFont));

        var results = (await _resultRepository.GetBySessionIdAsync(sessionId)).ToList();
        foreach (var result in results)
        {
            processTable.AddCell(result.ProcessId);
            processTable.AddCell(result.ProcessName);
            processTable.AddCell(result.ArrivalTime.ToString());
            processTable.AddCell(result.BurstTime.ToString());
            processTable.AddCell(result.CompletionTime.ToString());
            processTable.AddCell(result.TurnaroundTime.ToString());
            processTable.AddCell(result.WaitingTime.ToString());
        }
        document.Add(processTable);

        document.Close();
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateExcelReportAsync(int sessionId)
    {
        var report = await GenerateReportAsync(sessionId);

        using var workbook = new XLWorkbook();
        var metricsSheet = workbook.Worksheets.Add("Metrics");
        metricsSheet.Cell(1, 1).Value = "Metric";
        metricsSheet.Cell(1, 2).Value = "Value";
        metricsSheet.Cell(2, 1).Value = "Average Waiting Time";
        metricsSheet.Cell(2, 2).Value = report.Metrics.AverageWaitingTime;
        metricsSheet.Cell(3, 1).Value = "Average Turnaround Time";
        metricsSheet.Cell(3, 2).Value = report.Metrics.AverageTurnaroundTime;
        metricsSheet.Cell(4, 1).Value = "Average Response Time";
        metricsSheet.Cell(4, 2).Value = report.Metrics.AverageResponseTime;
        metricsSheet.Cell(5, 1).Value = "CPU Utilization (%)";
        metricsSheet.Cell(5, 2).Value = report.Metrics.CpuUtilization;
        metricsSheet.Cell(6, 1).Value = "Throughput";
        metricsSheet.Cell(6, 2).Value = report.Metrics.Throughput;
        metricsSheet.Cell(7, 1).Value = "Context Switches";
        metricsSheet.Cell(7, 2).Value = report.Metrics.ContextSwitchCount;
        metricsSheet.Cell(8, 1).Value = "Missed Deadlines";
        metricsSheet.Cell(8, 2).Value = report.Metrics.MissedDeadlines;
        metricsSheet.Cell(9, 1).Value = "Deadline Miss Ratio (%)";
        metricsSheet.Cell(9, 2).Value = report.Metrics.DeadlineMissRatio;
        metricsSheet.Columns().AdjustToContents();

        var processSheet = workbook.Worksheets.Add("Processes");
        processSheet.Cell(1, 1).Value = "PID";
        processSheet.Cell(1, 2).Value = "Name";
        processSheet.Cell(1, 3).Value = "Arrival Time";
        processSheet.Cell(1, 4).Value = "Burst Time";
        processSheet.Cell(1, 5).Value = "Deadline";
        processSheet.Cell(1, 6).Value = "Priority";

        int row = 2;
        foreach (var process in report.Processes)
        {
            processSheet.Cell(row, 1).Value = process.ProcessId;
            processSheet.Cell(row, 2).Value = process.Name;
            processSheet.Cell(row, 3).Value = process.ArrivalTime;
            processSheet.Cell(row, 4).Value = process.BurstTime;
            processSheet.Cell(row, 5).Value = process.Deadline;
            processSheet.Cell(row, 6).Value = process.Priority;
            row++;
        }
        processSheet.Columns().AdjustToContents();

        var resultsSheet = workbook.Worksheets.Add("Results");
        resultsSheet.Cell(1, 1).Value = "PID";
        resultsSheet.Cell(1, 2).Value = "Name";
        resultsSheet.Cell(1, 3).Value = "Arrival";
        resultsSheet.Cell(1, 4).Value = "Burst";
        resultsSheet.Cell(1, 5).Value = "Completion";
        resultsSheet.Cell(1, 6).Value = "Turnaround";
        resultsSheet.Cell(1, 7).Value = "Waiting";
        resultsSheet.Cell(1, 8).Value = "Response";
        resultsSheet.Cell(1, 9).Value = "Missed Deadline";

        var results = (await _resultRepository.GetBySessionIdAsync(sessionId)).ToList();
        row = 2;
        foreach (var result in results)
        {
            resultsSheet.Cell(row, 1).Value = result.ProcessId;
            resultsSheet.Cell(row, 2).Value = result.ProcessName;
            resultsSheet.Cell(row, 3).Value = result.ArrivalTime;
            resultsSheet.Cell(row, 4).Value = result.BurstTime;
            resultsSheet.Cell(row, 5).Value = result.CompletionTime;
            resultsSheet.Cell(row, 6).Value = result.TurnaroundTime;
            resultsSheet.Cell(row, 7).Value = result.WaitingTime;
            resultsSheet.Cell(row, 8).Value = result.ResponseTime;
            resultsSheet.Cell(row, 9).Value = result.IsMissedDeadline;
            row++;
        }
        resultsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
