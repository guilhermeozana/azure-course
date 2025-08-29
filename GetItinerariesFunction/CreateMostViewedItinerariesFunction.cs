using GetItinerariesFunction.API.Itineraries.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GetItinerariesFunction.API.Itineraries;

public class CreateMostViewedItinerariesFunction
{
    private readonly ILogger<CreateMostViewedItinerariesFunction> _logger;

    public CreateMostViewedItinerariesFunction(ILogger<CreateMostViewedItinerariesFunction> logger)
    {
        _logger = logger;
    }

    [Function("CreateMostViewedItinerariesFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post",
        Route = "mostvieweditineraries")] HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var itineraries = JsonSerializer.Deserialize<List<ItineraryDto>>(requestBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));


        return new OkObjectResult("Most viewed itineraries have been created for the current user.");
    }
}