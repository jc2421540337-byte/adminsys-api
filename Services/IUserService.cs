using NewAdminSystem.Api.DTOs.Common;
using NewAdminSystem.Api.DTOs.Users;
using NewAdminSystem.Api.Models;
namespace NewAdminSystem.Api.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User? GetUserById(int id);
        User CreateUser(User user);
        User? UpdateUser(int id, User user);
        bool DeleteUser(int id);
        User? GetByEmail(string email);
        User Register(User user, string password);
        bool VerifyPassword(User user, string password);
        bool SoftDelete(int id);
        IEnumerable<User> CheckSoftDeletedUsers();
        Task<PagedResultDto<UserListDto>> GetPagedAsync(PagedRequestDto request);
    }
}
