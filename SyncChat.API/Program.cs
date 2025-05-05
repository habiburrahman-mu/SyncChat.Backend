using Scalar.AspNetCore;
using SyncChat.API.Host;
using SyncChat.API.Routing;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Sender.Internal;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IQuerySender, QuerySender>();
builder.Services.AddScoped<ICommandSender, CommandSender>();

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

app.RegisterEndpoints(Assembly.GetExecutingAssembly());

app.UseCors(builder => builder.AllowAnyOrigin());

app.Run();