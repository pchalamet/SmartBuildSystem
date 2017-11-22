module Configuration.Master
open Helpers.Collections
open Helpers.Fs
open System.IO

[<RequireQualifiedAccess>]
type Repository =
    { Name : string
      Uri : string }

[<RequireQualifiedAccess>]
type Configuration =
    { Repositories : Repository set }




type private MasterConfig = FSharp.Configuration.YamlConfig<"Examples/Master.yaml">



let private convert (from : MasterConfig) =
    { Configuration.Repositories = from.repositories |> Seq.map (fun x -> { Repository.Name = x.name
                                                                            Repository.Uri = x.uri })
                                                     |> set }

let Load (wsDir : DirectoryInfo) =
    let config = MasterConfig()
    let content = wsDir |> GetDirectory ".sbs"
                        |> GetFile "build.yaml"
                        |> ReadAllText
    content |> config.LoadText
    config |> convert

