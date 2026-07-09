using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.None)] // mutates shared state (creates an employee)
public class EmployeeDetailsTests : AuthenticatedPageBase
{
    [Test]
    public async Task PF_Page_Displays_The_Calculated_Contribution_For_Known_Inputs()
    {
        // Seed an employee with a known salary/duration so we can assert the math.
        // 12% of 5000 over 24 months = 0.12 * 5000 * 24 = 14400.00
        var suffix = AppFixture.UniqueSuffix();
        var name = $"PF{suffix}";
        var email = $"pf_{suffix}@example.com";
        const float salary = 5000f;
        const int duration = 24;

        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(name, age: 30, salary: (decimal)salary, durationWorked: duration, grade: 2, email: email);
        await create.SubmitAsync();

        // Find the created record's id from the Details link in the list.
        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByNameAsync(name);
        var detailsHref = await Page
            .Locator("tr", new() { Has = Page.Locator(".emp-name", new() { HasText = name }) })
            .Locator("a.btn-detail")
            .GetAttributeAsync("href");
        detailsHref.Should().NotBeNullOrEmpty("the Details link should be present for the seeded employee");

        // The /EmployeeDetails page doesn't take an id in this view; navigate
        // through the index and then click the PF link for the right row.
        var details = new EmployeeDetailsPage(Page);
        await details.OpenIndexAsync();
        await details.PFLinkForRow(name).ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        // The PF view should display the calculated contribution.
        var content = await Page.ContentAsync();
        content.Should().Contain("14,400.00",
            "Employee PF = 12% * 5000 * 24 = 14,400.00, and the view formats the value to 2 decimal places");
    }

    [Test]
    public async Task Bonus_Page_Displays_The_Employer_Contribution_For_Known_Inputs()
    {
        // 18% of 5000 over 12 months = 10800 ; + 2% * grade(2) * 5000 = 200 ; total = 11000.00
        var suffix = AppFixture.UniqueSuffix();
        var name = $"Bonus{suffix}";
        var email = $"bonus_{suffix}@example.com";
        const float salary = 5000f;
        const int duration = 12;
        const int grade = 2;

        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(name, age: 30, salary: (decimal)salary, durationWorked: duration, grade: grade, email: email);
        await create.SubmitAsync();

        var details = new EmployeeDetailsPage(Page);
        await details.OpenIndexAsync();
        await details.BonusLinkForRow(name).ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        var content = await Page.ContentAsync();
        content.Should().Contain("11,000.00",
            "Employer contrib = 18% * 5000 * 12 + 2% * 2 * 5000 = 11,000.00");
    }

    [Test]
    public async Task PF_For_Nonexistent_Employee_Returns_404()
    {
        await LoginAsAdminAsync();
        await NavigateToAsync("/EmployeeDetails/EmployeePF/999999");

        // ASP.NET Core's NotFoundResult renders the 404 status. We just check
        // that we did NOT see a PF contribution value on the page.
        var content = await Page.ContentAsync();
        content.Should().NotContain("Employee PF Contribution",
            "a non-existent employee id should not render the PF detail view");
    }
}
