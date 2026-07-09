using System.ComponentModel.DataAnnotations;
using EAEmployee.Net8.Models;
using FluentAssertions;
using NUnit.Framework;

namespace EAEmployee.Tests.Tests.Unit;

[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.All)]
public class AccountViewModelTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    // ── LoginViewModel ───────────────────────────────────────────────────────

    [Test]
    public void LoginViewModel_Requires_Username_And_Password()
    {
        var results = Validate(new LoginViewModel());
        results.SelectMany(r => r.MemberNames).Should()
            .Contain(nameof(LoginViewModel.UserName))
            .And.Contain(nameof(LoginViewModel.Password));
    }

    [Test]
    public void LoginViewModel_Default_RememberMe_Is_False()
    {
        new LoginViewModel().RememberMe.Should().BeFalse();
    }

    [Test]
    public void LoginViewModel_Bot_Detection_Fields_Are_Optional()
    {
        // Honeypot, CaptchaToken and PageLoadTime are all string? — they must
        // not produce [Required] errors when the form first renders.
        var results = Validate(new LoginViewModel
        {
            UserName = "admin",
            Password = "password"
        });
        results.Should().BeEmpty();
    }

    // ── RegisterViewModel ────────────────────────────────────────────────────

    [Test]
    public void RegisterViewModel_Requires_All_Fields()
    {
        var results = Validate(new RegisterViewModel());
        results.SelectMany(r => r.MemberNames).Should()
            .Contain(nameof(RegisterViewModel.UserName))
            .And.Contain(nameof(RegisterViewModel.Email))
            .And.Contain(nameof(RegisterViewModel.Password));
    }

    [TestCase("short")]      // 5 chars — below MinimumLength = 6
    [TestCase("")]           // empty
    public void RegisterViewModel_Password_Must_Be_At_Least_Six_Chars(string password)
    {
        var model = new RegisterViewModel
        {
            UserName = "validUser",
            Email = "user@example.com",
            Password = password,
            ConfirmPassword = password
        };
        var results = Validate(model);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
    }

    [Test]
    public void RegisterViewModel_Rejects_Invalid_Email()
    {
        var model = new RegisterViewModel
        {
            UserName = "validUser",
            Email = "bogus",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        };
        var results = Validate(model);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
    }

    [Test]
    public void RegisterViewModel_ConfirmPassword_Must_Match_Password()
    {
        // The [Compare] attribute is only enforced by Validator when the
        // matching property is non-null and the values differ.
        var model = new RegisterViewModel
        {
            UserName = "validUser",
            Email = "user@example.com",
            Password = "Password1!",
            ConfirmPassword = "DIFFERENT"
        };
        var results = Validate(model);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }
}
