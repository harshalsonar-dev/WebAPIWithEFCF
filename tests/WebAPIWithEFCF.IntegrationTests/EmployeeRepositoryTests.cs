using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;
using WebAPIWithEFCF.Infrastructure.Data;
using WebAPIWithEFCF.Infrastructure.Repositories;
using Xunit;

namespace WebAPIWithEFCF.IntegrationTests;

public class EmployeeRepositoryTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersBySearchTerm_ReturnsMatchingEmployees()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var dept = new Department { Name = "Engineering", Code = "ENG" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        context.Employees.AddRange(
            new Employee { FirstName = "Harshal", LastName = "Sonar", Email = "harshal@example.com", Designation = "Lead Dev", Salary = 90000, DepartmentId = dept.Id },
            new Employee { FirstName = "John", LastName = "Doe", Email = "john@example.com", Designation = "QA", Salary = 60000, DepartmentId = dept.Id },
            new Employee { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Designation = "Dev", Salary = 75000, DepartmentId = dept.Id }
        );
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var parameters = new EmployeeQueryParameters { SearchTerm = "Sonar" };

        // Act
        var result = await repo.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FirstName.Should().Be("Harshal");
    }

    [Fact]
    public async Task GetPagedAsync_FiltersBySalaryRange_ReturnsEmployeesInRange()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var dept = new Department { Name = "Finance", Code = "FIN" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        context.Employees.AddRange(
            new Employee { FirstName = "Emp1", LastName = "Low", Email = "e1@example.com", Designation = "Dev", Salary = 40000, DepartmentId = dept.Id },
            new Employee { FirstName = "Emp2", LastName = "Mid", Email = "e2@example.com", Designation = "Dev", Salary = 70000, DepartmentId = dept.Id },
            new Employee { FirstName = "Emp3", LastName = "High", Email = "e3@example.com", Designation = "Dev", Salary = 120000, DepartmentId = dept.Id }
        );
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var parameters = new EmployeeQueryParameters { MinSalary = 50000, MaxSalary = 100000 };

        // Act
        var result = await repo.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FirstName.Should().Be("Emp2");
    }

    [Fact]
    public async Task SoftDelete_GlobalQueryFilterExcludesDeletedEmployees()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var dept = new Department { Name = "Operations", Code = "OPS" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var emp1 = new Employee { FirstName = "Active", LastName = "User", Email = "active@example.com", Designation = "Ops", Salary = 50000, DepartmentId = dept.Id, IsDeleted = false };
        var emp2 = new Employee { FirstName = "Deleted", LastName = "User", Email = "deleted@example.com", Designation = "Ops", Salary = 50000, DepartmentId = dept.Id, IsDeleted = true };

        context.Employees.AddRange(emp1, emp2);
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);

        // Act
        var paged = await repo.GetPagedAsync(new EmployeeQueryParameters());

        // Assert
        paged.Items.Should().HaveCount(1);
        paged.Items[0].FirstName.Should().Be("Active");
    }

    [Fact]
    public async Task GetDashboardMetricsAsync_CalculatesAccurateAggregates()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var dept = new Department { Name = "Engineering", Code = "ENG" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        context.Employees.AddRange(
            new Employee { FirstName = "A", LastName = "A", Email = "a@example.com", Designation = "Dev", Salary = 50000, DepartmentId = dept.Id, Status = EmploymentStatus.FullTime },
            new Employee { FirstName = "B", LastName = "B", Email = "b@example.com", Designation = "Dev", Salary = 150000, DepartmentId = dept.Id, Status = EmploymentStatus.FullTime }
        );
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);

        // Act
        var metrics = await repo.GetDashboardMetricsAsync();

        // Assert
        metrics.TotalEmployees.Should().Be(2);
        metrics.ActiveEmployees.Should().Be(2);
        metrics.TotalSalarySpend.Should().Be(200000);
        metrics.AverageSalary.Should().Be(100000);
        metrics.HighestSalary.Should().Be(150000);
        metrics.LowestSalary.Should().Be(50000);
    }
}
