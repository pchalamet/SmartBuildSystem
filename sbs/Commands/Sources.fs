module Commands.Sources
open Helpers
open Helpers.Fs
open Helpers.Collections
open Core.Repository
open System.IO

let rec private innerProcessRepositories (wsDir : DirectoryInfo) (config : Configuration.Master.Configuration) (patterns : string Set) (deps : bool) action processedRepositories =
    let repos = Helpers.Text.FilterMatch (config.Repositories |> Set) (fun x -> x.Name) patterns
    let repoToProcess = Set.difference repos processedRepositories
    let results = Threading.ParExec (fun repo -> async { return action repo wsDir }) repoToProcess
    results |> IO.CheckMultipleResponseCode

    let newProcessedRepositories = 
        if deps && repos <> Set.empty then
            let newPatterns = (repos |> Set.map (fun x -> FindDependencies wsDir config x.Name)
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
            Tools.Git.Clone repo wsDir info.Shallow info.Branch
        else
            sprintf "Project %s is already cloned" repo.Name |> IO.ResultOk

    processRepositories (info.Patterns |> Set.ofList) info.Dependencies doClone Set.empty |> ignore


let Checkout (info : CLI.Commands.CheckoutRepositories) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load
    let repos = config.Repositories 
                    |> Seq.filter (fun x -> wsDir |> GetDirectory x.Name |> Exists)
    for repo in repos do
        repo.Name |> Helpers.Console.PrintInfo
        let hasError = Tools.Git.Checkout repo wsDir info.Branch
        match hasError |> IO.ResultToError with
        | Some err -> err |> Helpers.Console.PrintError
        | None -> ()

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
            Tools.Git.Pull repo wsDir
        else
            IO.ResultOk ""

    processRepositories (info.Patterns |> Set.ofList) info.Dependencies doPull Set.empty |> ignore
