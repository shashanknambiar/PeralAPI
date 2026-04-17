namespace PeralAPI.Models.DTOs
{
    public class UsersDtos
    {
        public record User
        {
            public int Id { get; set; }
            public string UserName { get; set; }

        }
    }
}
