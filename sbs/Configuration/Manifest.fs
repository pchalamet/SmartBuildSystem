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
    { Name = "pouet" }
    //use file = System.IO.File.OpenText(file.FullName)
    //let ystm = YamlStream()
    //ystm.Load(file)
    //let rootNode = ystm.Documents.[0].RootNode :?> YamlMappingNode
    //let appNode = rootNode.Children.[YamlScalarNode("app")]   
    //let appScalarNode = appNode :?> YamlScalarNode
    //{ Name = appScalarNode.Value }
    