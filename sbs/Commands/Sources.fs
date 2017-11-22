module Commands.Sources
open Helpers

let rec Clone (info : CLI.Commands.CloneRepository) =
    let wsDir = Helpers.Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load

    // clone repository if necessary
    let repo = config.Repositories |> Seq.find (fun x -> x.Name = info.Name)
    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    if repoDir.Exists |> not then
        Helpers.Console.DisplayInfo (sprintf "Cloning repository %A" repo.Name) 
        Core.Git.GitClone repo wsDir "master" info.Shallow |> Helpers.IO.CheckResponseCode

    // clone dependencies
    if info.Dependencies then
        let repoConfig = Configuration.Repository.Load wsDir repo.Name config
        repoConfig.Dependencies |> Seq.iter (fun x -> { info with CLI.Commands.CloneRepository.Name = x.Name}
                                                            |> Clone )

let Build (info : CLI.Commands.BuildRepository) =
    ()

