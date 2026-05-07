using Shopwave.Shared;
using Shopwave.Shared.Results;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Mediator;

using Shopwave.Modules.Identity.Application.Abstractions;
using Shopwave.Modules.Identity.Application.Commands.RegisterUser;

using Shopwave.Modules.Identity.Infrastructure.Security;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

// ── SERVICES ─────────────────────────────

// mediator
builder.Services.AddScoped<IMediator, Mediator>();

// identity services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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

// unit of work
builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<IdentityDbContext>());

// database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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
        ? Results.CreatedAtRoute("GetUserById", new { id = result.Value }, result.Value)
        : Results.BadRequest(result.Error);
});

app.Run();