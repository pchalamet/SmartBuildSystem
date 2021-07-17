
module CLI.CommandLine
open Commands
open System.IO

type private TokenOption =
    | Only
    | Shallow
    | Release
    | Branch
    | Parallel
    
let private (|TokenOption|_|) token =
    match token with
    | "--only" -> Some TokenOption.Only
    | "--shallow" -> Some TokenOption.Shallow
    | "--release" -> Some TokenOption.Release
    | "--branch" -> Some TokenOption.Branch
    | "--parallel" -> Some TokenOption.Parallel
    | _ -> None

type private Token =
    | Version
    | Usage
    | View
    | Build
    | Rebuild
    | Test
    | Publish
    | Open

let private (|Token|_|) token =
    match token with
    | "version" -> Some Token.Version
    | "usage" -> Some Token.Usage
    | "view" -> Some Token.View
    | "build" -> Some Token.Build
    | "rebuild" -> Some Token.Rebuild
    | "test" -> Some Token.Test
    | "publish" -> Some Token.Publish
    | "open" -> Some Token.Open
    | _ -> None


let private (|Param|_|) (prm : string) =
    if prm.StartsWith("--") then None
    else Some prm

let private (|Params|_|) prms =
    let hasNotParam = prms |> List.exists (fun x -> match x with
                                                    | Param _ -> false
                                                    | _ -> true)
    if hasNotParam || prms = List.empty then None
    else Some prms


let private commandUsage args =
    match args with
    | _ -> Command.Usage

let rec commandView args =
    match args with
    | [ Param name; Param pattern ] -> Command.View { Name = name; Pattern = pattern }
    | _ -> Command.Error MainCommand.View

let rec private commandBuild par clean config args =
    match args with
    | TokenOption TokenOption.Release :: tail -> tail |> commandBuild par clean "Release" 
    | TokenOption TokenOption.Parallel :: tail -> tail |> commandBuild true clean config
    | [Param name] -> Command.Build { Name = name 
                                      Clean = clean
                                      Config = config 
                                      Parallel = par }
    | _ -> Command.Error MainCommand.Build

let rec private commandTest par config args =
    match args with
    | TokenOption TokenOption.Release :: tail -> tail |> commandTest par "Release" 
    | TokenOption TokenOption.Parallel :: tail -> tail |> commandTest true config
    | [Param name] -> Command.Test { Name = name 
                                     Config = config 
                                     Parallel = par }
    | _ -> Command.Error MainCommand.Test

let rec private commandPublish config args =
    match args with
    | TokenOption TokenOption.Release :: tail -> tail |> commandPublish "Release" 
    | [Param name] -> Command.Publish { Name = name 
                                        Config = config }
    | _ -> Command.Error MainCommand.Publish

let private commandOpen args =
    match args with
    | [Param name] -> Command.Open { Name = name }
    | _ -> Command.Error MainCommand.Open

let Parse (args : string list) : Command =
    match args with
    | [Token Token.Version] -> Command.Version
    | Token Token.Usage :: cmdArgs -> cmdArgs |> commandUsage
    | Token Token.View :: cmdArgs -> cmdArgs |> commandView
    | Token Token.Build :: cmdArgs -> cmdArgs |> commandBuild false false "Debug"
    | Token Token.Rebuild :: cmdArgs -> cmdArgs |> commandBuild false true "Debug"
    | Token Token.Test :: cmdArgs -> cmdArgs |> commandTest false "Debug"
    | Token Token.Publish :: cmdArgs -> cmdArgs |> commandPublish "Debug"
    | Token Token.Open :: cmdArgs -> cmdArgs |> commandOpen
    | _ -> Command.Error MainCommand.Usage

let IsVerbose (args : string list) : (bool * string list) =
    if (args <> List.empty && args |> List.head = "--verbose") then
        let newArgs = args.Tail
        (true, newArgs)
    else
        (false, args)
