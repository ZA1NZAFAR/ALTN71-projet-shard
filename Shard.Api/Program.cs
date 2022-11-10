using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shard.Api.Services;
using Shard.Shared.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ICelestialService, CelestialService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IClock, SystemClock>();

// json return serialization to camelCase
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Shard.Api
{
    public partial class Program
    {
    }
}