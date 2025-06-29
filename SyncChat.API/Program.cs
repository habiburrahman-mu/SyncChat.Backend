using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection jwtSection = builder.Configuration.GetSection("JWT");
builder.Services.Configure<JWTSettings>(jwtSection);
JWTSettings jwtSettingsInstance = jwtSection.Get<JWTSettings>()!;
IOptions<JWTSettings> jwtOptions = Options.Create(jwtSettingsInstance);

builder.Services.AddSingleton<MockDb>();
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<IQuerySender, QuerySender>();
builder.Services.AddScoped<ICommandSender, CommandSender>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration, jwtOptions);

builder.Services.RegisterRequestHandlers();

builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication().UseAuthorization();

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