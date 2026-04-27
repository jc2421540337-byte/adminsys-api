using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewAdminSystem.Api.DTOs.Common;
using NewAdminSystem.Api.DTOs.Users;
using NewAdminSystem.Api.Models;
using NewAdminSystem.Api.Services;

namespace NewAdminSystem.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService , IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [Authorize(Policy = "UserRead")]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            var result = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(result);
        }
        [Authorize(Policy = "UserRead")]
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(_mapper.Map<UserDto>(user));
        }
        [Authorize(Policy = "UserCreate")]
        [HttpPost]
        public IActionResult CreateUser(UserCreateDto dto)
        {
            if (string.IsNullOrEmpty(dto.Password))
                return BadRequest("Password is required");

            var user = _mapper.Map<User>(dto);

            var created = _userService.CreateUser(user);
            return CreatedAtAction(nameof(GetUser), new { id = created.Id }, _mapper.Map<UserDto>(created));
        }

        [Authorize(Policy = "UserUpdate")]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, UserUpdateDto dto)
        {
            var user = _mapper.Map<User>(dto);
            var updated = _userService.UpdateUser(id, user);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<UserDto>(updated));
        }

        [Authorize(Policy = "UserDelete")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var deleted = _userService.DeleteUser(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("Get Sorted Users")]
        public async Task<IActionResult> GetUsers([FromQuery] PagedRequestDto request)
        {
            var result = await _userService.GetPagedAsync(request);
            return Ok(result);
        }


        [HttpGet("test-exception")]
        public IActionResult TestException()
        {
            throw new Exception("This is a test exception!");
        }

    }
}
