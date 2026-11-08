using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.Employee;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public EmployeeService(ApplicationDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync(string? search = null, int? departmentId = null)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Tasks)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.EmployeeCode.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                e.Position.ToLower().Contains(s));
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                Position = e.Position,
                Salary = e.Salary,
                HireDate = e.HireDate,
                Status = e.Status,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.Name : "Unassigned",
                AvatarUrl = e.AvatarUrl,
                ActiveTasksCount = e.Tasks.Count(t => t.Status != TaskItemStatus.Completed),
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var e = await _context.Employees
            .Include(x => x.Department)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) return null;

        return new EmployeeDto
        {
            Id = e.Id,
            EmployeeCode = e.EmployeeCode,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Position = e.Position,
            Salary = e.Salary,
            HireDate = e.HireDate,
            Status = e.Status,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? "Unassigned",
            AvatarUrl = e.AvatarUrl,
            ActiveTasksCount = e.Tasks.Count(t => t.Status != TaskItemStatus.Completed),
            CreatedAt = e.CreatedAt
        };
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Position = dto.Position,
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            Status = dto.Status,
            DepartmentId = dto.DepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Create",
            "Employee",
            employee.Id.ToString(),
            $"Created employee record '{employee.FullName}' ({employee.EmployeeCode}) in department ID {employee.DepartmentId}.",
            ipAddress);

        return (await GetEmployeeByIdAsync(employee.Id))!;
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(UpdateEmployeeDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var employee = await _context.Employees.FindAsync(dto.Id);
        if (employee == null) return null;

        employee.EmployeeCode = dto.EmployeeCode;
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.Position = dto.Position;
        employee.Salary = dto.Salary;
        employee.HireDate = dto.HireDate;
        employee.Status = dto.Status;
        employee.DepartmentId = dto.DepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Update",
            "Employee",
            employee.Id.ToString(),
            $"Updated employee record '{employee.FullName}' ({employee.EmployeeCode}).",
            ipAddress);

        return (await GetEmployeeByIdAsync(employee.Id))!;
    }

    public async Task<bool> DeleteEmployeeAsync(int id, string? currentUserName = null, string? ipAddress = null)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Delete",
            "Employee",
            id.ToString(),
            $"Soft-deleted employee '{employee.FullName}' ({employee.EmployeeCode}).",
            ipAddress);

        return true;
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _context.Departments.OrderBy(d => d.Name).ToListAsync();
    }
}
