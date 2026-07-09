using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers;

[Authorize(Roles = "Administrator")]
public class AuditLogController : Controller
{
    private readonly ApplicationDbContext _db;

    public AuditLogController(ApplicationDbContext db) => _db = db;

    // GET: /AuditLog
    public async Task<IActionResult> Index(int? entityTypeFilter, int page = 1)
    {
        var logs = _db.AuditLogs.AsQueryable();

        if (entityTypeFilter.HasValue && entityTypeFilter.Value > 0)
            logs = logs.Where(l => l.EntityType == "Employee");

        ViewBag.EntityTypeFilter = entityTypeFilter;

        const int pageSize = 20;
        return View(await PaginatedList<AuditLog>.CreateAsync(logs.OrderByDescending(l => l.Timestamp), page, pageSize));
    }

    // GET: /AuditLog/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var log = await _db.AuditLogs.FindAsync(id);
        if (log == null) return NotFound();
        return View(log);
    }
}
