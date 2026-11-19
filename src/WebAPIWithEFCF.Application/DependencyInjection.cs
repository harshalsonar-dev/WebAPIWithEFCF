using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Application.Services;

namespace WebAPIWithEFCF.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
