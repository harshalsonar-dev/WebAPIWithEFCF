using System;
using System.Collections.Generic;
using WebAPIWithEFCF.Domain.Common;

namespace WebAPIWithEFCF.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Client { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
}
