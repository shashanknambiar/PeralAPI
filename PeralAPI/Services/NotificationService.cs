namespace PeralAPI.Services
{
    using Microsoft.AspNetCore.SignalR;
    using MongoDB.Driver;
    using PeralAPI.Database;
    using PeralAPI.Hubs;
    using PeralAPI.Models;
    using PeralAPI.Models.DTOs;
    using StackExchange.Redis;

    public class NotificationService
    {
        private readonly MongoDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IDatabase? _cache;
        private const int CacheTtlMinutes = 5;

        public NotificationService(MongoDbContext db, IHubContext<NotificationHub> hub, IServiceProvider sp)
        {
            _db = db;
            _hub = hub;
            _cache = sp.GetService<IConnectionMultiplexer>()?.GetDatabase();
        }

        public async Task<PagedNotificationsDto> GetNotificationsAsync(string userId, int page, int limit)
        {
            limit = Math.Clamp(limit, 1, 100);
            page = Math.Max(1, page);

            var filter = VisibleFilter(userId);
            var total = await _db.Notifications.CountDocumentsAsync(filter);

            var notifications = await _db.Notifications
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();

            var ids = notifications.Select(n => n.Id).ToList();
            var readSet = await GetReadSetAsync(userId, ids);

            var items = notifications.Select(n => ToDto(n, readSet.Contains(n.Id))).ToList();
            return new PagedNotificationsDto(items, page, limit, total);
        }

        public async Task<(bool Success, string Error, NotificationDto? Created)> CreateNotificationAsync(CreateNotificationDto dto)
        {
            if (dto.Type != "global" && dto.Type != "user")
                return (false, "Type must be 'global' or 'user'.", null);

            if (dto.Type == "user" && string.IsNullOrWhiteSpace(dto.UserId))
                return (false, "UserId is required for user notifications.", null);

            if (string.IsNullOrWhiteSpace(dto.Message))
                return (false, "Message is required.", null);

            var notification = new Notification
            {
                Type = dto.Type,
                UserId = dto.Type == "user" ? dto.UserId : null,
                Message = dto.Message,
                Metadata = dto.Metadata == null ? null : new NotificationMetadata
                {
                    Link = dto.Metadata.Link,
                    Icon = dto.Metadata.Icon,
                    Category = dto.Metadata.Category
                },
                CreatedAt = DateTime.UtcNow
            };

            await _db.Notifications.InsertOneAsync(notification);
            var created = ToDto(notification, false);

            if (dto.Type == "global")
            {
                // Global notifications affect all users' unread counts.
                // Cached counts will be invalidated naturally on next TTL expiry
                // (CacheTtlMinutes) since we can't cheaply update every user's key.
                await _hub.Clients.All.SendAsync("ReceiveNotification", created);
            }
            else
            {
                await IncrementCacheIfExistsAsync(dto.UserId!);
                await _hub.Clients.Group(dto.UserId!).SendAsync("ReceiveNotification", created);
            }

            return (true, string.Empty, created);
        }

        public async Task<(bool Success, string Error)> MarkAsReadAsync(string notificationId, string userId)
        {
            var notification = await _db.Notifications
                .Find(n => n.Id == notificationId)
                .FirstOrDefaultAsync();

            if (notification == null) return (false, "Notification not found.");

            var visible = notification.Type == "global" ||
                          (notification.Type == "user" && notification.UserId == userId);
            if (!visible) return (false, "Notification not found.");

            var alreadyRead = await _db.UserNotificationReads
                .Find(r => r.UserId == userId && r.NotificationId == notificationId)
                .AnyAsync();

            if (!alreadyRead)
            {
                await _db.UserNotificationReads.InsertOneAsync(new UserNotificationRead
                {
                    UserId = userId,
                    NotificationId = notificationId,
                    ReadAt = DateTime.UtcNow
                });

                var newCount = await DecrementCacheAsync(userId);
                await _hub.Clients.Group(userId).SendAsync("UnreadCountUpdated", newCount);
            }

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> MarkAllAsReadAsync(string userId)
        {
            var notificationIds = await _db.Notifications
                .Find(VisibleFilter(userId))
                .Project(n => n.Id)
                .ToListAsync();

            var readIds = await _db.UserNotificationReads
                .Find(r => r.UserId == userId)
                .Project(r => r.NotificationId)
                .ToListAsync();

            var readSet = readIds.ToHashSet();
            var unreadIds = notificationIds.Where(id => !readSet.Contains(id)).ToList();

            if (unreadIds.Count > 0)
            {
                var reads = unreadIds.Select(id => new UserNotificationRead
                {
                    UserId = userId,
                    NotificationId = id,
                    ReadAt = DateTime.UtcNow
                });

                try
                {
                    await _db.UserNotificationReads.InsertManyAsync(
                        reads, new InsertManyOptions { IsOrdered = false });
                }
                catch (MongoBulkWriteException)
                {
                    // Duplicate key errors from concurrent mark-all calls — safe to ignore
                }
            }

            await SetCacheAsync(userId, 0);
            await _hub.Clients.Group(userId).SendAsync("UnreadCountUpdated", 0);

            return (true, string.Empty);
        }

        public async Task<long> GetUnreadCountAsync(string userId)
        {
            if (_cache != null)
            {
                var cached = await _cache.StringGetAsync(CacheKey(userId));
                if (cached.HasValue && long.TryParse((string?)cached, out var count))
                    return count;
            }

            return await RecomputeAndCacheAsync(userId);
        }

        // --- Helpers ---

        private static FilterDefinition<Notification> VisibleFilter(string userId) =>
            Builders<Notification>.Filter.Or(
                Builders<Notification>.Filter.Eq(n => n.Type, "global"),
                Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.Type, "user"),
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId)
                )
            );

        private async Task<HashSet<string>> GetReadSetAsync(string userId, List<string> ids)
        {
            if (ids.Count == 0) return [];
            var records = await _db.UserNotificationReads
                .Find(r => r.UserId == userId && ids.Contains(r.NotificationId))
                .Project(r => r.NotificationId)
                .ToListAsync();
            return records.ToHashSet();
        }

        private async Task<long> RecomputeAndCacheAsync(string userId)
        {
            var visibleIds = await _db.Notifications
                .Find(VisibleFilter(userId))
                .Project(n => n.Id)
                .ToListAsync();

            if (visibleIds.Count == 0)
            {
                await SetCacheAsync(userId, 0);
                return 0;
            }

            var readCount = await _db.UserNotificationReads.CountDocumentsAsync(
                Builders<UserNotificationRead>.Filter.And(
                    Builders<UserNotificationRead>.Filter.Eq(r => r.UserId, userId),
                    Builders<UserNotificationRead>.Filter.In(r => r.NotificationId, visibleIds)
                ));

            var unread = visibleIds.Count - readCount;
            await SetCacheAsync(userId, unread);
            return unread;
        }

        private async Task IncrementCacheIfExistsAsync(string userId)
        {
            if (_cache == null) return;
            var key = CacheKey(userId);
            if (await _cache.KeyExistsAsync(key))
                await _cache.StringIncrementAsync(key);
        }

        private async Task<long> DecrementCacheAsync(string userId)
        {
            if (_cache == null) return await RecomputeAndCacheAsync(userId);
            var key = CacheKey(userId);
            if (!await _cache.KeyExistsAsync(key))
                return await RecomputeAndCacheAsync(userId);

            var updated = await _cache.StringDecrementAsync(key);
            if (updated < 0)
            {
                await _cache.KeyDeleteAsync(key);
                return await RecomputeAndCacheAsync(userId);
            }
            return updated;
        }

        private async Task SetCacheAsync(string userId, long value)
        {
            if (_cache == null) return;
            await _cache.StringSetAsync(CacheKey(userId), value, TimeSpan.FromMinutes(CacheTtlMinutes));
        }

        private static string CacheKey(string userId) => $"user:{userId}:unread_count";

        private static NotificationDto ToDto(Notification n, bool isRead) =>
            new(n.Id, n.Type, n.UserId, n.Message,
                n.Metadata == null ? null : new NotificationMetadataDto(n.Metadata.Link, n.Metadata.Icon, n.Metadata.Category),
                n.CreatedAt, isRead);
    }
}
