
module Helpers.Console

open System


let consoleLock = System.Object()

let PrintConsole (c : ConsoleColor) (s : string) =
    let oldColor = Console.ForegroundColor
    try
        Console.ForegroundColor <- c
        Console.WriteLine(s)
    finally
        Console.ForegroundColor <- oldColor


let PrintInfo msg = PrintConsole ConsoleColor.Yellow ("- " + msg)
let PrintSuccess msg = PrintConsole ConsoleColor.Green msg
let PrintError msg = PrintConsole ConsoleColor.Red msg

let PrintOutput info (execResult : Helpers.IO.Result) =
    let rec printl lines =
        match lines with
        | line :: tail -> printfn "%s" line; printl tail
        | [] -> ()

    let display () =
        info |> PrintInfo
        execResult.Out |> printl
        execResult.Error |> printl
        execResult

    lock consoleLock display

