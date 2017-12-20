module Commands.Help
open CLI.Commands


let private usageContent() =
    let content = [
        [MainCommand.Usage], "usage : display help on command or area"
        [MainCommand.Init], "init <folder> : initialize workspace"
        [MainCommand.Clone], "clone [--only] [--shallow] <repository...> : clone repositories using wildcards"
        [MainCommand.View], "view [--only] <name> <repository...> : create a solution with select repositories"
        [MainCommand.Checkout], "checkout <branch> : checkout given branch on all repositories"
        [MainCommand.Fetch], "fetch : fetch all branches on all repositories"
        [MainCommand.Pull], "pull : pull (ff-only) on all repositories"
        [MainCommand.Build], "build [--debug] <view> : build a repository" 
        [MainCommand.Build], "rebuild [--debug] <repository> : rebuild a repository" 
        [MainCommand.Open], "open <view> : open view with your favorite ide" 
        [MainCommand.Exec], "exec <cmd> :  execute command for each repository (variables: SBS_NAME, SBS_PATH, SBS_URL, SBS_WKS)"
        [MainCommand.Doctor], "doctor : check workspace consistency" ]
    content


let Usage (what : MainCommand) =
    let lines = usageContent () |> List.filter (fun (cmd, _) -> cmd |> Seq.contains what || what = MainCommand.Unknown)
                                |> List.map (fun (_, desc) -> desc)
    printfn "Usage:"
    lines |> Seq.iter (printfn "  %s")


let private versionContent() =
    let version = Helpers.Env.Version()
    let versionContent = sprintf "full-build %s" (version.ToString())
    [ versionContent ]


let Version () =
    versionContent() |> List.iter (fun x -> printfn "%s" x)
