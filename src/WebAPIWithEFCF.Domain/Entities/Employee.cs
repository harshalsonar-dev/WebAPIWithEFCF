using System;
using System.Collections.Generic;
using WebAPIWithEFCF.Domain.Common;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Domain.Entities;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.FullTime;

    // Foreign Key
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    // 1-to-1 Relationship
    public EmployeeProfile? Profile { get; set; }

    // Many-to-Many Relationship
    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
}
