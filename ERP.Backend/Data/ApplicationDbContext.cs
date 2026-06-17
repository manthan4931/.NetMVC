using Microsoft.EntityFrameworkCore;
using ERP.Backend.Models;
using ERP.Backend.Models.Enums;

namespace ERP.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Milestone> Milestones => Set<Milestone>();
        public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            // User - optional unique EmployeeId
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeId).IsUnique().HasFilter("[EmployeeId] IS NOT NULL");

            // Department - unique name
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name).IsUnique();

            // Department self-reference
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Parent)
                .WithMany(d => d.SubDepartments)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department -> Head (User)
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Head)
                .WithMany()
                .HasForeignKey(d => d.HeadId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Department
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Members)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Team
            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Team -> Lead (User)
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Lead)
                .WithMany()
                .HasForeignKey(t => t.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember - unique project+user combo
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();

            // Project -> CreatedBy
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Task -> Assignee
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task -> Reporter
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Reporter)
                .WithMany()
                .HasForeignKey(t => t.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // TimeEntry -> Task (optional)
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // TimeEntry -> User
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ActivityLog -> Project (optional)
            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.Project)
                .WithMany()
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store enums as strings
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Project>()
                .Property(p => p.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectMember>()
                .Property(pm => pm.Role)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.TaskType)
                .HasConversion<string>();

            modelBuilder.Entity<TimeEntry>()
                .Property(te => te.Status)
                .HasConversion<string>();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update timestamps
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            // Capture Task IDs associated with added, modified, or deleted TimeEntries
            var affectedTaskIds = ChangeTracker.Entries<TimeEntry>()
                .Where(e => e.State == EntityState.Added
                         || e.State == EntityState.Modified
                         || e.State == EntityState.Deleted)
                .Select(e => e.Entity.TaskId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Recalculate actual_hours for affected tasks
            if (affectedTaskIds.Any())
            {
                foreach (var taskId in affectedTaskIds)
                {
                    var task = await Tasks.FindAsync(new object[] { taskId }, cancellationToken);
                    if (task != null)
                    {
                        var totalMinutes = await TimeEntries
                            .Where(te => te.TaskId == taskId && !te.IsRunning)
                            .SumAsync(te => te.DurationMinutes, cancellationToken);

                        task.ActualHours = Math.Round((decimal)totalMinutes / 60m, 2);
                    }
                }
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}
