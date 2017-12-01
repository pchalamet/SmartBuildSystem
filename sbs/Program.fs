
module Main
open CLI.Commands

let processMain argv =
    let cmd = CLI.CommandLine.Parse (argv |> Seq.toList)
    match cmd with
    | Command.Init info -> Commands.Workspace.InitWorkspace info
    | Command.Clone info -> Commands.Sources.Clone info
    | Command.Checkout info -> Commands.Sources.Checkout info
    | Command.View info -> Commands.Workspace.CreateView info
    | Command.Build info -> Commands.Workspace.Build info
    | Command.Usage -> Commands.Help.PrintUsage MainCommand.Unknown
    | Command.Exec info -> Commands.Workspace.Exec info
    | Command.Error info -> Commands.Help.PrintUsage info
    | Command.Version -> Commands.Help.PrintVersion ()

    let retCode = match cmd with
                  | Command.Error _ -> 5
                  | _ -> 0
    retCode


let tryMain verbose argv =
    try
        processMain argv
    with
        x -> let err = if verbose |> not then sprintf "Error:\n%s" x.Message
                       else let sep = "---------------------------------------------------"
                            sprintf "%s\n%A\n%s" sep x sep
               
             Helpers.Console.PrintError err
             5

[<EntryPoint>]
let main argv =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let verbose, args = CLI.CommandLine.IsVerbose (argv |> List.ofArray)
    let retCode = tryMain verbose args
    stopWatch.Stop()
    let elapsed = stopWatch.Elapsed
    if verbose then printfn "Completed in %d seconds." ((int)elapsed.TotalSeconds)
    retCode
