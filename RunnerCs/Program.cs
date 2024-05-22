// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Threading.Channels;

Console.WriteLine("Hello, World!");

// Create channel which can hold only 3 items
var ch = Channel.CreateBounded<int>( new BoundedChannelOptions(3)
{
    SingleWriter = true,
    SingleReader = false,
    FullMode = BoundedChannelFullMode.Wait
});

// create 3 readers
for (int i = 0; i < 3; i++)
{
    _ = Task.Run(async () =>
    {
        // each reader continuously gets data from channel
        //await foreach (var message in ch.Reader.ReadAllAsync()) // instead of next 2 lines one can use IAsyncEnumerable
        while (await ch.Reader.WaitToReadAsync())
        {
            var message = ch.Reader.ReadAsync();
            Console.WriteLine($"Starting {message}-th task");
            
            // start and await for completion
            await StartAndWaitForProcess();
        }
    });
}

// single writer, which writes to channel as readers become available
for (int i = 0; i < 50; i++)
{
    while (!ch.Writer.TryWrite(i))
    {
        await Task.Delay(100);
    }
    
}
// finish writing
ch.Writer.Complete();


async Task StartAndWaitForProcess()
{
    var processStartInfo = new ProcessStartInfo(@"..\..\..\..\LongRunningOperation\bin\debug\net8.0\LongRunningOperation.exe");
    processStartInfo.RedirectStandardError = true;
    processStartInfo.UseShellExecute = false;
    var ret = Process.Start(processStartInfo);
    await ret!.WaitForExitAsync();
}