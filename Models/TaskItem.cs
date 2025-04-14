using HospitalManagement.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        // Values such as "Low", "Medium", "High"
        public string Priority { get; set; }

        [Required]
        // Values such as "Pending", "In Progress", "Completed"
        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        // Optionally assign the task to an employee.
        public int? AssignedEmployeeId { get; set; }
        public Employee AssignedEmployee { get; set; }
    }
}
