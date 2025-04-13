using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Order.API.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Configuration.GetValue<string>("APPLICATION_NAME");
var svcName = builder.Configuration.GetValue<string>("SERVICE_NAME");

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WEB API",
        Version = "v1"
    });
});

builder.Services.AddCustomLogging(builder.Configuration, builder.Host, svcName, appName);
builder.ConfigureOpenTelemetry(svcName, appName);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OPEN API");
        c.DocumentTitle = "WEB API";
        c.DocExpansion(DocExpansion.List);
    });
}

app.UseSerilogRequestLogging(opt => { opt.IncludeQueryInRequestPath = true; });
app.UseRouting();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();
app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });

app.MapGet("/api/v1/orders", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Retrieving all orders");

        var orders = new List<OrderDto>
        {
            new(
                Id: Guid.NewGuid(),
                CustomerName: "John Doe",
                OrderDate: DateTime.UtcNow,
                TotalAmount: 100.00m
            ),
            new(
                Id: Guid.NewGuid(),
                CustomerName: "Jane Smith",
                OrderDate: DateTime.UtcNow,
                TotalAmount: 200.00m
            )
        };

        logger.LogInformation("Retrieved {OrderCount} orders.", orders.Count);

        return Task.FromResult(Results.Ok(orders));
    })
    .WithName("GetAllOrdersMinimalApi")
    .WithOpenApi();

app.Run();

public record OrderDto(
    Guid Id,
    string? CustomerName,
    DateTime OrderDate,
    decimal TotalAmount);