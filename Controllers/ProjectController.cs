// Data/HospitalDB.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using HospitalManagement.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;

namespace HospitalManagement.Data
{
    public class HospitalDB
    {
        private readonly string _connectionString;

        public HospitalDB(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HospitalDB");
        }

        // Simulated DbSet-like properties
        public EmployeeCollection Employees => new EmployeeCollection(_connectionString);
        public ProjectCollection Projects => new ProjectCollection(_connectionString);
        public TaskItemCollection TaskItems => new TaskItemCollection(_connectionString);

        // Direct Add/Update/Remove methods for entities
        public void Add<T>(T entity) where T : class
        {
            if (entity is Employee employee)
                Employees.Add(employee);
            else if (entity is Project project)
                Projects.Add(project);
            else if (entity is TaskItem task)
                TaskItems.Add(task);
        }

        public void Update<T>(T entity) where T : class
        {
            if (entity is Employee employee)
                Employees.Update(employee);
            else if (entity is Project project)
                Projects.Update(project);
            else if (entity is TaskItem task)
                TaskItems.Update(task);
        }

        public void Remove<T>(T entity) where T : class
        {
            if (entity is Employee employee)
                Employees.Remove(employee);
            else if (entity is Project project)
                Projects.Remove(project);
            else if (entity is TaskItem task)
                TaskItems.Remove(task);
        }

        // Mimicking SaveChanges functionality
        public async Task<int> SaveChangesAsync()
        {
            // This method does nothing in our implementation
            // Changes are saved directly in each operation
            return await Task.FromResult(1);
        }

        // Collection classes to mimic EF Core behavior
        public class EmployeeCollection
        {
            private readonly string _connectionString;

            public EmployeeCollection(string connectionString)
            {
                _connectionString = connectionString;
            }

            // Add ToList method (non-async version)
            public List<Employee> ToList()
            {
                List<Employee> employees = new List<Employee>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Employees", conn))
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

                return employees;
            }

            public async Task<List<Employee>> ToListAsync()
            {
                List<Employee> employees = new List<Employee>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Employees", conn))
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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

                return employees;
            }

            public async Task<Employee> FindAsync(int? id)
            {
                if (id == null)
                    return null;

                Employee employee = null;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                employee = new Employee
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
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

                return employee;
            }

            public async Task<Employee> FirstOrDefaultAsync(Expression<Func<Employee, bool>> predicate)
            {
                // This is a simplified implementation that only supports Id equality
                var param = predicate.Parameters[0];
                var body = predicate.Body as BinaryExpression;

                if (body != null &&
                    body.NodeType == ExpressionType.Equal &&
                    body.Left is MemberExpression memberExpr &&
                    memberExpr.Member.Name == "Id")
                {
                    var right = body.Right as ConstantExpression;
                    int id = (int)right.Value;
                    return await FindAsync(id);
                }

                // For simplicity, just return the first employee
                var employees = await ToListAsync();
                return employees.FirstOrDefault();
            }

            public bool Any(Expression<Func<Employee, bool>> predicate)
            {
                // This is a simplified implementation that only supports Id equality
                var param = predicate.Parameters[0];
                var body = predicate.Body as BinaryExpression;

                if (body != null &&
                    body.NodeType == ExpressionType.Equal &&
                    body.Left is MemberExpression memberExpr &&
                    memberExpr.Member.Name == "Id")
                {
                    var right = body.Right as ConstantExpression;
                    int id = (int)right.Value;

                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Employees WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            conn.Open();
                            int count = (int)cmd.ExecuteScalar();
                            return count > 0;
                        }
                    }
                }

                // For simplicity, check if we have any employees
                return ToList().Any();
            }

            public void Add(Employee employee)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Employees (FirstName, LastName, Email, Role, Department) " +
                        "VALUES (@FirstName, @LastName, @Email, @Role, @Department); " +
                        "SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                        cmd.Parameters.AddWithValue("@Email", employee.Email);
                        cmd.Parameters.AddWithValue("@Role", employee.Role);
                        cmd.Parameters.AddWithValue("@Department", employee.Department as object ?? DBNull.Value);

