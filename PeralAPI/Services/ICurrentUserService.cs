namespace PeralAPI.Services
{
    public interface ICurrentUserService
    {
        string UserName { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserName =>
            _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Unknown";
    }
}
