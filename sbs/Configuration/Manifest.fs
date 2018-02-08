module Configuration.Manifest
open Helpers.Collections
open Helpers.Fs
open System.IO


type Manifest =
    { Name : string }


type ManifestConfiguration() = class
    member val app : string = null with get, set
end


let Load (file : FileInfo) =
    use file = System.IO.File.OpenText(file.FullName)
    let serializer = new SharpYaml.Serialization.Serializer()
    let manifestConfigFile = serializer.Deserialize<ManifestConfiguration>(file)

    { Name = manifestConfigFile.app }
