using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ERP.Backend.Data;
using ERP.Backend.Models;
using ERP.Backend.Services.Interfaces;
using ERP.Backend.ViewModels;

namespace ERP.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<(bool success, string token, string refreshToken, object userData, string error)>
            LoginAsync(LoginViewModel model)
        {
            var user = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return (false, "", "", null!, "Invalid credentials.");

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return (true, token, refreshToken, MapUserToDto(user), "");
        }

        public async Task<(bool success, string message, object? userData)>
            RegisterAsync(RegisterViewModel model)
        {
            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
                return (false, "A user with this email already exists.", null);

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return (true, "User registered successfully.", MapUserToDto(user));
        }

        public Task<(bool success, string token)> RefreshTokenAsync(string refreshToken)
        {
            // For simplicity, refresh tokens are validated client-side by re-login.
            // In production, store refresh tokens in DB.
            return Task.FromResult((false, ""));
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordViewModel model)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash)) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _db.SaveChangesAsync();
            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey1234567890AbcDef!!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "ERP.Backend",
                audience: _config["Jwt:Audience"] ?? "ERP.Frontend",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static object MapUserToDto(User user) => new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            fullName = user.FullName,
            role = user.Role.ToString(),
            avatar = user.Avatar,
            phone = user.Phone,
            bio = user.Bio,
            employeeId = user.EmployeeId,
            designation = user.Designation,
            skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.SkillsJson) ?? new(),
            dateOfJoining = user.DateOfJoining,
            departmentId = user.DepartmentId,
            department = user.Department == null ? null : new { id = user.Department.Id, name = user.Department.Name },
            teamId = user.TeamId,
            team = user.Team == null ? null : new { id = user.Team.Id, name = user.Team.Name },
            isActive = user.IsActive,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        };
    }
}
