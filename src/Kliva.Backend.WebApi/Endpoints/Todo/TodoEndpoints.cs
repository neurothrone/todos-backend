using Kliva.Backend.WebApi.Models;
using Kliva.Backend.WebApi.Services;
using System.Security.Claims;

namespace Kliva.Backend.WebApi.Endpoints.Todo;

/// <summary>
/// Endpoint mapping for Todo API routes
/// </summary>
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/todos")
            .RequireAuthorization();

        // Todo endpoints
        group.MapGet("/", TodoHandlers.GetUserTodosAsync)
            .WithSummary("Get all todos for the authenticated user");
            
        group.MapGet("/{id}", TodoHandlers.GetTodoByIdAsync)
            .WithSummary("Get a specific todo by ID");
            
        group.MapPost("/", TodoHandlers.CreateTodoAsync)
            .WithSummary("Create a new todo");
            
        group.MapPut("/{id}", TodoHandlers.UpdateTodoAsync)
            .WithSummary("Update an existing todo");
            
        group.MapDelete("/{id}", TodoHandlers.DeleteTodoAsync)
            .WithSummary("Delete a todo");
            
        group.MapPatch("/{id}/toggle", TodoHandlers.ToggleTodoCompletionAsync)
            .WithSummary("Toggle the completion status of a todo");
    }
}

/// <summary>
/// Handlers for Todo-related endpoints
/// </summary>
public static class TodoHandlers
{
    /// <summary>
    /// Gets all todos for the authenticated user
    /// </summary>
    public static async Task<IResult> GetUserTodosAsync(ClaimsPrincipal user, ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.GetUserTodosAsync(userId);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => TypedResults.Problem(error, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    /// <summary>
    /// Gets a specific todo by ID
    /// </summary>
    public static async Task<IResult> GetTodoByIdAsync(string id, ClaimsPrincipal user, ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.GetTodoByIdAsync(userId, id);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => error == "Todo not found" 
                ? TypedResults.NotFound(error) 
                : TypedResults.BadRequest(error)
        );
    }

    /// <summary>
    /// Creates a new todo
    /// </summary>
    public static async Task<IResult> CreateTodoAsync(
        CreateTodoDto todoDto, 
        ClaimsPrincipal user, 
        ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.CreateTodoAsync(userId, todoDto);
        return result.Match<IResult>(
            onSuccess: todo => TypedResults.Created($"/api/v1/todos/{todo.Id}", todo),
            onFailure: TypedResults.BadRequest
        );
    }

    /// <summary>
    /// Updates an existing todo
    /// </summary>
    public static async Task<IResult> UpdateTodoAsync(
        string id, 
        TodoDto todoDto, 
        ClaimsPrincipal user, 
        ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.UpdateTodoAsync(userId, id, todoDto);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => error == "Todo not found" 
                ? TypedResults.NotFound(error) 
                : TypedResults.BadRequest(error)
        );
    }

    /// <summary>
    /// Deletes a todo
    /// </summary>
    public static async Task<IResult> DeleteTodoAsync(
        string id, 
        ClaimsPrincipal user, 
        ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.DeleteTodoAsync(userId, id);
        return result.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => error == "Todo not found" 
                ? TypedResults.NotFound(error) 
                : TypedResults.BadRequest(error)
        );
    }

    /// <summary>
    /// Toggles the completion status of a todo
    /// </summary>
    public static async Task<IResult> ToggleTodoCompletionAsync(
        string id, 
        ClaimsPrincipal user, 
        ITodoService todoService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await todoService.ToggleTodoCompletionAsync(userId, id);
        return result.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => error == "Todo not found" 
                ? TypedResults.NotFound(error) 
                : TypedResults.BadRequest(error)
        );
    }
} 