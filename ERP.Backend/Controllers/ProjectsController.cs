using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IMilestoneService _milestoneService;

        public ProjectsController(IProjectService projectService, IMilestoneService milestoneService)
        {
            _projectService = projectService;
            _milestoneService = milestoneService;
        }

        /// <summary>GET /api/projects</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? userId = null)
        {
            var (items, total) = await _projectService.GetProjectsAsync(page, pageSize, search, status, userId);
            return Json(new
            {
                count = total,
                next = (page * pageSize < total) ? $"?page={page + 1}&pageSize={pageSize}" : null,
                previous = (page > 1) ? $"?page={page - 1}&pageSize={pageSize}" : null,
                results = items
            });
        }

        /// <summary>GET /api/projects/{id}</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _projectService.GetProjectByIdAsync(id);
            if (item == null) { Response.StatusCode = 404; return Json(new { detail = "Project not found." }); }
            return Json(item);
        }

        /// <summary>POST /api/projects</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _projectService.CreateProjectAsync(model, userId);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>PUT /api/projects/{id}</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectViewModel model)
        {
            var result = await _projectService.UpdateProjectAsync(id, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Project not found." }); }
            return Json(result);
        }

        /// <summary>DELETE /api/projects/{id}</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ClaimsHelper.IsAdminOrManager(User))
            {
                Response.StatusCode = 403;
                return Json(new { detail = "Insufficient permissions." });
            }
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Project not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }

        /// <summary>GET /api/projects/{id}/stats</summary>
        [HttpGet("{id:guid}/stats")]
        public async Task<IActionResult> GetStats(Guid id)
        {
            return Json(await _projectService.GetProjectStatsAsync(id));
        }

        // ─── Members ──────────────────────────────────────────────────────────

        /// <summary>GET /api/projects/{id}/members</summary>
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            return Json(await _projectService.GetMembersAsync(id));
        }

        /// <summary>POST /api/projects/{id}/members</summary>
        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberViewModel model)
        {
            var result = await _projectService.AddMemberAsync(id, model);
            if (result == null)
            {
                Response.StatusCode = 409;
                return Json(new { detail = "User is already a member of this project." });
            }
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>DELETE /api/projects/{id}/members/{userId}</summary>
        [HttpDelete("{id:guid}/members/{memberId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
        {
            var success = await _projectService.RemoveMemberAsync(id, memberId);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Member not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }

        // ─── Milestones ───────────────────────────────────────────────────────

        /// <summary>GET /api/projects/{id}/milestones</summary>
        [HttpGet("{id:guid}/milestones")]
        public async Task<IActionResult> GetMilestones(Guid id)
        {
            return Json(await _milestoneService.GetMilestonesAsync(id));
        }

        /// <summary>POST /api/projects/{id}/milestones</summary>
        [HttpPost("{id:guid}/milestones")]
        public async Task<IActionResult> CreateMilestone(Guid id, [FromBody] CreateMilestoneViewModel model)
        {
            var result = await _milestoneService.CreateMilestoneAsync(id, model);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>PUT /api/projects/{id}/milestones/{milestoneId}</summary>
        [HttpPut("{id:guid}/milestones/{milestoneId:guid}")]
        public async Task<IActionResult> UpdateMilestone(Guid id, Guid milestoneId,
            [FromBody] UpdateMilestoneViewModel model)
        {
            var result = await _milestoneService.UpdateMilestoneAsync(milestoneId, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Milestone not found." }); }
            return Json(result);
        }

        /// <summary>DELETE /api/projects/{id}/milestones/{milestoneId}</summary>
        [HttpDelete("{id:guid}/milestones/{milestoneId:guid}")]
        public async Task<IActionResult> DeleteMilestone(Guid id, Guid milestoneId)
        {
            var success = await _milestoneService.DeleteMilestoneAsync(milestoneId);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Milestone not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }
    }
}
