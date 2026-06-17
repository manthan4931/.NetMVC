using System;
using System.Collections.Generic;

namespace ERP.Backend.ViewModels
{
    public class EmployeeDetailViewModel
    {
        public UserModel? User { get; set; }
        public List<TaskModel> Tasks { get; set; } = new();
        public List<ActivityLogModel> ActivityLog { get; set; } = new();
        public List<TimeEntryModel> TimeEntries { get; set; } = new();
        public UserModel? CurrentUser { get; set; }
    }

    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public DateTime DateJoined { get; set; }
        public bool IsActive { get; set; }
    }

    public class TaskModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // TODO, IN_PROGRESS, IN_REVIEW, DONE, BLOCKED
        public string Priority { get; set; } = string.Empty; // CRITICAL, HIGH, MEDIUM, LOW
        public double? EstimatedHours { get; set; }
    }

    public class ActivityLogModel
    {
        public string Id { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // CREATED, UPDATED, STATUS_CHANGED, etc.
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }

    public class TimeEntryModel
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsBillable { get; set; }
        public string Status { get; set; } = string.Empty; // APPROVED, REJECTED, DRAFT
    }
}
