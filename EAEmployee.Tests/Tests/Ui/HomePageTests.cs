using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public class HomePageTests : PlaywrightTestBase
{
    [Test]
    public async Task Home_Page_Loads_For_Anonymous_User()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();
        await home.ShouldBeLoadedAsync();
    }

    [Test]
    public async Task Home_Page_Shows_Employee_Feature_Card_And_Cta()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        (await home.EmployeeCard.IsVisibleAsync())
            .Should().BeTrue("the Employee Management card should be on the home page");

        (await home.EmployeeCard.Locator("a", new() { HasText = "Browse Employees" }).IsVisibleAsync())
            .Should().BeTrue("the feature card should link to the employee list");
    }

    [Test]
    public async Task Home_Page_Shows_Login_And_Register_For_Anonymous_Users()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        (await home.LoginLink.IsVisibleAsync())
            .Should().BeTrue("anonymous users should see a Login link in the nav");
        (await home.RegisterLink.IsVisibleAsync())
            .Should().BeTrue("anonymous users should see a Register link in the nav");
    }

    [Test]
    public async Task Dashboard_Loads_And_Shows_Kpi_Cards_Or_Empty_State()
    {
        var home = new HomePage(Page);
        await home.OpenDashboardAsync();

        // Either the empty state OR the KPI cards are valid (depends on whether
        // the running test DB has any employees yet).
        var empty = home.EmptyState;
        var stats = Page.Locator(".stat-cards");

        var emptyVisible = await empty.IsVisibleAsync();
        var statsVisible = await stats.IsVisibleAsync();
        (emptyVisible || statsVisible)
            .Should().BeTrue("the dashboard should render either the empty state or the KPI cards");
    }

    [Test]
    public async Task About_Page_Loads()
    {
        await NavigateToAsync("/Home/About");
        var h = Page.Locator("h1, h2").First;
        (await h.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task Contact_Page_Loads()
    {
        await NavigateToAsync("/Home/Contact");
        var h = Page.Locator("h1, h2").First;
        (await h.IsVisibleAsync()).Should().BeTrue();
    }
}
