using System;
using System.Collections.Generic;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Application.DTOs;

public class EmployeeQueryParameters : QueryParameters
{
    public int? DepartmentId { get; set; }
    public EmploymentStatus? Status { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
}

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; } = DateTime.UtcNow;
    public EmploymentStatus Status { get; set; } = EmploymentStatus.FullTime;
    public int DepartmentId { get; set; }

    public EmployeeProfileDto? Profile { get; set; }
}

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public EmploymentStatus Status { get; set; }
    public int DepartmentId { get; set; }

    public EmployeeProfileDto? Profile { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class EmployeeProfileDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Skills { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public EmploymentStatus Status { get; set; }
    public string StatusName => Status.ToString();

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;

    public EmployeeProfileDto? Profile { get; set; }
    public List<EmployeeProjectResponseDto> Projects { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class EmployeeSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public EmploymentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime DateOfJoining { get; set; }
}

public class AssignProjectDto
{
    public int ProjectId { get; set; }
    public string RoleOnProject { get; set; } = "Developer";
    public int AllocationPercentage { get; set; } = 100;
}

public class EmployeeProjectResponseDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public string RoleOnProject { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public int AllocationPercentage { get; set; }
    public bool IsActive { get; set; }
}
