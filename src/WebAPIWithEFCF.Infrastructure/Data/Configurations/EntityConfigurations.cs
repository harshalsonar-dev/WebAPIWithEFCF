using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPIWithEFCF.Domain.Entities;

namespace WebAPIWithEFCF.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.Location)
            .HasMaxLength(100);

        builder.Property(d => d.RowVersion)
            .IsRowVersion();
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(e => e.Designation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Salary)
            .HasPrecision(18, 2);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.Ignore(e => e.FullName);

        // 1-to-Many: Department -> Employees
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1-to-1: Employee -> EmployeeProfile (Optional)
        builder.HasOne(e => e.Profile)
            .WithOne(p => p.Employee)
            .HasForeignKey<EmployeeProfile>(p => p.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ToTable("EmployeeProfiles");

        builder.HasKey(p => p.EmployeeId);

        builder.Property(p => p.Address).HasMaxLength(200);
        builder.Property(p => p.City).HasMaxLength(50);
        builder.Property(p => p.Country).HasMaxLength(50);
        builder.Property(p => p.EmergencyContactName).HasMaxLength(100);
        builder.Property(p => p.EmergencyContactPhone).HasMaxLength(20);
        builder.Property(p => p.Skills).HasMaxLength(500);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Client)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Budget)
            .HasPrecision(18, 2);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();
    }
}

public class EmployeeProjectConfiguration : IEntityTypeConfiguration<EmployeeProject>
{
    public void Configure(EntityTypeBuilder<EmployeeProject> builder)
    {
        builder.ToTable("EmployeeProjects");

        // Composite Key
        builder.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

        builder.Property(ep => ep.RoleOnProject)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(ep => ep.Employee)
            .WithMany(e => e.EmployeeProjects)
            .HasForeignKey(ep => ep.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ep => ep.Project)
            .WithMany(p => p.EmployeeProjects)
            .HasForeignKey(ep => ep.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
