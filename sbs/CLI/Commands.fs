module CLI.Commands

type InitWorkspace =
    { Path : string }

type CloneRepository =
    { Name : string
      Shallow : bool
      Dependencies : bool
      References : bool }

type BuildRepository =
    { Name : string 
      Config : string
      Clean : bool }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Build
    | Unknown

[<RequireQualifiedAccess>]
type Command =
    | Version
    | Usage
    | Init of InitWorkspace
    | Clone of CloneRepository
    | Build of BuildRepository
    | Error of MainCommand
