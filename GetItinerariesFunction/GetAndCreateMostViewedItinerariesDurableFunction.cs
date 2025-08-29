using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.DurableTask.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace GetItinerariesFunction.API.Itineraries;

public static class GetAndCreateMostViewedItinerariesDurableFunction
{
    [Function(nameof(GetAndCreateMostViewedItinerariesDurableFunction))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var hostAddress = context.GetInput<string>();

        var httpRetryOptions = new HttpRetryOptions()
        {
            MaxNumberOfAttempts = 3,
            FirstRetryInterval = TimeSpan.FromSeconds(5)
        };

        var request = new DurableHttpRequest(HttpMethod.Get, new Uri($"{hostAddress}/itineraries"))
        {
            HttpRetryOptions = httpRetryOptions
        };
        var getItinerariesResponse = await context.CallHttpAsync(request);

        if (getItinerariesResponse.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return ["Failed to get itineraries."];
        }

        var createMostViewedItinerariesResponse = await context.CallHttpAsync(
            HttpMethod.Post,
            new Uri($"{hostAddress}/mostvieweditineraries"),
            getItinerariesResponse.Content ?? "",
            retryOptions: httpRetryOptions);

        if (createMostViewedItinerariesResponse.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return ["Failed to create most viewed itineraries."];
        }

        return ["Most viewed itineraries created."];
    }

    [Function(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("SayHello");
        logger.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }

    [Function("GetAndCreateMostViewedItinerariesDurableFunction_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "generatemostvieweditineraries")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("GetAndCreateMostViewedItinerariesDurableFunction_HttpStart");

        // Get host address
        string hostAddress = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}" +
                             $"{req.Url.LocalPath.Substring(0, req.Url.LocalPath.IndexOf("generatehostviewedititineraries") - 1)}";


        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(GetAndCreateMostViewedItinerariesDurableFunction), hostAddress);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}