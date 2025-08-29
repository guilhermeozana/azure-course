using Azure.Core;
using Azure.Identity;
using GetItinerariesFunction.API.Itineraries.DbContexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
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
