using SyncChat.API.Features.Weather.GetWeather;
using SyncChat.API.Features.Weather.SaveWeather;
using SyncChat.API.Host;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Sender.Internal;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IQuerySender, QuerySender>();
builder.Services.AddScoped<ICommandSender, CommandSender>();

builder.Services.RegisterRequestHandlers();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecasts", async (IQuerySender sender) =>
{
    await sender.SendAsync(new GetWeatherQuery());
})
.WithName("GetWeatherForecast");

app.MapPost("/weatherforecasts", async (ICommandSender sender) =>
{
    await sender.SendAsync(new SaveWeatherCommand());
})
.WithName("SaveWeatherForecast");

app.UseCors(builder => builder.AllowAnyOrigin());

app.Run();



internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}