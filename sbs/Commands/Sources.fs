module Commands.Sources
open Helpers
open Helpers.Fs

let rec private cloneRepository wsDir (config : Configuration.Master.Configuration) (info : CLI.Commands.CloneRepository) =
    // clone repository if necessary
    let repos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) (info.Patterns |> Set)
    for repo in repos do
        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        if repoDir.Exists |> not then
            Helpers.Console.PrintInfo (sprintf "Cloning repository %A" repo.Name) 
            Tools.Git.Clone repo wsDir info.Shallow |> Helpers.IO.CheckResponseCode

        // clone dependencies
        if info.Dependencies then
            let repoConfig = Configuration.Repository.Load wsDir repo.Name config
            repoConfig.Dependencies |> Seq.map (fun x -> { info with CLI.Commands.Patterns = [x.Name]})
                                    |> Seq.iter (cloneRepository wsDir config)

    if repos = Set.empty then printfn "Warning: empty selection specified"

let Clone (info : CLI.Commands.CloneRepository) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    cloneRepository wsDir config info

let Checkout (info : CLI.Commands.CheckoutRepositories) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    let allres = config.Repositories 
                    |> Seq.filter (fun x -> wsDir |> GetDirectory x.Name |> Exists)
                    |> Seq.map (fun x -> x, Tools.Git.Checkout x wsDir info.Branch)
    for (repo,res) in allres do
        if res.Code <> 0 then Helpers.Console.PrintError repo.Name
        else Helpers.Console.PrintSuccess repo.Name

let Fetch () =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    let repos = config.Repositories
    for repo in repos do
        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        if repoDir.Exists then
            Helpers.Console.PrintInfo (sprintf "Fetching repository %A" repo.Name) 
            Tools.Git.Fetch repo wsDir |> Helpers.IO.CheckResponseCode

let Pull () =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    let repos = config.Repositories
    for repo in repos do
        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        if repoDir.Exists then
            Helpers.Console.PrintInfo (sprintf "Pulling repository %A" repo.Name) 
            Tools.Git.Pull repo wsDir |> Helpers.IO.CheckResponseCode
