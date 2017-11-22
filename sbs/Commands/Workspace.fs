module Commands.Init
open System.IO

let InitWorkspace (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    if wsDir.Exists then failwithf "Workspace already exists"

    // first clone master repository
    let masterRepo = { Configuration.Master.Repository.Name = ".sbs"
                       Configuration.Master.Repository.Uri = Helpers.Env.MasterRepository () }
    Core.Git.GitClone masterRepo wsDir "master" true
        |> Helpers.IO.CheckResponseCode
