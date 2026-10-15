using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

[ApiController]
public class UserController(UserService userService, ILogger<UserController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [HttpPost("/users")]
    public async Task<IActionResult> CreateUser()
    {
        var user = await ReadUser();
        if (user is null)
            return ApiResponse.InternalError();

        userService.SaveUser(user);
        return ApiResponse.Ok("User created");
    }

    [HttpGet("/users")]
    public IActionResult GetAllUsers() => ApiResponse.Json(userService.GetAllUsers());

    [HttpGet("/users/{id}")]
    public IActionResult GetUserById(int id)
    {
        try
        {
            var user = userService.GetUserById(id);
            return user is null ? ApiResponse.NotFound() : ApiResponse.Json(user);
        }
        catch (PostgresException exception)
        {
            logger.LogError(exception, "User lookup failed for ID {UserId}", id);
            return ApiResponse.NotFound();
        }
    }

    [HttpPut("/users/{id}")]
    public async Task<IActionResult> UpdateUser(int id)
    {
        var user = await ReadUser();
        if (user is null)
            return ApiResponse.InternalError();

        userService.UpdateUser(user, id);
        return ApiResponse.Ok("User updated");
    }

    [HttpDelete("/users/{id}")]
    public IActionResult DeleteUser(int id)
    {
        var success = userService.DeleteUserById(id);
        return success ? ApiResponse.Ok("User deleted") : ApiResponse.NotFound();
    }

    private async Task<UserRequest?> ReadUser()
    {
        try
        {
            var user = await JsonSerializer.DeserializeAsync<UserRequest>(
                Request.Body, RequestJsonOptions, HttpContext.RequestAborted);
            return user is { Name: not null, Email: not null } ? user : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
