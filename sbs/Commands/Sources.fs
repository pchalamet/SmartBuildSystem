module Commands.Sources
open Helpers
open Helpers.Fs
open Helpers.Collections
open Core.Repository
open System.IO

let rec private innerProcessRepositories (wsDir : DirectoryInfo) (config : Configuration.Master.Configuration) (patterns : string Set) (deps : bool) action processedRepositories =
    let repos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) patterns
    for repo in repos do
        if processedRepositories |> Set.contains repo |> not then
            action repo wsDir

    let newProcessedRepositories = 
        if deps && repos <> Set.empty then
            let newPatterns = (repos |> Set.map (fun x -> { RepositoryName = x.Name }.FindDependencies wsDir config)
                                     |> Set.unionMany)
                              |> Set.substract processedRepositories
                              |> Set.substract repos
                              |> Set.map (fun x -> x.Name)
            innerProcessRepositories wsDir config newPatterns deps action (processedRepositories |> Set.union repos)
        else
            repos

    processedRepositories |> Set.union newProcessedRepositories


let private processRepositories (patterns : string Set) (deps : bool) action processedRepositories =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    innerProcessRepositories wsDir config patterns deps action processedRepositories


let Clone (info : CLI.Commands.CloneRepository) =
    let doClone (repo : Configuration.Master.Repository) wsDir =
        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        if repoDir.Exists |> not then
            Helpers.Console.PrintInfo (sprintf "Cloning repository %A" repo.Name) 
            Tools.Git.Clone repo wsDir info.Shallow |> Helpers.IO.CheckResponseCode

    processRepositories (info.Patterns |> Set.ofList) info.Dependencies doClone Set.empty |> ignore


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


let Pull (info : CLI.Commands.PullRepositories) =
    let doPull (repo : Configuration.Master.Repository) wsDir = 
        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        if repoDir.Exists then
            Helpers.Console.PrintInfo (sprintf "Pulling repository %A" repo.Name) 
            Tools.Git.Pull repo wsDir |> Helpers.IO.CheckResponseCode

    processRepositories (info.Patterns |> Set.ofList) info.Dependencies doPull Set.empty |> ignore
