using EAEmployee.Tests.Fixtures;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Employee/Delete/{id}</c>.</summary>
public class EmployeeDeletePage
{
    private readonly IPage _page;

    public EmployeeDeletePage(IPage page) => _page = page;

    public Task OpenAsync(int id) => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Employee/Delete/" + id,
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator PageHeader => _page.Locator("h2");
    public ILocator ConfirmPrompt => _page.Locator("h3.text-danger");
    public ILocator DeleteButton => _page.Locator("button.btn-danger");
    public ILocator CancelLink => _page.Locator("a.btn-secondary");

    public async Task ConfirmDeleteAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }
}
