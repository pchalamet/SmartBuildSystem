module Commands.Sources
open Helpers
open Helpers.Fs
open Core.Repository

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
            { RepositoryName = repo.Name }.FindDependencies wsDir config
                |> Seq.map (fun x -> { info with CLI.Commands.Patterns = [x.Name]})
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


let rec private processRepositories (patterns : string Set) (deps : bool) action processedRepositories =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    let repos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) patterns
    for repo in repos do
        if processedRepositories |> Set.contains repo |> not then
            let repoDir = wsDir |> Fs.GetDirectory repo.Name
            if repoDir.Exists && (processedRepositories |> Set.contains repo |> not) then
                action repo wsDir

    let newProcessedRepositories = 
        if deps && repos <> Set.empty then
            let newPatterns = (repos |> Set.map (fun x -> { RepositoryName = x.Name }.FindDependencies wsDir config)
                                     |> Set.unionMany)
                              - processedRepositories
                              - repos
                              |> Set.map (fun x -> x.Name)
            processRepositories newPatterns deps action (processedRepositories |> Set.union repos)
        else
            repos

    processedRepositories |> Set.union newProcessedRepositories


let Pull (info : CLI.Commands.PullRepositories) =
    let doPull (repo : Configuration.Master.Repository) wsDir = 
        Helpers.Console.PrintInfo (sprintf "Pulling repository %A" repo.Name) 
        Tools.Git.Pull repo wsDir |> Helpers.IO.CheckResponseCode
    processRepositories (info.Patterns |> Set.ofList) info.Dependencies doPull Set.empty |> ignore

let Dependencies (info : CLI.Commands.Dependencies) =
    let doDisplay (repo : Configuration.Master.Repository) wsDir =
        printfn "%s" repo.Name
    processRepositories (info.Patterns |> Set.ofList) true doDisplay Set.empty |> ignore
