using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Infrastructure.Data;

namespace WebAPIWithEFCF.Infrastructure.Repositories;

public class DepartmentRepository : AsyncRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Department?> GetWithEmployeesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var query = _dbContext.Departments.Where(d => d.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentResponseDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Type = d.Type,
                Description = d.Description,
                Location = d.Location,
                EmployeeCount = d.Employees.Count
            })
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}

public class ProjectRepository : AsyncRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Project?> GetWithMembersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Include(p => p.EmployeeProjects)
                .ThenInclude(ep => ep.Employee)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectResponseDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Client = p.Client,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Budget = p.Budget,
                IsActive = p.IsActive,
                AssignedMembersCount = p.EmployeeProjects.Count
            })
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }
}
