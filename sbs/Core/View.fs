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




type View =
    { Name : string
      Patterns : string set
      Dependencies : bool
      Repositories : Configuration.Master.Repository set 
      Projects : string set }
with
    static member Materialize (wsDir : DirectoryInfo) (config : Configuration.Master.Configuration) (name : string) (patterns : string set) (dependencies : bool) =
        let selectedRepos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) patterns
        let repos = if dependencies then gatherDependencies wsDir config selectedRepos
                    else selectedRepos
        let projects = repos |> Seq.fold (fun s t -> Seq.append s (gatherProjects wsDir t)) Seq.empty
                             |> Seq.map (fun x -> x.FullName)
                             |> Set.ofSeq
        { Name = name
          Patterns = patterns
          Dependencies = dependencies
          Repositories = repos
          Projects = projects }

    member this.Save (wsDir : DirectoryInfo) =
        let viewFile = wsDir |> Fs.GetFile (sprintf "%s.view" this.Name)
        File.WriteAllLines(viewFile.FullName, this.Repositories |> Seq.map (fun x -> x.Name))
