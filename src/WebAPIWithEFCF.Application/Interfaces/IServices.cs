using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeSummaryDto>> GetEmployeesAsync(EmployeeQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, EmploymentStatus newStatus, CancellationToken cancellationToken = default);
    Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    Task AssignProjectAsync(int employeeId, AssignProjectDto dto, CancellationToken cancellationToken = default);
    Task RemoveFromProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponseDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDetailDto> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
}

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponseDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
    Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
