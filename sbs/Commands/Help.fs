module Commands.Help
open CLI.Commands


let private usageContent() =
    let content = [
        [MainCommand.Usage], "usage : display help on command or area"
        [MainCommand.View], "view <name> <folders...> : create a solution with select folders"
        [MainCommand.Build], "build [--release] [--parallel] <view> : build a view" 
        [MainCommand.Build], "rebuild [--release] <view> : rebuild a view" 
        [MainCommand.Test], "test [--release] [--parallel] <view> : test a view" 
        [MainCommand.Publish], "publish [--release] <view> : publish apps in view" 
        [MainCommand.Open], "open <view> : open view with your favorite ide" 
    ]
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
