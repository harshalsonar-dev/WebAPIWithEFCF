using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Application.Services;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;
using WebAPIWithEFCF.Domain.Exceptions;
using Xunit;

namespace WebAPIWithEFCF.UnitTests;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _deptRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(_deptRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsListOfDepartments()
    {
        // Arrange
        var departments = new List<DepartmentResponseDto>
        {
            new() { Id = 1, Name = "Engineering", Code = "ENG", EmployeeCount = 10 },
            new() { Id = 2, Name = "Human Resources", Code = "HR", EmployeeCount = 3 }
        };

        _deptRepoMock.Setup(r => r.GetAllWithCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        // Act
        var result = await _service.GetAllDepartmentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Code.Should().Be("ENG");
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithDuplicateCode_ThrowsDuplicateEntityException()
    {
        // Arrange
        var dto = new CreateDepartmentDto { Name = "Finance", Code = "FIN", Type = DepartmentType.Finance };
        _deptRepoMock.Setup(r => r.CodeExistsAsync("FIN", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _service.CreateDepartmentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WhenDepartmentHasEmployees_ThrowsBusinessRuleException()
    {
        // Arrange
        var dept = new Department
        {
            Id = 1,
            Name = "Engineering",
            Employees = new List<Employee> { new() { Id = 1, FirstName = "Test" } }
        };

        _deptRepoMock.Setup(r => r.GetWithEmployeesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        // Act
        var act = async () => await _service.DeleteDepartmentAsync(1);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WhenDepartmentHasNoEmployees_SetsIsDeletedTrue()
    {
        // Arrange
        var dept = new Department
        {
            Id = 1,
            Name = "Empty Dept",
            Employees = new List<Employee>()
        };

        _deptRepoMock.Setup(r => r.GetWithEmployeesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dept);

        // Act
        await _service.DeleteDepartmentAsync(1);

        // Assert
        dept.IsDeleted.Should().BeTrue();
        _deptRepoMock.Verify(r => r.Update(dept), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
