using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using EAEmployee.Net8.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public EmployeeController(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private const int PageSize = 5;

    // GET: Employee
    public async Task<IActionResult> Index(string? searchTerm, string? emailTerm, int? gradeFilter, int page = 1)
    {
        var employees = _db.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            employees = employees.Where(e => e.Name.StartsWith(searchTerm));

        if (!string.IsNullOrEmpty(emailTerm))
            employees = employees.Where(e => e.Email.Contains(emailTerm));

        if (gradeFilter.HasValue)
            employees = employees.Where(e => e.Grade == gradeFilter.Value);

        ViewBag.SearchTerm = searchTerm;
        ViewBag.EmailTerm = emailTerm;
        ViewBag.GradeFilter = gradeFilter;
        var paginatedList = await PaginatedList<Employee>.CreateAsync(employees, page, PageSize);
        return View(paginatedList);
    }

    // GET: Employee/ExportCsv
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ExportCsv(string? searchTerm, string? emailTerm, int? gradeFilter)
    {
        var employees = _db.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            employees = employees.Where(e => e.Name.StartsWith(searchTerm));

        if (!string.IsNullOrEmpty(emailTerm))
            employees = employees.Where(e => e.Email.Contains(emailTerm));

        if (gradeFilter.HasValue)
            employees = employees.Where(e => e.Grade == gradeFilter.Value);

        var list = await employees.OrderBy(e => e.Name).ToListAsync();

        // Build CSV content
        var csvLines = new List<string> { "Id,Name,Salary,Age,DurationWorked,Grade,Email" };
        foreach (var e in list.OrderBy(x => x.Name))
            csvLines.Add($"{e.Id},\"{e.Name}\",{e.Salary:F2},{e.Age},{e.DurationWorked},{GetGradeLabel(e.Grade)},\"{e.Email}\"");

        var csvContent = string.Join("\r\n", csvLines);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

        // Audit the export action
        await _audit.LogAsync("EmployeeExport", "ExportCsv", null, $"Exported {list.Count} employee(s) to CSV");

        return File(bytes, "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    private static string GetGradeLabel(int grade) => grade switch
    {
        1 => "Junior",
        2 => "Middle",
        3 => "Senior",
        4 => "C-Level",
        _ => grade.ToString()
    };

    // GET: Employee/Create
    [Authorize(Roles = "Administrator")]
    public IActionResult Create() => View();

    // POST: Employee/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            var existing = await _db.Employees
                .FirstOrDefaultAsync(e => e.Email == employee.Email);
            if (existing != null)
            {
                return Json(new
                {
                    isDuplicate = true,
                    employee = new
                    {
                        existing.Id,
                        existing.Name,
                        existing.Email,
                        existing.Salary,
                        existing.Age,
                        existing.DurationWorked,
                        existing.Grade
                    }
                });
            }

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            // Audit log
            await _audit.LogWithValuesAsync(
                "Employee", "Create", employee.Id, null, new { employee.Name, employee.Email, employee.Salary, employee.Grade },
                $"Created employee: {employee.Name}");

            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // GET: Employee/Edit/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    // POST: Employee/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return NotFound();

        var original = await _db.Employees.FindAsync(id);
        if (original == null) return NotFound();

        if (ModelState.IsValid)
        {
            // Capture old values before saving
            var oldValues = new
            {
                Name = original.Name,
                Salary = original.Salary,
                Age = original.Age,
                DurationWorked = original.DurationWorked,
                Grade = original.Grade,
                Email = original.Email
            };

            // Update the tracked entity's properties instead of attaching a new instance
            original.Name = employee.Name;
            original.Salary = employee.Salary;
            original.Age = employee.Age;
            original.DurationWorked = employee.DurationWorked;
            original.Grade = employee.Grade;
            original.Email = employee.Email;

            await _db.SaveChangesAsync();

            // Audit log with old/new values
            var newValues = new { original.Name, original.Email, original.Salary, original.Grade };
            await _audit.LogWithValuesAsync(
                "Employee", "Update", id, oldValues, newValues,
                $"Updated employee: {original.Name}");

            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // GET: Employee/Delete/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    // POST: Employee/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee != null)
        {
            // Audit before deleting
            await _audit.LogWithValuesAsync(
                "Employee", "Delete", id, new { employee.Name, employee.Email, employee.Salary }, null,
                $"Deleted employee: {employee.Name}");

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
