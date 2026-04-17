using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _users;
        public UsersController(UserService users) => _users = users;

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var (success, error) = await _users.RegisterAsync(dto);
            if (!success) return BadRequest(new { error });
            return Ok(new { message = "User registered successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _users.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _users.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("{userId}/roles/{role}")]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var (success, error) = await _users.AssignRoleAsync(userId, role);
            if (!success) return BadRequest(new { error });
            return Ok(new { message = $"Role '{role}' assigned." });
        }

        [HttpDelete("{userId}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var (success, error) = await _users.RemoveRoleAsync(userId, role);
            if (!success) return BadRequest(new { error });
            return Ok(new { message = $"Role '{role}' removed." });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto dto)
        {
            var (success, error) = await _users.UpdateAsync(userId, dto);
            if (!success) return BadRequest(new { error });
            return Ok(new { message = "User updated." });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            var (success, error) = await _users.DeleteAsync(userId);
            if (!success) return string.IsNullOrEmpty(error) ? NotFound() : BadRequest(new { error });
            return Ok(new { message = "User deleted." });
        }

        [HttpPatch("{userId}/deactivate")]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var success = await _users.DeactivateAsync(userId);
            return success ? Ok(new { message = "User deactivated." }) : NotFound();
        }
    }
}
