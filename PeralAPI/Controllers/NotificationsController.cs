namespace PeralAPI.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PeralAPI.Models.DTOs;
    using PeralAPI.Services;

    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notifications;
        public NotificationsController(NotificationService notifications) => _notifications = notifications;

        private string UserId => User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        // GET /api/notifications?page=1&limit=20
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 20)
            => Ok(await _notifications.GetNotificationsAsync(UserId, page, limit));

        // POST /api/notifications  (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            var (success, error, created) = await _notifications.CreateNotificationAsync(dto);
            if (!success) return BadRequest(new { error });
            return Ok(created);
        }

        // PATCH /api/notifications/read-all  — must be declared before /{id}/read
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notifications.MarkAllAsReadAsync(UserId);
            return Ok(new { message = "All notifications marked as read." });
        }

        // PATCH /api/notifications/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var (success, error) = await _notifications.MarkAsReadAsync(id, UserId);
            if (!success) return BadRequest(new { error });
            return Ok(new { message = "Marked as read." });
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
            => Ok(new UnreadCountDto(await _notifications.GetUnreadCountAsync(UserId)));
    }
}
