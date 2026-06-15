using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _service;
        public DepartmentsController(IDepartmentService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            var (items, total) = await _service.GetDepartmentsAsync(page, pageSize, search);
            return Json(new { count = total, results = items });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetDepartmentByIdAsync(id);
            if (item == null) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            return Json(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentViewModel model)
        {
            var result = await _service.CreateDepartmentAsync(model);
            Response.StatusCode = 201;
            return Json(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentViewModel model)
        {
            var result = await _service.UpdateDepartmentAsync(id, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            return Json(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteDepartmentAsync(id);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }
    }

    [Route("api/[controller]")]
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ITeamService _service;
        public TeamsController(ITeamService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            var (items, total) = await _service.GetTeamsAsync(page, pageSize, search);
            return Json(new { count = total, results = items });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetTeamByIdAsync(id);
            if (item == null) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            return Json(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamViewModel model)
        {
            var result = await _service.CreateTeamAsync(model);
            Response.StatusCode = 201;
            return Json(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamViewModel model)
        {
            var result = await _service.UpdateTeamAsync(id, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            return Json(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteTeamAsync(id);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }
    }
}
