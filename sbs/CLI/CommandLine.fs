
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
    | Init
    | Clone
    | View
    | Build
    | Publish
    | Rebuild
    | Checkout
    | Exec
    | Open
    | Fetch
    | Pull
    | Doctor

let private (|Token|_|) token =
    match token with
    | "version" -> Some Token.Version
    | "usage" -> Some Token.Usage
    | "init" -> Some Token.Init
    | "clone" -> Some Token.Clone
    | "view" -> Some Token.View
    | "build" -> Some Token.Build
    | "publish" -> Some Token.Publish
    | "rebuild" -> Some Token.Rebuild
    | "checkout" -> Some Token.Checkout
    | "exec" -> Some Token.Exec
    | "open" -> Some Token.Open
    | "fetch" -> Some Token.Fetch
    | "pull" -> Some Token.Pull
    | "doctor" -> Some Token.Doctor
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

let private commandInit args =
    match args with
    | [Param path; Param uri]
        -> Command.Init { Path = path; Uri = uri }
    | _ -> Command.Error MainCommand.Init

let rec private commandClone branch shallow deps args =
    match args with
    | TokenOption TokenOption.Shallow :: tail -> tail |> commandClone branch true deps
    | TokenOption TokenOption.Branch :: Param name :: tail -> tail |> commandClone (Some name) true deps
    | TokenOption TokenOption.Only :: tail -> tail |> commandClone branch shallow false
    | [] -> Command.Error MainCommand.Clone
    | Params patterns -> Command.Clone { Patterns = patterns
                                         Shallow = shallow
                                         Dependencies = deps 
                                         Branch = branch }
    | _ -> Command.Error MainCommand.Clone

let private commandCheckout args =
    match args with
    | [Param branch] -> Command.Checkout { Branch = branch }
    | _ -> Command.Error MainCommand.Checkout

let rec commandView deps args =
    match args with
    | TokenOption TokenOption.Only :: tail -> tail |> commandView true
    | Param name :: Params patterns -> Command.View { Name = name; Patterns = patterns; Dependencies = deps }
    | _ -> Command.Error MainCommand.View

let rec private commandBuild par clean config args =
    match args with
    | TokenOption TokenOption.Release :: tail -> tail |> commandBuild par clean "Release" 
    | TokenOption TokenOption.Parallel :: tail -> tail |> commandBuild true clean "Release" 
    | [Param name] -> Command.Build { Name = name 
                                      Clean = clean
                                      Config = config 
                                      Parallel = par }
    | _ -> Command.Error MainCommand.Build

let rec private commandPublish config args =
    match args with
    | TokenOption TokenOption.Release :: tail -> tail |> commandPublish "Release" 
    | [Param name] -> Command.Publish { Name = name 
                                        Config = config }
    | _ -> Command.Error MainCommand.Publish

let private commandExec args =
    match args with
    | Param cmd :: tail -> Command.Exec { Command = cmd 
                                          Patterns = tail }
    | _ -> Command.Error MainCommand.Exec

let private commandOpen args =
    match args with
    | [Param name] -> Command.Open { Name = name }
    | _ -> Command.Error MainCommand.Open

let private commandFetch args =
    match args with
    | [] -> Command.Fetch
    | _ -> Command.Error MainCommand.Fetch

let rec private commandPull deps args =
    match args with
    | TokenOption TokenOption.Only :: tail -> tail |> commandPull false
    | Params patterns -> Command.Pull { Dependencies = deps
                                        Patterns = patterns }
    | _ -> Command.Error MainCommand.Pull

let private commandDoctor args =
    match args with
    | [] -> Command.Doctor
    | _ -> Command.Error MainCommand.Doctor

let Parse (args : string list) : Command =
    match args with
    | [Token Token.Version] -> Command.Version
    | Token Token.Usage :: cmdArgs -> cmdArgs |> commandUsage
    | Token Token.Init :: cmdArgs -> cmdArgs |> commandInit
    | Token Token.Clone :: cmdArgs -> cmdArgs |> commandClone None false true
    | Token Token.View :: cmdArgs -> cmdArgs |> commandView true
    | Token Token.Build :: cmdArgs -> cmdArgs |> commandBuild false false "Debug"
    | Token Token.Publish :: cmdArgs -> cmdArgs |> commandPublish "Debug"
    | Token Token.Rebuild :: cmdArgs -> cmdArgs |> commandBuild false true "Debug"
    | Token Token.Checkout :: cmdArgs -> cmdArgs |> commandCheckout
    | Token Token.Exec :: cmdArgs -> cmdArgs |> commandExec
    | Token Token.Open :: cmdArgs -> cmdArgs |> commandOpen
    | Token Token.Fetch :: cmdArgs -> cmdArgs |> commandFetch
    | Token Token.Pull :: cmdArgs -> cmdArgs |> commandPull true
    | Token Token.Doctor :: cmdArgs -> cmdArgs |> commandDoctor
    | _ -> Command.Error MainCommand.Usage

let IsVerbose (args : string list) : (bool * string list) =
    if (args <> List.empty && args |> List.head = "--verbose") then
        let newArgs = args.Tail
        (true, newArgs)
    else
        (false, args)
