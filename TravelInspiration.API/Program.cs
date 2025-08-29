using TravelInspiration.API;
using TravelInspiration.API.Shared.Slices;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.RegisterApplicationServices();
builder.Services.RegisterPersistenceServices(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}
app.UseStatusCodePages();

app.MapSliceEndpoints();

app.Run();