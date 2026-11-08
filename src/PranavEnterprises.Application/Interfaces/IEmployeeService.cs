using PranavEnterprises.Application.DTOs.Employee;
using PranavEnterprises.Domain.Entities;

namespace PranavEnterprises.Application.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllEmployeesAsync(string? search = null, int? departmentId = null);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<EmployeeDto?> UpdateEmployeeAsync(UpdateEmployeeDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<bool> DeleteEmployeeAsync(int id, string? currentUserName = null, string? ipAddress = null);
    Task<List<Department>> GetDepartmentsAsync();
}
