using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public UserService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<(IEnumerable<object> items, int total)>
            GetUsersAsync(int page, int pageSize, string? search, string? role)
        {
            var query = _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .Where(u => u.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role.ToString() == role);

            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users.Select(u => AuthService.MapUserToDto(u)), total);
        }

        public async Task<object?> GetUserByIdAsync(Guid id)
        {
            var user = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == id);
            return user == null ? null : AuthService.MapUserToDto(user);
        }

        public async Task<object?> UpdateUserAsync(Guid id, UpdateProfileViewModel model)
        {
            var user = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            if (model.FirstName != null) user.FirstName = model.FirstName;
            if (model.LastName != null) user.LastName = model.LastName;
            if (model.Phone != null) user.Phone = model.Phone;
            if (model.Bio != null) user.Bio = model.Bio;
            if (model.Designation != null) user.Designation = model.Designation;
            if (model.EmployeeId != null) user.EmployeeId = model.EmployeeId;
            if (model.DateOfJoining.HasValue) user.DateOfJoining = model.DateOfJoining;
            if (model.DepartmentId.HasValue) user.DepartmentId = model.DepartmentId;
            if (model.TeamId.HasValue) user.TeamId = model.TeamId;
            if (model.Role.HasValue) user.Role = model.Role.Value;
            if (model.Skills != null) user.SkillsJson = JsonSerializer.Serialize(model.Skills);

            await _db.SaveChangesAsync();

            // Reload navigation properties
            await _db.Entry(user).Reference(u => u.Department).LoadAsync();
            await _db.Entry(user).Reference(u => u.Team).LoadAsync();

            return AuthService.MapUserToDto(user);
        }

        public async Task<object?> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            // Delete old avatar
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var oldPath = Path.Combine(_env.WebRootPath, user.Avatar.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }

            var ext = Path.GetExtension(file.FileName);
            var filename = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, filename);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            user.Avatar = $"/uploads/avatars/{filename}";
            await _db.SaveChangesAsync();

            return new { avatar = user.Avatar };
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return false;
            user.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
