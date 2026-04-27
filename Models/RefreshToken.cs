using NewAdminSystem.Api.Models.Base;
namespace NewAdminSystem.Api.Models
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
