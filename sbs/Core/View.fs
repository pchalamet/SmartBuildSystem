module Core.View
open System.IO
open Helpers
open Helpers.Collections
open Core.Repository
open System.Collections.Generic



let private gatherRepoDependencies wsDir (config : Configuration.Master.Configuration) (repo : Configuration.Master.Repository) =
    { RepositoryName = repo.Name }.FindDependencies wsDir config |> Set.toSeq

let rec private gatherDependencies wsDir (config : Configuration.Master.Configuration) (repos : Configuration.Master.Repository set) (processedRepos : Configuration.Master.Repository set) =
    if repos <> Set.empty then
        printfn "Scanning %s" (System.String.Join(", ", repos |> Seq.map (fun x -> x.Name)))

        let deps = repos |> Seq.fold (fun s t -> s |> Map.add t (gatherRepoDependencies wsDir config t)) Map.empty
        let newRepos = deps |> Seq.map (fun x -> x.Value)
                            |> Seq.concat
                            |> Set

        let newProcessedRepos = processedRepos + repos
        let unprocessedRepos = newRepos - newProcessedRepos
        deps |> Map.union (gatherDependencies wsDir config unprocessedRepos newProcessedRepos)
    else
        Map.empty


let private gatherProjects wsDir (repo : Configuration.Master.Repository) =
    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    let enumerateExtensions ext = repoDir.EnumerateFiles("*" + ext, SearchOption.AllDirectories)
    Core.Project.SupportedProjectExtensions
        |> Seq.fold (fun s t -> Seq.append s (enumerateExtensions t)) Seq.empty



type View =
    { Name : string
      Patterns : string set
      WithDependencies : bool
      Dependencies : Map<Configuration.Master.Repository, Configuration.Master.Repository seq>
      Projects : string set }
with
    static member Materialize (wsDir : DirectoryInfo) (config : Configuration.Master.Configuration) (name : string) (patterns : string set) (withDependencies : bool) =
        let selectedRepos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) patterns
        let dependencies = if withDependencies then gatherDependencies wsDir config selectedRepos Set.empty
                           else selectedRepos |> Seq.map (fun x -> x, Seq.empty) |> Map
        let repos = dependencies |> Seq.map (fun x -> x.Key)
        let projects = repos |> Seq.fold (fun s t -> Seq.append s (gatherProjects wsDir t)) Seq.empty
                             |> Seq.map (fun x -> x.FullName)
                             |> Set.ofSeq
        { Name = name
          Patterns = patterns
          WithDependencies = withDependencies
          Dependencies = dependencies
          Projects = projects }

    member this.Save (wsDir : DirectoryInfo) =
        let viewFile = wsDir |> Fs.GetFile (sprintf "%s.view" this.Name)
        let lines = this.Dependencies |> Seq.map (fun x -> let deps = x.Value |> Seq.map (fun x -> x.Name)
                                                           let sdeps = System.String.Join(", ", deps)
                                                           sprintf "%s <- %s" x.Key.Name sdeps)
                                                           
        File.WriteAllLines(viewFile.FullName, lines)
