using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Services.Interfaces;

namespace ERP.Backend.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _db;
        public ActivityService(ApplicationDbContext db) { _db = db; }

        public async Task LogAsync(Guid userId, string action, string entityType,
            Guid entityId, string entityTitle, Guid? projectId, string description, object? metadata = null)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityTitle = entityTitle,
                ProjectId = projectId,
                Description = description,
                MetadataJson = metadata != null
                    ? JsonSerializer.Serialize(metadata)
                    : "{}",
                Timestamp = DateTime.UtcNow
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<(IEnumerable<object> items, int total)> GetActivitiesAsync(
            int page, int pageSize, Guid? userId, Guid? projectId, string? entityType)
        {
            var query = _db.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .AsQueryable();

            if (userId.HasValue) query = query.Where(a => a.UserId == userId);
            if (projectId.HasValue) query = query.Where(a => a.ProjectId == projectId);
            if (!string.IsNullOrEmpty(entityType)) query = query.Where(a => a.EntityType == entityType);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items.Select(a => new
            {
                id = a.Id,
                userId = a.UserId,
                user = new { id = a.User.Id, fullName = a.User.FullName, avatar = a.User.Avatar },
                action = a.Action,
                entityType = a.EntityType,
                entityId = a.EntityId,
                entityTitle = a.EntityTitle,
                projectId = a.ProjectId,
                project = a.Project == null ? null : new { id = a.Project.Id, name = a.Project.Name },
                description = a.Description,
                metadata = JsonSerializer.Deserialize<object>(a.MetadataJson),
                timestamp = a.Timestamp
            }), total);
        }
    }
}
