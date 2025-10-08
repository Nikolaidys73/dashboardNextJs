using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configurar JSON
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS para el frontend Astro
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL de tu frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Activar CORS
app.UseCors("AllowLocal");

// ✅ Activar Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Lista en memoria de todos (CRUD)
var todos = new List<Todo>
{
    new Todo { Id = 1, Title = "Next Js", Completed = false },
    new Todo { Id = 2, Title = "Conectando con .NET", Completed = true }
};

// 🔹 CRUD Endpoints

// GET: listar todos
app.MapGet("/api/todos", () => todos)
   .WithName("GetTodos");

// GET: obtener uno
app.MapGet("/api/todos/{id:int}", (int id) =>
    todos.FirstOrDefault(t => t.Id == id) is Todo todo
        ? Results.Ok(todo)
        : Results.NotFound())
    .WithName("GetTodo");

// POST: crear
app.MapPost("/api/todos", (Todo todo) =>
{
    todo.Id = todos.Any() ? todos.Max(t => t.Id) + 1 : 1;
    todos.Add(todo);
    return Results.Created($"/api/todos/{todo.Id}", todo);
})
.WithName("CreateTodo");

// PUT: actualizar
app.MapPut("/api/todos/{id:int}", (int id, Todo updated) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();

    todo.Title = updated.Title;
    todo.Completed = updated.Completed;
    return Results.Ok(todo);
})
.WithName("UpdateTodo");

// DELETE: eliminar
app.MapDelete("/api/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();

    todos.Remove(todo);
    return Results.NoContent();
})
.WithName("DeleteTodo");

app.Run();

// 🔹 Modelo Todo
record Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool Completed { get; set; }
}
