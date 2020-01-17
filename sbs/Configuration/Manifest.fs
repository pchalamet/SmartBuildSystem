module Configuration.Manifest
open Helpers.Collections
open Helpers.Fs
open System.IO

open YamlDotNet.Serialization


[<CLIMutable>]
[<RequireQualifiedAccess>]
type Manifest =
    { [<YamlMember(Alias="name")>] Name : string }


type ManifestConfiguration() = class
    member val app : string = null with get, set
end


let Load (file : FileInfo) =
    let yaml = System.IO.File.ReadAllText(file.FullName)
    let deserializer = DeserializerBuilder().Build()
    let manifest = deserializer.Deserialize<Manifest>(yaml)
    manifest

