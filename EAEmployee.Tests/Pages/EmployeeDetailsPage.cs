using EAEmployee.Tests.Fixtures;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for the <c>/EmployeeDetails</c> views (Index, EmployeePF, EmployeeBonus).</summary>
public class EmployeeDetailsPage
{
    private readonly IPage _page;

    public EmployeeDetailsPage(IPage page) => _page = page;

    public Task OpenIndexAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/EmployeeDetails",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public Task OpenPFAsync(int id) => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/EmployeeDetails/EmployeePF/" + id,
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public Task OpenBonusAsync(int id) => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/EmployeeDetails/EmployeeBonus/" + id,
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator PageTitle => _page.Locator(".page-title");
    public ILocator Rows => _page.Locator(".employee-table-card tbody tr");
    public ILocator PFLinkForRow(string name) =>
        _page.Locator("tr", new() { Has = _page.Locator(".emp-name", new() { HasText = name }) })
             .Locator("a.btn-pf");
    public ILocator BonusLinkForRow(string name) =>
        _page.Locator("tr", new() { Has = _page.Locator(".emp-name", new() { HasText = name }) })
             .Locator("a.btn-bonus");
    public ILocator PFContributionValue =>
        _page.Locator("text=Employee PF Contribution").Locator("..").Locator("dd, .pf-value, strong");
}
