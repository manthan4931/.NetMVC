using ERP.Backend.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ErpTaskStatus = ERP.Backend.Models.Enums.TaskStatus;

namespace ERP.Backend.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public UserRole Role { get; set; } = UserRole.DEVELOPER;

        public string? Avatar { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string Designation { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public string SkillsJson { get; set; } = "[]";

        public DateTime? DateOfJoining { get; set; }

        public Guid? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public Guid? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team? Team { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }

    public class Department
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid? HeadId { get; set; }
        [ForeignKey("HeadId")]
        public User? Head { get; set; }

        public Guid? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Department? Parent { get; set; }

        public string Color { get; set; } = "#6366f1";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
        public ICollection<User> Members { get; set; } = new List<User>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }

    public class Team
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;

        public Guid? LeadId { get; set; }
        [ForeignKey("LeadId")]
        public User? Lead { get; set; }

        public string Color { get; set; } = "#06b6d4";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Members { get; set; } = new List<User>();
    }

    public class Project
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public ProjectStatus Status { get; set; } = ProjectStatus.PLANNING;

        [Column(TypeName = "TEXT")]
        public ProjectPriority Priority { get; set; } = ProjectPriority.MEDIUM;

        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public string TagsJson { get; set; } = "[]";

        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }

        public Guid? CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }

    public class ProjectMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Column(TypeName = "TEXT")]
        public ProjectMemberRole Role { get; set; } = ProjectMemberRole.MEMBER;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public class Milestone
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProjectTask
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        public Guid? AssigneeId { get; set; }
        [ForeignKey("AssigneeId")]
        public User? Assignee { get; set; }

        public Guid? ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        [Column(TypeName = "TEXT")]
        public ErpTaskStatus Status { get; set; } = ErpTaskStatus.TODO;

        [Column(TypeName = "TEXT")]
        public TaskPriority Priority { get; set; } = TaskPriority.MEDIUM;

        [Column(TypeName = "TEXT")]
        public TaskType TaskType { get; set; } = TaskType.TASK;

        [Column(TypeName = "TEXT")]
        public string LabelsJson { get; set; } = "[]";

        [Column(TypeName = "DECIMAL(10,2)")]
        public decimal EstimatedHours { get; set; } = 0;

        [Column(TypeName = "DECIMAL(10,2)")]
        public decimal ActualHours { get; set; } = 0;

        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int Order { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }

    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TaskId { get; set; }
        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; } = null!;

        [Required]
        public Guid AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Attachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TaskId { get; set; }
        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; } = null!;

        [Required]
        public Guid UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public User UploadedBy { get; set; } = null!;

        public string File { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int FileSize { get; set; } = 0;
        public string ContentType { get; set; } = "application/octet-stream";

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class TimeEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public Guid? TaskId { get; set; }
        [ForeignKey("TaskId")]
        public ProjectTask? Task { get; set; }

        [Required]
        public Guid ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int DurationMinutes { get; set; } = 0;
        public bool IsRunning { get; set; } = false;
        public bool IsBillable { get; set; } = true;

        [Column(TypeName = "TEXT")]
        public TimeEntryStatus Status { get; set; } = TimeEntryStatus.DRAFT;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ActivityLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string EntityTitle { get; set; } = string.Empty;

        public Guid? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [Column(TypeName = "TEXT")]
        public string MetadataJson { get; set; } = "{}";

        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
