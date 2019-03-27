module Configuration.Master
open Helpers.Collections
open Helpers.Fs
open System.IO
open FSharp.Configuration

type MasterConfig = YamlConfig<"Examples/master.yaml">


[<RequireQualifiedAccess>]
type Repository =
    { Name : string
      Uri : string }

[<RequireQualifiedAccess>]
type Configuration =
    { Repositories : Repository set }
with
    member this.GetRepository name =
        this.Repositories |> Seq.tryFind (fun x -> x.Name = name)


let Load (wsDir : DirectoryInfo) =
    let masterConfigFile = wsDir |> GetFile "sbs.yaml"
    use file = System.IO.File.OpenText(masterConfigFile.FullName)
    let config = MasterConfig()
    config.Load(masterConfigFile.FullName)

    let convertRepo (repoConfig : MasterConfig.repositories_Item_Type) =
        if repoConfig.name |> isNull || repoConfig.uri |> isNull then failwithf "sbs.yaml is invalid"

        { Repository.Name = repoConfig.name
          Repository.Uri = repoConfig.uri }

    { Configuration.Repositories = config.repositories |> Seq.map convertRepo |> Set }
