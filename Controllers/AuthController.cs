using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewAdminSystem.Api.DTOs.Auth;
using NewAdminSystem.Api.DTOs.Users;
using NewAdminSystem.Api.Models;
using NewAdminSystem.Api.Models.Common;
using NewAdminSystem.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewAdminSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly IConfiguration _config;
        //private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, IMapper mapper, IConfiguration config, ITokenService tokenService)
        {
            _userService = userService;
            _mapper = mapper;
            _config = config;
            _tokenService = tokenService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            if (_userService.GetByEmail(dto.Email) != null)
                return Unauthorized(ApiResponse<string>.Fail(
                    "Email already exist!",
                    "USER_EMAIL_EXIST"
                ));

            var user = _mapper.Map<User>(dto);
            var created = _userService.Register(user, dto.Password);

            return Ok(created.Id);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _userService.GetByEmail(dto.Email);
            if (user == null || !_userService.VerifyPassword(user, dto.Password))
                return Unauthorized(ApiResponse<string>.Fail(
                    "Invalid email or password",
                    "AUTH_INVALID_CREDENTIALS"
                ));

            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken(user);

            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token
            });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin")]
        public IActionResult AdminEndpoint()
        {
            return Ok("Admin access only");
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("User or Admin can access");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("Delete/api/users/{id}")]
        public IActionResult Delete(int id)
        {
            var deletedUser = _userService.SoftDelete(id);
            if (!deletedUser)
            {
                return Unauthorized(ApiResponse<string>.Fail(
                    "Invalid user ID",
                    "AUTH_INVALID_USER_ID"
                ));
            }

            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("GetDelete/api/users/")]
        public IActionResult GetDeletedUser()
        {
            var deletedUsers = _userService.CheckSoftDeletedUsers();
            if (deletedUsers == null)
            {
                return Unauthorized(ApiResponse<string>.Fail(
                    "Invalid user ID",
                    "AUTH_INVALID_USER_ID"
                ));
            }
            return Ok(_mapper.Map<IEnumerable<UserDto>>(deletedUsers));
        }

        private string GenerateJwtToken(User user)
        {
            var jwt = _config.GetSection("Jwt");

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwt["ExpireMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
