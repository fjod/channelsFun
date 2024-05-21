// For more information see https://aka.ms/fsharp-console-apps

open System.Threading

printfn "Long operation started"

for i = 0 to 10 do
  Thread.Sleep(333)
  printfn $"{i*10} completed"
  
printfn "Done"