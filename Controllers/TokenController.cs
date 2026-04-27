using Microsoft.AspNetCore.Mvc;
using NewAdminSystem.Api.Services;
using NewAdminSystem.Api.Models;
using NewAdminSystem.Api.DTOs.Token;
using NewAdminSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace NewAdminSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;
        public TokenController(ITokenService tokenService, AppDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }
        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenDto dto)
        {
            var token = _context.RefreshTokens
             .Include(r => r.User)
             .FirstOrDefault(r =>
                 r.Token == dto.RefreshToken &&
                 !r.IsRevoked &&
                 r.ExpiresAt > DateTime.UtcNow);

            if (token == null)
                return Unauthorized("Invalid refresh token");

            token.IsRevoked = true;

            var newAccessToken = _tokenService.CreateAccessToken(token.User);
            var newRefreshToken = _tokenService.CreateRefreshToken(token.User);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken.Token
            });

        }
    }
}
