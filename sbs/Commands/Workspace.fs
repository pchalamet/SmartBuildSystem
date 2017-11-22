module Commands.Init
open System.IO

let InitWorkspace (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    if wsDir.Exists then failwithf "Workspace already exists"

    // first clone master repository
    let masterRepo = { Configuration.Master.Repository.Name = ".sbs"
                       Configuration.Master.Repository.Uri = Helpers.Env.MasterRepository () }
    Core.Git.GitClone masterRepo wsDir "master" false
        |> Helpers.IO.CheckResponseCode

    let currentDir = System.Environment.CurrentDirectory
    try
        System.Environment.CurrentDirectory <- wsDir.FullName

        // delegate to clone then to get dependencies
        let cloneRepo = { CLI.Commands.CloneRepository.Name = masterRepo.Name
                          CLI.Commands.CloneRepository.Shallow = false
                          CLI.Commands.CloneRepository.Dependencies = true 
                          CLI.Commands.CloneRepository.References = false }
        cloneRepo |> Commands.Sources.Clone
    finally
        System.Environment.CurrentDirectory <- currentDir