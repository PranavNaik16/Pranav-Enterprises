using Microsoft.EntityFrameworkCore;
using Moq;
using PranavEnterprises.Application.DTOs.Project;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;
using PranavEnterprises.Infrastructure.Services;
using Xunit;

namespace PranavEnterprises.Tests;

public class ProjectServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldSaveProjectSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();
        var service = new ProjectService(context, mockAudit.Object);

        var dto = new CreateProjectDto
        {
            ProjectCode = "PRJ-TEST-10",
            Name = "Enterprise AI Integration",
            Description = "Deploying machine learning models into production pipelines.",
            ClientName = "Global FinCorp",
            Budget = 250000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(4),
            Status = ProjectStatus.Planning,
            Priority = PriorityLevel.High
        };

        // Act
        var project = await service.CreateProjectAsync(dto, "ManagerUser", "127.0.0.1");

        // Assert
        Assert.NotNull(project);
        Assert.Equal("PRJ-TEST-10", project.ProjectCode);
        Assert.Equal(250000, project.Budget);
        Assert.Equal(ProjectStatus.Planning, project.Status);

        var inDb = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == "PRJ-TEST-10");
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateProjectStatusAndAuditLog()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();

        var project = new Project
        {
            ProjectCode = "PRJ-STATUS-01",
            Name = "Migration Project",
            ClientName = "Internal",
            Status = ProjectStatus.Planning,
            Budget = 50000
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var service = new ProjectService(context, mockAudit.Object);

        // Act
        var result = await service.UpdateStatusAsync(project.Id, ProjectStatus.InProgress, "ManagerVikram", "127.0.0.1");

        // Assert
        Assert.True(result);
        var inDb = await context.Projects.FindAsync(project.Id);
        Assert.NotNull(inDb);
        Assert.Equal(ProjectStatus.InProgress, inDb.Status);

        mockAudit.Verify(a => a.LogAsync(
            It.IsAny<string?>(),
            "ManagerVikram",
            It.IsAny<string>(),
            "StatusChange",
            "Project",
            project.Id.ToString(),
            It.IsAny<string>(),
            "127.0.0.1"), Times.Once);
    }

    [Fact]
    public async Task ProgressPercentage_ShouldComputeCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();

        var project = new Project
        {
            ProjectCode = "PRJ-PROG-01",
            Name = "Task Calculation Project",
            ClientName = "Internal",
            Budget = 75000
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        context.ProjectTasks.AddRange(
            new ProjectTask { TaskCode = "T1", Title = "Task 1", ProjectId = project.Id, Status = TaskItemStatus.Completed },
            new ProjectTask { TaskCode = "T2", Title = "Task 2", ProjectId = project.Id, Status = TaskItemStatus.Completed },
            new ProjectTask { TaskCode = "T3", Title = "Task 3", ProjectId = project.Id, Status = TaskItemStatus.InProgress },
            new ProjectTask { TaskCode = "T4", Title = "Task 4", ProjectId = project.Id, Status = TaskItemStatus.Todo }
        );
        await context.SaveChangesAsync();

        var service = new ProjectService(context, mockAudit.Object);

        // Act
        var result = await service.GetProjectByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalTasks);
        Assert.Equal(2, result.CompletedTasks);
        Assert.Equal(50, result.ProgressPercentage); // 2 out of 4 = 50%
    }
}
