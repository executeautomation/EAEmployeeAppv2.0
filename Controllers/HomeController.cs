using System.Diagnostics;
using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index() => View();

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public async Task<IActionResult> Dashboard()
    {
        var employees = await _db.Employees.ToListAsync();
        var total = employees.Count;

        ViewBag.TotalEmployees = total;
        ViewBag.AverageSalary = total > 0 ? employees.Average(e => e.Salary) : 0;
        ViewBag.AverageAge = total > 0 ? employees.Average(e => e.Age) : 0;
        ViewBag.TotalSalaryBill = total > 0 ? employees.Sum(e => e.Salary) : 0;

        // Grade breakdown
        ViewBag.JuniorCount = employees.Count(e => e.Grade == 1);
        ViewBag.MiddleCount = employees.Count(e => e.Grade == 2);
        ViewBag.SeniorCount = employees.Count(e => e.Grade == 3);
        ViewBag.CLevelCount = employees.Count(e => e.Grade == 4);

        // Age distribution
        ViewBag.Under30 = employees.Count(e => e.Age < 30);
        ViewBag.Age30to39 = employees.Count(e => e.Age >= 30 && e.Age < 40);
        ViewBag.Age40to49 = employees.Count(e => e.Age >= 40 && e.Age < 50);
        ViewBag.Age50to59 = employees.Count(e => e.Age >= 50 && e.Age < 60);
        ViewBag.Age60Plus = employees.Count(e => e.Age >= 60);

        // Retirement
        ViewBag.RetirementCount = employees.Count(e => e.Age >= 60);
        ViewBag.NearRetirementCount = employees.Count(e => e.Age >= 55 && e.Age < 60);

        // Top earners
        ViewBag.TopEarners = employees.OrderByDescending(e => e.Salary).Take(5).ToList();

        // Most experienced
        ViewBag.MostExperienced = employees.OrderByDescending(e => e.DurationWorked).Take(5).ToList();

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
