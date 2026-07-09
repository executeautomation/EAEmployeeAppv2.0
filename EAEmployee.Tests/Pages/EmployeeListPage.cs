using EAEmployee.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Employee</c> (list with search + pagination).</summary>
public class EmployeeListPage
{
    private readonly IPage _page;

    public EmployeeListPage(IPage page) => _page = page;

    public Task OpenAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Employee",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator PageTitle => _page.Locator(".page-title");
    public ILocator TotalBadge => _page.Locator(".stat-badge");
    public ILocator NewEmployeeButton => _page.Locator("a.btn-create");
    public ILocator NameSearchInput => _page.Locator("input[name='searchTerm']");
    public ILocator EmailSearchInput => _page.Locator("input[name='emailTerm']");
    public ILocator GradeFilter => _page.Locator("select[name='gradeFilter']");
    public ILocator SearchButton => _page.Locator("button.btn-search");
    public ILocator ClearLink => _page.Locator("a[href*='/Employee']", new() { HasText = "Clear" });
    public ILocator EmptyState => _page.Locator(".empty-state");
    public ILocator Rows => _page.Locator(".employee-table-card tbody tr");
    public ILocator PaginationInfo => _page.Locator(".page-info");
    public ILocator PageNav => _page.Locator(".page-nav");
    public ILocator ActivePageButton => _page.Locator(".page-btn.active");
    public ILocator EditLink(string employeeName) =>
        _page.Locator("tr", new() { Has = _page.Locator(".emp-name", new() { HasText = employeeName }) })
             .Locator("a.btn-edit");
    public ILocator DeleteLink(string employeeName) =>
        _page.Locator("tr", new() { Has = _page.Locator(".emp-name", new() { HasText = employeeName }) })
             .Locator("a.btn-del");

    public async Task SearchByNameAsync(string name)
    {
        await NameSearchInput.FillAsync(name);
        await SearchButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    public async Task SearchByEmailAsync(string email)
    {
        await EmailSearchInput.FillAsync(email);
        await SearchButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    public async Task FilterByGradeAsync(int grade)
    {
        await GradeFilter.SelectOptionAsync(grade.ToString());
        await SearchButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    public async Task ClickPageAsync(int page)
    {
        await _page.Locator(".page-btn", new() { HasText = page.ToString() }).ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    public async Task ClickCreateAsync() => await NewEmployeeButton.ClickAsync();

    public async Task<int> RowCountAsync() => await Rows.CountAsync();

    public async Task<string[]> EmployeeNamesAsync()
    {
        var names = await _page.Locator(".emp-name").AllTextContentsAsync();
        return names.Select(n => n.Trim()).ToArray();
    }

    public async Task ShouldBeLoadedAsync()
    {
        await PageTitle.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
        (await PageTitle.TextContentAsync()).Should().Contain("Employees");
    }
}
