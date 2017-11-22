
module Helpers.Console

open System


let consoleLock = System.Object()

let ConsoleDisplay (c : ConsoleColor) (s : string) =
    let oldColor = Console.ForegroundColor
    try
        Console.ForegroundColor <- c
        Console.WriteLine(s)
    finally
        Console.ForegroundColor <- oldColor


let DisplayInfo msg = ConsoleDisplay ConsoleColor.Cyan ("- " + msg)
let DisplayError msg = ConsoleDisplay ConsoleColor.Red msg

let PrintOutput info (execResult : Helpers.IO.Result) =
    let rec printl lines =
        match lines with
        | line :: tail -> printfn "%s" line; printl tail
        | [] -> ()

    let display () =
        info |> DisplayInfo
        execResult.Out |> printl
        execResult.Error |> printl
        execResult

    lock consoleLock display

