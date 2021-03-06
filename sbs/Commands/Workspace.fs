﻿module Commands.Workspace
open System.IO
open Helpers
open Helpers.Collections
open Helpers.Fs
open System.Linq
open Core.Repository

let Init (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    let isWorkspaceNotEmpty = wsDir.Exists 
                              && (wsDir.EnumerateFiles().Count() > 0 || wsDir.EnumerateDirectories().Count() > 0)
    if isWorkspaceNotEmpty then failwithf "Workspace already exists"
    wsDir |> EnsureExists |> ignore

    // first clone master repository
    let masterUri = cmd.Uri
    let masterRepo = { Configuration.Master.Repository.Name = ""
                       Configuration.Master.Repository.Uri = masterUri }
    Tools.Git.Clone masterRepo wsDir false None |> Helpers.IO.CheckResponseCode

    let currentDir = System.Environment.CurrentDirectory
    try
        System.Environment.CurrentDirectory <- wsDir.FullName

        // delegate to clone then to get dependencies
        let cloneRepo = { CLI.Commands.CloneRepository.Patterns = [masterRepo.Name]
                          CLI.Commands.CloneRepository.Shallow = false
                          CLI.Commands.CloneRepository.Dependencies = true 
                          CLI.Commands.CloneRepository.Branch = None }
        cloneRepo |> Commands.Sources.Clone
    finally
        System.Environment.CurrentDirectory <- currentDir


let Exec (cmd : CLI.Commands.ExecCommand) =
    let wsDir = Env.WorkspaceDir()
    let config = Configuration.Master.Load wsDir
    let repos = Helpers.Text.FilterMatch (config.Repositories |> Set) (fun x -> x.Name) (cmd.Patterns |> Set)
    for repo in repos do
        let repoDir = wsDir |> GetDirectory repo.Name
        if repoDir.Exists then
            let vars = [ "SBS_REPO_NAME", repo.Name
                         "SBS_REPO_PATH", repoDir.FullName
                         "SBS_REPO_URL", repo.Uri
                         "SBS_WKS", wsDir.FullName ] |> Map.ofSeq

            try
                Console.PrintInfo repo.Name

                let shell, carry = if Env.IsWindows() then "cmd", "/c"
                                   else "sh", "-c"
                let args = sprintf @"%s ""%s""" carry cmd.Command

                Exec.Exec shell args repoDir vars |> IO.CheckResponseCode
            with
                e -> printfn "*** %s" e.Message
