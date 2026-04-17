namespace PeralAPI.Models.DTOs
{
    public record CreateNotificationDto(
        string Type,
        string? UserId,
        string Message,
        NotificationMetadataDto? Metadata
    );

    public record NotificationMetadataDto(
        string? Link,
        string? Icon,
        string? Category
    );

    public record NotificationDto(
        string Id,
        string Type,
        string? UserId,
        string Message,
        NotificationMetadataDto? Metadata,
        DateTime CreatedAt,
        bool IsRead
    );

    public record PagedNotificationsDto(
        List<NotificationDto> Items,
        int Page,
        int Limit,
        long TotalCount
    );

    public record UnreadCountDto(long Count);
}
