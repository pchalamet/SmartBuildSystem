
module CLI.CommandLine

open Commands
open Helpers.Collections




type private TokenOption =
    | Deps
    | Refs
    | Shallow
    
let private (|TokenOption|_|) (token : string) =
    match token with
    | "--deps" -> Some TokenOption.Deps
    | "--refs" -> Some TokenOption.Refs
    | "--shallow" -> Some TokenOption.Shallow
    | _ -> None

type private Token =
    | Version
    | Usage
    | Init
    | Clone
    | Build

let private (|Token|_|) (token : string) =
    match token with
    | "version" -> Some Token.Version
    | "usage" -> Some Token.Usage
    | "init" -> Some Token.Init
    | "clone" -> Some Token.Clone
    | "build" -> Some Token.Build
    | _ -> None


let private (|Param|_|) (prm : string) =
    if prm.StartsWith("--") then None
    else Some prm

let private (|Params|_|) (prms : string list) =
    let hasNotParam = prms |> List.exists (fun x -> match x with
                                                    | Param _ -> false
                                                    | _ -> true)
    if hasNotParam then None
    else Some prms


let private commandUsage (args : string list) =
    match args with
    | _ -> Command.Usage

let private commandInit (args : string list) =
    match args with
    | [Param path]
        -> Command.Init { Path = path }
    | _ -> Command.Error MainCommand.Init

let rec private commandClone (shallow : bool) (deps : bool) (refs : bool) (args : string list) =
    match args with
    | TokenOption TokenOption.Shallow :: tail -> tail |> commandClone true deps refs
    | TokenOption TokenOption.Deps :: tail -> tail |> commandClone shallow true refs
    | TokenOption TokenOption.Refs :: tail -> tail |> commandClone shallow deps true
    | [] -> Command.Error MainCommand.Clone
    | [Param name] -> Command.Clone { Name = name; Shallow = shallow; Dependencies = deps; References = refs }
    | _ -> Command.Error MainCommand.Clone

let rec private commandBuild (args : string list) =
    match args with
    | [Param name] -> Command.Build { Name = name }
    | _ -> Command.Error MainCommand.Build

let Parse (args : string list) : Command =
    match args with
    | [Token Token.Version] -> Command.Version
    | Token Token.Usage :: cmdArgs -> cmdArgs |> commandUsage
    | Token Token.Init :: cmdArgs -> cmdArgs |> commandInit
    | Token Token.Clone :: cmdArgs -> cmdArgs |> commandClone false false false
    | Token Token.Build :: cmdArgs -> cmdArgs |> commandBuild
    | _ -> Command.Error MainCommand.Usage


let IsVerbose (args : string list) : (bool * string list) =
    if (args <> List.empty && args |> List.head = "--verbose") then
        let newArgs = args.Tail
        (true, newArgs)
    else
        (false, args)
