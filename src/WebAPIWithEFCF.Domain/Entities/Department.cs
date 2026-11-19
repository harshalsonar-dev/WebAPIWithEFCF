using System.Collections.Generic;
using WebAPIWithEFCF.Domain.Common;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
