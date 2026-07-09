using EAEmployee.Tests.Fixtures;
using EAEmployee.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Ui;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.None)]
public class FormValidationTests : AuthenticatedPageBase
{
    [Test]
    public async Task Create_Form_Requires_All_Fields()
    {
        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();

        // Submit with all fields blank.
        await create.SubmitAsync();

        Page.Url.Should().Contain("/Employee/Create",
            "an empty form should re-render with validation errors");

        var summary = create.ValidationSummary;
        var summaryCount = await summary.CountAsync();
        var summaryText = summaryCount > 0 ? (await summary.TextContentAsync() ?? string.Empty) : string.Empty;
        var hasInlineErrors = await Page.Locator(".text-danger, .validation-msg, .field-validation")
            .CountAsync() > 0;

        (summaryCount > 0 || hasInlineErrors || summaryText.Contains("required", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("the form should surface at least one required-field error");
    }

    [Test]
    public async Task Create_Form_Rejects_Invalid_Email()
    {
        await LoginAsAdminAsync();

        var create = new EmployeeCreatePage(Page);
        await create.OpenAsync();
        var suffix = AppFixture.UniqueSuffix();

        await create.FillAsync(
            name: "BadEmail" + suffix,
            age: 30,
            salary: 4000m,
            durationWorked: 6,
            grade: 1,
            email: "not-a-valid-email"); // missing @ and domain

        await create.SubmitAsync();
        Page.Url.Should().Contain("/Employee/Create",
            "an invalid email should keep the user on the create form");

        var summaryText = await create.ValidationSummary.TextContentAsync();
        var emailErr = await create.FieldErrorEmail.TextContentAsync();
        (summaryText?.Contains("email", StringComparison.OrdinalIgnoreCase) == true
         || (emailErr?.Length ?? 0) > 0)
            .Should().BeTrue("the form should surface an email validation error");
    }

    [Test]
    public async Task Login_With_Empty_Fields_Shows_Required_Errors()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();

        // Bypass client-side bot wait; we want to test server-side validation.
        await Page.WaitForTimeoutAsync(1_000);
        await login.SubmitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        Page.Url.Should().Contain("/Account/Login",
            "an empty login form should re-render");

        var html = await Page.ContentAsync();
        html.Should().MatchEquivalentOf("*required*",
            "the form should mark at least one field as required");
    }

    [Test]
    public async Task Register_Form_Requires_All_Fields()
    {
        var register = new RegisterPage(Page);
        await register.OpenAsync();
        await register.SubmitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

        Page.Url.Should().Contain("/Account/Register",
            "an empty register form should re-render");

        // Client-side jQuery validation inserts required-field indicators.
        var html = await Page.ContentAsync();
        html.Should().MatchEquivalentOf("*required*",
            "the form should mark at least one field as required");
    }
}
