using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.None)] // mutates shared state (employee table)
public class EmployeeCrudTests : AuthenticatedPageBase
{
    [Test]
    public async Task Admin_Can_Create_Employee_And_See_It_In_The_List()
    {
        await LoginAsAdminAsync();

        var suffix = AppFixture.UniqueSuffix();
        var name = $"Create{suffix}";
        var email = $"create_{suffix}@example.com";

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(name, age: 30, salary: 5500m, durationWorked: 18, grade: 2, email: email);
        await create.SubmitAsync();

        // After successful create we are redirected to /Employee.
        Page.Url.Should().EndWith("/Employee", "successful create redirects to the list page");

        var list = new EmployeeListPage(Page);
        await list.SearchByNameAsync(name);

        var names = await list.EmployeeNamesAsync();
        names.Should().Contain(name, "the newly-created employee should be visible after a name search");
    }

    [Test]
    public async Task Admin_Can_Edit_Employee_Name()
    {
        // Seed an employee to edit.
        var suffix = AppFixture.UniqueSuffix();
        var originalName = $"Edit{suffix}";
        var email = $"edit_{suffix}@example.com";

        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(originalName, age: 35, salary: 6500m, durationWorked: 24, grade: 3, email: email);
        await create.SubmitAsync();

        // Open the list and capture the Id from the Edit link.
        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByNameAsync(originalName);

        var editHref = await list.EditLink(originalName).GetAttributeAsync("href");
        editHref.Should().NotBeNullOrEmpty("the Edit link should be present for an admin user");
        var idStr = editHref!.Split('/').Last();
        var id = int.Parse(idStr);

        var edit = new EmployeeEditPage(Page);
        await edit.OpenAsync(id);
        (await edit.HeaderSubtitle.TextContentAsync())
            .Should().Contain(originalName, "the edit form should show which employee is being edited");

        var newName = $"Edited{suffix}";
        await edit.UpdateFieldAsync(nameof(EAEmployee.Net8.Models.Employee.Name), newName);
        await edit.SaveAsync();

        Page.Url.Should().EndWith("/Employee",
            "successful edit should redirect back to the employee list");

        await list.SearchByNameAsync(newName);
        (await list.EmployeeNamesAsync()).Should().Contain(newName);
    }

    [Test]
    public async Task Admin_Can_Delete_Employee_And_It_Disappears_From_List()
    {
        var suffix = AppFixture.UniqueSuffix();
        var name = $"Delete{suffix}";
        var email = $"delete_{suffix}@example.com";

        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(name, age: 40, salary: 7000m, durationWorked: 36, grade: 4, email: email);
        await create.SubmitAsync();

        var list = new EmployeeListPage(Page);
        await list.OpenAsync();
        await list.SearchByNameAsync(name);

        var deleteHref = await list.DeleteLink(name).GetAttributeAsync("href");
        deleteHref.Should().NotBeNullOrEmpty("the Delete link should be present for an admin user");
        var id = int.Parse(deleteHref!.Split('/').Last());

        var deletePage = new EmployeeDeletePage(Page);
        await deletePage.OpenAsync(id);
        (await deletePage.ConfirmPrompt.IsVisibleAsync())
            .Should().BeTrue("the delete confirmation prompt should be shown");
        await deletePage.ConfirmDeleteAsync();

        Page.Url.Should().EndWith("/Employee",
            "successful delete should redirect back to the employee list");

        await list.SearchByNameAsync(name);
        var names = await list.EmployeeNamesAsync();
        names.Should().NotContain(name,
            "the deleted employee should no longer appear in the list");
    }

    [Test]
    public async Task Create_Page_Shows_Age_Range_Validation_Message()
    {
        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        var suffix = AppFixture.UniqueSuffix();

        await create.FillAsync(
            name: "TooYoung" + suffix,
            age: 10,            // below the [Range(18, 100)] rule
            salary: 4000m,
            durationWorked: 6,
            grade: 1,
            email: $"tooyoung_{suffix}@example.com");
        await create.SubmitAsync();

        // After server-side validation we stay on /Employee/Create.
        Page.Url.Should().Contain("/Employee/Create",
            "an invalid form should re-render the create page");

        // The validation summary or the Age field error should mention the range.
        var ageError = await create.FieldErrorAge.TextContentAsync();
        var summary = create.ValidationSummary;
        var summaryVisible = await summary.CountAsync() > 0;

        var ageErrorText = ageError?.Trim() ?? string.Empty;
        var ok = ageErrorText.Contains("18") || ageErrorText.Contains("100") || summaryVisible;
        ok.Should().BeTrue(
            "the form should surface a validation error for Age outside [18, 100]");
    }

    [Test]
    public async Task Create_With_Duplicate_Email_Shows_Modal_With_Existing_Record()
    {
        var suffix = AppFixture.UniqueSuffix();
        var name = $"Dup{suffix}";
        var email = $"dup_{suffix}@example.com";

        await LoginAsAdminAsync();

        // 1. Create the original record.
        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        await create.FillAsync(name, age: 32, salary: 5000m, durationWorked: 12, grade: 2, email: email);
        await create.SubmitAsync();

        // 2. Try to create a second employee with the SAME email.
        await create.OpenAsync();
        await create.FillAsync(
            name: "DupDifferentName" + suffix,
            age: 28,
            salary: 4000m,
            durationWorked: 8,
            grade: 1,
            email: email); // same email
        await create.SubmitAsync();

        // The Create form's JS intercept handler should pop the duplicate modal.
        var modal = create.DuplicateModal;
        await modal.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 5_000 });
        (await modal.IsVisibleAsync()).Should().BeTrue(
            "submitting a duplicate email should trigger the Duplicate Employee modal");

        // The modal's Edit Existing link should point at the original record.
        var editHref = await create.DuplicateEditLink.GetAttributeAsync("href");
        editHref.Should().NotBeNullOrEmpty("the duplicate modal should expose an Edit link");
        editHref.Should().Contain("/Employee/Edit/", "the link should point at the Employee/Edit action");
    }
}
