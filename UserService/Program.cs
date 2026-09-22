using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<List<User>>();

var app = builder.Build();

app.MapPost("/users", CreateUser);

app.MapGet("/users/{id:int}", GetUser);

app.Run();

static IResult CreateUser(
    CreateUserRequest request,
    List<User> users)
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new
        {
            message = "Name and email are required"
        });
    }

    var newId = users.Count == 0
        ? 1
        : users.Max(user => user.Id) + 1;

    var user = new User
    {
        Id = newId,
        Name = request.Name,
        Email = request.Email
    };

    users.Add(user);

    return Results.Created($"/users/{user.Id}", user);
}

static IResult GetUser(
    int id,
    List<User> users)
{
    var user = users.FirstOrDefault(user => user.Id == id);

    if (user is null)
    {
        return Results.NotFound(new
        {
            message = $"User with ID {id} was not found"
        });
    }

    return Results.Ok(user);
}