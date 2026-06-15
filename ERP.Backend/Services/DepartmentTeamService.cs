using Microsoft.EntityFrameworkCore;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _db;
        public DepartmentService(ApplicationDbContext db) { _db = db; }

        private static object MapDepartment(Department d) => new
        {
            id = d.Id,
            name = d.Name,
            description = d.Description,
            headId = d.HeadId,
            head = d.Head == null ? null : new { id = d.Head.Id, fullName = d.Head.FullName, email = d.Head.Email },
            parentId = d.ParentId,
            color = d.Color,
            createdAt = d.CreatedAt,
            memberCount = d.Members?.Count ?? 0,
            teamCount = d.Teams?.Count ?? 0
        };

        public async Task<(IEnumerable<object> items, int total)>
            GetDepartmentsAsync(int page, int pageSize, string? search)
        {
            var query = _db.Departments
                .Include(d => d.Head)
                .Include(d => d.Members)
                .Include(d => d.Teams)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items.Select(d => MapDepartment(d)), total);
        }

        public async Task<object?> GetDepartmentByIdAsync(Guid id)
        {
            var d = await _db.Departments
                .Include(d => d.Head)
                .Include(d => d.Members)
                .Include(d => d.Teams)
                .Include(d => d.SubDepartments)
                .FirstOrDefaultAsync(d => d.Id == id);
            return d == null ? null : MapDepartment(d);
        }

        public async Task<object> CreateDepartmentAsync(CreateDepartmentViewModel model)
        {
            var dept = new Department
            {
                Name = model.Name,
                Description = model.Description,
                HeadId = model.HeadId,
                ParentId = model.ParentId,
                Color = model.Color
            };
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();

            if (model.HeadId.HasValue)
                await _db.Entry(dept).Reference(d => d.Head).LoadAsync();

            return MapDepartment(dept);
        }

        public async Task<object?> UpdateDepartmentAsync(Guid id, UpdateDepartmentViewModel model)
        {
            var dept = await _db.Departments
                .Include(d => d.Head)
                .Include(d => d.Members)
                .Include(d => d.Teams)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dept == null) return null;

            if (model.Name != null) dept.Name = model.Name;
            if (model.Description != null) dept.Description = model.Description;
            if (model.HeadId.HasValue) dept.HeadId = model.HeadId;
            if (model.ParentId.HasValue) dept.ParentId = model.ParentId;
            if (model.Color != null) dept.Color = model.Color;

            await _db.SaveChangesAsync();
            await _db.Entry(dept).Reference(d => d.Head).LoadAsync();

            return MapDepartment(dept);
        }

        public async Task<bool> DeleteDepartmentAsync(Guid id)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept == null) return false;
            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _db;
        public TeamService(ApplicationDbContext db) { _db = db; }

        private static object MapTeam(Team t) => new
        {
            id = t.Id,
            name = t.Name,
            departmentId = t.DepartmentId,
            department = t.Department == null ? null : new { id = t.Department.Id, name = t.Department.Name },
            leadId = t.LeadId,
            lead = t.Lead == null ? null : new { id = t.Lead.Id, fullName = t.Lead.FullName, email = t.Lead.Email },
            color = t.Color,
            createdAt = t.CreatedAt,
            memberCount = t.Members?.Count ?? 0
        };

        public async Task<(IEnumerable<object> items, int total)>
            GetTeamsAsync(int page, int pageSize, string? search)
        {
            var query = _db.Teams
                .Include(t => t.Department)
                .Include(t => t.Lead)
                .Include(t => t.Members)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items.Select(t => MapTeam(t)), total);
        }

        public async Task<object?> GetTeamByIdAsync(Guid id)
        {
            var t = await _db.Teams
                .Include(t => t.Department)
                .Include(t => t.Lead)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
            return t == null ? null : MapTeam(t);
        }

        public async Task<object> CreateTeamAsync(CreateTeamViewModel model)
        {
            var team = new Team
            {
                Name = model.Name,
                DepartmentId = model.DepartmentId,
                LeadId = model.LeadId,
                Color = model.Color
            };
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
            await _db.Entry(team).Reference(t => t.Department).LoadAsync();
            if (model.LeadId.HasValue)
                await _db.Entry(team).Reference(t => t.Lead).LoadAsync();
            return MapTeam(team);
        }

        public async Task<object?> UpdateTeamAsync(Guid id, UpdateTeamViewModel model)
        {
            var team = await _db.Teams
                .Include(t => t.Department)
                .Include(t => t.Lead)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return null;

            if (model.Name != null) team.Name = model.Name;
            if (model.DepartmentId.HasValue) team.DepartmentId = model.DepartmentId.Value;
            if (model.LeadId.HasValue) team.LeadId = model.LeadId;
            if (model.Color != null) team.Color = model.Color;

            await _db.SaveChangesAsync();
            await _db.Entry(team).Reference(t => t.Department).LoadAsync();
            await _db.Entry(team).Reference(t => t.Lead).LoadAsync();
            return MapTeam(team);
        }

        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            var team = await _db.Teams.FindAsync(id);
            if (team == null) return false;
            _db.Teams.Remove(team);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
