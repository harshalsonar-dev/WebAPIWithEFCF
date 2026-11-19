using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;

namespace WebAPIWithEFCF.API.Controllers;

/// <summary>
/// Manages organizational departments.
/// </summary>
public class DepartmentsController : BaseApiController
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Retrieves all departments with their current headcount.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponseDto>>> GetAllDepartments(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    /// <summary>
    /// Retrieves a department by ID with all affiliated employees.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDetailDto>> GetDepartmentById(int id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
        return Ok(department);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment(
        [FromBody] CreateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _departmentService.CreateDepartmentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetDepartmentById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponseDto>> UpdateDepartment(
        int id,
        [FromBody] UpdateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _departmentService.UpdateDepartmentAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a department if it contains no assigned employees.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        await _departmentService.DeleteDepartmentAsync(id, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Manages company projects.
/// </summary>
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Retrieves all projects with their assigned member counts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponseDto>>> GetAllProjects(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllProjectsAsync(cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(
        [FromBody] CreateProjectDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _projectService.CreateProjectAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }
}

/// <summary>
/// Executive dashboard metrics and aggregate analytics.
/// </summary>
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retrieves workforce analytics, salary distribution, and department statistics.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}

/// <summary>
/// System health and readiness check.
/// </summary>
public class HealthController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = System.DateTime.UtcNow,
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Framework = ".NET 9.0"
        });
    }
}
