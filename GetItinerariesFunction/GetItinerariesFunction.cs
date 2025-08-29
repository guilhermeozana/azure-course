using GetItinerariesFunction.API.Itineraries.DbContexts;
using GetItinerariesFunction.API.Itineraries.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetItinerariesFunction;

public class GetItinerariesFunction
{
    private readonly ILogger<GetItinerariesFunction> _logger;
    private readonly TravelInspirationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public GetItinerariesFunction(ILogger<GetItinerariesFunction> logger, TravelInspirationDbContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [Function("GetItinerariesFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "itineraries")] HttpRequest req)
    {
        string? searchForValue = req.Query["SearchFor"];

        if (!int.TryParse(_configuration["MaximumAmountOfItinerariesToReturn"],
            out int maximumAmountOfItinerariesToReturn) || maximumAmountOfItinerariesToReturn <= 0)
        {
            throw new Exception("MaximumAmountOfItinerariesToReturn must be a positive integer");
        }


        var itineraryEntities = await _dbContext.Itineraries
            .Where(i => string.IsNullOrWhiteSpace(searchForValue) ||
                (!string.IsNullOrEmpty(i.Name) && i.Name.Contains(searchForValue)) ||
                (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(searchForValue)))
            .OrderBy(i => i.Name)
            .Take(maximumAmountOfItinerariesToReturn)
            .ToListAsync();

        var itineraryDtos = itineraryEntities.Select(i => new ItineraryDto
        {
            Id = i.Id,
            Description = i.Description,
            Name = i.Name,
            UserId = i.UserId
        });

        return new OkObjectResult(itineraryDtos);

    }
}