
module Helpers.Console

open System


let consoleLock = System.Object()

let printConsole (c : ConsoleColor) (s : string) =
    let oldColor = Console.ForegroundColor
    try
        Console.ForegroundColor <- c
        Console.WriteLine(s)
    finally
        Console.ForegroundColor <- oldColor

let PrintInfo msg = 
    lock consoleLock (fun () -> printConsole ConsoleColor.Yellow (">>> " + msg))

let PrintSuccess msg = 
    lock consoleLock (fun () -> printConsole ConsoleColor.Green msg)

let PrintError msg = 
    lock consoleLock (fun () -> printConsole ConsoleColor.Red msg)

let PrintOutput info (execResult : Helpers.IO.Result) =
    let rec printl lines =
        match lines with
        | line :: tail -> printfn "%s" line; printl tail
        | [] -> ()

    let display () =
        info |> printConsole ConsoleColor.Yellow
        execResult.Out |> printl
        execResult.Error |> printl
        execResult

    lock consoleLock display

