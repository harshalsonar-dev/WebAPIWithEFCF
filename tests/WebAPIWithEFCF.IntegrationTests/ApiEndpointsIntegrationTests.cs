using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Domain.Enums;
using Xunit;

namespace WebAPIWithEFCF.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}

public class ApiEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_Returns200AndHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
        content.Should().Contain(".NET 9.0");
    }

    [Fact]
    public async Task GetEmployees_Returns200WithPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/employees?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeSummaryDto>>(JsonOptions);
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeEmpty();
        paged.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetEmployeeById_WhenExists_Returns200WithDetails()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/employees/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(JsonOptions);
        employee.Should().NotBeNull();
        employee!.Id.Should().Be(1);
        employee.Email.Should().NotBeNullOrWhiteSpace();
        employee.DepartmentName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetEmployeeById_WhenNotExists_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/employees/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateEmployee_WithInvalidEmail_Returns400BadRequest()
    {
        // Arrange
        var newEmployee = new CreateEmployeeDto
        {
            FirstName = "Invalid",
            LastName = "EmailUser",
            Email = "not-an-email",
            Designation = "Dev",
            Salary = 70000,
            DepartmentId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/employees", newEmployee);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_Returns201Created()
    {
        // Arrange
        var uniqueEmail = $"integration.test.{Guid.NewGuid():N}@example.com";
        var newEmployee = new CreateEmployeeDto
        {
            FirstName = "Test",
            LastName = "Candidate",
            Email = uniqueEmail,
            PhoneNumber = "+1-555-8899",
            Designation = "Software Engineer",
            Salary = 85000,
            DepartmentId = 1,
            Status = EmploymentStatus.FullTime,
            Profile = new EmployeeProfileDto
            {
                City = "Seattle",
                Country = "USA",
                Skills = "C#, Web API"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/employees", newEmployee);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(JsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Email.Should().Be(uniqueEmail.ToLowerInvariant());
        created.Profile.Should().NotBeNull();
        created.Profile!.City.Should().Be("Seattle");
    }

    [Fact]
    public async Task GetDepartments_Returns200WithList()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/departments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await response.Content.ReadFromJsonAsync<DepartmentResponseDto[]>(JsonOptions);
        departments.Should().NotBeNull();
        departments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboardMetrics_Returns200WithAggregates()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>(JsonOptions);
        metrics.Should().NotBeNull();
        metrics!.TotalEmployees.Should().BeGreaterThan(0);
        metrics.TotalDepartments.Should().BeGreaterThan(0);
        metrics.TotalSalarySpend.Should().BeGreaterThan(0);
    }
}
