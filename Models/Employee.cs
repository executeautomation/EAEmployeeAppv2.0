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
    [Display(Name = "Duration Worked (months)")]
    public int DurationWorked { get; set; }

    [Required]
    [Display(Name = "Grade")]
    public int Grade { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
