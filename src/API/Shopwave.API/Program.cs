var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


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
