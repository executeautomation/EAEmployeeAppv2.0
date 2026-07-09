using EAEmployee.Tests.Fixtures;
using Microsoft.Playwright;

namespace EAEmployee.Tests.Pages;

/// <summary>Page object for <c>/Employee/Create</c>.</summary>
public class EmployeeCreatePage
{
    private readonly IPage _page;

    public EmployeeCreatePage(IPage page) => _page = page;

    public Task OpenAsync() => _page.GotoAsync(
        AppFixture.BaseUrl.TrimEnd('/') + "/Employee/Create",
        new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

    public ILocator PageHeader => _page.Locator(".form-page-header h2");
    public ILocator NameInput => _page.Locator("input[name='Name']");
    public ILocator AgeInput => _page.Locator("input[name='Age']");
    public ILocator SalaryInput => _page.Locator("input[name='Salary']");
    public ILocator DurationInput => _page.Locator("input[name='DurationWorked']");
    public ILocator GradeSelect => _page.Locator("select[name='Grade']");
    public ILocator EmailInput => _page.Locator("input[name='Email']");
    public ILocator SubmitButton => _page.Locator("button.btn-submit");
    public ILocator CancelLink => _page.Locator("a.btn-cancel");
    public ILocator ValidationSummary => _page.Locator(".alert.alert-danger");
    public ILocator FieldErrorAge => _page.Locator("span[data-valmsg-for='Age']");
    public ILocator FieldErrorEmail => _page.Locator("span[data-valmsg-for='Email']");
    public ILocator DuplicateModal => _page.Locator("#duplicateModal");
    public ILocator DuplicateModalTitle => _page.Locator("#duplicateModal .modal-title, #duplicateModal h5");
    public ILocator DuplicateEditLink => _page.Locator("#dupEditLink");

    public async Task FillAsync(
        string name,
        int age,
        decimal salary,
        int durationWorked,
        int grade,
        string email)
    {
        await NameInput.FillAsync(name);
        await AgeInput.FillAsync(age.ToString());
        await SalaryInput.FillAsync(salary.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await DurationInput.FillAsync(durationWorked.ToString());
        await GradeSelect.SelectOptionAsync(grade.ToString());
        await EmailInput.FillAsync(email);
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }
}
