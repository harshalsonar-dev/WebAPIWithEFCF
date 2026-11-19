using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Infrastructure.Data;
using WebAPIWithEFCF.Infrastructure.Repositories;

namespace WebAPIWithEFCF.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var inMemoryVal = configuration["UseInMemoryDatabase"];
        var useInMemory = (bool.TryParse(inMemoryVal, out var parsed) && parsed) ||
                          string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("WebAPIWithEFCF_TestingDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration.GetConnectionString("ConStr")
                ?? "Server=.;Database=WebAPIWithEFCF_DB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                }));
        }

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IAsyncRepository<>), typeof(AsyncRepository<>));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
