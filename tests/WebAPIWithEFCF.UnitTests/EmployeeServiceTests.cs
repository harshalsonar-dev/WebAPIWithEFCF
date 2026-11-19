using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebAPIWithEFCF.Application.Common;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Application.Services;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;
using WebAPIWithEFCF.Domain.Exceptions;
using Xunit;

namespace WebAPIWithEFCF.UnitTests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepoMock = new();
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _service = new EmployeeService(
            _employeeRepoMock.Object,
            _departmentRepoMock.Object,
            _projectRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetEmployeesAsync_WithValidParameters_ReturnsPagedResult()
    {
        // Arrange
        var parameters = new EmployeeQueryParameters { PageNumber = 1, PageSize = 10 };
        var employees = new List<Employee>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Designation = "Dev", Salary = 75000, DateOfJoining = DateTime.UtcNow, Department = new Department { Name = "Engineering" } }
        };
        var paged = new PagedResult<Employee>(employees, 1, 1, 10);

        _employeeRepoMock.Setup(r => r.GetPagedAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        // Act
        var result = await _service.GetEmployeesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].FullName.Should().Be("John Doe");
        result.Items[0].DepartmentName.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenFound_ReturnsResponseDto()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 5,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Designation = "Architect",
            Salary = 120000,
            Department = new Department { Name = "Tech", Code = "TECH" }
        };

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.GetEmployeeByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.FullName.Should().Be("Alice Smith");
        result.DepartmentName.Should().Be("Tech");
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var act = async () => await _service.GetEmployeeByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithValidDto_CreatesAndReturnsEmployee()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Bob",
            LastName = "Taylor",
            Email = "bob.taylor@example.com",
            Designation = "QA Engineer",
            Salary = 60000,
            DepartmentId = 1
        };

        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _departmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = 1, Name = "Engineering" });

        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((e, _) => e.Id = 10)
            .ReturnsAsync(new Employee { Id = 10 });

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee
            {
                Id = 10,
                FirstName = "Bob",
                LastName = "Taylor",
                Email = "bob.taylor@example.com",
                Designation = "QA Engineer",
                Salary = 60000,
                DepartmentId = 1,
                Department = new Department { Name = "Engineering", Code = "ENG" }
            });

        // Act
        var result = await _service.CreateEmployeeAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.FullName.Should().Be("Bob Taylor");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithDuplicateEmail_ThrowsDuplicateEntityException()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Bob",
            LastName = "Taylor",
            Email = "duplicate@example.com",
            DepartmentId = 1
        };

        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _service.CreateEmployeeAsync(dto);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateEmployeeAsync_WhenDepartmentDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Bob",
            LastName = "Taylor",
            Email = "unique@example.com",
            DepartmentId = 999
        };

        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _departmentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act
        var act = async () => await _service.CreateEmployeeAsync(dto);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteEmployeeAsync_WhenFound_SetsIsDeletedTrue()
    {
        // Arrange
        var employee = new Employee { Id = 3, IsDeleted = false };
        _employeeRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _service.DeleteEmployeeAsync(3);

        // Assert
        employee.IsDeleted.Should().BeTrue();
        _employeeRepoMock.Verify(r => r.Update(employee), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
