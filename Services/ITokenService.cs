using NewAdminSystem.Api.Models;

namespace NewAdminSystem.Api.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        RefreshToken CreateRefreshToken(User user);
    }
}
