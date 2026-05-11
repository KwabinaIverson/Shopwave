using Shopwave.Shared;
using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Mediator;

using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.RegisterUser;
using Shopwave.Modules.Identity.Application.Commands.LoginUser;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

using Shopwave.Modules.Identity.Infrastructure.Security;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scrutor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ── SERVICES ─────────────────────────────

// mediator
builder.Services.AddScoped<IMediator, Mediator>();

// identity services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// auto-register ALL handlers
builder.Services.Scan(scan => scan
    .FromApplicationDependencies()

    // command handlers (no result)
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime()

    // command handlers (with result)
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime()

    // query handlers
    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime()

    // domain event handlers
    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

//jwttoken
builder.Services.AddScoped<ITokenService, TokenService>();

// unit of work
builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<IdentityDbContext>());

// database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Services
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shopwave API",
        Version = "v1"
    });
});

// ── APP ─────────────────────────────

var app = builder.Build();

// swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

// test endpoint
app.MapGet("/", () => "shopwave api running");

// register user endpoint
app.MapPost("/users/register", async (
    RegisterUserCommand command,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send<RegisterUserCommand, Result<Guid>>(command, ct);

    return result.IsSuccess
        ? Results.Created($"/users/{result.Value}", result.Value)
        : Results.BadRequest(result.Error);
}).RequireAuthorization();

app.MapPost("/users/login", async (
    LoginUserCommand command,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send<LoginUserCommand, Result<LoginUserResponse>>(command, ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
});

app.MapPost("/auth/refresh", async (
    RefreshTokenCommand command,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send<RefreshTokenCommand, Result<RefreshTokenResponse>>(command, ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
});

app.Run();