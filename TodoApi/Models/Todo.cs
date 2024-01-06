using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
namespace TodoApi.Models;
public class Todo
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public string? Icon { get; set; }
}


public static class TodoEndpoints
{
	public static void MapTodoEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Todo").WithTags(nameof(Todo));

        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            return await db.Todos.ToListAsync();
        })
        .WithName("GetAllTodos")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (int id, ApplicationDbContext db) =>
        {
            return await db.Todos.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Todo model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetTodoById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Todo todo, ApplicationDbContext db) =>
        {
            var affected = await db.Todos
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.Id, todo.Id)
                  .SetProperty(m => m.Name, todo.Name)
                  .SetProperty(m => m.IsCompleted, todo.IsCompleted)
                  .SetProperty(m => m.Icon, todo.Icon)
                  );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateTodo")
        .WithOpenApi();

        group.MapPost("/", async (Todo todo, ApplicationDbContext db) =>
        {
            db.Todos.Add(todo);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Todo/{todo.Id}",todo);
        })
        .WithName("CreateTodo")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ApplicationDbContext db) =>
        {
            var affected = await db.Todos
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteTodo")
        .WithOpenApi();
    }
}