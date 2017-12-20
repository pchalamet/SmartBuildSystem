
module Main
open CLI.Commands

let processMain argv =
    let cmd = CLI.CommandLine.Parse (argv |> Seq.toList)
    match cmd with
    | Command.Init info -> Commands.Workspace.Init info
    | Command.Clone info -> Commands.Sources.Clone info
    | Command.Checkout info -> Commands.Sources.Checkout info
    | Command.View info -> Commands.View.Create info
    | Command.Build info -> Commands.View.Build info
    | Command.Usage -> Commands.Help.Usage MainCommand.Unknown
    | Command.Exec info -> Commands.Workspace.Exec info
    | Command.Open info -> Commands.View.Open info
    | Command.Fetch -> Commands.Sources.Fetch ()
    | Command.Pull -> Commands.Sources.Pull ()
    | Command.Doctor -> Commands.Workspace.Doctor ()
    | Command.Error info -> Commands.Help.Usage info
    | Command.Version -> Commands.Help.Version ()

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
