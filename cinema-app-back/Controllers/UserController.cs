namespace cinema_app_back.Controllers;
using cinema_app_back.Models;

using cinema_app_back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;
    private readonly TokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager<User> userManager, DataContext context, TokenService tokenService, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpGet]
    [Route("auth-test")]
    [Authorize]
    public ActionResult<object> CheckAuth()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var isInAdminRole = User.IsInRole("Admin");

            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

            return Ok(new
            {
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail,
                UserRole = userRole,
                IsAuthenticated = User.Identity.IsAuthenticated,
                AuthenticationType = User.Identity.AuthenticationType,
                IsInAdminRole = isInAdminRole,
                Claims = claims
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Username))
        {
            return BadRequest(new { message = "Email and Username are required" });
        }

        var user = new User 
        { 
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Birthday = request.Birthday,
            Role = request.Role == Role.Admin ? Role.Admin : Role.User // Ensure role is set correctly
        };

        var result = await _userManager.CreateAsync(user, request.Password!);

        if (result.Succeeded)
        {
            // Assign the role using UserManager
            if (user.Role == Role.Admin)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation($"Assigned Admin role to user {user.UserName} ({user.Email})");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation($"Assigned User role to user {user.UserName} ({user.Email})");
            }

            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email, role = user.Role }, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid model state during login attempt for email: {request.Email}");
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Login attempt for email: {request.Email}");
            var user = await _userManager.FindByEmailAsync(request.Email!);
            
            if (user == null)
            {
                _logger.LogWarning($"Login failed: User not found for email: {request.Email}");
                return BadRequest(new { message = "Невірний email або пароль" });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password!);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Login failed: Invalid password for user: {user.UserName} ({user.Email})");
                return BadRequest(new { message = "Невірний email або пароль" });
            }

            // Отримуємо ролі користувача
            var userRoles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User {user.UserName} ({user.Email}) has roles: {string.Join(", ", userRoles)}");

            // Створюємо токен з асинхронним методом
            var accessToken = await _tokenService.CreateTokenAsync(user);
            
            // Зберігаємо будь-які зміни в контексті
            await _context.SaveChangesAsync();

            // Створюємо та повертаємо відповідь автентифікації
            var response = new AuthResponse
            {
                Email = user.Email,
                Token = accessToken,
                Role = userRoles.Contains("Admin") ? "Admin" : "User" // Використовуємо роль з UserManager
            };

            _logger.LogInformation($"Login successful for user: {user.UserName} ({user.Email}), role: {response.Role}, token length: {accessToken.Length}");
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during login for email: {request.Email}");
            return StatusCode(500, new { error = "Під час входу сталася помилка. Спробуйте ще раз пізніше." });
        }
    }
}

