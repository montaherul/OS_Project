using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProcessController : Controller
{
    public IActionResult Import(int sessionId)
    {
        return RedirectToAction("Import", "Process", new { area = "", sessionId });
    }

    public IActionResult ImportResults(int sessionId)
    {
        return RedirectToAction("ImportResults", "Process", new { area = "", sessionId });
    }

    public IActionResult DownloadSampleCsv()
    {
        return RedirectToAction("DownloadSampleCsv", "Process", new { area = "" });
    }

    public IActionResult DownloadSampleExcel()
    {
        return RedirectToAction("DownloadSampleExcel", "Process", new { area = "" });
    }
}
