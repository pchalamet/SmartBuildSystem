module Configuration.Repository
open Helpers.Collections
open Helpers.Fs
open System.IO

[<RequireQualifiedAccess>]
type Configuration =
    { Dependencies : Master.Repository set }


type private RepositoryConfig = FSharp.Configuration.YamlConfig<"Examples/Repository.yaml">


let private convert (masterConfig : Master.Configuration) (from : RepositoryConfig) =
    let repoMap = masterConfig.Repositories |> Seq.map (fun x -> x.Name, x) 
                                            |> Map
    { Configuration.Dependencies = from.dependencies |> Seq.map (fun x -> repoMap.[x]) |> set }


let Load (wsDir : DirectoryInfo) (repoName : string) (masterConfig : Master.Configuration) =
    // validate repo name
    let repo = match masterConfig.Repositories |> Seq.tryFind (fun x -> x.Name = repoName) with
               | Some x -> x
               | None -> failwithf "Repository %A does not exist" repoName

    let repoConfig = wsDir |> GetDirectory repo.Name
                           |> GetFile "repository.yaml"
    if repoConfig.Exists |> not then { Configuration.Dependencies = Set.empty }
    else
        // Load configuration
        let config = RepositoryConfig()
        repoConfig |> ReadAllText
                   |> config.LoadText
        config |> convert masterConfig
