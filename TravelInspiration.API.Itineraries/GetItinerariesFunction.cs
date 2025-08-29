using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TravelInspiration.API.Itineraries
{
    public class GetItinerariesFunction
    {
        private readonly ILogger<GetItinerariesFunction> _logger;

        public GetItinerariesFunction(ILogger<GetItinerariesFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetItinerariesFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "itineraries")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
