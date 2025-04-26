using Google.Cloud.Firestore;

namespace Todos.Backend.WebApi.Models;

[FirestoreData]
public class Todo
{
    [FirestoreDocumentId]
    public string? Id { get; set; }
    
    [FirestoreProperty]
    public string Title { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Description { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public bool IsCompleted { get; set; }
    
    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TodoDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class UpdateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class CreateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 