                        conn.Open();
                        employee.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }

            public void Update(Employee employee)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, " +
                        "Email = @Email, Role = @Role, Department = @Department " +
                        "WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", employee.Id);
                        cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                        cmd.Parameters.AddWithValue("@Email", employee.Email);
                        cmd.Parameters.AddWithValue("@Role", employee.Role);
                        cmd.Parameters.AddWithValue("@Department", employee.Department as object ?? DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            public void Remove(Employee employee)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", employee.Id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public class ProjectCollection
        {
            private readonly string _connectionString;

            public ProjectCollection(string connectionString)
            {
                _connectionString = connectionString;
            }

            public List<Project> ToList()
            {
                List<Project> projects = new List<Project>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Projects", conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                projects.Add(new Project
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"])
                                });
                            }
                        }
                    }
                }

                return projects;
            }

            public async Task<List<Project>> ToListAsync()
            {
                List<Project> projects = new List<Project>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Projects", conn))
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                projects.Add(new Project
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"])
                                });
                            }
                        }
                    }
                }

                return projects;
            }

            public async Task<Project> FindAsync(int? id)
            {
                if (id == null)
                    return null;

                Project project = null;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Projects WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                project = new Project
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"])
                                };
                            }
                        }
                    }
                }

                return project;
            }

            public async Task<Project> FirstOrDefaultAsync(Expression<Func<Project, bool>> predicate)
            {
                // Simplified implementation
                var projects = await ToListAsync();
                return projects.FirstOrDefault();
            }

            public bool Any(Expression<Func<Project, bool>> predicate)
            {
                // Simplified implementation
                return ToList().Any();
            }

            public void Add(Project project)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Projects (ProjectName, Description, StartDate, EndDate) " +
                        "VALUES (@ProjectName, @Description, @StartDate, @EndDate); " +
                        "SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                        cmd.Parameters.AddWithValue("@Description", project.Description as object ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartDate", project.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", project.EndDate);

                        conn.Open();
                        project.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }

            public void Update(Project project)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Projects SET ProjectName = @ProjectName, Description = @Description, " +
                        "StartDate = @StartDate, EndDate = @EndDate " +
                        "WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", project.Id);
                        cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                        cmd.Parameters.AddWithValue("@Description", project.Description as object ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartDate", project.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", project.EndDate);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            public void Remove(Project project)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Projects WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", project.Id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public class TaskItemCollection
        {
            private readonly string _connectionString;
            private bool _includeEmployee = false;

            public TaskItemCollection(string connectionString)
            {
                _connectionString = connectionString;
            }

            // Add Include method to mimic Entity Framework
            public TaskItemCollection Include(string navigationPropertyPath)
            {
                if (navigationPropertyPath == "AssignedEmployee")
                    _includeEmployee = true;
                return this;
            }

            public List<TaskItem> ToList()
            {
                List<TaskItem> tasks = new List<TaskItem>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = "SELECT t.* FROM Tasks t";
                    if (_includeEmployee)
                    {
                        sql = "SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department " +
                              "FROM Tasks t LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id";
                    }

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

                                    if (_includeEmployee)
                                    {
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

                                tasks.Add(task);
                            }
                        }
                    }
                }

                return tasks;
            }

            public async Task<List<TaskItem>> ToListAsync()
            {
                List<TaskItem> tasks = new List<TaskItem>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = "SELECT t.* FROM Tasks t";
                    if (_includeEmployee)
                    {
                        sql = "SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department " +
                              "FROM Tasks t LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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

                                    if (_includeEmployee)
                                    {
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

                                tasks.Add(task);
                            }
                        }
                    }
                }

                return tasks;
            }

            public async Task<TaskItem> FindAsync(int? id)
            {
                if (id == null)
                    return null;

                TaskItem task = null;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = "SELECT t.* FROM Tasks t WHERE t.Id = @Id";
                    if (_includeEmployee)
                    {
                        sql = "SELECT t.*, e.Id as EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, e.Department " +
                              "FROM Tasks t LEFT JOIN Employees e ON t.AssignedEmployeeId = e.Id " +
                              "WHERE t.Id = @Id";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
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

                                    if (_includeEmployee)
                                    {
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
                }

                return task;
            }

            public async Task<TaskItem> FirstOrDefaultAsync(Expression<Func<TaskItem, bool>> predicate)
            {
                // Simplified implementation
                var tasks = await ToListAsync();
                return tasks.FirstOrDefault();
            }

            public bool Any(Expression<Func<TaskItem, bool>> predicate)
            {
                // Simplified implementation
                return ToList().Any();
            }

            public void Add(TaskItem task)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Tasks (Title, Description, Priority, Status, DueDate, AssignedEmployeeId) " +
                        "VALUES (@Title, @Description, @Priority, @Status, @DueDate, @AssignedEmployeeId); " +
                        "SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", task.Title);
                        cmd.Parameters.AddWithValue("@Description", task.Description as object ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Priority", task.Priority);
                        cmd.Parameters.AddWithValue("@Status", task.Status);
                        cmd.Parameters.AddWithValue("@DueDate", task.DueDate);
                        cmd.Parameters.AddWithValue("@AssignedEmployeeId", task.AssignedEmployeeId as object ?? DBNull.Value);

                        conn.Open();
                        task.Id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }

            public void Update(TaskItem task)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Tasks SET Title = @Title, Description = @Description, " +
                        "Priority = @Priority, Status = @Status, DueDate = @DueDate, " +
                        "AssignedEmployeeId = @AssignedEmployeeId " +
                        "WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", task.Id);
                        cmd.Parameters.AddWithValue("@Title", task.Title);
                        cmd.Parameters.AddWithValue("@Description", task.Description as object ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Priority", task.Priority);
                        cmd.Parameters.AddWithValue("@Status", task.Status);
                        cmd.Parameters.AddWithValue("@DueDate", task.DueDate);
                        cmd.Parameters.AddWithValue("@AssignedEmployeeId", task.AssignedEmployeeId as object ?? DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            public void Remove(TaskItem task)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Tasks WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", task.Id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}