using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/timeentries")]
    [Authorize]
    public class TimeEntriesController : Controller
    {
        private readonly ITimeTrackingService _service;

        public TimeEntriesController(ITimeTrackingService service)
        {
            _service = service;
        }

        /// <summary>GET /api/timeentries</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TimeEntryFilterParams filters)
        {
            var (items, total) = await _service.GetTimeEntriesAsync(filters);
            return Json(new
            {
                count = total,
                next = (filters.Page * filters.PageSize < total) ? $"?page={filters.Page + 1}" : null,
                previous = (filters.Page > 1) ? $"?page={filters.Page - 1}" : null,
                results = items
            });
        }

        /// <summary>GET /api/timeentries/{id}</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetTimeEntryByIdAsync(id);
            if (item == null) { Response.StatusCode = 404; return Json(new { detail = "Time entry not found." }); }
            return Json(item);
        }

        /// <summary>POST /api/timeentries</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTimeEntryViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _service.CreateTimeEntryAsync(model, userId);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>PUT /api/timeentries/{id}</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _service.UpdateTimeEntryAsync(id, model, userId);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Time entry not found." }); }
            return Json(result);
        }

        /// <summary>DELETE /api/timeentries/{id}</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var success = await _service.DeleteTimeEntryAsync(id, userId);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Time entry not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }

        // ─── Timer Actions ─────────────────────────────────────────────────────

        /// <summary>POST /api/timeentries/start</summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartTimerViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _service.StartTimerAsync(model, userId);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>POST /api/timeentries/stop</summary>
        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var (success, entry, error) = await _service.StopTimerAsync(userId);
            if (!success)
            {
                Response.StatusCode = 404;
                return Json(new { detail = error });
            }
            return Json(entry);
        }

        /// <summary>GET /api/timeentries/running</summary>
        [HttpGet("running")]
        public async Task<IActionResult> GetRunning()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var entry = await _service.GetRunningTimerAsync(userId);
            if (entry == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "No running timer found." });
            }
            return Json(entry);
        }

        /// <summary>GET /api/timeentries/export_csv</summary>
        [HttpGet("export_csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] TimeEntryFilterParams filters)
        {
            var csvData = await _service.GenerateCsvBytesAsync(filters);
            return File(csvData, "text/csv", $"time_entries_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }

    // ─── Today Dashboard Controller ─────────────────────────────────────────────
    [Route("api/today")]
    [Authorize]
    public class TodayController : Controller
    {
        private readonly ITimeTrackingService _service;
        public TodayController(ITimeTrackingService service) { _service = service; }

        /// <summary>GET /api/today</summary>
        [HttpGet]
        public async Task<IActionResult> GetTodayStats()
        {
            return Json(await _service.GetTodayStatsAsync());
        }
    }
}
