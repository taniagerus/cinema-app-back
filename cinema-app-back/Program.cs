using cinema_app_back;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using cinema_app_back.Models; // Updated namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using cinema_app_back.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.SetIsOriginAllowed(origin => true) // Для розробки
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("WWW-Authenticate", "Authorization")
               .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add JSON options before Swagger configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Cinema API", 
        Version = "v1",
        Description = "API for Cinema Application",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@cinema.com"
        }
    });
    
    // Configure Swagger to handle enums as strings
    option.UseInlineDefinitionsForEnums();
    
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<TokenService, TokenService>();

// Configure JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Specify identity requirements
// Must be added before .AddAuthentication otherwise a 404 is thrown on authorized endpoints
builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>();

// Додавання AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// These will eventually be moved to a secrets file, but for alpha development appsettings is fine
var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey)
            ),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // Додамо обробку подій для діагностики
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError($"Authentication failed: {context.Exception.Message}");
                
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var claims = identity?.Claims?.Select(c => $"{c.Type}: {c.Value}")?.ToList() ?? new List<string>();
                
                logger.LogInformation($"Token validated successfully. Claims: {string.Join(", ", claims)}");
                
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                
                if (!string.IsNullOrEmpty(token))
                {
                    logger.LogInformation($"JWT token received, length: {token.Length}");
                }
                else
                {
                    // Перевіряємо наявність токена в query string (для тестування)
                    token = context.Request.Query["auth"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                        logger.LogInformation($"JWT token received from query string, length: {token.Length}");
                    }
                    else
                    {
                        logger.LogWarning("No JWT token found in request");
                    }
                }
                
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning($"Authentication challenge issued: {context.Error}, {context.ErrorDescription}");
                
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Configure error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false;
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinema API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add CORS middleware
app.UseCors();

// Додаємо підтримку статичних файлів
app.UseStaticFiles();

// Додаємо діагностичний ендпоінт для JWT
app.MapGet("/debug-jwt", (HttpContext context) => {
    var authHeader = context.Request.Headers.Authorization.ToString();
    var claims = context.User?.Claims?.Select(c => new { c.Type, c.Value })?.ToList();
    
    return Results.Ok(new { 
        IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
        AuthType = context.User?.Identity?.AuthenticationType,
        AuthHeader = authHeader,
        Claims = claims,
        IsInAdminRole = context.User?.IsInRole("Admin")
    });
});

// Діагностичний ендпоінт для перевірки токенів
app.MapGet("/api/debug-token", (HttpContext context) => {
    try {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var headers = context.Request.Headers;
        var auth = headers.Authorization.ToString();
        
        logger.LogInformation($"Auth header: {auth}");
        
        var authParts = auth.Split(' ');
        string token = auth;
        
        if (authParts.Length > 1 && authParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            token = authParts[1];
        }
        
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = null;
        
        try {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error parsing token");
            
            // Спробуємо знайти токен в query string
            token = context.Request.Query["auth"].ToString();
            if (!string.IsNullOrEmpty(token)) {
                try {
                    jwtToken = handler.ReadJwtToken(token);
                }
                catch {
                    // Ігноруємо помилку
                }
            }
        }
        
        return Results.Ok(new {
            TokenValid = jwtToken != null,
            Headers = headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Claims = jwtToken?.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            UserAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
            UserClaims = context.User?.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
    catch (Exception ex) {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Додаємо тестовий ендпоінт для перевірки JWT токенів з frontend
app.MapGet("/api/test-auth", [Authorize] (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var identity = context.User?.Identity;
    var claims = context.User?.Claims?.Select(c => new { c.Type, c.Value })?.ToList();
    
    logger.LogInformation($"Токен авторизації пройшов перевірку. Користувач: {identity?.Name}, Ролі: {string.Join(", ", context.User?.Claims?.Where(c => c.Type == ClaimTypes.Role)?.Select(c => c.Value) ?? Array.Empty<string>())}");
    
    return Results.Ok(new { 
        isAuthenticated = identity?.IsAuthenticated,
        userName = identity?.Name,
        claims = claims,
        roles = context.User?.Claims?.Where(c => c.Type == ClaimTypes.Role)?.Select(c => c.Value)?.ToList()
    });
});

app.UseHttpsRedirection();
app.UseStatusCodePages();

// Обов'язково перші Authentication, потім Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
