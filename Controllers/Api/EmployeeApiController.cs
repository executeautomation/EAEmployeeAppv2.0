using EAEmployee.Net8.Data;
using EAEmployee.Net8.Filters;
using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAEmployee.Net8.Controllers.Api;

[ApiController]
[Route("api/employees")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class EmployeeApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EmployeeApiController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns a paginated list of employees with optional filters.</summary>
    /// <param name="searchTerm">Filter by name (starts with)</param>
    /// <param name="emailTerm">Filter by email (contains)</param>
    /// <param name="gradeFilter">Filter by exact grade</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    [HttpGet]
    [ProducesResponseType(typeof(EmployeeListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? searchTerm,
        [FromQuery] string? emailTerm,
        [FromQuery] int? gradeFilter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var query = _db.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(e => e.Name.StartsWith(searchTerm));

        if (!string.IsNullOrEmpty(emailTerm))
            query = query.Where(e => e.Email.Contains(emailTerm));

        if (gradeFilter.HasValue)
            query = query.Where(e => e.Grade == gradeFilter.Value);

        var totalCount = await query.CountAsync();
        var employees = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new EmployeeListResponse
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = employees
        });
    }

    /// <summary>Returns a single employee by ID.</summary>
    /// <param name="id">Employee ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { error = $"Employee with ID {id} not found." });

        return Ok(employee);
    }
}

public class EmployeeListResponse
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<Employee> Data { get; set; } = [];
}
