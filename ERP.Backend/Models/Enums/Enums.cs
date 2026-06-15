namespace ERP.Backend.Models.Enums
{
    public enum UserRole
    {
        ADMIN,
        PROJECT_MANAGER,
        TEAM_LEAD,
        DEVELOPER,
        VIEWER
    }

    public enum ProjectStatus
    {
        PLANNING,
        ACTIVE,
        ON_HOLD,
        COMPLETED,
        ARCHIVED
    }

    public enum ProjectPriority
    {
        LOW,
        MEDIUM,
        HIGH,
        CRITICAL
    }

    public enum ProjectMemberRole
    {
        OWNER,
        MANAGER,
        MEMBER,
        VIEWER
    }

    public enum TaskStatus
    {
        TODO,
        IN_PROGRESS,
        IN_REVIEW,
        DONE,
        BLOCKED
    }

    public enum TaskPriority
    {
        LOW,
        MEDIUM,
        HIGH,
        CRITICAL
    }

    public enum TaskType
    {
        FEATURE,
        BUG,
        IMPROVEMENT,
        TASK
    }

    public enum TimeEntryStatus
    {
        DRAFT,
        SUBMITTED,
        APPROVED,
        REJECTED
    }
}
