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
    { Configuration.Dependencies = from.repositories |> Seq.map (fun x -> repoMap.[x]) |> set }


let Load (wsDir : DirectoryInfo) (repoName : string) (masterConfig : Master.Configuration) =
    let config = RepositoryConfig()
    let content = wsDir |> GetDirectory repoName
                        |> GetFile "build.yaml"
                        |> ReadAllText
    content |> config.LoadText
    config |> convert masterConfig

