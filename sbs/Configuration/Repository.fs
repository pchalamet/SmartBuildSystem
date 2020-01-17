module Configuration.Repository
open System.IO
open System
open Helpers
open YamlDotNet.Serialization


[<CLIMutable>]
[<RequireQualifiedAccess>]
type Configuration =
    { [<YamlMember(Alias="auto-dependencies")>] AutoDependencies : bool
      [<YamlMember(Alias="dependencies")>] Dependencies : string array }
with
    member this.GetRepository name =
        this.Dependencies |> Seq.tryFind (fun x -> x = name)


let Load (repoDir : DirectoryInfo) =
    if repoDir.Exists |> not then failwithf "Repository %A is not cloned" repoDir.Name
    let repoConfig = repoDir |> Fs.GetFile "repository.yaml"
    if repoConfig.Exists |> not then
        { Configuration.AutoDependencies = true
          Configuration.Dependencies = Array.empty }
    else 
        let yaml = System.IO.File.ReadAllText(repoConfig.FullName)
        let deserializer = DeserializerBuilder().Build()
        let config = deserializer.Deserialize<Configuration>(yaml)
        config
