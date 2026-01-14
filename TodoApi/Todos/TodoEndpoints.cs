using Microsoft.EntityFrameworkCore;

namespace TodoApi.Todos
{
    public static class TodoEndpoints
    {
        public static void RegisterTodoItemsEndpoints(this WebApplication app)
        {
            var todoItems = app.MapGroup("/todoitems");

            todoItems.MapGet("/", GetAllTodos);
            todoItems.MapGet("/complete", GetCompleteTodos);
            todoItems.MapGet("/{id}", GetTodo);
            todoItems.MapPost("/", CreateTodo);
            todoItems.MapPut("/{id}", UpdateTodo);
            todoItems.MapPatch("/{id}", PatchTodo);
            todoItems.MapDelete("/{id}", DeleteTodo);

            static async Task<IResult> GetAllTodos(TodoDb db)
            {
                return TypedResults.Ok(await db.Todos.Select(todo => new TodoItemDTO(todo)).ToArrayAsync());
            }

            static async Task<IResult> GetCompleteTodos(TodoDb db)
            {
                return TypedResults.Ok(await db.Todos.Select(todo => new TodoItemDTO(todo)).Where(t => t.IsComplete).ToListAsync());
            }

            static async Task<IResult> GetTodo(int id, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);
                return todo is not null
                        ? TypedResults.Ok(new TodoItemDTO(todo))
                        : TypedResults.NotFound();
            }

            static async Task<IResult> CreateTodo(TodoItemDTO dto, TodoDb db)
            {
                if (dto.Name is null)
                    return TypedResults.BadRequest("Name is required.");

                var todoItem = new Todo
                {
                    IsComplete = dto.IsComplete,
                    Name = dto.Name
                };

                db.Todos.Add(todoItem);
                await db.SaveChangesAsync();

                var todoItemDto = new TodoItemDTO(todoItem);

                return TypedResults.Created($"/todoitems/{todoItemDto.Id}", todoItemDto);
            }

            static async Task<IResult> UpdateTodo(int id, TodoItemDTO inputTodo, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);

                if (todo is null) return TypedResults.NotFound();

                todo.Name = inputTodo.Name;
                todo.IsComplete = inputTodo.IsComplete;

                await db.SaveChangesAsync();

                return TypedResults.NoContent();
            }

            static async Task<IResult> DeleteTodo(int id, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);
                
                if (todo is null)
                {
                    return TypedResults.NotFound();
                }
                
                db.Todos.Remove(todo);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            static async Task<IResult> PatchTodo(int id, TodoPatch patch, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);
                if (todo is null) return TypedResults.NotFound();

                if (patch.Name is not null)
                    todo.Name = patch.Name;

                if (patch.IsComplete.HasValue)
                    todo.IsComplete = patch.IsComplete.Value;

                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }
        }
    }
}
