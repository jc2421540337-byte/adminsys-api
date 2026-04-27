using System.ComponentModel.DataAnnotations;
using NewAdminSystem.Api.Models.Base;
using NewAdminSystem.Api.Authorization;

namespace NewAdminSystem.Api.Models
{
    public class User : BaseEntity
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = Roles.User;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
