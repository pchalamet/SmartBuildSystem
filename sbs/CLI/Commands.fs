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
       Patterns : string list }

type BuildView =
    { Name : string 
      Config : string
      Clean : bool }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Checkout
    | View
    | Build
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
    | Error of MainCommand
