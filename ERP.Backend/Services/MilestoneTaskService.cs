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
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _db;
        public MilestoneService(ApplicationDbContext db) { _db = db; }

        private static object MapMilestone(Milestone m) => new
        {
            id = m.Id,
            projectId = m.ProjectId,
            name = m.Name,
            description = m.Description,
            dueDate = m.DueDate,
            isCompleted = m.IsCompleted,
            completedAt = m.CompletedAt,
            order = m.Order,
            createdAt = m.CreatedAt
        };

        public async Task<IEnumerable<object>> GetMilestonesAsync(Guid projectId)
        {
            var items = await _db.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.Order)
                .ThenBy(m => m.DueDate)
                .ToListAsync();
            return items.Select(m => MapMilestone(m));
        }

        public async Task<object?> GetMilestoneByIdAsync(Guid id)
        {
            var m = await _db.Milestones.FindAsync(id);
            return m == null ? null : MapMilestone(m);
        }

        public async Task<object> CreateMilestoneAsync(Guid projectId, CreateMilestoneViewModel model)
        {
            var ms = new Milestone
            {
                ProjectId = projectId,
                Name = model.Name,
                Description = model.Description,
                DueDate = model.DueDate,
                Order = model.Order
            };
            _db.Milestones.Add(ms);
            await _db.SaveChangesAsync();
            return MapMilestone(ms);
        }

        public async Task<object?> UpdateMilestoneAsync(Guid id, UpdateMilestoneViewModel model)
        {
            var ms = await _db.Milestones.FindAsync(id);
            if (ms == null) return null;

            if (model.Name != null) ms.Name = model.Name;
            if (model.Description != null) ms.Description = model.Description;
            if (model.DueDate.HasValue) ms.DueDate = model.DueDate;
            if (model.Order.HasValue) ms.Order = model.Order.Value;
            if (model.IsCompleted.HasValue)
            {
                ms.IsCompleted = model.IsCompleted.Value;
                if (model.IsCompleted.Value && !ms.CompletedAt.HasValue)
                    ms.CompletedAt = DateTime.UtcNow;
                else if (!model.IsCompleted.Value)
                    ms.CompletedAt = null;
            }

            await _db.SaveChangesAsync();
            return MapMilestone(ms);
        }

        public async Task<bool> DeleteMilestoneAsync(Guid id)
        {
            var ms = await _db.Milestones.FindAsync(id);
            if (ms == null) return false;
            _db.Milestones.Remove(ms);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IActivityService _activity;

        public TaskService(ApplicationDbContext db, IWebHostEnvironment env, IActivityService activity)
        {
            _db = db;
            _env = env;
            _activity = activity;
        }

        private static object MapTask(ProjectTask t) => new
        {
            id = t.Id,
            title = t.Title,
            description = t.Description,
            projectId = t.ProjectId,
            project = t.Project == null ? null : new { id = t.Project.Id, name = t.Project.Name },
            assigneeId = t.AssigneeId,
            assignee = t.Assignee == null ? null : new
            {
                id = t.Assignee.Id,
                fullName = t.Assignee.FullName,
                email = t.Assignee.Email,
                avatar = t.Assignee.Avatar
            },
            reporterId = t.ReporterId,
            reporter = t.Reporter == null ? null : new
            {
                id = t.Reporter.Id,
                fullName = t.Reporter.FullName,
                email = t.Reporter.Email
            },
            status = t.Status.ToString(),
            priority = t.Priority.ToString(),
            taskType = t.TaskType.ToString(),
            labels = JsonSerializer.Deserialize<List<string>>(t.LabelsJson) ?? new(),
            estimatedHours = t.EstimatedHours,
            actualHours = t.ActualHours,
            dueDate = t.DueDate,
            completedAt = t.CompletedAt,
            assignedAt = t.AssignedAt,
            order = t.Order,
            createdAt = t.CreatedAt,
            updatedAt = t.UpdatedAt,
            commentCount = t.Comments?.Count ?? 0,
            attachmentCount = t.Attachments?.Count ?? 0
        };

        public async Task<(IEnumerable<object> items, int total)> GetTasksAsync(TaskFilterParams filters)
        {
            var query = _db.Tasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .AsQueryable();

            if (filters.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == filters.ProjectId);
            if (filters.AssigneeId.HasValue)
                query = query.Where(t => t.AssigneeId == filters.AssigneeId);
            if (filters.Status.HasValue)
                query = query.Where(t => t.Status == filters.Status);
            if (filters.Priority.HasValue)
                query = query.Where(t => t.Priority == filters.Priority);
            if (filters.TaskType.HasValue)
                query = query.Where(t => t.TaskType == filters.TaskType);
            if (!string.IsNullOrEmpty(filters.Search))
                query = query.Where(t => t.Title.Contains(filters.Search) || t.Description.Contains(filters.Search));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Order)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (items.Select(t => MapTask(t)), total);
        }

        public async Task<object?> GetTaskByIdAsync(Guid id)
        {
            var t = await _db.Tasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Include(t => t.Comments).ThenInclude(c => c.Author)
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
            return t == null ? null : MapTask(t);
        }

        public async Task<object> CreateTaskAsync(CreateTaskViewModel model, Guid reporterId)
        {
            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                ProjectId = model.ProjectId,
                AssigneeId = model.AssigneeId,
                ReporterId = reporterId,
                Status = model.Status,
                Priority = model.Priority,
                TaskType = model.TaskType,
                LabelsJson = JsonSerializer.Serialize(model.Labels),
                EstimatedHours = model.EstimatedHours,
                DueDate = model.DueDate,
                Order = model.Order
            };

            if (model.AssigneeId.HasValue)
                task.AssignedAt = DateTime.UtcNow;

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            await _activity.LogAsync(reporterId, "CREATE", "Task", task.Id,
                task.Title, task.ProjectId, $"Created task '{task.Title}'");

            return (await GetTaskByIdAsync(task.Id))!;
        }

        public async Task<object?> UpdateTaskAsync(Guid id, UpdateTaskViewModel model)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return null;

            if (model.Title != null) task.Title = model.Title;
            if (model.Description != null) task.Description = model.Description;
            if (model.Status.HasValue)
            {
                task.Status = model.Status.Value;
                if (model.Status == ErpTaskStatus.DONE && !task.CompletedAt.HasValue)
                    task.CompletedAt = DateTime.UtcNow;
                else if (model.Status != ErpTaskStatus.DONE)
                    task.CompletedAt = null;
            }
            if (model.Priority.HasValue) task.Priority = model.Priority.Value;
            if (model.TaskType.HasValue) task.TaskType = model.TaskType.Value;
            if (model.Labels != null) task.LabelsJson = JsonSerializer.Serialize(model.Labels);
            if (model.EstimatedHours.HasValue) task.EstimatedHours = model.EstimatedHours.Value;
            if (model.DueDate.HasValue) task.DueDate = model.DueDate;
            if (model.Order.HasValue) task.Order = model.Order.Value;
            if (model.AssigneeId.HasValue)
            {
                task.AssigneeId = model.AssigneeId;
                if (task.AssigneeId.HasValue) task.AssignedAt = DateTime.UtcNow;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await GetTaskByIdAsync(id);
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return false;
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetCommentsAsync(Guid taskId)
        {
            var comments = await _db.Comments
                .Include(c => c.Author)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new
            {
                id = c.Id,
                taskId = c.TaskId,
                authorId = c.AuthorId,
                author = new { id = c.Author.Id, fullName = c.Author.FullName, avatar = c.Author.Avatar },
                content = c.Content,
                createdAt = c.CreatedAt,
                updatedAt = c.UpdatedAt
            });
        }

        public async Task<object> AddCommentAsync(Guid taskId, Guid authorId, CreateCommentViewModel model)
        {
            var comment = new Comment
            {
                TaskId = taskId,
                AuthorId = authorId,
                Content = model.Content
            };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            await _db.Entry(comment).Reference(c => c.Author).LoadAsync();

            return new
            {
                id = comment.Id,
                taskId = comment.TaskId,
                authorId = comment.AuthorId,
                author = new { id = comment.Author.Id, fullName = comment.Author.FullName, avatar = comment.Author.Avatar },
                content = comment.Content,
                createdAt = comment.CreatedAt,
                updatedAt = comment.UpdatedAt
            };
        }

        public async Task<object?> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentViewModel model)
        {
            var comment = await _db.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == userId);
            if (comment == null) return null;

            comment.Content = model.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new
            {
                id = comment.Id,
                taskId = comment.TaskId,
                authorId = comment.AuthorId,
                author = new { id = comment.Author.Id, fullName = comment.Author.FullName },
                content = comment.Content,
                createdAt = comment.CreatedAt,
                updatedAt = comment.UpdatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _db.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == userId);
            if (comment == null) return false;
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<object> UploadAttachmentAsync(Guid taskId, Guid userId, IFormFile file)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "attachments");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var filename = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, filename);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var attachment = new Attachment
            {
                TaskId = taskId,
                UploadedById = userId,
                File = $"/uploads/attachments/{filename}",
                FileName = file.FileName,
                FileSize = (int)file.Length,
                ContentType = file.ContentType
            };
            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync();
            await _db.Entry(attachment).Reference(a => a.UploadedBy).LoadAsync();

            return new
            {
                id = attachment.Id,
                taskId = attachment.TaskId,
                file = attachment.File,
                fileName = attachment.FileName,
                fileSize = attachment.FileSize,
                contentType = attachment.ContentType,
                uploadedById = attachment.UploadedById,
                uploadedBy = new { id = attachment.UploadedBy.Id, fullName = attachment.UploadedBy.FullName },
                uploadedAt = attachment.UploadedAt
            };
        }

        public async Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid userId)
        {
            var attachment = await _db.Attachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.UploadedById == userId);
            if (attachment == null) return false;

            var filePath = Path.Combine(_env.WebRootPath, attachment.File.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
