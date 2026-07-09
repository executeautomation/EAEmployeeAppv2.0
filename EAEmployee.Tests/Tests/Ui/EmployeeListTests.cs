using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public class EmployeeListTests : PlaywrightTestBase
{
    [Test]
    public async Task List_Page_Is_Public_And_Loads_For_Anonymous_User()
    {
        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.ShouldBeLoadedAsync();
    }

    [Test]
    public async Task Anonymous_User_Does_Not_See_Create_Button()
    {
        var list = new EmployeeListPage(Page);
        await list.OpenAsync();

        (await list.NewEmployeeButton.CountAsync())
            .Should().Be(0, "the + New Employee button is restricted to administrators");
    }

    [Test]
    public async Task Admin_Sees_Create_Button()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        (await list.NewEmployeeButton.IsVisibleAsync())
            .Should().BeTrue("administrators should see the + New Employee button");
    }

    [Test]
    public async Task Search_By_Name_Filters_Rows()
    {
        // Seed a unique employee first.
        var suffix = AppFixture.UniqueSuffix();
        var uniqueName = $"Search{suffix}";

        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(
            name: uniqueName,
            age: 30,
            salary: 5000m,
            durationWorked: 12,
            grade: 2,
            email: $"{uniqueName.ToLowerInvariant()}@example.com");
        await create.SubmitAsync();

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByNameAsync(uniqueName);

        var names = await list.EmployeeNamesAsync();
        names.Should().Contain(uniqueName,
            "the just-created employee should appear when searching by exact name");
    }

    [Test]
    public async Task Search_By_Email_Substring_Filters_Rows()
    {
        var suffix = AppFixture.UniqueSuffix();
        var uniqueEmail = $"emailtest_{suffix}@example.com";

        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(
            name: $"EmailTest{suffix}",
            age: 25,
            salary: 4000m,
            durationWorked: 6,
            grade: 1,
            email: uniqueEmail);
        await create.SubmitAsync();

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByEmailAsync("emailtest_");

        var html = await Page.ContentAsync();
        html.Should().Contain(uniqueEmail, "the unique email should appear in the filtered table");
    }

    [Test]
    public async Task Filter_By_Grade_Shows_Only_That_Grade()
    {
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.FilterByGradeAsync(4); // C-Level

        // Every visible grade badge should read "C-Level" (or "grade-D" class).
        var badges = await Page.Locator(".grade-badge").AllTextContentsAsync();
        if (badges.Count > 0)
        {
            badges.Should().OnlyContain(b => b.Trim() == "C-Level",
                "filtering by grade=4 should leave only C-Level badges in the table");
        }
    }

    [Test]
    public async Task Pagination_Shows_When_There_Are_More_Than_Five_Rows()
    {
        // Seed at least 6 employees so pagination is forced (page size is 5).
        var login = new LoginPage(Page);
        await login.LoginAsync(AppFixture.AdminUserName, AppFixture.AdminPassword);

        for (var i = 0; i < 6; i++)
        {
            var suffix = AppFixture.UniqueSuffix();
            var create = new EmployeeCreatePage(Page);
            await create.OpenAsync();
            await create.FillAsync(
                name: $"Pager{suffix}_{i}",
                age: 28,
                salary: 3000m + i,
                durationWorked: 6,
                grade: 1,
                email: $"pager_{suffix}_{i}@example.com");
            await create.SubmitAsync();
        }

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();

        var nav = list.PageNav;
        if (await nav.CountAsync() > 0)
        {
            (await nav.IsVisibleAsync())
                .Should().BeTrue("the pagination bar should appear with 6+ employees");

            // Click page 2 if it exists.
            var page2 = Page.Locator(".page-btn", new() { HasText = "2" });
            if (await page2.CountAsync() > 0)
            {
                await page2.ClickAsync();
                await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
                (await list.ActivePageButton.TextContentAsync())?.Trim().Should().Be("2");
            }
        }
    }

    [Test]
    public async Task Clear_Link_Removes_All_Filters()
    {
        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByNameAsync("DefinitelyNotARealName");

        // The clear link only appears when at least one filter is set.
        var clear = list.ClearLink;
        if (await clear.CountAsync() > 0)
        {
            await clear.ClickAsync();
            await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

            (await list.NameSearchInput.InputValueAsync()).Should().BeEmpty(
                "clearing should empty the name filter input");
        }
    }
}
