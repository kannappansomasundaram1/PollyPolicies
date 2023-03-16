var vortexEventWrappers = new List<string>();
var batchSize = 20; 

var tasks = Enumerable.Range(0,100).Select(Map).Chunk(batchSize);
foreach (var vortexEventsBatchTasks in tasks)
{
    var mappedString = (await Task.WhenAll(vortexEventsBatchTasks)).Where(t => t != null).ToList();
    vortexEventWrappers.AddRange(mappedString);
}

Console.WriteLine(vortexEventWrappers.Aggregate((a, b) => $"{a},{b}"));
static async Task<string> Map(int i)
{
    await Task.Delay(0);
    Console.WriteLine($"Calling External API to get string for {i}");
    return i.ToString();
}