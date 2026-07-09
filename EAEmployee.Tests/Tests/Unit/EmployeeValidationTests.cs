using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EAEmployee.Net8.Models;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Unit;

/// <summary>DataAnnotation tests for the <see cref="Employee"/> model.</summary>
[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.All)]
public class EmployeeValidationTests
{
    private static IList<ValidationResult> Validate(Employee employee)
    {
        var ctx = new ValidationContext(employee);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(employee, ctx, results, validateAllProperties: true);
        return results;
    }

    private static Employee ValidEmployee() => new()
    {
        Name = "Jane Smith",
        Salary = 5000f,
        Age = 30,
        DurationWorked = 24,
        Grade = 2,
        Email = "jane@example.com"
    };

    [Test]
    public void Valid_Employee_Passes_Validation()
    {
        Validate(ValidEmployee()).Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase(null)]
    public void Name_Is_Required(string? name)
    {
        var emp = ValidEmployee();
        emp.Name = name!;
        Validate(emp).Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(Employee.Name));
    }

    [Test]
    public void Salary_Has_Required_Attribute_But_No_Other_Constraint()
    {
        // The model marks Salary as [Required] (the form relies on it) but
        // intentionally has no [Range] — negative salaries are blocked by the
        // database CHECK constraint, not DataAnnotations.
        var prop = typeof(Employee).GetProperty(nameof(Employee.Salary))!;
        prop.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
        prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>().Should().BeNull();
    }

    [TestCase(17)]
    [TestCase(101)]
    [TestCase(0)]
    public void Age_Must_Be_Between_18_And_100(int age)
    {
        var emp = ValidEmployee();
        emp.Age = age;
        var results = Validate(emp);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Employee.Age)));
    }

    [TestCase(18)]
    [TestCase(60)]
    [TestCase(100)]
    public void Age_In_Range_Passes_Validation(int age)
    {
        var emp = ValidEmployee();
        emp.Age = age;
        Validate(emp).Should().BeEmpty();
    }

    [TestCase("not-an-email")]
    [TestCase("")]
    public void Email_Must_Be_A_Valid_Address(string email)
    {
        var emp = ValidEmployee();
        emp.Email = email;
        var results = Validate(emp);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Employee.Email)));
    }

    [Test]
    public void Duration_Worked_Is_Required_But_Has_No_Range()
    {
        // DurationWorked is [Required] only — bounds (≥ 0) are enforced by the
        // database CHECK constraint, not DataAnnotations.
        var prop = typeof(Employee).GetProperty(nameof(Employee.DurationWorked))!;
        prop.GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
        prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RangeAttribute>().Should().BeNull();
    }

    [Test]
    public void Display_Names_Are_Configured_For_All_Fields()
    {
        // Guards the human-readable labels shown by tag helpers.
        typeof(Employee).GetProperty(nameof(Employee.Name))!
            .GetCustomAttribute<DisplayAttribute>()!.Name.Should().Be("Name");
        typeof(Employee).GetProperty(nameof(Employee.Age))!
            .GetCustomAttribute<DisplayAttribute>()!.Name.Should().Be("Age");
        typeof(Employee).GetProperty(nameof(Employee.DurationWorked))!
            .GetCustomAttribute<DisplayAttribute>()!.Name.Should().Be("Duration Worked (months)");
        typeof(Employee).GetProperty(nameof(Employee.Grade))!
            .GetCustomAttribute<DisplayAttribute>()!.Name.Should().Be("Grade");
    }
}
