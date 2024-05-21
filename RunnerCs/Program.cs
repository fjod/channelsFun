// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Threading.Channels;

Console.WriteLine("Hello, World!");

var ch = Channel.CreateBounded<int>( new BoundedChannelOptions(3)
{
    SingleWriter = true,
    SingleReader = false,
    FullMode = BoundedChannelFullMode.Wait
});

List<Task> consumers = new List<Task>();
for (int i = 0; i < 3; i++)
{
    var consumer = Task.Run(async () =>
    {
        await foreach (var message in ch.Reader.ReadAllAsync())
        {
            Console.WriteLine($"Starting {message}-th task");
            await StartAndWaitForProcess();
        }
    });
    
    consumers.Add(consumer);
}

for (int i = 0; i < 50; i++)
{
    while (!ch.Writer.TryWrite(i))
    {
        await Task.Delay(100);
    }
    
}
ch.Writer.Complete();
await Task.WhenAll(consumers);

async Task StartAndWaitForProcess()
{
    var processStartInfo = new ProcessStartInfo("job\\LongRunningOperation.exe");
    processStartInfo.RedirectStandardError = true;
    processStartInfo.UseShellExecute = false;
    var ret = Process.Start(processStartInfo);
    await ret!.WaitForExitAsync();
}