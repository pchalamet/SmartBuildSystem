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

type PublishView =
    { Name : string 
      Config : string }

type OpenView =
    { Name : string }

type ExecCommand =
    { Command : string }

type PullRepositories =
    { Dependencies : bool
      Patterns : string list }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Checkout
    | View
    | Build
    | Publish
    | Exec
    | Open
    | Fetch
    | Pull
    | Doctor
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
    | Publish of PublishView
    | Exec of ExecCommand
    | Error of MainCommand
    | Open of OpenView
    | Fetch
    | Pull of PullRepositories
    | Doctor