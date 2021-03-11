module CLI.Commands

type InitWorkspace =
    { Path : string 
      Uri : string }

type CloneRepository =
    { Patterns : string list
      Shallow : bool
      Dependencies : bool 
      Branch : string option }

type CheckoutRepositories =
    { Branch : string }

type CreateView =
    {  Name : string
       Pattern : string }

type BuildView =
    { Name : string 
      Config : string
      Clean : bool 
      Parallel : bool }

type TestView =
    { Name : string 
      Config : string
      Parallel : bool }

type PublishView =
    { Name : string 
      Config : string }

type OpenView =
    { Name : string }

type ExecCommand =
    { Patterns : string list
      Command : string }

type PullRepositories =
    { Dependencies : bool
      Patterns : string list }

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | Init
    | Clone
    | Checkout
    | Impact
    | View
    | Build
    | Test
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
    | Test of TestView
    | Publish of PublishView
    | Exec of ExecCommand
    | Error of MainCommand
    | Open of OpenView
    | Fetch
    | Pull of PullRepositories
    | Doctor