using ERP.Backend.Models.Enums;
using ErpTaskStatus = ERP.Backend.Models.Enums.TaskStatus;

namespace ERP.Backend.ViewModels
{
    // ─── Auth ViewModels ───────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.DEVELOPER;
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // ─── User ViewModels ───────────────────────────────────────────────────────
    public class UpdateProfileViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? Designation { get; set; }
        public string? EmployeeId { get; set; }
        public List<string>? Skills { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? TeamId { get; set; }
        public UserRole? Role { get; set; }
    }

    // ─── Department ViewModels ─────────────────────────────────────────────────
    public class CreateDepartmentViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? HeadId { get; set; }
        public Guid? ParentId { get; set; }
        public string Color { get; set; } = "#6366f1";
    }

    public class UpdateDepartmentViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? HeadId { get; set; }
        public Guid? ParentId { get; set; }
        public string? Color { get; set; }
    }

    // ─── Team ViewModels ───────────────────────────────────────────────────────
    public class CreateTeamViewModel
    {
        public string Name { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid? LeadId { get; set; }
        public string Color { get; set; } = "#06b6d4";
    }

    public class UpdateTeamViewModel
    {
        public string? Name { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? LeadId { get; set; }
        public string? Color { get; set; }
    }

    // ─── Project ViewModels ────────────────────────────────────────────────────
    public class CreateProjectViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; } = ProjectStatus.PLANNING;
        public ProjectPriority Priority { get; set; } = ProjectPriority.MEDIUM;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public List<Guid> MemberIds { get; set; } = new();
    }

    public class UpdateProjectViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus? Status { get; set; }
        public ProjectPriority? Priority { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class AddProjectMemberViewModel
    {
        public Guid UserId { get; set; }
        public ProjectMemberRole Role { get; set; } = ProjectMemberRole.MEMBER;
    }

    // ─── Milestone ViewModels ──────────────────────────────────────────────────
    public class CreateMilestoneViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int Order { get; set; } = 0;
    }

    public class UpdateMilestoneViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? IsCompleted { get; set; }
        public int? Order { get; set; }
    }

    // ─── Task ViewModels ───────────────────────────────────────────────────────
    public class CreateTaskViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? ReporterId { get; set; }
        public ErpTaskStatus Status { get; set; } = ErpTaskStatus.TODO;
        public TaskPriority Priority { get; set; } = TaskPriority.MEDIUM;
        public TaskType TaskType { get; set; } = TaskType.TASK;
        public List<string> Labels { get; set; } = new();
        public decimal EstimatedHours { get; set; } = 0;
        public DateTime? DueDate { get; set; }
        public int Order { get; set; } = 0;
    }

    public class UpdateTaskViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? ReporterId { get; set; }
        public ErpTaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public TaskType? TaskType { get; set; }
        public List<string>? Labels { get; set; }
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Order { get; set; }
    }

    // ─── Comment ViewModels ────────────────────────────────────────────────────
    public class CreateCommentViewModel
    {
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentViewModel
    {
        public string Content { get; set; } = string.Empty;
    }

    // ─── TimeEntry ViewModels ──────────────────────────────────────────────────
    public class StartTimerViewModel
    {
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsBillable { get; set; } = true;
    }

    public class StopTimerViewModel
    {
        public string? Description { get; set; }
    }

    public class CreateTimeEntryViewModel
    {
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationMinutes { get; set; } = 0;
        public bool IsBillable { get; set; } = true;
        public TimeEntryStatus Status { get; set; } = TimeEntryStatus.DRAFT;
    }

    public class UpdateTimeEntryViewModel
    {
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMinutes { get; set; }
        public bool? IsBillable { get; set; }
        public TimeEntryStatus? Status { get; set; }
    }

    // ─── Filter / Pagination ViewModels ───────────────────────────────────────
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Ordering { get; set; }
    }

    public class TimeEntryFilterParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public TimeEntryStatus? Status { get; set; }
        public bool? IsBillable { get; set; }
    }

    public class TaskFilterParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public Guid? AssigneeId { get; set; }
        public ErpTaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public TaskType? TaskType { get; set; }
    }
}
