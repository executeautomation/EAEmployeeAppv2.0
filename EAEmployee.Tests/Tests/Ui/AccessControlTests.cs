using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.None)] // mutates shared state (registers a new user)
public class AccessControlTests : AuthenticatedPageBase
{
    [Test]
    public async Task Anonymous_User_Is_Redirected_To_Login_When_Accessing_Manage()
    {
        await NavigateToAsync("/Manage");
        Page.Url.Should().Contain("/Account/Login",
            "anonymous users hitting a protected page should be redirected to login");
        Page.Url.Should().Contain("returnUrl=",
            "the redirect should preserve the original URL as returnUrl");
    }

    [Test]
    public async Task Anonymous_User_Is_Redirected_To_Login_When_Accessing_EmployeeDetails()
    {
        await NavigateToAsync("/EmployeeDetails");
        Page.Url.Should().Contain("/Account/Login",
            "/EmployeeDetails is restricted to Administrator + User roles");
    }

    [Test]
    public async Task Non_Admin_Cannot_Access_Create_Page()
    {
        var (userName, _) = await RegisterFreshUserAsync();

        // Newly-registered user is in the User role — should NOT see /Employee/Create.
        await NavigateToAsync("/Employee/Create");
        // ASP.NET Core returns 403 and the framework may render a 403 page or
        // redirect; either way, we must NOT be on the create form.
        var create = new EmployeeCreatePage(Page);
        var onCreate = await create.PageHeader.IsVisibleAsync();
        onCreate.Should().BeFalse(
            $"the freshly-registered user '{userName}' (User role) must not reach the Create form");
    }

    [Test]
    public async Task Non_Admin_Does_Not_See_Create_Button_On_List()
    {
        await RegisterFreshUserAsync();

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();

        (await list.NewEmployeeButton.CountAsync())
            .Should().Be(0,
                "a non-admin user should not see the + New Employee button");
    }

    [Test]
    public async Task Non_Admin_Can_Access_Employee_Details_Index()
    {
        await RegisterFreshUserAsync();

        var details = new EmployeeDetailsPage(Page);
        await details.OpenIndexAsync();
        await details.PageTitle.WaitForAsync(
            new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 5_000 });
        (await details.PageTitle.IsVisibleAsync())
            .Should().BeTrue("User role should be able to open the EmployeeDetails index");
    }
}
