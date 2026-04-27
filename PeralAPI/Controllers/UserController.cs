using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto dto)
        {
            var (success, error, user) = await _users.RegisterAsync(dto);
            if (!success) return BadRequest(new { error });
            UserDto userDto = new UserDto(user!.Id, user.UserName, user.Email, user.Roles, user.AvatarUrl, user.IsActive, user.CreatedAt);
            return CreatedAtAction(nameof(Register), new { id = user!.Id }, userDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll() =>
            Ok(await _users.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _users.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<UserDto>> Update(string userId, [FromBody] UpdateUserDto dto)
        {
            var (success, error) = await _users.UpdateAsync(userId, dto);
            if (!success) return BadRequest(new { error });
            var result = await _users.GetByIdAsync(userId);
            return Ok(result);
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> Delete(string userId)
        {
            var (success, error) = await _users.DeleteAsync(userId);
            if (!success) return string.IsNullOrEmpty(error) ? NotFound() : BadRequest(new { error });
            return Ok(new { message = "User deleted." });
        }

        [HttpPatch("{userId}/deactivate")]
        public async Task<ActionResult> Deactivate(string userId)
        {
            var success = await _users.DeactivateAsync(userId);
            return success ? Ok(new { message = "User deactivated." }) : NotFound();
        }
    }
}
