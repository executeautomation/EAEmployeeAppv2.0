using System.ComponentModel.DataAnnotations;

namespace EAEmployee.Net8.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Salary")]
    public float Salary { get; set; }

    [Required]
    [Display(Name = "Age")]
    [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
    public int Age { get; set; }

    [Required]
    [Display(Name = "Duration Worked (months)")]
    public int DurationWorked { get; set; }

    [Required]
    [Display(Name = "Grade")]
    public int Grade { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // Contact & Address
    [Display(Name = "Phone")]
    [Phone]
    public string? Phone { get; set; }

    [Display(Name = "Street Address")]
    public string? StreetAddress { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    [Display(Name = "State / Province")]
    public string? State { get; set; }

    [Display(Name = "Postal Code")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid postal code format")]
    public string? PostalCode { get; set; }

    [Display(Name = "Country")]
    public string? Country { get; set; } = "New Zealand";

    // Tax details
    [Display(Name = "Tax ID / IRD Number")]
    public string? TaxId { get; set; }

    [Display(Name = "Tax Bracket")]
    public string? TaxBracket { get; set; }

    // Employment metadata
    [Display(Name = "Department")]
    public string? Department { get; set; }

    [Display(Name = "Job Title")]
    public string? JobTitle { get; set; }

    [Display(Name = "Date of Joining")]
    [DataType(DataType.Date)]
    public DateTime? DateOfJoining { get; set; }

    [Display(Name = "Employment Type")]
    [RegularExpression(@"^(Full-Time|Part-Time|Contract|Freelance)$", ErrorMessage = "Invalid employment type")]
    public string? EmploymentType { get; set; }

    [Display(Name = "Marital Status")]
    [RegularExpression(@"^(Single|Married|Divorced|Widowed)$", ErrorMessage = "Invalid marital status")]
    public string? MaritalStatus { get; set; }

    // Emergency contact
    [Display(Name = "Emergency Contact Name")]
    public string? EmergencyContactName { get; set; }

    [Display(Name = "Emergency Contact Phone")]
    [Phone]
    public string? EmergencyContactPhone { get; set; }

    // Bank details
    [Display(Name = "Bank Account Number")]
    public string? BankAccountNumber { get; set; }

    [Display(Name = "Bank Name")]
    public string? BankName { get; set; }

    [Display(Name = "Bank Sort Code / BSB")]
    public string? BankSortCode { get; set; }
}