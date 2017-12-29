module Core.View
open System.IO
open Helpers
open Helpers.Collections
open Core.Repository


let rec private gatherDependencies wsDir (config : Configuration.Master.Configuration) (closure : Configuration.Master.Repository set) =
    let getDependencies (repo : Configuration.Master.Repository) (state : Configuration.Master.Repository set) =
        let newDependencies = { RepositoryName = repo.Name }.FindDependencies wsDir config
        state + newDependencies

    let repoToGather = closure |> Set.fold (fun s t -> getDependencies t s) closure
    if repoToGather <> closure then gatherDependencies wsDir config repoToGather
    else closure

    
let private gatherProjects wsDir (repo : Configuration.Master.Repository) =
    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    let enumerateExtensions ext = repoDir.EnumerateFiles("*" + ext, SearchOption.AllDirectories)
    Core.Project.SupportedProjectExtensions
        |> Seq.fold (fun s t -> Seq.append s (enumerateExtensions t)) Seq.empty



[<RequireQualifiedAccess>]
type View =
    { RepositorySelector : string list 
      SelectorOnly : bool }
with
    member this.SelectedProjects (wsDir : DirectoryInfo) (masterConfig : Configuration.Master.Configuration) =
        let selectedRepos = Helpers.Text.FilterMatch (masterConfig.Repositories) (fun x -> x.Name) (this.RepositorySelector |> Set)
        let repos = if this.SelectorOnly then selectedRepos
                    else gatherDependencies wsDir masterConfig selectedRepos
        let projects = repos |> Seq.fold (fun s t -> Seq.append s (gatherProjects wsDir t)) Seq.empty
        projects
