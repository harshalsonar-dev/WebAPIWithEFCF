using System;

namespace WebAPIWithEFCF.Domain.Entities;

public class EmployeeProfile
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Skills { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
