using Google.Cloud.Firestore;
using Todos.Backend.WebApi.Models;

namespace Todos.Backend.WebApi.Services;

public interface ITodoService
{
    Task<Result<List<Todo>>> GetUserTodosAsync(string userId);
    Task<Result<Todo>> GetTodoByIdAsync(string userId, string todoId);
    Task<Result<Todo>> CreateTodoAsync(string userId, CreateTodoDto todoDto);
    Task<Result<Todo>> UpdateTodoAsync(string userId, string todoId, UpdateTodoDto updateTodoDto);
    Task<Result<bool>> DeleteTodoAsync(string userId, string todoId);
    Task<Result<bool>> ToggleTodoCompletionAsync(string userId, string todoId);
}

public class TodoService : ITodoService
{
    private readonly IFirebaseService _firebaseService;
    private readonly CollectionReference _todosCollection;

    public TodoService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
        _todosCollection = _firebaseService
            .GetFirestoreDb()
            .Collection("todos");
    }

    public async Task<Result<List<Todo>>> GetUserTodosAsync(string userId)
    {
        try
        {
            var snapshot = await _todosCollection
                .WhereEqualTo("UserId", userId)
                .OrderByDescending("CreatedAt")
                .GetSnapshotAsync();

            var todos = snapshot.Documents
                .Select(doc => doc.ConvertTo<Todo>())
                .ToList();

            return Result<List<Todo>>.Success(todos);
        }
        catch (Exception ex)
        {
            return Result<List<Todo>>.Failure($"Failed to get todos: {ex.Message}");
        }
    }

    public async Task<Result<Todo>> GetTodoByIdAsync(string userId, string todoId)
    {
        try
        {
            var todoDoc = await _todosCollection.Document(todoId).GetSnapshotAsync();

            if (!todoDoc.Exists)
            {
                return Result<Todo>.Failure("Todo not found");
            }

            var todo = todoDoc.ConvertTo<Todo>();

            // Security check: make sure the todo belongs to the user
            if (todo.UserId != userId)
            {
                return Result<Todo>.Failure("Unauthorized access to todo");
            }

            return Result<Todo>.Success(todo);
        }
        catch (Exception ex)
        {
            return Result<Todo>.Failure($"Failed to get todo: {ex.Message}");
        }
    }

    public async Task<Result<Todo>> CreateTodoAsync(string userId, CreateTodoDto todoDto)
    {
        try
        {
            var todo = new Todo
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                IsCompleted = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var docRef = await _todosCollection.AddAsync(todo);
            todo.Id = docRef.Id;

            return Result<Todo>.Success(todo);
        }
        catch (Exception ex)
        {
            return Result<Todo>.Failure($"Failed to create todo: {ex.Message}");
        }
    }

    public async Task<Result<Todo>> UpdateTodoAsync(string userId, string todoId, UpdateTodoDto todoDto)
    {
        try
        {
            // Get the current todo to check ownership
            var result = await GetTodoByIdAsync(userId, todoId);

            if (result.Match(
                    onSuccess: _ => true,
                    onFailure: _ => false
                ) == false)
            {
                return result; // Return the failure result
            }

            var todoRef = _todosCollection.Document(todoId);

            await todoRef.UpdateAsync(new Dictionary<string, object>
            {
                { "Title", todoDto.Title },
                { "Description", todoDto.Description },
                { "IsCompleted", todoDto.IsCompleted }
            });

            var updatedTodo = await todoRef.GetSnapshotAsync();
            return Result<Todo>.Success(updatedTodo.ConvertTo<Todo>());
        }
        catch (Exception ex)
        {
            return Result<Todo>.Failure($"Failed to update todo: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTodoAsync(string userId, string todoId)
    {
        try
        {
            // Get the current todo to check ownership
            var result = await GetTodoByIdAsync(userId, todoId);

            if (result.Match(
                    onSuccess: _ => true,
                    onFailure: _ => false
                ) == false)
            {
                return Result<bool>.Failure(result.Match(
                    onSuccess: _ => string.Empty,
                    onFailure: error => error
                ));
            }

            await _todosCollection.Document(todoId).DeleteAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete todo: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ToggleTodoCompletionAsync(string userId, string todoId)
    {
        try
        {
            // Get the current todo to check ownership and current state
            var result = await GetTodoByIdAsync(userId, todoId);

            if (result.Match(
                    onSuccess: _ => true,
                    onFailure: _ => false
                ) == false)
            {
                return Result<bool>.Failure(result.Match(
                    onSuccess: _ => string.Empty,
                    onFailure: error => error
                ));
            }

            var todo = result.Match(
                onSuccess: todo => todo,
                onFailure: _ => null
            );

            if (todo == null)
            {
                return Result<bool>.Failure("Todo not found");
            }

            var todoRef = _todosCollection.Document(todoId);

            await todoRef.UpdateAsync("IsCompleted", !todo.IsCompleted);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to toggle todo completion: {ex.Message}");
        }
    }
}