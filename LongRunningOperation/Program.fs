// For more information see https://aka.ms/fsharp-console-apps

open System
open System.Threading

printfn "Long operation started"
let rand = Random()

for i = 0 to 10 do
  Thread.Sleep(333)
  let chance = rand.Next(0, 100)
  if chance > 90 then
      printfn "Operation completed prematurely"
      exit(0)
  else
      printfn $"{i*10} completed"
  
printfn "Done"