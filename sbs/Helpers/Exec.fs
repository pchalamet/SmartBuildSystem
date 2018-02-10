module Helpers.Exec
open Helpers.IO
open System.Diagnostics
open System.IO


[<NoComparison; RequireQualifiedAccess>]
type private MonitorCommand =
    | Out of string list
    | Err of string list
    | End of int

let private defaultPSI (command : string) (args : string) (dir : DirectoryInfo) (vars : Map<string, string>) redirect =
    let psi = ProcessStartInfo (FileName = command,
                                Arguments = args,
                                UseShellExecute = false,
                                WorkingDirectory = dir.FullName,
                                LoadUserProfile = true,
                                RedirectStandardOutput = redirect,
                                RedirectStandardError = redirect)
    for var in vars do
        psi.EnvironmentVariables.Add(var.Key, var.Value)

    psi


let private supervisedExec redirect (command : string) (args : string) (dir : DirectoryInfo) (vars : Map<string, string>) =
    let psi = defaultPSI command args dir vars redirect
    use proc = Process.Start (psi)
    if proc |> isNull then failwithf "Failed to start process %A with arguments %A" command args

    let rec read (stm : System.IO.TextReader) buffer =
        let line = stm.ReadLine()
        if line |> isNull then buffer
        else
            read stm buffer@[line]

    let asyncOut = if redirect then async { return read proc.StandardOutput List.empty |> MonitorCommand.Out }
                               else async { return List.empty |> MonitorCommand.Out }
    let asyncErr = if redirect then async { return read proc.StandardError List.empty |> MonitorCommand.Err }
                               else async { return List.empty |> MonitorCommand.Err }
    let asyncCode = async { proc.WaitForExit(); return proc.ExitCode |> MonitorCommand.End }
    let res = [ asyncCode ; asyncOut ; asyncErr ] |> Async.Parallel |> Async.RunSynchronously
    match res with
    | [| MonitorCommand.End code; MonitorCommand.Out out; MonitorCommand.Err err |]
            -> { Result.Code=code
                 Result.Out=out
                 Result.Error=err
                 Result.Info = sprintf "%s %s @ %s" command args dir.FullName }
    | _ -> failwith "Unexpected results"

let Exec =
    supervisedExec false

let ExecGetOutput =
    supervisedExec true

let Spawn (command : string) (args : string) (verb : string) =
    let psi = ProcessStartInfo (FileName = command, UseShellExecute = true, Arguments = args, Verb = verb)
    use proc = Process.Start (psi)
    ()
