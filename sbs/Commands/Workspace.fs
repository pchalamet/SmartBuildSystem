module Commands.Workspace
open System.IO
open Helpers
open Helpers.Collections
open Helpers.Fs
open System.Linq

let Init (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    let isWorkspaceNotEmpty = wsDir.Exists 
                              && (wsDir.EnumerateFiles().Count() > 0 || wsDir.EnumerateDirectories().Count() > 0)
    if isWorkspaceNotEmpty then failwithf "Workspace already exists"
    wsDir |> EnsureExists |> ignore

    // first clone master repository
    let masterRepo = { Configuration.Master.Repository.Name = ""
                       Configuration.Master.Repository.Uri = Helpers.Env.MasterRepository () }
    Tools.Git.Clone masterRepo wsDir false |> Helpers.IO.CheckResponseCode

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


let Exec (cmd : CLI.Commands.ExecCommand) =
    let wsDir = Env.WorkspaceDir()
    let config = Configuration.Master.Load wsDir
    for repo in config.Repositories do
        let repoDir = wsDir |> GetDirectory repo.Name
        if repoDir.Exists then
            let vars = [ "SBS_REPO_NAME", repo.Name
                         "SBS_REPO_PATH", repoDir.FullName
                         "SBS_REPO_URL", repo.Uri
                         "SBS_WKS", wsDir.FullName ] |> Map.ofSeq
            let args = sprintf @"/c ""%s""" cmd.Command

            try
                Console.PrintInfo repo.Name

                Exec.Exec "cmd" args repoDir vars |> IO.CheckResponseCode
            with
                e -> printfn "*** %s" e.Message

let Doctor () =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    
    let mutable missingRepos : Map<string, string list> = Map.empty

    // check all dependencies are available for each repository
    for repo in config.Repositories do
        let repoDir = wsDir |> GetDirectory repo.Name
        if repoDir |> Exists then
            let repoConfig = Configuration.Repository.Load wsDir repo.Name config
            for dependency in repoConfig.Dependencies do
                let depDir = wsDir |> GetDirectory dependency.Name
                if depDir |> Exists |> not then
                    let missingEntry = match dependency.Name |> missingRepos.TryFind with
                                       | Some x -> repo.Name :: x
                                       | None -> [repo.Name]
                    missingRepos <- missingRepos |> Map.add dependency.Name missingEntry
    
    if missingRepos <> Map.empty then
        for missingRepo in missingRepos do
            let forRepos = System.String.Join(", ", missingRepo.Value |> Array.ofList)
            printfn "Missing repository %A as dependency for: %s" missingRepo.Key forRepos

    let hasNoError = missingRepos = Map.empty
    if hasNoError then printfn "No error detected!"   