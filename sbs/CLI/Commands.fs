module CLI.Commands

type InitWorkspace =
    { Path : string }

type CloneRepository =
    { Name : string
      Shallow : bool
      Dependencies : bool }

type CheckoutRepositories =
    { Branch : string }

type BuildRepository =
    { Name : string 
      Config : string
      Clean : bool }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Checkout
    | Build
    | Unknown

[<RequireQualifiedAccess>]
type Command =
    | Version
    | Usage
    | Init of InitWorkspace
    | Clone of CloneRepository
    | Checkout of CheckoutRepositories
    | Build of BuildRepository
    | Error of MainCommand
