using NewAdminSystem.Api.Data;
using NewAdminSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NewAdminSystem.Api.DTOs.Users;
using NewAdminSystem.Api.DTOs.Common;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace NewAdminSystem.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public IEnumerable<User> GetAllUsers() => _context.Users.AsNoTracking().ToList();

    public User? GetUserById(int id) => _context.Users.Find(id);

    public User CreateUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public User? UpdateUser(int id, User user)
    {
        var existing = _context.Users.Find(id);
        if (existing == null) return null;

        existing.Username = user.Username;
        existing.Email = user.Email;
        existing.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
        _context.SaveChanges();
        return existing;
    }

    public bool DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        _context.SaveChanges();
        return true;
    }

    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(x => x.Email == email);
    }

    public User Register(User user, string password)
    {
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
            );

        return result == PasswordVerificationResult.Success;
    }

    public bool SoftDelete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return false;

        user.IsDeleted = true;
        _context.SaveChanges();
        return true;
    }

    public IEnumerable<User> CheckSoftDeletedUsers() => _context.Users.IgnoreQueryFilters().Where(u => u.IsDeleted).ToList();

    // Additional methods for pagination and sorting can be added here
    public async Task<PagedResultDto<UserListDto>> GetPagedAsync(PagedRequestDto request)
    {
        var query = _context.Users.AsQueryable();

        // Searching
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(u =>
                u.Username.Contains(keyword) ||
                u.Email.Contains(keyword));
        }

        // Sorting
        query = request.SortBy switch
        {
            "username" => request.SortDir == "desc"
                ? query.OrderByDescending(u => u.Username)
                : query.OrderBy(u => u.Username),

            "email" => request.SortDir == "desc"
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResultDto<UserListDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

}
