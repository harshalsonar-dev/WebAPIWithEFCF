using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPIWithEFCF.Domain.Entities;
using WebAPIWithEFCF.Domain.Enums;

namespace WebAPIWithEFCF.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Apply pending migrations for relational databases, or EnsureCreated for In-Memory
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Departments.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 1. Seed Departments
        var eng = new Department
        {
            Name = "Engineering",
            Code = "ENG",
            Type = DepartmentType.Engineering,
            Description = "Software development, architecture, and infrastructure engineering.",
            Location = "Building A, Floor 4",
            CreatedAtUtc = DateTime.UtcNow
        };

        var hr = new Department
        {
            Name = "Human Resources",
            Code = "HR",
            Type = DepartmentType.HumanResources,
            Description = "Talent acquisition, employee experience, and people operations.",
            Location = "Building B, Floor 2",
            CreatedAtUtc = DateTime.UtcNow
        };

        var fin = new Department
        {
            Name = "Finance",
            Code = "FIN",
            Type = DepartmentType.Finance,
            Description = "Corporate accounting, financial planning, and audit compliance.",
            Location = "Building A, Floor 2",
            CreatedAtUtc = DateTime.UtcNow
        };

        var mkt = new Department
        {
            Name = "Marketing",
            Code = "MKT",
            Type = DepartmentType.Marketing,
            Description = "Product marketing, growth analytics, and branding strategy.",
            Location = "Building B, Floor 3",
            CreatedAtUtc = DateTime.UtcNow
        };

        var ops = new Department
        {
            Name = "Operations",
            Code = "OPS",
            Type = DepartmentType.Operations,
            Description = "Business operations, IT support, and facilities management.",
            Location = "Building C, Floor 1",
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Departments.AddRange(eng, hr, fin, mkt, ops);
        await context.SaveChangesAsync();

        // 2. Seed Projects
        var proj1 = new Project
        {
            Name = "Cloud Infrastructure Migration",
            Client = "Enterprise Core",
            Description = "Migrate legacy on-prem services to Azure Kubernetes Service with CI/CD.",
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = DateTime.UtcNow.AddMonths(6),
            Budget = 250000m,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var proj2 = new Project
        {
            Name = "Workforce Portal 2.0",
            Client = "Internal Operations",
            Description = "Next-gen employee self-service portal built with ASP.NET Core Web API.",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(3),
            Budget = 120000m,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var proj3 = new Project
        {
            Name = "ERP Finance Synchronization",
            Client = "Global Finance Group",
            Description = "Automated real-time transactional sync between SAP and local databases.",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(11),
            Budget = 380000m,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Projects.AddRange(proj1, proj2, proj3);
        await context.SaveChangesAsync();

        // 3. Seed Employees with Profiles
        var employees = new List<Employee>
        {
            new()
            {
                FirstName = "Harshal",
                LastName = "Sonar",
                Email = "harshal.sonar@example.com",
                PhoneNumber = "+91-9876543210",
                Designation = "Senior Backend Developer",
                Salary = 95000m,
                DateOfJoining = DateTime.UtcNow.AddYears(-2),
                Status = EmploymentStatus.FullTime,
                DepartmentId = eng.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "123 Tech Park Road",
                    City = "Pune",
                    Country = "India",
                    EmergencyContactName = "Sunil Sonar",
                    EmergencyContactPhone = "+91-9876543211",
                    Skills = "C#, .NET 9, EF Core, SQL Server, Web API, Docker, Clean Architecture"
                }
            },
            new()
            {
                FirstName = "Sarah",
                LastName = "Jenkins",
                Email = "sarah.jenkins@example.com",
                PhoneNumber = "+1-555-0192",
                Designation = "Principal Software Architect",
                Salary = 135000m,
                DateOfJoining = DateTime.UtcNow.AddYears(-4),
                Status = EmploymentStatus.FullTime,
                DepartmentId = eng.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "456 Silicon Way",
                    City = "Seattle",
                    Country = "USA",
                    EmergencyContactName = "Mark Jenkins",
                    EmergencyContactPhone = "+1-555-0193",
                    Skills = "Distributed Systems, Microservices, Cloud Native, .NET Core"
                }
            },
            new()
            {
                FirstName = "Rahul",
                LastName = "Sharma",
                Email = "rahul.sharma@example.com",
                PhoneNumber = "+91-9822334455",
                Designation = "DevOps Engineer",
                Salary = 82000m,
                DateOfJoining = DateTime.UtcNow.AddMonths(-14),
                Status = EmploymentStatus.FullTime,
                DepartmentId = eng.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "78 Ring Road",
                    City = "Bengaluru",
                    Country = "India",
                    EmergencyContactName = "Pooja Sharma",
                    EmergencyContactPhone = "+91-9822334456",
                    Skills = "Kubernetes, Terraform, GitHub Actions, Azure, Linux"
                }
            },
            new()
            {
                FirstName = "Emily",
                LastName = "Watson",
                Email = "emily.watson@example.com",
                PhoneNumber = "+44-20-7946-0912",
                Designation = "HR Director",
                Salary = 105000m,
                DateOfJoining = DateTime.UtcNow.AddYears(-3),
                Status = EmploymentStatus.FullTime,
                DepartmentId = hr.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "12 Oxford Street",
                    City = "London",
                    Country = "UK",
                    EmergencyContactName = "David Watson",
                    EmergencyContactPhone = "+44-20-7946-0913",
                    Skills = "Talent Strategy, Performance Management, Executive Coaching"
                }
            },
            new()
            {
                FirstName = "Michael",
                LastName = "Chen",
                Email = "michael.chen@example.com",
                PhoneNumber = "+1-555-0144",
                Designation = "Financial Controller",
                Salary = 110000m,
                DateOfJoining = DateTime.UtcNow.AddYears(-2),
                Status = EmploymentStatus.FullTime,
                DepartmentId = fin.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "88 Wall Street",
                    City = "New York",
                    Country = "USA",
                    EmergencyContactName = "Linda Chen",
                    EmergencyContactPhone = "+1-555-0145",
                    Skills = "Financial Modeling, Audit, GAAP, SAP Financials"
                }
            },
            new()
            {
                FirstName = "Ananya",
                LastName = "Deshmukh",
                Email = "ananya.deshmukh@example.com",
                PhoneNumber = "+91-9765432100",
                Designation = "Product Marketing Manager",
                Salary = 78000m,
                DateOfJoining = DateTime.UtcNow.AddMonths(-8),
                Status = EmploymentStatus.FullTime,
                DepartmentId = mkt.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "50 Baner Road",
                    City = "Pune",
                    Country = "India",
                    EmergencyContactName = "Ramesh Deshmukh",
                    EmergencyContactPhone = "+91-9765432101",
                    Skills = "Content Strategy, SEO/SEM, Product Positioning, SaaS Marketing"
                }
            },
            new()
            {
                FirstName = "David",
                LastName = "Miller",
                Email = "david.miller@example.com",
                PhoneNumber = "+1-555-0177",
                Designation = "Operations Specialist",
                Salary = 65000m,
                DateOfJoining = DateTime.UtcNow.AddMonths(-18),
                Status = EmploymentStatus.FullTime,
                DepartmentId = ops.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "742 Evergreen Terrace",
                    City = "Springfield",
                    Country = "USA",
                    EmergencyContactName = "Susan Miller",
                    EmergencyContactPhone = "+1-555-0178",
                    Skills = "Logistics, Vendor Management, IT Helpdesk"
                }
            },
            new()
            {
                FirstName = "Priya",
                LastName = "Patil",
                Email = "priya.patil@example.com",
                PhoneNumber = "+91-9988776655",
                Designation = "Junior Backend Developer",
                Salary = 52000m,
                DateOfJoining = DateTime.UtcNow.AddMonths(-4),
                Status = EmploymentStatus.FullTime,
                DepartmentId = eng.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Profile = new EmployeeProfile
                {
                    Address = "15 MG Road",
                    City = "Mumbai",
                    Country = "India",
                    EmergencyContactName = "Nitin Patil",
                    EmergencyContactPhone = "+91-9988776656",
                    Skills = "C#, Web API, Entity Framework Core, SQL"
                }
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // 4. Assign projects
        var harshal = employees[0];
        var sarah = employees[1];
        var rahul = employees[2];
        var priya = employees[7];

        context.EmployeeProjects.AddRange(
            new EmployeeProject
            {
                EmployeeId = harshal.Id,
                ProjectId = proj2.Id,
                RoleOnProject = "Tech Lead & Lead Developer",
                AllocationPercentage = 80,
                AssignedDate = DateTime.UtcNow.AddMonths(-3)
            },
            new EmployeeProject
            {
                EmployeeId = harshal.Id,
                ProjectId = proj1.Id,
                RoleOnProject = "Backend Specialist",
                AllocationPercentage = 20,
                AssignedDate = DateTime.UtcNow.AddMonths(-2)
            },
            new EmployeeProject
            {
                EmployeeId = sarah.Id,
                ProjectId = proj1.Id,
                RoleOnProject = "Principal Architect",
                AllocationPercentage = 100,
                AssignedDate = DateTime.UtcNow.AddMonths(-6)
            },
            new EmployeeProject
            {
                EmployeeId = rahul.Id,
                ProjectId = proj1.Id,
                RoleOnProject = "DevOps Lead",
                AllocationPercentage = 100,
                AssignedDate = DateTime.UtcNow.AddMonths(-5)
            },
            new EmployeeProject
            {
                EmployeeId = priya.Id,
                ProjectId = proj2.Id,
                RoleOnProject = "API Developer",
                AllocationPercentage = 100,
                AssignedDate = DateTime.UtcNow.AddMonths(-3)
            }
        );

        await context.SaveChangesAsync();
    }
}
