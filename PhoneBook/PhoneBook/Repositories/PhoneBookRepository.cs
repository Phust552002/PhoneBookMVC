using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Diagnostics; // ⚠️ thêm để dùng Debug.WriteLine
using PhoneBook.Models;
using Microsoft.Extensions.Configuration;

public interface IPhoneBookRepository
{
    Task<List<Department>> GetDepartmentsAsync();
    Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<List<Employee>> GetAllEmployeesAsync();
}

public class PhoneBookRepository : IPhoneBookRepository
{
    private readonly string _connectionString;

    public PhoneBookRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("HRMDb");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    // 🔹 Lấy danh sách phòng ban và dựng cây
    public async Task<List<Department>> GetDepartmentsAsync()
    {
        const string sql = @"
            SELECT DepartmentId, DepartmentName, ParentId, Level, RootName, Status
            FROM H0_Departments
            WHERE Status > 0
            ORDER BY ParentId, DepartmentName";

        using var conn = CreateConnection();

        Debug.WriteLine("📡 [GetDepartmentsAsync] Đang thực thi truy vấn SQL...");
        var all = (await conn.QueryAsync<Department>(sql)).ToList();

        Debug.WriteLine($"✅ Đã lấy {all.Count} phòng ban từ DB.");

        if (!all.Any())
        {
            Debug.WriteLine("⚠️ Không có dữ liệu phòng ban nào được trả về (có thể Status != 1 hoặc bảng trống).");
        }

        // Dựng cây (Tree View)
        var lookup = all.ToDictionary(d => d.DepartmentId);
        foreach (var dept in all)
        {
            if (dept.ParentId != -1 && lookup.ContainsKey(dept.ParentId))
            {
                lookup[dept.ParentId].Children.Add(dept);
                Debug.WriteLine($"🌿 Gắn {dept.DepartmentName} vào cha {lookup[dept.ParentId].DepartmentName}");
            }
        }

        var roots = all.Where(d => d.ParentId == -1).ToList();
        Debug.WriteLine($"🌳 Có {roots.Count} phòng ban gốc được tạo cây.");

        return roots;
    }

    // 🔹 Lấy nhân viên theo phòng ban
    public async Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        const string sql = @"
        ;WITH cte AS (
            SELECT DepartmentId 
            FROM H0_Departments 
            WHERE DepartmentId = @departmentId
            UNION ALL
            SELECT d.DepartmentId
            FROM H0_Departments d
            INNER JOIN cte ON d.ParentId = cte.DepartmentId
        )
        SELECT DISTINCT 
            e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
            e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status
        FROM H0_DepartmentEmployee de
        INNER JOIN Employees e ON e.UserId = de.UserId
        INNER JOIN cte ON de.DepartmentId = cte.DepartmentId
        WHERE e.Status > 0
        ORDER BY e.FullName";

        using var conn = CreateConnection();
        var result = (await conn.QueryAsync<Employee>(sql, new { departmentId })).ToList();

        Debug.WriteLine($"✅ Đã lấy {result.Count} nhân viên (bao gồm cả phòng con) của phòng ban {departmentId}.");
        return result;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        const string sql = @"
        SELECT 
            e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
            e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status
        FROM Employees e
        WHERE e.Status > 0
        ORDER BY e.UserId";

        using var connection = CreateConnection();
        var employees = await connection.QueryAsync<Employee>(sql);
        return employees.ToList();
    }
}
