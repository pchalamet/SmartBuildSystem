module CLI.Commands

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

[<RequireQualifiedAccess>]
type MainCommand =
    | Usage
    | View
    | Build
    | Test
    | Publish
    | Open
    | Unknown

[<RequireQualifiedAccess>]
type Command =
    | Version
    | Usage
    | View of CreateView
    | Build of BuildView
    | Test of TestView
    | Publish of PublishView
    | Error of MainCommand
