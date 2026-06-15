using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        public TasksController(ITaskService taskService) { _taskService = taskService; }

        /// <summary>GET /api/tasks</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskFilterParams filters)
        {
            var (items, total) = await _taskService.GetTasksAsync(filters);
            return Json(new
            {
                count = total,
                next = (filters.Page * filters.PageSize < total) ? $"?page={filters.Page + 1}" : null,
                previous = (filters.Page > 1) ? $"?page={filters.Page - 1}" : null,
                results = items
            });
        }

        /// <summary>GET /api/tasks/{id}</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _taskService.GetTaskByIdAsync(id);
            if (item == null) { Response.StatusCode = 404; return Json(new { detail = "Task not found." }); }
            return Json(item);
        }

        /// <summary>POST /api/tasks</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _taskService.CreateTaskAsync(model, userId);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>PUT /api/tasks/{id}</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskViewModel model)
        {
            var result = await _taskService.UpdateTaskAsync(id, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Task not found." }); }
            return Json(result);
        }

        /// <summary>DELETE /api/tasks/{id}</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Task not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }

        // ─── Comments ─────────────────────────────────────────────────────────

        /// <summary>GET /api/tasks/{id}/comments</summary>
        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            return Json(await _taskService.GetCommentsAsync(id));
        }

        /// <summary>POST /api/tasks/{id}/comments</summary>
        [HttpPost("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _taskService.AddCommentAsync(id, userId, model);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>PUT /api/tasks/{taskId}/comments/{commentId}</summary>
        [HttpPut("{taskId:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> UpdateComment(Guid taskId, Guid commentId,
            [FromBody] UpdateCommentViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _taskService.UpdateCommentAsync(commentId, userId, model);
            if (result == null) { Response.StatusCode = 404; return Json(new { detail = "Comment not found." }); }
            return Json(result);
        }

        /// <summary>DELETE /api/tasks/{taskId}/comments/{commentId}</summary>
        [HttpDelete("{taskId:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid taskId, Guid commentId)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var success = await _taskService.DeleteCommentAsync(commentId, userId);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Comment not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }

        // ─── Attachments ───────────────────────────────────────────────────────

        /// <summary>POST /api/tasks/{id}/attachments</summary>
        [HttpPost("{id:guid}/attachments")]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Response.StatusCode = 400;
                return Json(new { detail = "No file provided." });
            }
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _taskService.UploadAttachmentAsync(id, userId, file);
            Response.StatusCode = 201;
            return Json(result);
        }

        /// <summary>DELETE /api/tasks/{taskId}/attachments/{attachmentId}</summary>
        [HttpDelete("{taskId:guid}/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DeleteAttachment(Guid taskId, Guid attachmentId)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var success = await _taskService.DeleteAttachmentAsync(attachmentId, userId);
            if (!success) { Response.StatusCode = 404; return Json(new { detail = "Attachment not found." }); }
            Response.StatusCode = 204;
            return Json(new { });
        }
    }
}
