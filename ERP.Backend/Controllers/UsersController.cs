using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) { _userService = userService; }

        /// <summary>GET /api/users</summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            var (items, total) = await _userService.GetUsersAsync(page, pageSize, search, role);
            return Json(new
            {
                count = total,
                next = (page * pageSize < total) ? $"?page={page + 1}&pageSize={pageSize}" : null,
                previous = (page > 1) ? $"?page={page - 1}&pageSize={pageSize}" : null,
                results = items
            });
        }

        /// <summary>GET /api/users/{id}</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            return Json(user);
        }

        /// <summary>GET /api/users/me</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            return Json(user);
        }

        /// <summary>PUT /api/users/{id}</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateProfileViewModel model)
        {
            var currentUserId = ClaimsHelper.GetUserId(User);
            if (currentUserId != id && !ClaimsHelper.IsAdmin(User))
            {
                Response.StatusCode = 403;
                return Json(new { detail = "You don't have permission to update this user." });
            }

            var result = await _userService.UpdateUserAsync(id, model);
            if (result == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            return Json(result);
        }

        /// <summary>PUT /api/users/me</summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await _userService.UpdateUserAsync(userId, model);
            if (result == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            return Json(result);
        }

        /// <summary>POST /api/users/upload-avatar</summary>
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Response.StatusCode = 400;
                return Json(new { detail = "No file provided." });
            }

            var userId = ClaimsHelper.GetUserId(User);
            var result = await _userService.UploadAvatarAsync(userId, file);
            if (result == null)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            return Json(result);
        }

        /// <summary>DELETE /api/users/{id}</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            if (!ClaimsHelper.IsAdmin(User))
            {
                Response.StatusCode = 403;
                return Json(new { detail = "Only admins can delete users." });
            }

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                Response.StatusCode = 404;
                return Json(new { detail = "User not found." });
            }
            Response.StatusCode = 204;
            return Json(new { });
        }
    }
}
