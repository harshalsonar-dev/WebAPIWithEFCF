using System;

namespace WebAPIWithEFCF.Domain.Entities;

public class EmployeeProject
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string RoleOnProject { get; set; } = "Contributor";
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public int AllocationPercentage { get; set; } = 100;
}
