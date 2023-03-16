var mappedStrings = new List<string>();
var batchSize = 20; 
Console.WriteLine("Creating Tasks without calling mapper");
//Bad - ToArray() AfterSelect
// var tasks = Enumerable.Range(0,100).Select(Map).ToArray().Chunk(batchSize);

//Bad - ToArray() After Chunk
var tasks = Enumerable.Range(0,100).Select(Map).Chunk(batchSize).ToArray();

//Good
// var tasks = Enumerable.Range(0,100).Select(Map).Chunk(batchSize);

Console.WriteLine("Executing Calls in batches");
foreach (var taskBatch in tasks)
{
    var mappedString = (await Task.WhenAll(taskBatch))
        .ToList();
    mappedStrings.AddRange(mappedString);
}

Console.WriteLine(mappedStrings.Aggregate((a, b) => $"{a},{b}"));

static async Task<string> Map(int i)
{
    await Task.Delay(0);
    Console.WriteLine($"Calling External API to get string for {i}");
    return i.ToString();
}