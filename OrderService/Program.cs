using System.Net;
using System.Net.Http.Json;
using OrderService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<List<Order>>();

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");

    client.Timeout = TimeSpan.FromSeconds(2);
});

var app = builder.Build();

app.MapPost("/orders", CreateOrder);

app.MapGet("/orders/{id:int}", GetOrder);

app.Run();

static async Task<IResult> CreateOrder(
    CreateOrderRequest request,
    List<Order> orders,
    IHttpClientFactory httpClientFactory)
{
    if (string.IsNullOrWhiteSpace(request.Product))
    {
        return Results.BadRequest(new
        {
            message = "Product is required"
        });
    }

    if (request.Quantity <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Quantity must be greater than zero"
        });
    }

    var userResult = await GetUserFromUserService(
        request.UserId,
        httpClientFactory);

    if (userResult.Error is not null)
    {
        return userResult.Error;
    }

    var newId = orders.Count == 0
        ? 1
        : orders.Max(order => order.Id) + 1;

    var order = new Order
    {
        Id = newId,
        UserId = request.UserId,
        Product = request.Product,
        Quantity = request.Quantity
    };

    orders.Add(order);

    return Results.Created(
        $"/orders/{order.Id}",
        new
        {
            order,
            user = userResult.User
        });
}

static async Task<IResult> GetOrder(
    int id,
    List<Order> orders,
    IHttpClientFactory httpClientFactory)
{
    var order = orders.FirstOrDefault(order => order.Id == id);

    if (order is null)
    {
        return Results.NotFound(new
        {
            message = $"Order with ID {id} was not found"
        });
    }

    var userResult = await GetUserFromUserService(
        order.UserId,
        httpClientFactory);

    if (userResult.Error is not null)
    {
        return userResult.Error;
    }

    return Results.Ok(new
    {
        order,
        user = userResult.User
    });
}

static async Task<UserServiceResult> GetUserFromUserService(
    int userId,
    IHttpClientFactory httpClientFactory)
{
    var client = httpClientFactory.CreateClient("UserService");

    try
    {
        var response = await client.GetAsync($"/users/{userId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new UserServiceResult
            {
                Error = Results.BadRequest(new
                {
                    message = $"User with ID {userId} does not exist"
                })
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            return new UserServiceResult
            {
                Error = Results.Json(
                    new
                    {
                        message = "User service returned an error"
                    },
                    statusCode: 503)
            };
        }

        var user =
            await response.Content.ReadFromJsonAsync<UserResponse>();

        if (user is null)
        {
            return new UserServiceResult
            {
                Error = Results.Json(
                    new
                    {
                        message = "Invalid response from UserService"
                    },
                    statusCode: 503)
            };
        }

        return new UserServiceResult
        {
            User = user
        };
    }
    catch (HttpRequestException)
    {
        return new UserServiceResult
        {
            Error = Results.Json(
                new
                {
                    message = "UserService is unavailable"
                },
                statusCode: 503)
        };
    }
    catch (TaskCanceledException)
    {
        return new UserServiceResult
        {
            Error = Results.Json(
                new
                {
                    message = "UserService is unavailable"
                },
                statusCode: 503)
        };
    }
}

public class UserServiceResult
{
    public UserResponse? User { get; set; }

    public IResult? Error { get; set; }
}