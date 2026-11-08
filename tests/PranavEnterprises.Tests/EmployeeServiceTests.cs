using Microsoft.EntityFrameworkCore;
using Moq;
using PranavEnterprises.Application.DTOs.Employee;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;
using PranavEnterprises.Infrastructure.Services;
using Xunit;

namespace PranavEnterprises.Tests;

public class EmployeeServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldAddEmployeeAndLogActivity()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();

        var dept = new Department { Name = "Technology", Code = "TECH" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var service = new EmployeeService(context, mockAudit.Object);

        var dto = new CreateEmployeeDto
        {
            EmployeeCode = "EMP-TEST-01",
            FirstName = "Aarav",
            LastName = "Naik",
            Email = "aarav.naik@pranav.com",
            Phone = "+91 99000 11223",
            Position = "Backend Architect",
            Salary = 135000,
            HireDate = DateTime.UtcNow,
            Status = EmployeeStatus.Active,
            DepartmentId = dept.Id
        };

        // Act
        var result = await service.CreateEmployeeAsync(dto, "TestAdmin", "127.0.0.1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EMP-TEST-01", result.EmployeeCode);
        Assert.Equal("Aarav Naik", result.FullName);
        Assert.Equal(135000, result.Salary);

        var inDb = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP-TEST-01");
        Assert.NotNull(inDb);
        Assert.Equal("Aarav", inDb.FirstName);

        mockAudit.Verify(a => a.LogAsync(
            It.IsAny<string?>(),
            "TestAdmin",
            It.IsAny<string>(),
            "Create",
            "Employee",
            It.IsAny<string>(),
            It.IsAny<string>(),
            "127.0.0.1"), Times.Once);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_WithSearch_ShouldReturnMatchingEmployees()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();

        var dept = new Department { Name = "Engineering", Code = "ENG" };
        context.Departments.Add(dept);

        context.Employees.AddRange(
            new Employee { EmployeeCode = "E01", FirstName = "John", LastName = "Doe", Email = "john@pranav.com", Position = "Dev", Department = dept },
            new Employee { EmployeeCode = "E02", FirstName = "Alice", LastName = "Smith", Email = "alice@pranav.com", Position = "QA", Department = dept }
        );
        await context.SaveChangesAsync();

        var service = new EmployeeService(context, mockAudit.Object);

        // Act
        var results = await service.GetAllEmployeesAsync("alice", null);

        // Assert
        Assert.Single(results);
        Assert.Equal("Alice Smith", results[0].FullName);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldSoftDeleteEmployee()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var mockAudit = new Mock<IActivityLogService>();

        var emp = new Employee
        {
            EmployeeCode = "DEL-01",
            FirstName = "Mark",
            LastName = "Taylor",
            Email = "mark@pranav.com",
            Position = "Intern",
            DepartmentId = 1
        };
        context.Employees.Add(emp);
        await context.SaveChangesAsync();

        var service = new EmployeeService(context, mockAudit.Object);

        // Act
        var success = await service.DeleteEmployeeAsync(emp.Id, "AdminUser");

        // Assert
        Assert.True(success);
        var dbEmp = await context.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == emp.Id);
        Assert.NotNull(dbEmp);
        Assert.True(dbEmp.IsDeleted);
    }
}
