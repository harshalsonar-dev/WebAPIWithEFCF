using System.Collections.Generic;

namespace WebAPIWithEFCF.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public decimal TotalSalarySpend { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal HighestSalary { get; set; }
    public decimal LowestSalary { get; set; }
    public Dictionary<string, int> HeadcountByDepartment { get; set; } = new();
    public Dictionary<string, int> HeadcountByStatus { get; set; } = new();
    public Dictionary<string, decimal> SalarySpendByDepartment { get; set; } = new();
}
