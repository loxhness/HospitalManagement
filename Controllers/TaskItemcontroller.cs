using HospitalManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace HospitalManagement.Controllers
{
    public class TaskItemController : Controller
    {
        private readonly string _connectionString;

        public TaskItemController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HospitalDB");
        }

        // GET: TaskItem
        public IActionResult Index()
        {
            List<TaskItem> tasks = new List<TaskItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department 
                               FROM Tasks t 
                               LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaskItem task = new TaskItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString(),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"])
                            };

                            if (reader["AssignedEmployeeId"] != DBNull.Value)
                            {
                                task.AssignedEmployeeId = Convert.ToInt32(reader["AssignedEmployeeId"]);
                                task.AssignedEmployee = new Employee
                                {
                                    Id = Convert.ToInt32(reader["EmployeeId"]),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Department = reader["Department"]?.ToString()
                                };
                            }

                            tasks.Add(task);
                        }
                    }
                }
            }

            return View(tasks);
        }

        // GET: TaskItem/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TaskItem task = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department 
                               FROM Tasks t 
                               LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id
                               WHERE t.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new TaskItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString(),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"])
                            };

                            if (reader["AssignedEmployeeId"] != DBNull.Value)
                            {
                                task.AssignedEmployeeId = Convert.ToInt32(reader["AssignedEmployeeId"]);
                                task.AssignedEmployee = new Employee
                                {
                                    Id = Convert.ToInt32(reader["EmployeeId"]),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Department = reader["Department"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: TaskItem/Create
        public IActionResult Create()
        {
            // Prepare the dropdown list for employees.
            List<Employee> employees = new List<Employee>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Employees";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"]?.ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Employees = employees;
            return View();
        }

        // POST: TaskItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = @"INSERT INTO Tasks (Title, Description, Priority, Status, DueDate, AssignedEmployeeId) 
                                   VALUES (@Title, @Description, @Priority, @Status, @DueDate, @AssignedEmployeeId);
                                   SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", taskItem.Title);
                        cmd.Parameters.AddWithValue("@Description", taskItem.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Priority", taskItem.Priority);
                        cmd.Parameters.AddWithValue("@Status", taskItem.Status);
                        cmd.Parameters.AddWithValue("@DueDate", taskItem.DueDate);
                        cmd.Parameters.AddWithValue("@AssignedEmployeeId",
                            taskItem.AssignedEmployeeId.HasValue ? (object)taskItem.AssignedEmployeeId.Value : DBNull.Value);

                        conn.Open();
                        taskItem.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // If we get here, something failed - redisplay the form
            List<Employee> employees = new List<Employee>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Employees";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"]?.ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Employees = employees;
            return View(taskItem);
        }

        // GET: TaskItem/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TaskItem task = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Tasks WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new TaskItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString(),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"])
                            };

                            if (reader["AssignedEmployeeId"] != DBNull.Value)
                            {
                                task.AssignedEmployeeId = Convert.ToInt32(reader["AssignedEmployeeId"]);
                            }
                        }
                    }
                }
            }

            if (task == null)
            {
                return NotFound();
            }

            // Get employees list for dropdown
            List<Employee> employees = new List<Employee>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Employees";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"]?.ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Employees = employees;
            return View(task);
        }

        // POST: TaskItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = @"UPDATE Tasks SET 
                                  Title = @Title, 
                                  Description = @Description, 
                                  Priority = @Priority, 
                                  Status = @Status, 
                                  DueDate = @DueDate, 
                                  AssignedEmployeeId = @AssignedEmployeeId 
                                  WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", taskItem.Id);
                        cmd.Parameters.AddWithValue("@Title", taskItem.Title);
                        cmd.Parameters.AddWithValue("@Description", taskItem.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Priority", taskItem.Priority);
                        cmd.Parameters.AddWithValue("@Status", taskItem.Status);
                        cmd.Parameters.AddWithValue("@DueDate", taskItem.DueDate);
                        cmd.Parameters.AddWithValue("@AssignedEmployeeId",
                            taskItem.AssignedEmployeeId.HasValue ? (object)taskItem.AssignedEmployeeId.Value : DBNull.Value);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            // Task wasn't found
                            return NotFound();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // If we get here, something failed - redisplay the form
            List<Employee> employees = new List<Employee>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Employees";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"]?.ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Employees = employees;
            return View(taskItem);
        }

        // GET: TaskItem/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TaskItem task = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department 
                               FROM Tasks t 
                               LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id
                               WHERE t.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new TaskItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString(),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"])
                            };

                            if (reader["AssignedEmployeeId"] != DBNull.Value)
                            {
                                task.AssignedEmployeeId = Convert.ToInt32(reader["AssignedEmployeeId"]);
                                task.AssignedEmployee = new Employee
                                {
                                    Id = Convert.ToInt32(reader["EmployeeId"]),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Department = reader["Department"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: TaskItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Tasks WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}