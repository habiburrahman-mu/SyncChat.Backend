using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SyncChat.API.Host;
using SyncChat.API.Infrastructure;
using SyncChat.API.Infrastructure.Exceptions;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Routing;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Sender.Internal;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Options pattern
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));

builder.Services.AddOpenApi();

builder.Services.AddSingleton<MockDb>();
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<IQuerySender, QuerySender>();
builder.Services.AddScoped<ICommandSender, CommandSender>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.RegisterRequestHandlers();

builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.RegisterEndpoints(Assembly.GetExecutingAssembly());

app.Use(async (context, next) =>
{
    await Task.Delay(1000); // 1 second delay
    await next();
});

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    // log or handle error properly
    Console.WriteLine($"Database migration failed: {ex.Message}");
}

app.Run();