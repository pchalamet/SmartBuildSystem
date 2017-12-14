module CLI.Commands

type InitWorkspace =
    { Path : string }

type CloneRepository =
    { Patterns : string list
      Shallow : bool
      Dependencies : bool }

type CheckoutRepositories =
    { Branch : string }

type CreateView =
    {  Name : string
       Patterns : string list 
       Dependencies : bool }

type BuildView =
    { Name : string 
      Config : string
      Clean : bool }

type OpenView =
    { Name : string }

type ExecCommand =
    { Command : string }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Checkout
    | View
    | Build
    | Exec
    | Open
    | Fetch
    | Unknown

[<RequireQualifiedAccess>]
type Command =
    | Version
    | Usage
    | Init of InitWorkspace
    | Clone of CloneRepository
    | Checkout of CheckoutRepositories
    | View of CreateView
    | Build of BuildView
    | Exec of ExecCommand
    | Error of MainCommand
    | Open of OpenView
    | Fetch