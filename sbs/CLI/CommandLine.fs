
module CLI.CommandLine
open Commands

type private TokenOption =
    | NoDeps
    | Shallow
    | Debug
    
let private (|TokenOption|_|) token =
    match token with
    | "--no-dep" -> Some TokenOption.NoDeps
    | "--shallow" -> Some TokenOption.Shallow
    | "--debug" -> Some TokenOption.Debug
    | _ -> None

type private Token =
    | Version
    | Usage
    | Init
    | Clone
    | View
    | Build
    | Rebuild
    | Checkout
    | Exec
    | Open
    | Fetch
    | Pull

let private (|Token|_|) token =
    match token with
    | "version" -> Some Token.Version
    | "usage" -> Some Token.Usage
    | "init" -> Some Token.Init
    | "clone" -> Some Token.Clone
    | "view" -> Some Token.View
    | "build" -> Some Token.Build
    | "rebuild" -> Some Token.Rebuild
    | "checkout" -> Some Token.Checkout
    | "exec" -> Some Token.Exec
    | "open" -> Some Token.Open
    | "fetch" -> Some Token.Fetch
    | "pull" -> Some Token.Pull
    | _ -> None


let private (|Param|_|) (prm : string) =
    if prm.StartsWith("--") then None
    else Some prm

let private (|Params|_|) prms =
    let hasNotParam = prms |> List.exists (fun x -> match x with
                                                    | Param _ -> false
                                                    | _ -> true)
    if hasNotParam then None
    else Some prms


let private commandUsage args =
    match args with
    | _ -> Command.Usage

let private commandInit args =
    match args with
    | [Param path]
        -> Command.Init { Path = path }
    | _ -> Command.Error MainCommand.Init

let rec private commandClone shallow deps args =
    match args with
    | TokenOption TokenOption.Shallow :: tail -> tail |> commandClone true deps
    | TokenOption TokenOption.NoDeps :: tail -> tail |> commandClone shallow false
    | [] -> Command.Error MainCommand.Clone
    | Params patterns -> Command.Clone { Patterns = patterns; Shallow = shallow; Dependencies = deps }
    | _ -> Command.Error MainCommand.Clone

let private commandCheckout args =
    match args with
    | [Param branch] -> Command.Checkout { Branch = branch }
    | _ -> Command.Error MainCommand.Checkout

let rec commandView deps args =
    match args with
    | TokenOption TokenOption.NoDeps :: tail -> tail |> commandView false
    | Param name :: Params patterns -> Command.View { Name = name; Patterns = patterns; Dependencies = deps }
    | _ -> Command.Error MainCommand.View

let rec private commandBuild clean config args =
    match args with
    | TokenOption TokenOption.Debug :: tail -> tail |> commandBuild clean "Debug" 
    | [Param name] -> Command.Build { Name = name 
                                      Clean = clean
                                      Config = config }
    | _ -> Command.Error MainCommand.Build

let private commandExec args =
    match args with
    | [Param cmd] -> Command.Exec { Command = cmd }
    | _ -> Command.Error MainCommand.Exec

let private commandOpen args =
    match args with
    | [Param name] -> Command.Open { Name = name }
    | _ -> Command.Error MainCommand.Open

let private commandFetch args =
    match args with
    | [] -> Command.Fetch
    | _ -> Command.Error MainCommand.Fetch

let private commandPull args =
    match args with
    | [] -> Command.Pull
    | _ -> Command.Error MainCommand.Pull

let Parse (args : string list) : Command =
    match args with
    | [Token Token.Version] -> Command.Version
    | Token Token.Usage :: cmdArgs -> cmdArgs |> commandUsage
    | Token Token.Init :: cmdArgs -> cmdArgs |> commandInit
    | Token Token.Clone :: cmdArgs -> cmdArgs |> commandClone false true
    | Token Token.View :: cmdArgs -> cmdArgs |> commandView true
    | Token Token.Build :: cmdArgs -> cmdArgs |> commandBuild false "Release"
    | Token Token.Rebuild :: cmdArgs -> cmdArgs |> commandBuild true "Release"
    | Token Token.Checkout :: cmdArgs -> cmdArgs |> commandCheckout
    | Token Token.Exec :: cmdArgs -> cmdArgs |> commandExec
    | Token Token.Open :: cmdArgs -> cmdArgs |> commandOpen
    | Token Token.Fetch :: cmdArgs -> cmdArgs |> commandFetch
    | Token Token.Pull :: cmdArgs -> cmdArgs |> commandPull
    | _ -> Command.Error MainCommand.Usage

let IsVerbose (args : string list) : (bool * string list) =
    if (args <> List.empty && args |> List.head = "--verbose") then
        let newArgs = args.Tail
        (true, newArgs)
    else
        (false, args)
