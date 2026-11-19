using System.Collections.Generic;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Application.DTOs;

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DepartmentType Type { get; set; } = DepartmentType.Engineering;
    public string? Description { get; set; }
    public string? Location { get; set; }
}

public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
}

public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentDetailDto : DepartmentResponseDto
{
    public List<EmployeeSummaryDto> Employees { get; set; } = new();
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Client { get; set; } = string.Empty;
    public System.DateTime StartDate { get; set; } = System.DateTime.UtcNow;
    public System.DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Client { get; set; } = string.Empty;
    public System.DateTime StartDate { get; set; }
    public System.DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public bool IsActive { get; set; }
    public int AssignedMembersCount { get; set; }
}
