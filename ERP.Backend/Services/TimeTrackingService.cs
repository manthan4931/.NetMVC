using Microsoft.EntityFrameworkCore;
using System.Text;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Models.Enums;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;
using ErpTaskStatus = ERP.Backend.Models.Enums.TaskStatus;

namespace ERP.Backend.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivityService _activity;

        public TimeTrackingService(ApplicationDbContext db, IActivityService activity)
        {
            _db = db;
            _activity = activity;
        }

        private static object MapTimeEntry(TimeEntry te) => new
        {
            id = te.Id,
            userId = te.UserId,
            user = te.User == null ? null : new { id = te.User.Id, fullName = te.User.FullName, email = te.User.Email },
            taskId = te.TaskId,
            task = te.Task == null ? null : new { id = te.Task.Id, title = te.Task.Title },
            projectId = te.ProjectId,
            project = te.Project == null ? null : new { id = te.Project.Id, name = te.Project.Name },
            description = te.Description,
            startTime = te.StartTime,
            endTime = te.EndTime,
            durationMinutes = te.DurationMinutes,
            durationHours = Math.Round((double)te.DurationMinutes / 60, 2),
            isRunning = te.IsRunning,
            isBillable = te.IsBillable,
            status = te.Status.ToString(),
            createdAt = te.CreatedAt,
            updatedAt = te.UpdatedAt
        };

        public async Task<(IEnumerable<object> items, int total)>
            GetTimeEntriesAsync(TimeEntryFilterParams filters)
        {
            var query = _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .AsQueryable();

            if (filters.ProjectId.HasValue) query = query.Where(te => te.ProjectId == filters.ProjectId);
            if (filters.TaskId.HasValue) query = query.Where(te => te.TaskId == filters.TaskId);
            if (filters.UserId.HasValue) query = query.Where(te => te.UserId == filters.UserId);
            if (filters.DateFrom.HasValue) query = query.Where(te => te.StartTime >= filters.DateFrom);
            if (filters.DateTo.HasValue) query = query.Where(te => te.StartTime <= filters.DateTo);
            if (filters.Status.HasValue) query = query.Where(te => te.Status == filters.Status);
            if (filters.IsBillable.HasValue) query = query.Where(te => te.IsBillable == filters.IsBillable);
            if (!string.IsNullOrEmpty(filters.Search))
                query = query.Where(te => te.Description.Contains(filters.Search));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(te => te.StartTime)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (items.Select(te => MapTimeEntry(te)), total);
        }

        public async Task<object?> GetTimeEntryByIdAsync(Guid id)
        {
            var te = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.Id == id);
            return te == null ? null : MapTimeEntry(te);
        }

        public async Task<object> CreateTimeEntryAsync(CreateTimeEntryViewModel model, Guid userId)
        {
            int duration = model.DurationMinutes;
            if (duration == 0 && model.EndTime.HasValue)
                duration = (int)(model.EndTime.Value - model.StartTime).TotalMinutes;

            var te = new TimeEntry
            {
                UserId = userId,
                ProjectId = model.ProjectId,
                TaskId = model.TaskId,
                Description = model.Description,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                DurationMinutes = duration,
                IsBillable = model.IsBillable,
                Status = model.Status,
                IsRunning = !model.EndTime.HasValue
            };
            _db.TimeEntries.Add(te);
            await _db.SaveChangesAsync();

            await _db.Entry(te).Reference(t => t.User).LoadAsync();
            await _db.Entry(te).Reference(t => t.Project).LoadAsync();
            if (te.TaskId.HasValue) await _db.Entry(te).Reference(t => t.Task).LoadAsync();

            return MapTimeEntry(te);
        }

        public async Task<object?> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryViewModel model, Guid userId)
        {
            var te = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.Id == id && te.UserId == userId);
            if (te == null) return null;

            if (model.ProjectId.HasValue) te.ProjectId = model.ProjectId.Value;
            if (model.TaskId.HasValue) te.TaskId = model.TaskId;
            if (model.Description != null) te.Description = model.Description;
            if (model.StartTime.HasValue) te.StartTime = model.StartTime.Value;
            if (model.EndTime.HasValue) te.EndTime = model.EndTime;
            if (model.DurationMinutes.HasValue) te.DurationMinutes = model.DurationMinutes.Value;
            if (model.IsBillable.HasValue) te.IsBillable = model.IsBillable.Value;
            if (model.Status.HasValue) te.Status = model.Status.Value;
            te.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _db.Entry(te).Reference(t => t.Project).LoadAsync();
            if (te.TaskId.HasValue) await _db.Entry(te).Reference(t => t.Task).LoadAsync();

            return MapTimeEntry(te);
        }

        public async Task<bool> DeleteTimeEntryAsync(Guid id, Guid userId)
        {
            var te = await _db.TimeEntries.FirstOrDefaultAsync(te => te.Id == id && te.UserId == userId);
            if (te == null) return false;
            _db.TimeEntries.Remove(te);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<object> StartTimerAsync(StartTimerViewModel model, Guid userId)
        {
            // Stop any existing running timer
            var runningTimer = await _db.TimeEntries
                .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning);

            if (runningTimer != null)
            {
                runningTimer.IsRunning = false;
                runningTimer.EndTime = DateTime.UtcNow;
                runningTimer.DurationMinutes = (int)(runningTimer.EndTime.Value - runningTimer.StartTime).TotalMinutes;
                runningTimer.UpdatedAt = DateTime.UtcNow;
            }

            var newEntry = new TimeEntry
            {
                UserId = userId,
                ProjectId = model.ProjectId,
                TaskId = model.TaskId,
                Description = model.Description,
                StartTime = DateTime.UtcNow,
                IsRunning = true,
                IsBillable = model.IsBillable
            };
            _db.TimeEntries.Add(newEntry);
            await _db.SaveChangesAsync();

            await _db.Entry(newEntry).Reference(te => te.User).LoadAsync();
            await _db.Entry(newEntry).Reference(te => te.Project).LoadAsync();
            if (newEntry.TaskId.HasValue) await _db.Entry(newEntry).Reference(te => te.Task).LoadAsync();

            await _activity.LogAsync(userId, "START_TIMER", "TimeEntry", newEntry.Id,
                $"Timer for {newEntry.Project?.Name}", model.ProjectId, "Started timer");

            return MapTimeEntry(newEntry);
        }

        public async Task<(bool success, object? entry, string error)> StopTimerAsync(Guid userId)
        {
            var te = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning);

            if (te == null)
                return (false, null, "No running timer found.");

            te.IsRunning = false;
            te.EndTime = DateTime.UtcNow;
            te.DurationMinutes = (int)(te.EndTime.Value - te.StartTime).TotalMinutes;
            te.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _activity.LogAsync(userId, "STOP_TIMER", "TimeEntry", te.Id,
                $"Timer for {te.Project?.Name}", te.ProjectId, $"Stopped timer — {te.DurationMinutes} min");

            return (true, MapTimeEntry(te), "");
        }

        public async Task<object?> GetRunningTimerAsync(Guid userId)
        {
            var te = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.UserId == userId && te.IsRunning);

            return te == null ? null : MapTimeEntry(te);
        }

        public async Task<byte[]> GenerateCsvBytesAsync(TimeEntryFilterParams filters)
        {
            var (items, _) = await GetTimeEntriesAsync(new TimeEntryFilterParams
            {
                Page = 1, PageSize = 10000,
                ProjectId = filters.ProjectId,
                TaskId = filters.TaskId,
                UserId = filters.UserId,
                DateFrom = filters.DateFrom,
                DateTo = filters.DateTo,
                Status = filters.Status,
                IsBillable = filters.IsBillable
            });

            var sb = new StringBuilder();
            sb.AppendLine("ID,User,Project,Task,Description,Start Time,End Time,Duration (minutes),Billable,Status");

            foreach (dynamic item in items)
            {
                sb.AppendLine($"{item.id},{item.user?.fullName},{item.project?.name},{item.task?.title}," +
                              $"\"{item.description}\",{item.startTime},{item.endTime}," +
                              $"{item.durationMinutes},{item.isBillable},{item.status}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<object> GetTodayStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var activeTimers = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Task)
                .Include(te => te.Project)
                .Where(te => te.IsRunning)
                .ToListAsync();

            var entriesThisWeek = await _db.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Project)
                .Include(te => te.Task)
                .Where(te => te.StartTime >= today && te.StartTime < tomorrow && !te.IsRunning)
                .ToListAsync();

            var assignedTasks = await _db.Tasks
                .Where(t => t.AssignedAt >= today && t.AssignedAt < tomorrow)
                .CountAsync();

            var completedTasks = await _db.Tasks
                .Where(t => t.CompletedAt >= today && t.CompletedAt < tomorrow)
                .CountAsync();

            var totalMinutes = entriesThisWeek.Sum(te => te.DurationMinutes);

            // Developer summary
            var developers = await _db.Users
                .Where(u => u.IsActive)
                .Include(u => u.TimeEntries)
                .ToListAsync();

            var devSummary = developers.Select(dev =>
            {
                var devEntriesToday = dev.TimeEntries
                    .Where(te => te.StartTime >= today && te.StartTime < tomorrow)
                    .ToList();
                var devRunning = dev.TimeEntries.Any(te => te.IsRunning);
                var devTotalMin = devEntriesToday.Where(te => !te.IsRunning).Sum(te => te.DurationMinutes);

                return new
                {
                    userId = dev.Id,
                    fullName = dev.FullName,
                    email = dev.Email,
                    avatar = dev.Avatar,
                    status = devRunning ? "Active" : "Idle",
                    totalHoursToday = Math.Round((double)devTotalMin / 60, 2),
                    activeTaskCount = _db.Tasks.Count(t => t.AssigneeId == dev.Id
                        && t.Status != ErpTaskStatus.DONE && t.Status != ErpTaskStatus.BLOCKED)
                };
            }).ToList();

            return new
            {
                stats = new
                {
                    activeTimersCount = activeTimers.Count,
                    assignedTaskCount = assignedTasks,
                    completedTaskCount = completedTasks,
                    totalHoursLogged = Math.Round((double)totalMinutes / 60, 2)
                },
                activeTimers = activeTimers.Select(te => new
                {
                    id = te.Id,
                    userId = te.UserId,
                    user = new { id = te.User.Id, fullName = te.User.FullName, avatar = te.User.Avatar },
                    projectId = te.ProjectId,
                    project = te.Project == null ? null : new { id = te.Project.Id, name = te.Project.Name },
                    taskId = te.TaskId,
                    task = te.Task == null ? null : new { id = te.Task.Id, title = te.Task.Title },
                    startTime = te.StartTime,
                    elapsedMinutes = (int)(DateTime.UtcNow - te.StartTime).TotalMinutes
                }),
                entriesToday = entriesThisWeek.Select(te => new
                {
                    id = te.Id,
                    userId = te.UserId,
                    user = new { id = te.User.Id, fullName = te.User.FullName },
                    project = te.Project == null ? null : new { id = te.Project.Id, name = te.Project.Name },
                    task = te.Task == null ? null : new { id = te.Task.Id, title = te.Task.Title },
                    description = te.Description,
                    startTime = te.StartTime,
                    endTime = te.EndTime,
                    durationMinutes = te.DurationMinutes,
                    isBillable = te.IsBillable
                }),
                developersSummary = devSummary
            };
        }
    }
}
