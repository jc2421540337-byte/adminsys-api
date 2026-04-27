using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewAdminSystem.Api.Authorization;
using NewAdminSystem.Api.Data;
using NewAdminSystem.Api.Middlewares;
using NewAdminSystem.Api.Models;
using NewAdminSystem.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                      "Data Source=NewAdminSystem.db"));

// Dependency Injection for Services
builder.Services.AddScoped<IUserService, UserService>();
// Token Service Registration
builder.Services.AddScoped<ITokenService, TokenService>();

// Password Hasher Configuration
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Add services to the container.
builder.Services.AddControllers();

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NewAdminSystem API",
        Version = "v1"
    });

    // Swagger JWT Authentication Configuration
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new()
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


// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),

            RoleClaimType = ClaimTypes.Role
        };
    });

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(Program));
// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));

    options.AddPolicy("SelfOrAdmin", policy =>
        policy.RequireAssertion(context =>
        {
            if (context.User.IsInRole("Admin"))
                return true;

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                            ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
                return false;

            var routeId = (context.Resource as HttpContext)?
                .Request
                .RouteValues["id"]?
                .ToString();

            return routeId == userIdClaim.Value;
        }));
    options.AddPolicy("UserRead", policy =>
       policy.RequireClaim("permission", Permissions.UserRead));

    options.AddPolicy("UserCreate", policy =>
        policy.RequireClaim("permission", Permissions.UserCreate));

    options.AddPolicy("UserUpdate", policy =>
        policy.RequireClaim("permission", Permissions.UserUpdate));

    options.AddPolicy("UserDelete", policy =>
        policy.RequireClaim("permission", Permissions.UserDelete));
});

// HttpContextAccessor Registration
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply pending migrations and create the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdminSys API v1");
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
