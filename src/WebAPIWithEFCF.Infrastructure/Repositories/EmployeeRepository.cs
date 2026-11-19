using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;
using WebAPIWithEFCF.Infrastructure.Data;

namespace WebAPIWithEFCF.Infrastructure.Repositories;

public class EmployeeRepository : AsyncRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Profile)
            .Include(e => e.EmployeeProjects)
                .ThenInclude(ep => ep.Project)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _dbContext.Employees.Where(e => e.Email.ToLower() == normalizedEmail);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<Employee>> GetPagedAsync(
        EmployeeQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees
            .Include(e => e.Department)
            .AsNoTracking();

        // Filtering
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                e.Designation.ToLower().Contains(search));
        }

        if (parameters.DepartmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == parameters.DepartmentId.Value);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(e => e.Status == parameters.Status.Value);
        }

        if (parameters.MinSalary.HasValue)
        {
            query = query.Where(e => e.Salary >= parameters.MinSalary.Value);
        }

        if (parameters.MaxSalary.HasValue)
        {
            query = query.Where(e => e.Salary <= parameters.MaxSalary.Value);
        }

        // Total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);

        // Pagination
        var skip = (parameters.PageNumber - 1) * parameters.PageSize;
        var items = await query.Skip(skip).Take(parameters.PageSize).ToListAsync(cancellationToken);

        return new PagedResult<Employee>(items, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id);
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "firstname" => descending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "lastname" => descending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "email" => descending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            "salary" => descending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
            "dateofjoining" => descending ? query.OrderByDescending(e => e.DateOfJoining) : query.OrderBy(e => e.DateOfJoining),
            "designation" => descending ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }

    public async Task<DashboardSummaryDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _dbContext.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalDepartments = await _dbContext.Departments.CountAsync(cancellationToken);
        var projects = await _dbContext.Projects.AsNoTracking().ToListAsync(cancellationToken);

        var totalCount = employees.Count;
        var activeCount = employees.Count(e => e.Status == EmploymentStatus.FullTime || e.Status == EmploymentStatus.PartTime || e.Status == EmploymentStatus.Contract);

        var totalSalary = employees.Sum(e => e.Salary);
        var avgSalary = totalCount > 0 ? employees.Average(e => e.Salary) : 0;
        var maxSalary = totalCount > 0 ? employees.Max(e => e.Salary) : 0;
        var minSalary = totalCount > 0 ? employees.Min(e => e.Salary) : 0;

        var headcountByDept = employees
            .GroupBy(e => e.Department?.Name ?? "Unassigned")
            .ToDictionary(g => g.Key, g => g.Count());

        var headcountByStatus = employees
            .GroupBy(e => e.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var salaryByDept = employees
            .GroupBy(e => e.Department?.Name ?? "Unassigned")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Salary));

        return new DashboardSummaryDto
        {
            TotalEmployees = totalCount,
            ActiveEmployees = activeCount,
            TotalDepartments = totalDepartments,
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.IsActive),
            TotalSalarySpend = totalSalary,
            AverageSalary = Math.Round(avgSalary, 2),
            HighestSalary = maxSalary,
            LowestSalary = minSalary,
            HeadcountByDepartment = headcountByDept,
            HeadcountByStatus = headcountByStatus,
            SalarySpendByDepartment = salaryByDept
        };
    }
}
