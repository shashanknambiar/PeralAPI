namespace PeralAPI.Models.DTOs
{
    public record RegisterDto(
     string UserName,
     string Email,
     string Password,
     string? AvatarUrl,
     List<string> Roles
 );

    public record LoginDto(
        string UserName,
        string Password
    );

    public record RefreshTokenDto(
        string RefreshToken
    );

    public record AssignRoleDto(
        string UserId,
        string Role
    );

    public record AuthResponseDto(
        string AccessToken,
        string RefreshToken,
        string UserId,
        string UserName,
        string? AvatarUrl,
        List<string> Roles
    );

    public record UserDto(
        string Id,
        string UserName,
        string Email,
        List<string> Roles,
        string? AvatarUrl,
        bool IsActive,
        DateTime CreatedAt
    );

    public record UpdateUserDto(
        string UserName,
        List<string> Roles,
        bool ChangePassword,
        string? NewPassword,
        string? ConfirmPassword
    );
}
