using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Models.Enums;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;
using ErpTaskStatus = ERP.Backend.Models.Enums.TaskStatus;

namespace ERP.Backend.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivityService _activity;

        public ProjectService(ApplicationDbContext db, IActivityService activity)
        {
            _db = db;
            _activity = activity;
        }

        private static object MapProject(Project p, bool detailed = false)
        {
            var dto = new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description,
                status = p.Status.ToString(),
                priority = p.Priority.ToString(),
                category = p.Category,
                tags = JsonSerializer.Deserialize<List<string>>(p.TagsJson) ?? new(),
                startDate = p.StartDate,
                deadline = p.Deadline,
                createdById = p.CreatedById,
                createdBy = p.CreatedBy == null ? null : new
                {
                    id = p.CreatedBy.Id,
                    fullName = p.CreatedBy.FullName,
                    email = p.CreatedBy.Email
                },
                createdAt = p.CreatedAt,
                updatedAt = p.UpdatedAt,
                memberCount = p.Members?.Count ?? 0,
                members = p.Members?.Select(m => new
                {
                    id = m.Id,
                    userId = m.UserId,
                    user = new { id = m.User.Id, fullName = m.User.FullName, email = m.User.Email, avatar = m.User.Avatar },
                    role = m.Role.ToString(),
                    joinedAt = m.JoinedAt
                }) ?? Enumerable.Empty<object>(),
                taskCount = p.Tasks?.Count ?? 0,
                completedTaskCount = p.Tasks?.Count(t => t.Status == ErpTaskStatus.DONE) ?? 0
            };
            return dto;
        }

        public async Task<(IEnumerable<object> items, int total)>
            GetProjectsAsync(int page, int pageSize, string? search, string? status, Guid? userId)
        {
            var query = _db.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.Members).ThenInclude(m => m.User)
                .Include(p => p.Tasks)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProjectStatus>(status, true, out var ps))
                query = query.Where(p => p.Status == ps);

            if (userId.HasValue)
                query = query.Where(p => p.Members.Any(m => m.UserId == userId));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items.Select(p => MapProject(p)), total);
        }

        public async Task<object?> GetProjectByIdAsync(Guid id)
        {
            var p = await _db.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.Members).ThenInclude(m => m.User)
                .Include(p => p.Tasks)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == id);
            return p == null ? null : MapProject(p, true);
        }

        public async Task<object> CreateProjectAsync(CreateProjectViewModel model, Guid createdById)
        {
            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                Category = model.Category,
                TagsJson = JsonSerializer.Serialize(model.Tags),
                StartDate = model.StartDate,
                Deadline = model.Deadline,
                CreatedById = createdById
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Add creator as OWNER
            _db.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = createdById,
                Role = ProjectMemberRole.OWNER
            });

            // Add additional members
            foreach (var memberId in model.MemberIds.Where(m => m != createdById))
            {
                _db.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = memberId,
                    Role = ProjectMemberRole.MEMBER
                });
            }
            await _db.SaveChangesAsync();

            await _activity.LogAsync(createdById, "CREATE", "Project", project.Id,
                project.Name, project.Id, $"Created project '{project.Name}'");

            return (await GetProjectByIdAsync(project.Id))!;
        }

        public async Task<object?> UpdateProjectAsync(Guid id, UpdateProjectViewModel model)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return null;

            if (model.Name != null) project.Name = model.Name;
            if (model.Description != null) project.Description = model.Description;
            if (model.Status.HasValue) project.Status = model.Status.Value;
            if (model.Priority.HasValue) project.Priority = model.Priority.Value;
            if (model.Category != null) project.Category = model.Category;
            if (model.Tags != null) project.TagsJson = JsonSerializer.Serialize(model.Tags);
            if (model.StartDate.HasValue) project.StartDate = model.StartDate;
            if (model.Deadline.HasValue) project.Deadline = model.Deadline;
            project.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetProjectByIdAsync(id);
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return false;
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<object?> AddMemberAsync(Guid projectId, AddProjectMemberViewModel model)
        {
            var exists = await _db.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == model.UserId);
            if (exists) return null;

            var pm = new ProjectMember
            {
                ProjectId = projectId,
                UserId = model.UserId,
                Role = model.Role
            };
            _db.ProjectMembers.Add(pm);
            await _db.SaveChangesAsync();
            await _db.Entry(pm).Reference(m => m.User).LoadAsync();

            return new
            {
                id = pm.Id,
                userId = pm.UserId,
                user = new { id = pm.User.Id, fullName = pm.User.FullName, email = pm.User.Email },
                role = pm.Role.ToString(),
                joinedAt = pm.JoinedAt
            };
        }

        public async Task<bool> RemoveMemberAsync(Guid projectId, Guid userId)
        {
            var pm = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (pm == null) return false;
            _db.ProjectMembers.Remove(pm);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetMembersAsync(Guid projectId)
        {
            var members = await _db.ProjectMembers
                .Include(m => m.User)
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            return members.Select(m => new
            {
                id = m.Id,
                userId = m.UserId,
                user = new
                {
                    id = m.User.Id,
                    fullName = m.User.FullName,
                    email = m.User.Email,
                    avatar = m.User.Avatar,
                    designation = m.User.Designation
                },
                role = m.Role.ToString(),
                joinedAt = m.JoinedAt
            });
        }

        public async Task<object> GetProjectStatsAsync(Guid projectId)
        {
            var tasks = await _db.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();
            var timeEntries = await _db.TimeEntries.Where(te => te.ProjectId == projectId).ToListAsync();
            var members = await _db.ProjectMembers.CountAsync(m => m.ProjectId == projectId);

            var totalMinutes = timeEntries.Where(te => !te.IsRunning).Sum(te => te.DurationMinutes);

            return new
            {
                totalTasks = tasks.Count,
                completedTasks = tasks.Count(t => t.Status == ErpTaskStatus.DONE),
                inProgressTasks = tasks.Count(t => t.Status == ErpTaskStatus.IN_PROGRESS),
                blockedTasks = tasks.Count(t => t.Status == ErpTaskStatus.BLOCKED),
                totalMembers = members,
                totalHoursLogged = Math.Round((double)totalMinutes / 60, 2),
                totalTimeEntries = timeEntries.Count
            };
        }
    }
}
