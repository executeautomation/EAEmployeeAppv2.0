using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers;

[Authorize(Roles = "Administrator,User")]
public class EmployeeDetailsController : Controller
{
    private readonly ApplicationDbContext _db;

    public EmployeeDetailsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: EmployeeDetails
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var employees = _db.Employees.AsQueryable();
        if (!string.IsNullOrEmpty(searchTerm))
            employees = employees.Where(e => e.Name.StartsWith(searchTerm));

        return View(await employees.ToListAsync());
    }

    // Provident Fund (PF) employee contribution
    // Standard India PF: 12% of Basic Salary per month * months worked
    public async Task<IActionResult> EmployeePF(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var employeeContrib = CalculatePFContribution(employee.Salary, employee.DurationWorked);
        ViewBag.EmployeeContrib = employeeContrib.ToString("F2");

        return View(employee);
    }

    // PF employer (company) contribution
    public async Task<IActionResult> EmployeeBonus(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var employerContrib = CalculateEmployerContribution(employee.Salary, employee.DurationWorked, employee.Grade);
        ViewBag.EmployerContrib = employerContrib.ToString("F2");

        return View(employee);
    }

    /// <summary>Employee PF contribution: 12% of salary per month for DurationWorked months.</summary>
    private static double CalculatePFContribution(float salary, int durationMonths)
        => Math.Round(salary * 0.12 * durationMonths, 2);

    /// <summary>Employer contribution: 12% salary PF + grade-based bonus allowance.</summary>
    private static double CalculateEmployerContribution(float salary, int durationMonths, int grade)
    {
        double pfContrib = salary * 0.12 * durationMonths;
        double gradeBonus = grade * salary * 0.02; // 2% per grade level as bonus allowance
        return Math.Round(pfContrib + gradeBonus, 2);
    }
}
