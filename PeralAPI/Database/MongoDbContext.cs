using MongoDB.Bson;
using MongoDB.Driver;
using PeralAPI.Models;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            Client = new MongoClient(config["MongoDB:ConnectionString"]);
            _database = Client.GetDatabase(config["MongoDB:DatabaseName"]);

            EnsureIndexes();
            EnsureProductQuantityView();
            EnsureVendorCreditView();
        }
        public IMongoClient Client { get; }
        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        public IMongoCollection<RefreshToken> RefreshTokens =>
            _database.GetCollection<RefreshToken>("refresh_tokens");

        public IMongoCollection<Notification> Notifications =>
            _database.GetCollection<Notification>("notifications");

        public IMongoCollection<UserNotificationRead> UserNotificationReads =>
            _database.GetCollection<UserNotificationRead>("user_notification_reads");

        public IMongoCollection<ProductModel> Products =>
            _database.GetCollection<ProductModel>("products");

        public IMongoCollection<VendorModel> Vendors =>
            _database.GetCollection<VendorModel>("vendors");

        public IMongoCollection<InventoryOrderModel> InventoryOrders =>
            _database.GetCollection<InventoryOrderModel>("inventory_orders");

        public IMongoCollection<ProductTransactionLedgerModel> ProductTransactionLedger =>
            _database.GetCollection<ProductTransactionLedgerModel>("product_transaction_ledger");

        public IMongoCollection<ProductQuantityViewModel> ProductQuantityView =>
    _database.GetCollection<ProductQuantityViewModel>("product_quantity_view");
        public IMongoCollection<VendorCreditViewModel> VendorCreditView =>
    _database.GetCollection<VendorCreditViewModel>("vendor_credit_view");

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

        private void EnsureProductQuantityView()
        {
            var viewName = "product_quantity_view";
            var existingCollections = _database.ListCollectionNames().ToList();

            if (existingCollections.Contains(viewName))
                return;

            var pipeline = new[]
            {
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$productId" },
            { "totalQuantity", new BsonDocument("$sum", "$quantity") }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "productId", "$_id" },
            { "totalQuantity", 1 }
        })};

            _database.CreateView(
                viewName,
                "product_transaction_ledger",
                PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline)
            );
        }

        private void EnsureVendorCreditView()
        {
            var viewName = "vendor_credit_view";
            var existingCollections = _database.ListCollectionNames().ToList();

            if (existingCollections.Contains(viewName))
                return;

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("status", 1)),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$vendorId" },
            { "credit", new BsonDocument("$sum", new BsonDocument(
                "$subtract", new BsonArray { "$paymentInformation.value", "$paymentInformation.amountPaid" }
            ))}
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "vendorId", "$_id" },
            { "credit", 1 }
        })
    };

            _database.CreateView(
                viewName,
                "inventory_orders",
                PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline)
            );
        }
    }
}
