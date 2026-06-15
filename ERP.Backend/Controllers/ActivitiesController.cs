using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ActivitiesController : Controller
    {
        private readonly IActivityService _activityService;
        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>GET /api/activities</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? projectId = null,
            [FromQuery] string? entityType = null)
        {
            var (items, total) = await _activityService.GetActivitiesAsync(page, pageSize, userId, projectId, entityType);
            return Json(new
            {
                count = total,
                next = (page * pageSize < total) ? $"?page={page + 1}&pageSize={pageSize}" : null,
                previous = (page > 1) ? $"?page={page - 1}&pageSize={pageSize}" : null,
                results = items
            });
        }

        /// <summary>GET /api/activities/my</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyActivities(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var (items, total) = await _activityService.GetActivitiesAsync(page, pageSize, userId, null, null);
            return Json(new { count = total, results = items });
        }
    }
}
