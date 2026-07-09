using System.Reflection;
using EAEmployee.Net8.Controllers;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Unit;

/// <summary>
/// Unit tests for the Provident Fund and employer-contribution math in
/// <see cref="EmployeeDetailsController"/>. The calculation helpers are
/// private static — we reach them via reflection to keep the suite fast
/// (no DB, no HTTP, no Identity) and to lock in the business rules.
/// </summary>
[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.All)]
public class PfCalculationTests
{
    // Reflect once per test process; the binding flags are stable.
    private static readonly MethodInfo PfMethod = typeof(EmployeeDetailsController)
        .GetMethod("CalculatePFContribution", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("CalculatePFContribution not found via reflection");

    private static readonly MethodInfo EmployerMethod = typeof(EmployeeDetailsController)
        .GetMethod("CalculateEmployerContribution", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("CalculateEmployerContribution not found via reflection");

    private static double CallPF(float salary, int months)
        => (double)PfMethod.Invoke(null, new object?[] { salary, months })!;

    private static double CallEmployer(float salary, int months, int grade)
        => (double)EmployerMethod.Invoke(null, new object?[] { salary, months, grade })!;

    [TestCase(5000f, 24, 14400.00)]   // 0.12 * 5000 * 24
    [TestCase(10000f, 12, 14400.00)]  // 0.12 * 10000 * 12
    [TestCase(0f, 12, 0.00)]          // zero salary → zero PF
    [TestCase(5000f, 0, 0.00)]        // zero months → zero PF
    [TestCase(3333.33f, 6, 2399.9976)] // fractional salary rounding
    public void Employee_PF_Is_Twelve_Percent_Of_Salary_Times_Months(
        float salary, int months, double expected)
    {
        var actual = CallPF(salary, months);
        actual.Should().BeApproximately(expected, 0.01);
    }

    [Test]
    public void Employer_Contribution_Is_Eighteen_Percent_PF_Plus_Grade_Bonus()
    {
        // 18% * 5000 * 12 = 10800 ; 2% * 2 * 5000 = 200 ; total 11000
        var actual = CallEmployer(5000f, 12, grade: 2);
        actual.Should().BeApproximately(11000.00, 0.01);
    }

    [Test]
    public void Employer_Contribution_Scales_Linearly_With_Grade()
    {
        // Same salary/months, double the grade → +2% of salary extra.
        var baseContribution = CallEmployer(5000f, 12, grade: 1);
        var doubleGrade = CallEmployer(5000f, 12, grade: 2);
        (doubleGrade - baseContribution).Should().BeApproximately(100.00, 0.01,
            "raising the grade by 1 should add exactly 2% of monthly salary");
    }

    [Test]
    public void Employer_Contribution_With_Zero_Months_Is_Only_Grade_Bonus()
    {
        // 18% * 5000 * 0 = 0 ; 2% * 4 * 5000 = 400 ; total 400
        var actual = CallEmployer(5000f, 0, grade: 4);
        actual.Should().BeApproximately(400.00, 0.01);
    }

    [Test]
    public void Employer_Contribution_With_Grade_Zero_Has_No_Bonus()
    {
        // 18% * 5000 * 12 = 10800 ; 2% * 0 * 5000 = 0 ; total 10800
        var actual = CallEmployer(5000f, 12, grade: 0);
        actual.Should().BeApproximately(10800.00, 0.01);
    }

    [Test]
    public void PF_And_Employer_Are_Always_Both_Positive_For_Positive_Inputs()
    {
        var cases = new[]
        {
            (salary: 1000f, months: 1, grade: 1),
            (salary: 9999f, months: 60, grade: 4),
            (salary: 0.01f, months: 1, grade: 1),
        };
        foreach (var (salary, months, grade) in cases)
        {
            CallPF(salary, months).Should().BeGreaterThanOrEqualTo(0);
            CallEmployer(salary, months, grade).Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
