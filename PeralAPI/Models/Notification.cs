namespace PeralAPI.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!; // "global" | "user"

        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; } = null!;

        [BsonElement("metadata")]
        public NotificationMetadata? Metadata { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationMetadata
    {
        [BsonElement("link")]
        public string? Link { get; set; }

        [BsonElement("icon")]
        public string? Icon { get; set; }

        [BsonElement("category")]
        public string? Category { get; set; }
    }

    public class UserNotificationRead
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("notificationId")]
        public string NotificationId { get; set; } = null!;

        [BsonElement("readAt")]
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
