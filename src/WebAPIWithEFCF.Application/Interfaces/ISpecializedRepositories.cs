using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Domain.Entities;

namespace WebAPIWithEFCF.Application.Interfaces;

public interface IEmployeeRepository : IAsyncRepository<Employee>
{
    Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IAsyncRepository<Department>
{
    Task<Department?> GetWithEmployeesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentResponseDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default);
}

public interface IProjectRepository : IAsyncRepository<Project>
{
    Task<Project?> GetWithMembersAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectResponseDto>> GetAllWithCountsAsync(CancellationToken cancellationToken = default);
}
