module Commands.Sources
open Helpers
open Helpers.Fs

let rec Clone (info : CLI.Commands.CloneRepository) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load

    // clone repository if necessary
    let repo = match config.Repositories |> Seq.tryFind (fun x -> x.Name = info.Name) with
               | Some x -> x
               | None -> failwithf "Repository %A does not exist" info.Name
    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    if repoDir.Exists |> not then
        Helpers.Console.DisplayInfo (sprintf "Cloning repository %A" repo.Name) 
        Core.Git.GitClone repo wsDir info.Shallow |> Helpers.IO.CheckResponseCode

    // clone dependencies
    if info.Dependencies then
        let repoConfig = Configuration.Repository.Load wsDir repo.Name config
        repoConfig.Dependencies |> Seq.map (fun x -> { info with CLI.Commands.CloneRepository.Name = x.Name})
                                |> Seq.iter Clone



let Build (info : CLI.Commands.BuildRepository) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load

    let repo = match config.Repositories |> Seq.tryFind (fun x -> x.Name = info.Name) with
               | Some x -> x
               | None -> failwithf "Repository %A does not exist" info.Name
    let repoDir = wsDir |> GetDirectory repo.Name
    if repoDir.Exists |> not then failwithf "Repository %A is not cloned" repo.Name
    let slns = repoDir.EnumerateFiles("*.sln", System.IO.SearchOption.AllDirectories)

    let buildRepo x = 
        Helpers.Console.DisplayInfo (sprintf "Building solution %A" x)
        Core.MsBuild.Build info.Clean info.Config wsDir x

    slns |> Seq.iter buildRepo
