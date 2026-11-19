using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.API.Controllers;

/// <summary>
/// Manages employee lifecycle, profile, department allocation, and project assignments.
/// </summary>
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Retrieves a paginated and filterable list of employees.
    /// </summary>
    /// <param name="parameters">Query parameters for pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of employee summaries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeSummaryDto>>> GetEmployees(
        [FromQuery] EmployeeQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves full details of an employee by their unique ID.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee details including profile and project assignments.</returns>
    [HttpGet("{id:int}", Name = nameof(GetEmployeeById))]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new employee with optional profile information.
    /// </summary>
    /// <param name="dto">Employee creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created employee.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(
        [FromBody] CreateEmployeeDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _employeeService.CreateEmployeeAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing employee and their associated profile.
    /// </summary>
    /// <param name="id">The employee ID to update.</param>
    /// <param name="dto">Updated employee data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated employee.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(
        int id,
        [FromBody] UpdateEmployeeDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _employeeService.UpdateEmployeeAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the employment status of an employee.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="status">The new employment status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromQuery] EmploymentStatus status,
        CancellationToken cancellationToken)
    {
        await _employeeService.UpdateStatusAsync(id, status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an employee from the system.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Assigns an employee to a project with a specific role and allocation percentage.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="dto">Project assignment payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:int}/projects")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignProject(
        int id,
        [FromBody] AssignProjectDto dto,
        CancellationToken cancellationToken)
    {
        await _employeeService.AssignProjectAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Removes an employee from a specific project.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:int}/projects/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromProject(
        int id,
        int projectId,
        CancellationToken cancellationToken)
    {
        await _employeeService.RemoveFromProjectAsync(id, projectId, cancellationToken);
        return NoContent();
    }
}
