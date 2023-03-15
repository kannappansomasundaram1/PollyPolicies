using System.Diagnostics;
using AmiUpdater;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.ConfigureServices();
var serviceProvider = serviceCollection.BuildServiceProvider();
var handler = serviceProvider.GetRequiredService<PolicyTester>();
await handler.Test();

public class PolicyTester
{
    private readonly IWeatherClient _client;

    public PolicyTester(IWeatherClient client)
    {
        _client = client;
    }
    public async Task Test()
    {
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 100).Select(x => _client.GetForeCast()).ToList();
        var foreCasts = await Task.WhenAll(tasks);
        var failedCount = foreCasts.Count(x => x != null);
        var successfulCount = foreCasts.Count(x => x == null);
        Console.WriteLine(stopwatch.ElapsedMilliseconds);
    }
}