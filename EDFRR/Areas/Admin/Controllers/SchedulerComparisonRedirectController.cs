using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SchedulerComparisonController : Controller
{
    public IActionResult Index(int? sessionId)
    {
        return RedirectToAction("Index", "SchedulerComparison", new { area = "", sessionId });
    }
}
