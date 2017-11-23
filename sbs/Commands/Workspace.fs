module Commands.Workspace
open System.IO
open Helpers

let InitWorkspace (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    if wsDir.Exists then failwithf "Workspace already exists"

    // first clone master repository
    let masterRepo = { Configuration.Master.Repository.Name = ".sbs"
                       Configuration.Master.Repository.Uri = Helpers.Env.MasterRepository () }
    Core.Git.Clone masterRepo wsDir false
        |> Helpers.IO.CheckResponseCode

    let currentDir = System.Environment.CurrentDirectory
    try
        System.Environment.CurrentDirectory <- wsDir.FullName

        // delegate to clone then to get dependencies
        let cloneRepo = { CLI.Commands.CloneRepository.Patterns = [masterRepo.Name]
                          CLI.Commands.CloneRepository.Shallow = false
                          CLI.Commands.CloneRepository.Dependencies = true }
        cloneRepo |> Commands.Sources.Clone
    finally
        System.Environment.CurrentDirectory <- currentDir

let CreateView (cmd : CLI.Commands.CreateView) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load



    
let Build (cmd : CLI.Commands.BuildView) =
    let wsDir = Env.WorkspaceDir()

    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Building view %A" cmd.Name |> Helpers.Console.PrintInfo
    Core.MsBuild.Build cmd.Clean cmd.Config wsDir sln
