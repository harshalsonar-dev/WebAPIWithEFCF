using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Exceptions;

namespace WebAPIWithEFCF.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DepartmentResponseDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _departmentRepository.GetAllWithCountsAsync(cancellationToken);
    }

    public async Task<DepartmentDetailDto> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetWithEmployeesAsync(id, cancellationToken);
        if (department == null)
        {
            throw new EntityNotFoundException(nameof(Department), id);
        }

        return new DepartmentDetailDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Type = department.Type,
            Description = department.Description,
            Location = department.Location,
            EmployeeCount = department.Employees.Count,
            Employees = department.Employees.Select(e => new EmployeeSummaryDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Designation = e.Designation,
                Salary = e.Salary,
                DepartmentName = department.Name,
                Status = e.Status,
                DateOfJoining = e.DateOfJoining
            }).ToList()
        };
    }

    public async Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var codeExists = await _departmentRepository.CodeExistsAsync(dto.Code, null, cancellationToken);
        if (codeExists)
        {
            throw new DuplicateEntityException($"Department with code '{dto.Code}' already exists.");
        }

        var department = new Department
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            Type = dto.Type,
            Description = dto.Description?.Trim(),
            Location = dto.Location?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Type = department.Type,
            Description = department.Description,
            Location = department.Location,
            EmployeeCount = 0
        };
    }

    public async Task<DepartmentResponseDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department == null)
        {
            throw new EntityNotFoundException(nameof(Department), id);
        }

        var codeExists = await _departmentRepository.CodeExistsAsync(dto.Code, id, cancellationToken);
        if (codeExists)
        {
            throw new DuplicateEntityException($"Department with code '{dto.Code}' already exists.");
        }

        department.Name = dto.Name.Trim();
        department.Code = dto.Code.Trim().ToUpperInvariant();
        department.Type = dto.Type;
        department.Description = dto.Description?.Trim();
        department.Location = dto.Location?.Trim();
        department.UpdatedAtUtc = DateTime.UtcNow;

        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var employeeCount = await _departmentRepository.GetWithEmployeesAsync(id, cancellationToken);

        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Type = department.Type,
            Description = department.Description,
            Location = department.Location,
            EmployeeCount = employeeCount?.Employees.Count ?? 0
        };
    }

    public async Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetWithEmployeesAsync(id, cancellationToken);
        if (department == null)
        {
            throw new EntityNotFoundException(nameof(Department), id);
        }

        if (department.Employees.Any())
        {
            throw new BusinessRuleException($"Cannot delete department '{department.Name}' because it currently has {department.Employees.Count} active employee(s).");
        }

        department.IsDeleted = true;
        department.UpdatedAtUtc = DateTime.UtcNow;
        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProjectResponseDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetAllWithCountsAsync(cancellationToken);
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Client = dto.Client.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Client = project.Client,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Budget = project.Budget,
            IsActive = project.IsActive,
            AssignedMembersCount = 0
        };
    }
}

public class DashboardService : IDashboardService
{
    private readonly IEmployeeRepository _employeeRepository;

    public DashboardService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetDashboardMetricsAsync(cancellationToken);
    }
}
