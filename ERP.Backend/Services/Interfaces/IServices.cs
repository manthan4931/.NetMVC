using ERP.Backend.Models;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string token, string refreshToken, object userData, string error)>
            LoginAsync(LoginViewModel model);
        Task<(bool success, string message, object? userData)>
            RegisterAsync(RegisterViewModel model);
        Task<(bool success, string token)>
            RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordViewModel model);
        string GenerateJwtToken(User user);
    }

    public interface IUserService
    {
        Task<object?> GetUserByIdAsync(Guid id);
        Task<(IEnumerable<object> items, int total)> GetUsersAsync(int page, int pageSize, string? search, string? role);
        Task<object?> UpdateUserAsync(Guid id, UpdateProfileViewModel model);
        Task<object?> UploadAvatarAsync(Guid userId, IFormFile file);
        Task<bool> DeleteUserAsync(Guid id);
    }

    public interface IDepartmentService
    {
        Task<(IEnumerable<object> items, int total)> GetDepartmentsAsync(int page, int pageSize, string? search);
        Task<object?> GetDepartmentByIdAsync(Guid id);
        Task<object> CreateDepartmentAsync(CreateDepartmentViewModel model);
        Task<object?> UpdateDepartmentAsync(Guid id, UpdateDepartmentViewModel model);
        Task<bool> DeleteDepartmentAsync(Guid id);
    }

    public interface ITeamService
    {
        Task<(IEnumerable<object> items, int total)> GetTeamsAsync(int page, int pageSize, string? search);
        Task<object?> GetTeamByIdAsync(Guid id);
        Task<object> CreateTeamAsync(CreateTeamViewModel model);
        Task<object?> UpdateTeamAsync(Guid id, UpdateTeamViewModel model);
        Task<bool> DeleteTeamAsync(Guid id);
    }

    public interface IProjectService
    {
        Task<(IEnumerable<object> items, int total)> GetProjectsAsync(int page, int pageSize, string? search, string? status, Guid? userId);
        Task<object?> GetProjectByIdAsync(Guid id);
        Task<object> CreateProjectAsync(CreateProjectViewModel model, Guid createdById);
        Task<object?> UpdateProjectAsync(Guid id, UpdateProjectViewModel model);
        Task<bool> DeleteProjectAsync(Guid id);
        Task<object?> AddMemberAsync(Guid projectId, AddProjectMemberViewModel model);
        Task<bool> RemoveMemberAsync(Guid projectId, Guid userId);
        Task<IEnumerable<object>> GetMembersAsync(Guid projectId);
        Task<object> GetProjectStatsAsync(Guid projectId);
    }

    public interface IMilestoneService
    {
        Task<IEnumerable<object>> GetMilestonesAsync(Guid projectId);
        Task<object?> GetMilestoneByIdAsync(Guid id);
        Task<object> CreateMilestoneAsync(Guid projectId, CreateMilestoneViewModel model);
        Task<object?> UpdateMilestoneAsync(Guid id, UpdateMilestoneViewModel model);
        Task<bool> DeleteMilestoneAsync(Guid id);
    }

    public interface ITaskService
    {
        Task<(IEnumerable<object> items, int total)> GetTasksAsync(TaskFilterParams filters);
        Task<object?> GetTaskByIdAsync(Guid id);
        Task<object> CreateTaskAsync(CreateTaskViewModel model, Guid reporterId);
        Task<object?> UpdateTaskAsync(Guid id, UpdateTaskViewModel model);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<IEnumerable<object>> GetCommentsAsync(Guid taskId);
        Task<object> AddCommentAsync(Guid taskId, Guid authorId, CreateCommentViewModel model);
        Task<object?> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentViewModel model);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<object> UploadAttachmentAsync(Guid taskId, Guid userId, IFormFile file);
        Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid userId);
    }

    public interface ITimeTrackingService
    {
        Task<(IEnumerable<object> items, int total)> GetTimeEntriesAsync(TimeEntryFilterParams filters);
        Task<object?> GetTimeEntryByIdAsync(Guid id);
        Task<object> CreateTimeEntryAsync(CreateTimeEntryViewModel model, Guid userId);
        Task<object?> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryViewModel model, Guid userId);
        Task<bool> DeleteTimeEntryAsync(Guid id, Guid userId);
        Task<object> StartTimerAsync(StartTimerViewModel model, Guid userId);
        Task<(bool success, object? entry, string error)> StopTimerAsync(Guid userId);
        Task<object?> GetRunningTimerAsync(Guid userId);
        Task<byte[]> GenerateCsvBytesAsync(TimeEntryFilterParams filters);
        Task<object> GetTodayStatsAsync();
    }

    public interface IActivityService
    {
        Task LogAsync(Guid userId, string action, string entityType, Guid entityId,
                      string entityTitle, Guid? projectId, string description, object? metadata = null);
        Task<(IEnumerable<object> items, int total)> GetActivitiesAsync(
            int page, int pageSize, Guid? userId, Guid? projectId, string? entityType);
    }
}
