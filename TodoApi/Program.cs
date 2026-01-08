using Microsoft.EntityFrameworkCore;
using TodoApi;
using TodoApi.Todos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.RegisterTodoItemsEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(exceptionHandlerApp
        => exceptionHandlerApp.Run(async context => await Results.Problem().ExecuteAsync(context)));
}

app.UseStatusCodePages(async statusCodeContext
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

app.MapGet("/exception", () =>
{
    throw new InvalidOperationException("Sample Exception");
});

app.Run();