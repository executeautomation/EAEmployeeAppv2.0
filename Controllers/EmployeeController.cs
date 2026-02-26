using EAEmployee.Net8.Data;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _db;

    public EmployeeController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: Employee
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var employees = _db.Employees.AsQueryable();
        if (!string.IsNullOrEmpty(searchTerm))
            employees = employees.Where(e => e.Name.StartsWith(searchTerm));

        ViewBag.SearchTerm = searchTerm;
        return View(await employees.ToListAsync());
    }

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
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
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

        if (ModelState.IsValid)
        {
            _db.Entry(employee).State = EntityState.Modified;
            await _db.SaveChangesAsync();
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
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
