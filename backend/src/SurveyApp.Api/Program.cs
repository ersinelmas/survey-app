using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SurveyApp.Application.Interfaces;
using SurveyApp.Infrastructure.Data;
using SurveyApp.Infrastructure.Services;
using SurveyApp.Application.Services;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// https://www.cloudflare.com/ips/ - only these ranges are trusted to set CF-Connecting-IP
var cloudflareRanges = new[]
{
    "173.245.48.0/20", "103.21.244.0/22", "103.22.200.0/22", "103.31.4.0/22",
    "141.101.64.0/18", "108.162.192.0/18", "190.93.240.0/20", "188.114.96.0/20",
    "197.234.240.0/22", "198.41.128.0/17", "162.158.0.0/15", "104.16.0.0/13",
    "104.24.0.0/14", "172.64.0.0/13", "131.0.72.0/22",
    "2400:cb00::/32", "2606:4700::/32", "2803:f800::/32", "2405:b500::/32",
    "2405:8100::/32", "2a06:98c0::/29", "2c0f:f248::/32",
}.Select(IPNetwork.Parse).ToArray();

string GetClientIp(HttpContext context)
{
    var remoteIp = context.Connection.RemoteIpAddress;
    if (remoteIp is not null && remoteIp.IsIPv4MappedToIPv6)
        remoteIp = remoteIp.MapToIPv4();

    var requestFromCloudflare = remoteIp is not null && cloudflareRanges.Any(range => range.Contains(remoteIp));

    if (requestFromCloudflare)
    {
        var headerIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerIp))
            return headerIp;
    }

    return remoteIp?.ToString() ?? "unknown";
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SurveyApp.Application.Validators.CreateAnswerTemplateRequestValidator>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins konfigürasyonu eksik veya boş.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Custom services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<AuthService>(sp => new AuthService(
    sp.GetRequiredService<IUserRepository>(),
    sp.GetRequiredService<IRefreshTokenRepository>(),
    sp.GetRequiredService<IPasswordHasher>(),
    sp.GetRequiredService<IJwtTokenGenerator>(),
    int.Parse(builder.Configuration["Jwt:RefreshTokenExpiryDays"]!)));
builder.Services.AddScoped<IAnswerTemplateRepository, AnswerTemplateRepository>();
builder.Services.AddScoped<AnswerTemplateService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<SurveyService>();
builder.Services.AddScoped<ISurveyAssignmentRepository, SurveyAssignmentRepository>();
builder.Services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
builder.Services.AddScoped<SurveyFillingService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireAssertion(ctx => ctx.User.FindFirst("IsAdmin")?.Value == "true"));
});
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler,
    SurveyApp.Api.Middleware.CustomAuthorizationMiddlewareResultHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientIp(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
        }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientIp(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
        }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token'ı girin"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<SurveyApp.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<SurveyApp.Api.Middleware.SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed admin users
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    context.Database.Migrate();

    void SeedAdmin(string configSection, bool required)
    {
        var email = builder.Configuration[$"{configSection}:Email"];
        var password = builder.Configuration[$"{configSection}:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            if (required)
            {
                throw new InvalidOperationException(
                    $"{configSection}:Email ve {configSection}:Password konfigürasyonu eksik. " +
                    $"Geliştirme ortamında 'dotnet user-secrets set {configSection}:Password <şifre>' ile ayarlayın.");
            }
            return;
        }

        if (context.Users.Any(u => u.Email == email))
            return;

        var admin = new SurveyApp.Core.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            IsAdmin = true
        };

        context.Users.Add(admin);
        context.SaveChanges();

        Console.WriteLine($"Seed: Admin kullanıcı oluşturuldu -> {email}");
    }

    SeedAdmin("AdminSeed", required: true);
}

app.Run();