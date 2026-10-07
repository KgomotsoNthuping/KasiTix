using FluentValidation;
using KasiTix.Api.Common;
using Scalar.AspNetCore;
using KasiTix.Domain.Repositories;
using KasiTix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using KasiTix.Infrastructure.Data;
using KasiTix.Api.Services;

var builder = WebApplication.CreateBuilder(args);
// Keeps "Async" in action names, so CreatedAtAction(nameof(GetByIdAsync), ...) finds its route.
builder.Services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("KasiTix") ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext< KasiTixDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
        });
});

builder.Services.AddScoped<
    IEventRepository,
    EfEventRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    EfOrderRepository>();

builder.Services.AddScoped<
    IEventService,
    EventService>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

// TODO (Week 5):
// - read the connection string from user-secrets, and fail loudly at startup if it's missing
// - AddDbContext with UseNpgsql + EnableRetryOnFailure
// - register every repository and service, each with a lifetime you can defend
var app = builder.Build();

// TODO (Week 5): apply pending migrations on startup.
if (app.Environment.IsDevelopment())
{
 app.MapOpenApi();
 app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.MapControllers();

app.Run();

// Lets KasiTix.Tests boot this app with WebApplicationFactory<Program>.
public partial class Program { }