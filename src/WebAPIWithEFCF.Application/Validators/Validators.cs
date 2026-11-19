using System;
using FluentValidation;
using WebAPIWithEFCF.Application.DTOs;

namespace WebAPIWithEFCF.Application.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required.")
            .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Salary cannot exceed 1,000,000.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("A valid department must be selected.");

        RuleFor(x => x.DateOfJoining)
            .NotEmpty().WithMessage("Date of joining is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(30)).WithMessage("Date of joining cannot be more than 30 days in the future.");

        When(x => x.Profile != null, () =>
        {
            RuleFor(x => x.Profile!.Address).MaximumLength(200);
            RuleFor(x => x.Profile!.City).MaximumLength(50);
            RuleFor(x => x.Profile!.Country).MaximumLength(50);
            RuleFor(x => x.Profile!.EmergencyContactName).MaximumLength(100);
            RuleFor(x => x.Profile!.EmergencyContactPhone).MaximumLength(20);
        });
    }
}

public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required.")
            .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Salary cannot exceed 1,000,000.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("A valid department must be selected.");
    }
}

public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Department code is required.")
            .MaximumLength(10).WithMessage("Department code cannot exceed 10 characters.")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Department code must contain only uppercase letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(150).WithMessage("Project name cannot exceed 150 characters.");

        RuleFor(x => x.Client)
            .NotEmpty().WithMessage("Client is required.")
            .MaximumLength(100).WithMessage("Client cannot exceed 100 characters.");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget cannot be negative.");
    }
}
