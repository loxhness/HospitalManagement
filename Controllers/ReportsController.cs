using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace HospitalManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly string _connectionString;

        // Use dependency injection to get configuration
        public ReportsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HospitalDB");
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StaffWorkload()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT s.StaffId, s.FirstName + ' ' + s.LastName AS StaffName, d.Name AS Department, COUNT(t.TaskId) AS TotalTasks, SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks FROM Staff s LEFT JOIN Departments d ON s.DepartmentId = d.DepartmentId LEFT JOIN Tasks t ON t.AssignedToId = s.StaffId GROUP BY s.StaffId, s.FirstName, s.LastName, d.Name", conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    conn.Open();
                    adapter.Fill(dt);
                }
            }
            return View(dt);
        }

        public IActionResult DepartmentPerformance()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT d.DepartmentId, d.Name, COUNT(DISTINCT s.StaffId) AS StaffCount, COUNT(t.TaskId) AS TotalTasks, SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks FROM Departments d LEFT JOIN Staff s ON d.DepartmentId = s.DepartmentId LEFT JOIN Tasks t ON t.AssignedToId = s.StaffId GROUP BY d.DepartmentId, d.Name", conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    conn.Open();
                    adapter.Fill(dt);
                }
            }
            return View(dt);
        }
    }
}