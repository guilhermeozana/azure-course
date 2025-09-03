using Azure.Core;
using Azure.Identity;
using GetItinerariesFunction.API.Itineraries.DbContexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
{
    return new OpenApiConfigurationOptions()
    {
        Info = new Microsoft.OpenApi.Models.OpenApiInfo()
        {
            Title = "Travel inspiration function app endpoints",
            Description = "All travel inspiration API endpoints the have been migrated to a function app."
        }
    };
});

var credential = new DefaultAzureCredential();

// Get a token to access Azure SQL
var accessTokenResponse = credential.GetToken(
    new TokenRequestContext(["https://database.windows.net/.default"]));

var sqlConnection = new SqlConnection(
    builder.Configuration["TravelInspirationDbConnection"])
{
    AccessToken = accessTokenResponse.Token
};

builder.Services.AddDbContext<TravelInspirationDbContext>(options =>
    options.UseSqlServer(
        sqlConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Build().Run();
