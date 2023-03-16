var tasks = Enumerable.Range(0,100).Select(async numb => await Map(numb)).ToArray();

var vortexEventWrappers = new List<string>();
var batchSize = 20; 
var mappedTasks = tasks.Chunk(batchSize).ToArray();
foreach (var vortexEventsBatchTasks in mappedTasks)
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