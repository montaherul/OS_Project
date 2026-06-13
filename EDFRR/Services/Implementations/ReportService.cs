using System.Text;
using ClosedXML.Excel;
using EDFRR.Models.DTOs;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EDFRR.Services.Implementations;

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
        var session = await _sessionRepository.GetByIdAsync(sessionId);

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4, 28, 28, 28, 28);
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();

        var headingFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15);
        var chipFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
        var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var tableFont = FontFactory.GetFont(FontFactory.COURIER, 9);
        var logFont = FontFactory.GetFont(FontFactory.COURIER, 9);
        var logBodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);

        var white = new BaseColor(255, 255, 255);
        var surface = new BaseColor(249, 250, 251);
        var borderColor = new BaseColor(222, 226, 230);
        var textDark = new BaseColor(33, 37, 41);
        var textMuted = new BaseColor(108, 117, 125);
        var green = new BaseColor(16, 185, 129);
        var greenBg = new BaseColor(209, 250, 229);
        var red = new BaseColor(239, 68, 68);
        var redBg = new BaseColor(254, 226, 226);
        var amber = new BaseColor(245, 158, 11);
        var amberBg = new BaseColor(254, 243, 199);
        var purple = new BaseColor(139, 92, 246);
        var purpleBg = new BaseColor(237, 233, 254);

        // ─── HEADER ───────────────────────────────────────────────────────

        var titlePara = new Paragraph("EDFRR - CPU scheduling report", headingFont)
        {
            SpacingAfter = 4
        };
        document.Add(titlePara);

        var metaFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        document.Add(new Paragraph(report.SessionName, metaFont) { SpacingAfter = 2 });
        document.Add(new Paragraph($"Generated {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}", metaFont) { SpacingAfter = 10 });

        // Chips row
        var chips = new[]
        {
            (report.AlgorithmType, purple, purpleBg),
            (session?.IsPreemptive == true ? "Preemptive" : "Non-preemptive", purple, purpleBg),
            (session != null ? $"Quantum: {session.TimeQuantum}" : "", purple, purpleBg),
            ($"{report.Metrics.TotalProcesses} processes", purple, purpleBg)
        };

        var chipsTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 18 };
        chipsTable.SetWidths([1f, 1f, 1f, 1f]);
        foreach (var (text, fg, bg) in chips)
        {
            var chipCell = new PdfPCell
            {
                BackgroundColor = purpleBg,
                Border = Rectangle.NO_BORDER,
                Padding = 6,
                PaddingLeft = 10,
                PaddingRight = 10,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            chipCell.Phrase = new Phrase(text, chipFont);
            chipCell.Phrase.Font.Color = purple;
            chipsTable.AddCell(chipCell);
        }
        document.Add(chipsTable);

        // ─── PERFORMANCE METRICS ─────────────────────────────────────────

        var metricsHeading = new Paragraph("Performance metrics", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
        {
            SpacingAfter = 10
        };
        document.Add(metricsHeading);

        var metricDefs = new[]
        {
            ("Avg waiting time", report.Metrics.AverageWaitingTime.ToString("F2"), amber),
            ("Avg turnaround time", report.Metrics.AverageTurnaroundTime.ToString("F2"), amber),
            ("Avg response time", report.Metrics.AverageResponseTime.ToString("F2"), amber),
            ("Cpu utilization", $"{report.Metrics.CpuUtilization:F2}%", green),
            ("Throughput", report.Metrics.Throughput.ToString("F4"), green),
            ("Context switches", report.Metrics.ContextSwitchCount.ToString(), amber),
            ("Missed deadlines", report.Metrics.MissedDeadlines.ToString(), red),
            ("Deadline miss ratio", $"{report.Metrics.DeadlineMissRatio:F2}%", red)
        };

        var metricsTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
        metricsTable.SetWidths([1f, 1f, 1f, 1f]);
        foreach (var (label, value, color) in metricDefs)
        {
            var cell = new PdfPCell
            {
                Border = Rectangle.BOX,
                BorderColor = borderColor,
                BorderWidth = 0.5f,
                Padding = 10,
                PaddingTop = 7,
                PaddingBottom = 12,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = surface
            };
            var labelPhrase = new Phrase(label, labelFont) { Font = { Color = textMuted } };
            var valuePhrase = new Phrase($"\n{value}", valueFont) { Font = { Color = color } };
            cell.Phrase = labelPhrase;
            cell.Phrase.Add(valuePhrase);
            metricsTable.AddCell(cell);
        }
        document.Add(metricsTable);

        // ─── PROCESS EXECUTION TABLE ─────────────────────────────────────

        var tableHeading = new Paragraph("Process execution results", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
        {
            SpacingAfter = 10
        };
        document.Add(tableHeading);

        var procTable = new PdfPTable(9) { WidthPercentage = 100, SpacingAfter = 20 };
        procTable.SetWidths([1.2f, 0.7f, 0.7f, 0.9f, 0.9f, 0.7f, 0.8f, 0.8f, 0.8f]);

        var headerBg = new BaseColor(248, 249, 250);
        string[] headers = ["Name", "Arrival", "Burst", "Completion", "Turnaround", "Waiting", "Response", "Deadline", "Status"];
        foreach (var h in headers)
        {
            var hCell = new PdfPCell(new Phrase(h, tableHeaderFont))
            {
                BackgroundColor = headerBg,
                BorderColor = borderColor,
                BorderWidth = 0.5f,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            procTable.AddCell(hCell);
        }

        foreach (var p in report.Processes)
        {
            var isMissed = p.MissedDeadline;
            var rowBg = isMissed ? new BaseColor(254, 242, 242) : white;

            AddProcCell(procTable, p.Name, tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.ArrivalTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.BurstTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.CompletionTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.TurnaroundTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.WaitingTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.ResponseTime.ToString(), tableFont, textDark, borderColor, rowBg);
            AddProcCell(procTable, p.Deadline.ToString(), tableFont, textDark, borderColor, rowBg);

            var statusText = isMissed ? "Missed" : "Met";
            var statusBg = isMissed ? redBg : greenBg;
            var statusFg = isMissed ? red : green;
            var statusCell = new PdfPCell
            {
                BackgroundColor = statusBg,
                BorderColor = borderColor,
                BorderWidth = 0.5f,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            var statusPhrase = new Phrase(statusText, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9));
            statusPhrase.Font.Color = statusFg;
            statusCell.Phrase = statusPhrase;
            procTable.AddCell(statusCell);
        }
        document.Add(procTable);

        // ─── EXECUTION LOG ───────────────────────────────────────────────

        if (report.ExecutionLogs.Count > 0)
        {
            var logHeading = new Paragraph("Execution log", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(logHeading);

            foreach (var log in report.ExecutionLogs)
            {
                var outcome = DetermineOutcome(log.Action);
                var outcomeBg = outcome switch
                {
                    "Completed" => greenBg,
                    "Preempted" => amberBg,
                    _ => redBg
                };
                var outcomeFg = outcome switch
                {
                    "Completed" => green,
                    "Preempted" => amber,
                    _ => red
                };

                var logTable = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 3 };
                logTable.SetWidths([0.5f, 2.5f, 1f]);

                // Timestamp cell
                var tsCell = new PdfPCell(new Phrase($"T={log.TimeStep}", logFont))
                {
                    BorderColor = borderColor,
                    BorderWidth = 0.5f,
                    Padding = 5,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BackgroundColor = surface
                };
                tsCell.Phrase.Font.Color = textMuted;
                logTable.AddCell(tsCell);

                // Description cell — includes algorithm tag + process name in bold + details
                var descPhrase = new Phrase();
                var algoChip = new Chunk(" EDF ", chipFont);
                algoChip.Font.Color = purple;
                algoChip.SetBackground(purpleBg);
                descPhrase.Add(algoChip);
                descPhrase.Add(new Chunk(" ", logBodyFont));
                var pidBold = new Chunk(log.ProcessName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10));
                descPhrase.Add(pidBold);
                descPhrase.Add(new Chunk($"  {log.Details ?? log.Action}", logBodyFont));

                var descCell = new PdfPCell(descPhrase)
                {
                    BorderColor = borderColor,
                    BorderWidth = 0.5f,
                    Padding = 5,
                    PaddingLeft = 8
                };
                logTable.AddCell(descCell);

                // Outcome badge
                var outcomePhrase = new Phrase(outcome, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9));
                outcomePhrase.Font.Color = outcomeFg;
                var outcomeCell = new PdfPCell(outcomePhrase)
                {
                    BackgroundColor = outcomeBg,
                    BorderColor = borderColor,
                    BorderWidth = 0.5f,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                logTable.AddCell(outcomeCell);

                document.Add(logTable);
            }
        }

        document.Close();
        return stream.ToArray();
    }

    private static void AddProcCell(PdfPTable table, string text, Font font, BaseColor textColor, BaseColor borderColor, BaseColor bg)
    {
        var cell = new PdfPCell(new Phrase(text, font))
        {
            BorderColor = borderColor,
            BorderWidth = 0.5f,
            Padding = 5,
            HorizontalAlignment = Element.ALIGN_CENTER,
            BackgroundColor = bg
        };
        cell.Phrase.Font.Color = textColor;
        table.AddCell(cell);
    }

    private static string DetermineOutcome(string action)
    {
        var a = action?.ToLowerInvariant() ?? "";
        if (a.Contains("complet") || a == "completed") return "Completed";
        if (a.Contains("preempt") || a == "preempted") return "Preempted";
        if (a.Contains("miss") || a.Contains("deadline")) return "Missed deadline";
        return "Completed";
    }

    public async Task<byte[]> GenerateExcelReportAsync(int sessionId)
    {
        var report = await GenerateReportAsync(sessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId);

        using var workbook = new XLWorkbook();

        var infoSheet = workbook.Worksheets.Add("Session Info");
        infoSheet.Cell(1, 1).Value = "Property";
        infoSheet.Cell(1, 2).Value = "Value";
        infoSheet.Cell(2, 1).Value = "Session Name";
        infoSheet.Cell(2, 2).Value = report.SessionName;
        infoSheet.Cell(3, 1).Value = "Algorithm";
        infoSheet.Cell(3, 2).Value = report.AlgorithmType;
        if (session != null)
        {
            infoSheet.Cell(4, 1).Value = "Time Quantum";
            infoSheet.Cell(4, 2).Value = session.TimeQuantum;
            infoSheet.Cell(5, 1).Value = "Preemptive";
            infoSheet.Cell(5, 2).Value = session.IsPreemptive ? "Yes" : "No";
        }
        infoSheet.Cell(6, 1).Value = "Total Processes";
        infoSheet.Cell(6, 2).Value = report.Metrics.TotalProcesses;
        infoSheet.Cell(7, 1).Value = "Completed";
        infoSheet.Cell(7, 2).Value = report.Metrics.CompletedProcesses;
        infoSheet.Cell(8, 1).Value = "Generated At";
        infoSheet.Cell(8, 2).Value = report.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss");
        infoSheet.Columns().AdjustToContents();
        var infoHeaderRow = infoSheet.Row(1);
        infoHeaderRow.Cell(1).Style.Font.Bold = true;
        infoHeaderRow.Cell(2).Style.Font.Bold = true;

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
        var metricsHeaderRow = metricsSheet.Row(1);
        metricsHeaderRow.Cell(1).Style.Font.Bold = true;
        metricsHeaderRow.Cell(2).Style.Font.Bold = true;

        var resultsSheet = workbook.Worksheets.Add("Results");
        resultsSheet.Cell(1, 1).Value = "PID";
        resultsSheet.Cell(1, 2).Value = "Name";
        resultsSheet.Cell(1, 3).Value = "Arrival";
        resultsSheet.Cell(1, 4).Value = "Burst";
        resultsSheet.Cell(1, 5).Value = "Deadline";
        resultsSheet.Cell(1, 6).Value = "Completion";
        resultsSheet.Cell(1, 7).Value = "Turnaround";
        resultsSheet.Cell(1, 8).Value = "Waiting";
        resultsSheet.Cell(1, 9).Value = "Response";
        resultsSheet.Cell(1, 10).Value = "Missed Deadline";

        var headerRow = resultsSheet.Row(1);
        for (int c = 1; c <= 10; c++)
            headerRow.Cell(c).Style.Font.Bold = true;

        var results = (await _resultRepository.GetBySessionIdAsync(sessionId)).ToList();
        int row = 2;
        foreach (var result in results)
        {
            resultsSheet.Cell(row, 1).Value = result.ProcessId;
            resultsSheet.Cell(row, 2).Value = result.ProcessName;
            resultsSheet.Cell(row, 3).Value = result.ArrivalTime;
            resultsSheet.Cell(row, 4).Value = result.BurstTime;
            resultsSheet.Cell(row, 5).Value = result.Deadline;
            resultsSheet.Cell(row, 6).Value = result.CompletionTime;
            resultsSheet.Cell(row, 7).Value = result.TurnaroundTime;
            resultsSheet.Cell(row, 8).Value = result.WaitingTime;
            resultsSheet.Cell(row, 9).Value = result.ResponseTime;
            resultsSheet.Cell(row, 10).Value = result.IsMissedDeadline ? "Yes" : "No";
            row++;
        }
        resultsSheet.Columns().AdjustToContents();

        var logs = await _executionLogRepository.GetBySessionIdAsync(sessionId);
        if (logs.Any())
        {
            var logsSheet = workbook.Worksheets.Add("Execution Log");
            logsSheet.Cell(1, 1).Value = "Time Step";
            logsSheet.Cell(1, 2).Value = "Process";
            logsSheet.Cell(1, 3).Value = "Action";
            logsSheet.Cell(1, 4).Value = "Details";
            logsSheet.Cell(1, 5).Value = "Ready Queue";

            var logHeaderRow = logsSheet.Row(1);
            for (int c = 1; c <= 5; c++)
                logHeaderRow.Cell(c).Style.Font.Bold = true;

            int logRow = 2;
            foreach (var log in logs)
            {
                logsSheet.Cell(logRow, 1).Value = log.TimeStep;
                logsSheet.Cell(logRow, 2).Value = log.ExecutingProcessName;
                logsSheet.Cell(logRow, 3).Value = log.Action;
                logsSheet.Cell(logRow, 4).Value = log.Details;
                logsSheet.Cell(logRow, 5).Value = log.ReadyQueueSnapshot;
                logRow++;
            }
            logsSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
