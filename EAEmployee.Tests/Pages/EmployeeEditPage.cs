using EAEmployee.Tests.Fixtures;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Employee/Edit/{id}</c>.</summary>
public class EmployeeEditPage
{
    private readonly IPage _page;

    public EmployeeEditPage(IPage page) => _page = page;

    public Task OpenAsync(int id) => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Employee/Edit/" + id,
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator PageHeader => _page.Locator(".form-page-header h2");
    public ILocator HeaderSubtitle => _page.Locator(".form-card-header p strong");
    public ILocator NameInput => _page.Locator("input[name='Name']");
    public ILocator AgeInput => _page.Locator("input[name='Age']");
    public ILocator SalaryInput => _page.Locator("input[name='Salary']");
    public ILocator DurationInput => _page.Locator("input[name='DurationWorked']");
    public ILocator GradeSelect => _page.Locator("select[name='Grade']");
    public ILocator EmailInput => _page.Locator("input[name='Email']");
    public ILocator IdInput => _page.Locator("input[name='Id']");
    public ILocator SubmitButton => _page.Locator("button.btn-submit");

    public async Task UpdateFieldAsync(string field, string value)
    {
        var input = field switch
        {
            nameof(EAEmployee.Net8.Models.Employee.Name) => NameInput,
            nameof(EAEmployee.Net8.Models.Employee.Age) => AgeInput,
            nameof(EAEmployee.Net8.Models.Employee.Salary) => SalaryInput,
            nameof(EAEmployee.Net8.Models.Employee.DurationWorked) => DurationInput,
            nameof(EAEmployee.Net8.Models.Employee.Email) => EmailInput,
            nameof(EAEmployee.Net8.Models.Employee.Grade) => GradeSelect,
            _ => throw new ArgumentException($"Unknown field: {field}", nameof(field)),
        };
        if (field == nameof(EAEmployee.Net8.Models.Employee.Grade))
            await GradeSelect.SelectOptionAsync(value);
        else
            await input.FillAsync(value);
    }

    public async Task SaveAsync()
    {
        await SubmitButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }
}
