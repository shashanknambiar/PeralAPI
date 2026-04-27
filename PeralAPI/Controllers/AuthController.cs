using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) => _auth = auth;

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var (result, error) = await _auth.LoginAsync(dto);
            if (result == null) return Unauthorized(new { error });
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto dto)
        {
            var result = await _auth.RefreshAsync(dto.RefreshToken);
            if (result == null) return Unauthorized(new { error = "Invalid or expired refresh token." });
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDto dto)
        {
            var revoked = await _auth.RevokeAsync(dto.RefreshToken);
            if (!revoked) return BadRequest(new { error = "Token not found or already revoked." });
            return Ok(new { message = "Logged out successfully." });
        }

    }
}
