using MongoDB.Driver;
using PeralAPI.Models;

namespace PeralAPI.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(config["MongoDB:DatabaseName"]);

            EnsureIndexes();
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        public IMongoCollection<RefreshToken> RefreshTokens =>
            _database.GetCollection<RefreshToken>("refresh_tokens");

        public IMongoCollection<Notification> Notifications =>
            _database.GetCollection<Notification>("notifications");

        public IMongoCollection<UserNotificationRead> UserNotificationReads =>
            _database.GetCollection<UserNotificationRead>("user_notification_reads");

        private void EnsureIndexes()
        {
            // Unique index on username and email
            var userIndexes = Users.Indexes;

            userIndexes.CreateOne(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.UserName),
                new CreateIndexOptions { Unique = true }));

            userIndexes.CreateOne(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }));

            // TTL index — auto-delete expired refresh tokens after 30 days
            RefreshTokens.Indexes.CreateOne(new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(r => r.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) }));

            // notifications: query indexes + TTL auto-expiry after 90 days
            var notifIndexes = Notifications.Indexes;

            notifIndexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(n => n.Type)));

            notifIndexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(n => n.UserId)));

            notifIndexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(n => n.CreatedAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) }));

            // user_notification_reads: unique compound index + TTL after 90 days
            var readIndexes = UserNotificationReads.Indexes;

            readIndexes.CreateOne(new CreateIndexModel<UserNotificationRead>(
                Builders<UserNotificationRead>.IndexKeys
                    .Ascending(r => r.UserId)
                    .Ascending(r => r.NotificationId),
                new CreateIndexOptions { Unique = true }));

            readIndexes.CreateOne(new CreateIndexModel<UserNotificationRead>(
                Builders<UserNotificationRead>.IndexKeys.Ascending(r => r.ReadAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(90) }));
        }
    }
}
