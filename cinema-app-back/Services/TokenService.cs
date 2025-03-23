namespace cinema_app_back.Services;
using cinema_app_back.Models;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

public class TokenService
{
    // Specify how long until the token expires
    private const int ExpirationMinutes = 30;
    private readonly ILogger<TokenService> _logger;
    private readonly UserManager<User> _userManager;

    public TokenService(ILogger<TokenService> logger, UserManager<User> userManager = null)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<string> CreateTokenAsync(User user)
    {
        try
        {
            _logger.LogInformation($"Creating token for user: {user.UserName}, Email: {user.Email}, ID: {user.Id}");
            
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            
            // Отримуємо ролі користувача з UserManager, якщо він доступний
            List<string> userRoles = new List<string>();
            if (_userManager != null)
            {
                userRoles = (await _userManager.GetRolesAsync(user)).ToList();
                _logger.LogInformation($"User roles from UserManager: {string.Join(", ", userRoles)}");
            }
            else if (user.Role != Role.User) // Якщо UserManager недоступний, використовуємо властивість Role
            {
                userRoles.Add(user.Role.ToString());
                _logger.LogInformation($"Using role from user object: {user.Role}");
            }
            
            var claims = CreateClaims(user, userRoles);
            var token = CreateJwtToken(
                claims,
                CreateSigningCredentials(),
                expiration
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"JWT Token created, expires: {expiration}, length: {tokenString.Length}");
            _logger.LogInformation($"Claims in token: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating token");
            throw;
        }
    }

    // Для зворотної сумісності
    public string CreateToken(User user)
    {
        return CreateTokenAsync(user).GetAwaiter().GetResult();
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration) =>
        new(
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["ValidIssuer"],
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["ValidAudience"],
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

    private List<Claim> CreateClaims(User user, List<string> roles = null)
    {
        var jwtSub = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["JwtRegisteredClaimNamesSub"];

        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtSub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Додаємо роль зі списку ролей, якщо він не порожній
            if (roles != null && roles.Count > 0)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                _logger.LogInformation($"Added roles from roles list: {string.Join(", ", roles)}");
            }
            // Або використовуємо властивість Role користувача як запасний варіант
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
                _logger.LogInformation($"Added role from user.Role: {user.Role}");
            }

            return claims;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating claims");
            throw;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var symmetricSecurityKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["SymmetricSecurityKey"];

        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}
