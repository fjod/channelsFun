// For more information see https://aka.ms/fsharp-console-apps

open System
open System.Diagnostics
open System.Threading
open System.Threading.Channels

printfn "Hello from F#"

let boundedOptions = BoundedChannelOptions(3)
boundedOptions.SingleWriter <- true
boundedOptions.SingleReader <- false
boundedOptions.FullMode <- BoundedChannelFullMode.Wait

let mainChannel = Channel.CreateBounded<int>(boundedOptions)

let StartLongRun(writer: ChannelWriter<string>) (token : CancellationToken) (counter : int) =
    task{
        let processStartInfo = ProcessStartInfo(@"..\..\..\..\LongRunningOperation\bin\debug\net8.0\LongRunningOperation.exe")
        processStartInfo.RedirectStandardError <- true
        processStartInfo.UseShellExecute <- false
        let ret = Process.Start(processStartInfo)
        let! _ = ret.WaitForExitAsync(token)
        let! _ = writer.WriteAsync($"finished {counter}", token)
        ()
    }
    
let StartReader() =
    let localChan = Channel.CreateBounded(boundedOptions)
    let localToken = new CancellationTokenSource()
    task { 
      while localToken.IsCancellationRequested |> not do
        let! gotMsg = mainChannel.Reader.WaitToReadAsync(localToken.Token)
        match gotMsg with
            | true ->
                        let! msg = mainChannel.Reader.ReadAsync(localToken.Token)
                        printfn $"Starting {msg}-th task"
                        StartLongRun localChan.Writer localToken.Token msg |> ignore // start hot task
            | false ->
                let! result = localChan.Reader.ReadAsync(localToken.Token) // as any of operation completes
                printfn $"Completed with task:{result}"
                localToken.Cancel() //cancel other ones
      printfn "reader while loop exited"
    } |> ignore
    ()


// create reader as well
StartReader()
// push into reader as many operations as you want
mainChannel.Writer.TryWrite(1) |> ignore
mainChannel.Writer.TryWrite(2) |> ignore
mainChannel.Writer.TryWrite(3) |> ignore
Thread.Sleep 100
mainChannel.Writer.TryWrite(555) |> ignore

// when any operation completes - all other operations inside this reader will be cancelled
mainChannel.Writer.Complete()

Console.ReadKey() |> ignore