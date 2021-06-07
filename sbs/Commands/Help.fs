module Commands.Help
open CLI.Commands


let private usageContent() =
    let content = [
        [MainCommand.Usage], "usage : display help on command or area"
        [MainCommand.Init], "init <folder> <master-repository-uri>: initialize workspace"
        [MainCommand.Clone], "clone [--only] [--shallow] [--branch name] <repository...> : clone repositories using wildcards"
        [MainCommand.Impact], "impact <sub-path> : compute impact"
        [MainCommand.View], "view <name> <folders...> : create a solution with select folders"
        [MainCommand.Checkout], "checkout <branch> : checkout given branch on all repositories"
        [MainCommand.Fetch], "fetch : fetch all branches on all repositories"
        [MainCommand.Pull], "pull [--only] <repository...> : pull (ff-only) on all repositories"
        [MainCommand.Build], "build [--release] [--parallel] <view> : build a view" 
        [MainCommand.Build], "rebuild [--release] <view> : rebuild a view" 
        [MainCommand.Test], "test [--release] [--parallel] <view> : test a view" 
        [MainCommand.Publish], "publish [--release] <view> : publish apps in view" 
        [MainCommand.Open], "open <view> : open view with your favorite ide" 
        [MainCommand.Exec], "exec <cmd> <repository...> :  execute command for each matching repository (variables: SBS_REPO_NAME, SBS_REPO_PATH, SBS_REPO_URL, SBS_WKS)"
        [MainCommand.Doctor], "doctor : check workspace consistency" ]
    content


let Usage (what : MainCommand) =
    let lines = usageContent () |> List.filter (fun (cmd, _) -> cmd |> Seq.contains what || what = MainCommand.Unknown)
                                |> List.map (fun (_, desc) -> desc)
    printfn "Current directory: %s\n" (System.Environment.CurrentDirectory)
    printfn "Usage:"
    lines |> Seq.iter (printfn "  %s")

let private versionContent() =
    let version = Helpers.Env.Version()
    let versionContent = sprintf "sbs %s" (version.ToString())
    [ versionContent ]


let Version () =
    versionContent() |> List.iter (fun x -> printfn "%s" x)
