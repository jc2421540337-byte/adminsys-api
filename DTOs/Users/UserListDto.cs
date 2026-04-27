namespace NewAdminSystem.Api.DTOs.Users
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
