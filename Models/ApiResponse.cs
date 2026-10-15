using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public sealed class ApiResponse(int statusCode, string body) : IActionResult
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ApiResponse Ok(string message) => new(200, message);

    public static ApiResponse Json<T>(T value) => new(200, JsonSerializer.Serialize(value, JsonOptions));

    public static ApiResponse NotFound(string message = "User not found") => new(404, message);

    public static ApiResponse InternalError() => new(500, "Internal error");

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = statusCode;
        response.ContentType = statusCode == 200 ? "application/json" : null;
        await response.WriteAsync(body, context.HttpContext.RequestAborted);
    }
}
