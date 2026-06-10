using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERDRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "AdminDashboard", new { area = "Admin" });
    }
}
