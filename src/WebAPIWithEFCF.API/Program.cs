using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WebAPIWithEFCF.API.Middleware;
using WebAPIWithEFCF.Application;
using WebAPIWithEFCF.Infrastructure;
using WebAPIWithEFCF.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers with JSON formatting
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Add FluentValidation automatic model validation
builder.Services.AddFluentValidationAutoValidation();

// 3. Register Clean Architecture Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 5. Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebAPIWithEFCF - Workforce Management API",
        Version = "v1",
        Description = "Enterprise RESTful Web API built with ASP.NET Core 9.0 and Entity Framework Core 9.0 Code First.",
        Contact = new OpenApiContact
        {
            Name = "Harshal Sonar",
            Email = "sonarharshal0609@gmail.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 6. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Configure HTTP pipeline
if (app.Environment.IsDevelopment() || true) // Enable Swagger in all environments for testing ease
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIWithEFCF v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at application root (http://localhost:PORT/)
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// 8. Automatic Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.SeedAsync(dbContext);
        logger.LogInformation("Database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
    }
}

app.Run();

// Required for WebApplicationFactory in Integration Tests
public partial class Program { }
