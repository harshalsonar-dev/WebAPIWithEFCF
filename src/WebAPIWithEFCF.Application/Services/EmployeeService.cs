using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;
using WebAPIWithEFCF.Domain.Exceptions;

namespace WebAPIWithEFCF.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<EmployeeSummaryDto>> GetEmployeesAsync(
        EmployeeQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _employeeRepository.GetPagedAsync(parameters, cancellationToken);

        var dtos = pagedResult.Items.Select(e => new EmployeeSummaryDto
        {
            Id = e.Id,
            FullName = e.FullName,
            Email = e.Email,
            Designation = e.Designation,
            Salary = e.Salary,
            DepartmentName = e.Department?.Name ?? "Unassigned",
            Status = e.Status,
            DateOfJoining = e.DateOfJoining
        }).ToList();

        return new PagedResult<EmployeeSummaryDto>(dtos, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<EmployeeResponseDto> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), id);
        }

        return MapToResponseDto(employee);
    }

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        // Check duplicate email
        var emailExists = await _employeeRepository.EmailExistsAsync(dto.Email, null, cancellationToken);
        if (emailExists)
        {
            throw new DuplicateEntityException($"An employee with email '{dto.Email}' already exists.");
        }

        // Check department exists
        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken);
        if (department == null)
        {
            throw new EntityNotFoundException(nameof(Department), dto.DepartmentId);
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Designation = dto.Designation.Trim(),
            Salary = dto.Salary,
            DateOfJoining = dto.DateOfJoining,
            Status = dto.Status,
            DepartmentId = dto.DepartmentId,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (dto.Profile != null)
        {
            employee.Profile = new EmployeeProfile
            {
                Address = dto.Profile.Address,
                City = dto.Profile.City,
                Country = dto.Profile.Country,
                EmergencyContactName = dto.Profile.EmergencyContactName,
                EmergencyContactPhone = dto.Profile.EmergencyContactPhone,
                Skills = dto.Profile.Skills,
                DateOfBirth = dto.Profile.DateOfBirth
            };
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch back with navigation properties
        var created = await _employeeRepository.GetWithDetailsAsync(employee.Id, cancellationToken);
        return MapToResponseDto(created ?? employee);
    }

    public async Task<EmployeeResponseDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), id);
        }

        // Concurrency check if row version provided
        if (dto.RowVersion != null && employee.RowVersion != null)
        {
            if (!dto.RowVersion.SequenceEqual(employee.RowVersion))
            {
                throw new ConcurrencyException("The employee was modified by another user. Please refresh and try again.");
            }
        }

        // Email uniqueness
        var emailExists = await _employeeRepository.EmailExistsAsync(dto.Email, id, cancellationToken);
        if (emailExists)
        {
            throw new DuplicateEntityException($"An employee with email '{dto.Email}' already exists.");
        }

        // Check department exists
        if (employee.DepartmentId != dto.DepartmentId)
        {
            var deptExists = await _departmentRepository.AnyAsync(d => d.Id == dto.DepartmentId, cancellationToken);
            if (!deptExists)
            {
                throw new EntityNotFoundException(nameof(Department), dto.DepartmentId);
            }
        }

        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.Email = dto.Email.Trim().ToLowerInvariant();
        employee.PhoneNumber = dto.PhoneNumber?.Trim();
        employee.Designation = dto.Designation.Trim();
        employee.Salary = dto.Salary;
        employee.DateOfJoining = dto.DateOfJoining;
        employee.Status = dto.Status;
        employee.DepartmentId = dto.DepartmentId;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        if (dto.Profile != null)
        {
            if (employee.Profile == null)
            {
                employee.Profile = new EmployeeProfile { EmployeeId = employee.Id };
            }

            employee.Profile.Address = dto.Profile.Address;
            employee.Profile.City = dto.Profile.City;
            employee.Profile.Country = dto.Profile.Country;
            employee.Profile.EmergencyContactName = dto.Profile.EmergencyContactName;
            employee.Profile.EmergencyContactPhone = dto.Profile.EmergencyContactPhone;
            employee.Profile.Skills = dto.Profile.Skills;
            employee.Profile.DateOfBirth = dto.Profile.DateOfBirth;
        }

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _employeeRepository.GetWithDetailsAsync(employee.Id, cancellationToken);
        return MapToResponseDto(updated ?? employee);
    }

    public async Task UpdateStatusAsync(int id, EmploymentStatus newStatus, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), id);
        }

        employee.Status = newStatus;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), id);
        }

        // Soft delete
        employee.IsDeleted = true;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignProjectAsync(int employeeId, AssignProjectDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), employeeId);
        }

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new EntityNotFoundException(nameof(Project), dto.ProjectId);
        }

        var existingAssignment = employee.EmployeeProjects.FirstOrDefault(ep => ep.ProjectId == dto.ProjectId);
        if (existingAssignment != null)
        {
            existingAssignment.RoleOnProject = dto.RoleOnProject;
            existingAssignment.AllocationPercentage = dto.AllocationPercentage;
        }
        else
        {
            employee.EmployeeProjects.Add(new EmployeeProject
            {
                EmployeeId = employeeId,
                ProjectId = dto.ProjectId,
                RoleOnProject = dto.RoleOnProject,
                AllocationPercentage = dto.AllocationPercentage,
                AssignedDate = DateTime.UtcNow
            });
        }

        employee.UpdatedAtUtc = DateTime.UtcNow;
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new EntityNotFoundException(nameof(Employee), employeeId);
        }

        var assignment = employee.EmployeeProjects.FirstOrDefault(ep => ep.ProjectId == projectId);
        if (assignment != null)
        {
            employee.EmployeeProjects.Remove(assignment);
            employee.UpdatedAtUtc = DateTime.UtcNow;
            _employeeRepository.Update(employee);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static EmployeeResponseDto MapToResponseDto(Employee employee)
    {
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Designation = employee.Designation,
            Salary = employee.Salary,
            DateOfJoining = employee.DateOfJoining,
            Status = employee.Status,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? "Unassigned",
            DepartmentCode = employee.Department?.Code ?? "N/A",
            CreatedAtUtc = employee.CreatedAtUtc,
            UpdatedAtUtc = employee.UpdatedAtUtc,
            RowVersion = employee.RowVersion,
            Profile = employee.Profile == null ? null : new EmployeeProfileDto
            {
                Address = employee.Profile.Address,
                City = employee.Profile.City,
                Country = employee.Profile.Country,
                EmergencyContactName = employee.Profile.EmergencyContactName,
                EmergencyContactPhone = employee.Profile.EmergencyContactPhone,
                Skills = employee.Profile.Skills,
                DateOfBirth = employee.Profile.DateOfBirth
            },
            Projects = employee.EmployeeProjects.Select(ep => new EmployeeProjectResponseDto
            {
                ProjectId = ep.ProjectId,
                ProjectName = ep.Project?.Name ?? "Unknown Project",
                Client = ep.Project?.Client ?? "N/A",
                RoleOnProject = ep.RoleOnProject,
                AssignedDate = ep.AssignedDate,
                AllocationPercentage = ep.AllocationPercentage,
                IsActive = ep.Project?.IsActive ?? false
            }).ToList()
        };
    }
}
