
module Main
open CLI.Commands

let processMain argv =
    let cmd = CLI.CommandLine.Parse (argv |> Seq.toList)
    match cmd with
    | Command.Init info -> Commands.Workspace.Init info; 0
    | Command.Clone info -> Commands.Sources.Clone info; 0
    | Command.Checkout info -> Commands.Sources.Checkout info; 0
    | Command.View info -> Commands.View.Create info
    | Command.Build info -> Commands.View.Build info; 0
    | Command.Test info -> Commands.View.Test info; 0
    | Command.Publish info -> Commands.View.Publish info; 0
    | Command.Usage -> Commands.Help.Usage MainCommand.Unknown; 0
    | Command.Exec info -> Commands.Workspace.Exec info; 0
    | Command.Open info -> Commands.View.Open info; 0
    | Command.Fetch -> Commands.Sources.Fetch (); 0
    | Command.Pull info -> Commands.Sources.Pull info; 0
    | Command.Doctor -> Commands.Doctor.Check (); 0
    | Command.Error info -> Commands.Help.Usage info; 5
    | Command.Version -> Commands.Help.Version (); 0


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
