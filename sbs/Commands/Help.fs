module Commands.Help
open CLI.Commands


let private UsageContent() =
    let content = [
        [MainCommand.Usage], "usage : display help on command or area"
        [MainCommand.Init], "init <folder> : initialize workspace"
        [MainCommand.Clone], "clone <repository> : clone a repository"
        [MainCommand.Build], "build <repository> : build a repository" ]
    content



let PrintUsage (what : MainCommand) =
    let lines = UsageContent () |> List.filter (fun (cmd, _) -> cmd |> Seq.contains what || what = MainCommand.Unknown)
                                |> List.map (fun (_, desc) -> desc)

    printfn "Usage:"
    lines |> Seq.iter (printfn "  %s")


let private VersionContent() =
    let version = Helpers.Env.Version()
    let versionContent = sprintf "full-build %s" (version.ToString())

    [ versionContent ]


let PrintVersion () =
    VersionContent() |> List.iter (fun x -> printfn "%s" x)
