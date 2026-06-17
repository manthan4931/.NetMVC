







using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Backend.Helpers;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>POST /api/auth/login</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { detail = "Invalid request data." });

            var (success, token, refreshToken, userData, error) = await _authService.LoginAsync(model);
            if (!success)
            {
                Response.StatusCode = 401;
                return Json(new { detail = error });
            }

            return Json(new
            {
                access = token,
                refresh = refreshToken,
                user = userData
            });
        }

        /// <summary>POST /api/auth/register</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var (success, message, userData) = await _authService.RegisterAsync(model);
            if (!success)
            {
                Response.StatusCode = 400;
                return Json(new { detail = message });
            }

            Response.StatusCode = 201;
            return Json(new { message, user = userData });
        }

        /// <summary>POST /api/auth/token/refresh</summary>
        [HttpPost("token/refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var (success, token) = await _authService.RefreshTokenAsync(model.Refresh);
            if (!success)
            {
                Response.StatusCode = 401;
                return Json(new { detail = "Invalid or expired refresh token." });
            }
            return Json(new { access = token });
        }

        /// <summary>GET /api/auth/me</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me([FromServices] IUserService userService)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null) return Json(new { detail = "User not found." });
            return Json(user);
        }

        /// <summary>POST /api/auth/change-password</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var success = await _authService.ChangePasswordAsync(userId, model);
            if (!success)
            {
                Response.StatusCode = 400;
                return Json(new { detail = "Current password is incorrect." });
            }
            return Json(new { message = "Password changed successfully." });
        }

        /// <summary>POST /api/auth/logout</summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // JWT is stateless; client discards the token.
            return Json(new { message = "Logged out successfully." });
        }
    }

    public class RefreshTokenRequest
    {
        public string Refresh { get; set; } = string.Empty;
    }
}
