using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scrutor;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using Shopwave.Shared;
using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Mediator;
using Shopwave.Shared.Events.Identity;

using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;
using Shopwave.Modules.Identity.Infrastructure.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Security;
using Shopwave.Modules.Identity.Application.Commands.RegisterUser;
using Shopwave.Modules.Identity.Application.Commands.LoginUser;
using Shopwave.Modules.Identity.Application.Commands.LoginUser.Responses;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken;
using Shopwave.Modules.Identity.Application.Commands.RefreshToken.Responses;

using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Application.Commands.RegisterStore;
using Shopwave.Modules.Stores.Application.Commands.DocumentVerification;
using Shopwave.Modules.Stores.Application.Commands.AddStorePayoutMethod;
using Shopwave.Modules.Stores.Domain.Repositories;
using Shopwave.Modules.Stores.Infrastructure.Persistence;
using Shopwave.Modules.Stores.Infrastructure.Repositories;
using Shopwave.Modules.Stores.Application.EventHandlers;

// API Endpoints
using Shopwave.API.Endpoints;
using Shopwave.API.Endpoints.Stores; 

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

//
// ─────────────────────────────────────────────
// Cloudflare R2 Configuration
// ─────────────────────────────────────────────
//
var accountId = builder.Configuration["CloudflareR2:AccountId"];
var accessKey = builder.Configuration["CloudflareR2:AccessKey"];
var secretKey = builder.Configuration["CloudflareR2:SecretKey"];

var s3Config = new AmazonS3Config
{
    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
    AuthenticationRegion = "auto"
};

var s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

builder.Services.AddSingleton<IAmazonS3>(s3Client);
builder.Services.AddSingleton<IObjectStorageService, CloudflareR2StorageService>();


//
// ─────────────────────────────────────────────
// Database Registration
// ─────────────────────────────────────────────
//
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseNpgsql(connectionString)
);


//
// ─────────────────────────────────────────────
// Core Services & Unit of Work
// ─────────────────────────────────────────────
//
builder.Services.AddScoped<IMediator, Mediator>();

builder.Services.AddScoped<IIdentityUnitOfWork>(sp =>
    sp.GetRequiredService<IdentityDbContext>()
);

builder.Services.AddScoped<IStoreUnitOfWork>(sp =>
    sp.GetRequiredService<StoreDbContext>()
);


//
// ─────────────────────────────────────────────
// Identity & Store Services (Repositories / Auth)
// ─────────────────────────────────────────────
//
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<ISellerValidationService, SellerValidationService>();


//
// ─────────────────────────────────────────────
// Manual CQRS Handler Registrations
// ─────────────────────────────────────────────
//

// Identity Handlers
builder.Services.AddScoped<
    ICommandHandler<RegisterUserCommand, Result<Guid>>, 
    RegisterUserCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<LoginUserCommand, Result<LoginUserResponse>>, 
    LoginUserCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>, 
    RefreshTokenCommandHandler>();


// Store Handlers
builder.Services.AddScoped<
    ICommandHandler<RegisterStoreCommand, Result<Guid>>, 
    RegisterStoreCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<AddStorePayoutMethodCommand, Result<Guid>>, 
    AddStorePayoutMethodCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<SubmitVerificationBundleCommand, Result<Guid>>, 
    SubmitVerificationBundleCommandHandler>();

builder.Services.AddScoped<ISellerReferenceRepository, SellerReferenceRepository>();


//
// ─────────────────────────────────────────────
// Event Handler Registrations (Automated)
// ─────────────────────────────────────────────
//
// Automatically finds every class in the Stores module that implements IDomainEventHandler
var infrastructureAssembly = typeof(OnSellerCreatedHandler).Assembly;

var eventHandlers = infrastructureAssembly.GetTypes()
    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
    .Where(t => !t.IsAbstract && !t.IsInterface);

foreach (var handler in eventHandlers)
{
    var interfaces = handler.GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

    foreach (var @interface in interfaces)
    {
        builder.Services.AddTransient(@interface, handler);
    }
}

//
// ─────────────────────────────────────────────
// Authentication / Authorization
// ─────────────────────────────────────────────
//
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenHandlers.Clear();
        options.TokenHandlers.Add(new JwtSecurityTokenHandler());

        // 1. Declare variables
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

        // 2. Pass those variables INTO the object initializer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey!)
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
               var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
              if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
              {
                  context.Token = authHeader.Substring("Bearer ".Length).Trim();
              }
    
              return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();


//
// ─────────────────────────────────────────────
// Swagger Configuration
// ─────────────────────────────────────────────
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shopwave API",
        Version = "v1"
    });

    // 1. Add the Security Definition (Creates the "Authorize" padlock button)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOi...\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // 2. Add the Security Requirement (Tells Swagger to send the token with every request)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


//
// ─────────────────────────────────────────────
// Build App & Middleware
// ─────────────────────────────────────────────
//
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


//
// ─────────────────────────────────────────────
// Identity Endpoints
// ─────────────────────────────────────────────
//
app.MapGet("/", () => "shopwave api running");

app.MapPost("/sellers/register", async (RegisterUserCommand command, IMediator mediator, CancellationToken ct) =>
{
    var secureCommand = command with { Role = UserRole.Seller };
    var result = await mediator.Send<RegisterUserCommand, Result<Guid>>(secureCommand, ct);
    
    return result.IsSuccess
        ? Results.Created($"/users/{result.Value}", result.Value)
        : Results.BadRequest(result.Error);
}).AllowAnonymous();


app.MapPost("/buyers/register", async (RegisterUserCommand command, IMediator mediator, CancellationToken ct) =>
{
    var secureCommand = command with { Role = UserRole.Buyer };
    var result = await mediator.Send<RegisterUserCommand, Result<Guid>>(secureCommand, ct);
    
    return result.IsSuccess
        ? Results.Created($"/users/{result.Value}", result.Value)
        : Results.BadRequest(result.Error);
}).AllowAnonymous();


app.MapPost("/auth/login", async (LoginUserCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send<LoginUserCommand, Result<LoginUserResponse>>(command, ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
}).AllowAnonymous();


app.MapPost("/auth/refresh", async (RefreshTokenCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send<RefreshTokenCommand, Result<RefreshTokenResponse>>(command, ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Error);
}).AllowAnonymous();


//
// ─────────────────────────────────────────────
// Store Endpoints
// ─────────────────────────────────────────────
//
app.MapStoreEndpoints();
app.MapStorePayoutEndpoints();
app.MapVerificationEndpoints();
app.MapDocumentEndpoints();


//
// ─────────────────────────────────────────────
// Run Application
// ─────────────────────────────────────────────
//
app.Run();