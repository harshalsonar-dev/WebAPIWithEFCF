using System;
using FluentAssertions;
using WebAPIWithEFCF.Application.DTOs;
using WebAPIWithEFCF.Application.Validators;
using Xunit;

namespace WebAPIWithEFCF.UnitTests;

public class ValidatorTests
{
    private readonly CreateEmployeeDtoValidator _employeeValidator = new();
    private readonly CreateDepartmentDtoValidator _departmentValidator = new();
    private readonly CreateProjectDtoValidator _projectValidator = new();

    [Fact]
    public void CreateEmployeeValidator_WhenEmailIsInvalid_FailsValidation()
    {
        var dto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email-address",
            Designation = "Dev",
            Salary = 50000,
            DepartmentId = 1
        };

        var result = _employeeValidator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmployeeDto.Email));
    }

    [Fact]
    public void CreateEmployeeValidator_WhenSalaryIsZeroOrNegative_FailsValidation()
    {
        var dto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "valid@example.com",
            Designation = "Dev",
            Salary = -100,
            DepartmentId = 1
        };

        var result = _employeeValidator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmployeeDto.Salary));
    }

    [Fact]
    public void CreateDepartmentValidator_WhenCodeContainsLowercase_FailsValidation()
    {
        var dto = new CreateDepartmentDto
        {
            Name = "Engineering",
            Code = "eng_lower"
        };

        var result = _departmentValidator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDepartmentDto.Code));
    }

    [Fact]
    public void CreateProjectValidator_WhenBudgetIsNegative_FailsValidation()
    {
        var dto = new CreateProjectDto
        {
            Name = "Project Alpha",
            Client = "Acme",
            Budget = -500
        };

        var result = _projectValidator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectDto.Budget));
    }
}
