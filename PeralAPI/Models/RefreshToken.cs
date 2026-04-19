namespace PeralAPI.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("token")]
        public string Token { get; set; } = null!;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("revokedAt")]
        public DateTime? RevokedAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        [BsonIgnore]
        public bool IsRevoked => RevokedAt != null;

        [BsonIgnore]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
