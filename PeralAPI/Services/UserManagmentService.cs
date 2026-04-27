namespace PeralAPI.Services
{
    using MongoDB.Driver;
    using PeralAPI.Database;
    using PeralAPI.Models;
    using PeralAPI.Models.DTOs;
    using PeralAPI.Utilities;

    public class UserService
    {
        private readonly MongoDbContext _db;

        public UserService(MongoDbContext db) => _db = db;

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _db.Users.Find(_ => true).ToListAsync();
            return users.Select(ToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user == null ? null : ToDto(user);
        }

        public async Task<(bool Success, string Error)> AssignRoleAsync(string userId, string role)
        {
            var validRoles = new[] { "Admin", "Manager", "User" };
            if (!validRoles.Contains(role))
                return (false, $"Invalid role. Valid roles: {string.Join(", ", validRoles)}");

            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return (false, "User not found.");
            if (user.Roles.Contains(role)) return (false, "User already has this role.");

            await _db.Users.UpdateOneAsync(
                u => u.Id == userId,
                Builders<User>.Update.Push(u => u.Roles, role));

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> RemoveRoleAsync(string userId, string role)
        {
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return (false, "User not found.");
            if (!user.Roles.Contains(role)) return (false, "User does not have this role.");
            if (user.Roles.Count == 1) return (false, "User must have at least one role.");

            await _db.Users.UpdateOneAsync(
                u => u.Id == userId,
                Builders<User>.Update.Pull(u => u.Roles, role));

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error, User? user)> RegisterAsync(CreateUserDto dto)
        {
            if (dto.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return (false, "The username 'Admin' is reserved.", null);

            var (isValid, error) = PasswordValidator.Validate(dto.Password);
            if (!isValid) return (false, error, null);

            var exists = await _db.Users.Find(u => u.UserName == dto.UserName).AnyAsync();
            if (exists) return (false, "Username already taken.", null);

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                AvatarUrl = dto.AvatarUrl ?? "",
                Roles = dto.Roles
            };

            await _db.Users.InsertOneAsync(user);
            return (true, string.Empty, user);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(string userId, UpdateUserDto dto)
        {
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return (false, "User not found.");

            if (user.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                && !dto.Roles.Contains("Admin"))
                return (false, "The 'Admin' user must always retain the 'Admin' role.");

            if (!user.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                && dto.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return (false, "The username 'Admin' is reserved.");

            if (dto.Roles.Count == 0)
                return (false, "User must have at least one role.");

            var updates = new List<UpdateDefinition<User>>
            {
                Builders<User>.Update.Set(u => u.UserName, dto.UserName),
                Builders<User>.Update.Set(u => u.Roles, dto.Roles)
            };

            if (dto.ChangePassword)
            {
                if (string.IsNullOrWhiteSpace(dto.NewPassword))
                    return (false, "New password is required when ChangePassword is true.");

                if (dto.NewPassword != dto.ConfirmPassword)
                    return (false, "Passwords do not match.");

                var (isValid, error) = PasswordValidator.Validate(dto.NewPassword);
                if (!isValid) return (false, error);

                updates.Add(Builders<User>.Update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(dto.NewPassword)));
            }

            await _db.Users.UpdateOneAsync(u => u.Id == userId, Builders<User>.Update.Combine(updates));
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteAsync(string userId)
        {
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return (false, string.Empty);

            if (user.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return (false, "The 'Admin' user cannot be deleted.");

            await _db.Users.DeleteOneAsync(u => u.Id == userId);
            await _db.RefreshTokens.DeleteManyAsync(r => r.UserId == userId);
            return (true, string.Empty);
        }

        public async Task<bool> DeactivateAsync(string userId)
        {
            var result = await _db.Users.UpdateOneAsync(
                u => u.Id == userId,
                Builders<User>.Update.Set(u => u.IsActive, false));

            return result.ModifiedCount > 0;
        }

        private static UserDto ToDto(User u) =>
            new(u.Id, u.UserName, u.Email, u.Roles, u.AvatarUrl, u.IsActive, u.CreatedAt);
    }
}
