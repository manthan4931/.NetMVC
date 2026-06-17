using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EmployeesController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Renders the Employee Detail Razor View.
        /// Route: /Employees/Detail/{id?}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detail(Guid? id)
        {
            // 1. Resolve current logged-in user context
            User? currentDbUser = null;
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (isAuthenticated)
            {
                var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && Guid.TryParse(claim.Value, out var parsedId))
                {
                    currentDbUser = await _db.Users
                        .Include(u => u.Department)
                        .Include(u => u.Team)
                        .FirstOrDefaultAsync(u => u.Id == parsedId && u.IsActive);
                }
            }

            // Fallback for easy local development / testing without full authorization cookies
            if (currentDbUser == null)
            {
                currentDbUser = await _db.Users
                    .Include(u => u.Department)
                    .Include(u => u.Team)
                    .FirstOrDefaultAsync(u => u.Role == Models.Enums.UserRole.ADMIN && u.IsActive)
                    ?? await _db.Users
                    .Include(u => u.Department)
                    .Include(u => u.Team)
                    .FirstOrDefaultAsync(u => u.IsActive);
            }

            // If no ID was provided, view own profile
            var targetId = id ?? currentDbUser?.Id;

            if (targetId == null)
            {
                return View(new EmployeeDetailViewModel { User = null });
            }

            // 2. Fetch target user details
            var targetUser = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == targetId);

            if (targetUser == null)
            {
                return View(new EmployeeDetailViewModel { User = null });
            }

            // 3. Fetch related records
            var tasks = await _db.Tasks
                .Include(t => t.Project)
                .Where(t => t.AssigneeId == targetId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TaskModel
                {
                    Id = t.Id.ToString(),
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    ProjectName = t.Project.Name,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    EstimatedHours = (double?)t.EstimatedHours
                })
                .ToListAsync();

            var activityLog = await _db.ActivityLogs
                .Include(a => a.Project)
                .Where(a => a.UserId == targetId)
                .OrderByDescending(a => a.Timestamp)
                .Take(50) // Limit feed count
                .Select(a => new ActivityLogModel
                {
                    Id = a.Id.ToString(),
                    Action = a.Action,
                    Timestamp = a.Timestamp,
                    Description = a.Description,
                    ProjectName = a.Project != null ? a.Project.Name : string.Empty
                })
                .ToListAsync();

            var timeEntries = await _db.TimeEntries
                .Include(t => t.Project)
                .Include(t => t.Task)
                .Where(t => t.UserId == targetId)
                .OrderByDescending(t => t.StartTime)
                .Select(t => new TimeEntryModel
                {
                    Id = t.Id.ToString(),
                    Description = t.Description,
                    TaskTitle = t.Task != null ? t.Task.Title : string.Empty,
                    DurationMinutes = t.DurationMinutes,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    IsBillable = t.IsBillable,
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            // 4. Populate ViewModel
            var viewModel = new EmployeeDetailViewModel
            {
                User = new UserModel
                {
                    Id = targetUser.Id.ToString(),
                    FirstName = targetUser.FirstName,
                    LastName = targetUser.LastName,
                    Email = targetUser.Email,
                    Role = targetUser.Role.ToString(),
                    Designation = targetUser.Designation,
                    DepartmentName = targetUser.Department?.Name ?? "Unassigned",
                    TeamName = targetUser.Team?.Name ?? "No Team",
                    DateJoined = targetUser.DateOfJoining ?? targetUser.CreatedAt,
                    IsActive = targetUser.IsActive
                },
                Tasks = tasks,
                ActivityLog = activityLog,
                TimeEntries = timeEntries,
                CurrentUser = currentDbUser != null ? new UserModel
                {
                    Id = currentDbUser.Id.ToString(),
                    FirstName = currentDbUser.FirstName,
                    LastName = currentDbUser.LastName,
                    Email = currentDbUser.Email,
                    Role = currentDbUser.Role.ToString(),
                    Designation = currentDbUser.Designation,
                    DepartmentName = currentDbUser.Department?.Name ?? "Unassigned",
                    TeamName = currentDbUser.Team?.Name ?? "No Team",
                    DateJoined = currentDbUser.DateOfJoining ?? currentDbUser.CreatedAt,
                    IsActive = currentDbUser.IsActive
                } : null
            };

            return View(viewModel);
        }
    }
}
