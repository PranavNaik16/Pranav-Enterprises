using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Domain.Common;
using PranavEnterprises.Domain.Entities;

namespace PranavEnterprises.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables for clean enterprise naming
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(b => b.ToTable("UserTokens"));

        // Global query filter for soft delete
        builder.Entity<Department>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Employee>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Project>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProjectTask>().HasQueryFilter(x => !x.IsDeleted);

        // Department configuration
        builder.Entity<Department>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        });

        // Employee configuration
        builder.Entity<Employee>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            b.Property(x => x.Email).HasMaxLength(100).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Position).HasMaxLength(100).IsRequired();
            b.Property(x => x.Salary).HasPrecision(18, 2);

            b.HasOne(x => x.Department)
             .WithMany(d => d.Employees)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Project configuration
        builder.Entity<Project>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ProjectCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.ClientName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Budget).HasPrecision(18, 2);

            b.HasOne(x => x.Manager)
             .WithMany()
             .HasForeignKey(x => x.ManagerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ProjectTask configuration
        builder.Entity<ProjectTask>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TaskCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.Title).HasMaxLength(150).IsRequired();
            b.Property(x => x.EstimatedHours).HasPrecision(18, 2);
            b.Property(x => x.LoggedHours).HasPrecision(18, 2);

            b.HasOne(x => x.Project)
             .WithMany(p => p.Tasks)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.AssignedEmployee)
             .WithMany(e => e.Tasks)
             .HasForeignKey(x => x.AssignedEmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ActivityLog configuration
        builder.Entity<ActivityLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Action).HasMaxLength(50).IsRequired();
            b.Property(x => x.Module).HasMaxLength(50).IsRequired();
            b.Property(x => x.Details).HasMaxLength(1000).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IBaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
