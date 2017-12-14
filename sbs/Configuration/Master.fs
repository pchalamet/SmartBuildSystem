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



type RepositoryConfiguration() = class
    member val name : string = null with get, set
    member val uri : string = null with get, set
end

type MasterConfiguration() = class
    member val repositories : RepositoryConfiguration array = null with get, set 
end


let Load (wsDir : DirectoryInfo) =
    let masterConfigFile = wsDir |> GetFile "sbs.yaml"
    use file = System.IO.File.OpenText(masterConfigFile.FullName)
    let serializer = new SharpYaml.Serialization.Serializer()
    let masterConfig = serializer.Deserialize<MasterConfiguration>(file)

    let convertRepo (repoConfig : RepositoryConfiguration) =
        if repoConfig.name |> isNull || repoConfig.uri |> isNull then failwithf "master.yaml is invalid"
        { Repository.Name = repoConfig.name
          Repository.Uri = repoConfig.uri }

    { Configuration.Repositories = masterConfig.repositories |> Seq.map convertRepo |> Set }
