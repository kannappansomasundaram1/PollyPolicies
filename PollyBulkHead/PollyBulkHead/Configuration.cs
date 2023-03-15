using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using WeatherForeCastApi;

namespace AmiUpdater;

public static  class ConfigurationExtensions
{
    public static void ConfigureServices(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient<IWeatherClient, WeatherClient>(
                client =>
                {
                    client.BaseAddress = new Uri("https://localhost:7159/");
                })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(BuildBulkheadLimitAsyncPolicy(50, 20, 2));
            
        serviceCollection.AddTransient<PolicyTester>();
    }
    
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(20), 2),
                onRetryAsync: (e, ts, i, ctx) =>
                {
                    Console.WriteLine($"Bulkhead Retry: {i}");
                    return Task.CompletedTask;
                });
    }
    
    static AsyncPolicy<HttpResponseMessage> BuildBulkheadLimitAsyncPolicy(int maxParallelExecutions, int maxQueuingActions, int retryingNumber)
    {
        var asyncBulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelExecutions, maxQueuingActions,
            ctx =>
            {
                Console.WriteLine("Bulkhead Rejected");
                return Task.CompletedTask;
            });

        return asyncBulkheadPolicy;
    }

    private static AsyncRetryPolicy AsyncRetryPolicy()
    {
        var sleepDurations = 
            Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(20), 2);
        var retryPolicy = Policy
            .Handle<BulkheadRejectedException>()
            .WaitAndRetryAsync(
                sleepDurations,
                onRetryAsync: (e, ts, i, ctx) =>
                {
                    Console.WriteLine($"Bulkhead Retry: {i}");
                    return Task.CompletedTask;
                });
        return retryPolicy;
    }
}

public class WeatherClient : IWeatherClient
{
    private readonly HttpClient _httpClient;

    public WeatherClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast[]> GetForeCast()
    {
        try
        {
            var fromJsonAsync = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            Console.WriteLine("Successful Response");
            return fromJsonAsync;
        }
        catch (BulkheadRejectedException e)
        {
            Console.WriteLine("Final BulkheadRejectedException");
            return null;
        }
    }
}

public interface IWeatherClient
{
    Task<WeatherForecast[]> GetForeCast();
}