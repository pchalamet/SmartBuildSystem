module Configuration.Repository
open System.IO
open System
open Helpers


type RepositoryConfiguration() = class
    member val ``auto-dependencies`` : bool = true with get, set 
    member val dependencies : string array = null with get, set
end


let Load (repoDir : DirectoryInfo) =
    let repoConfig = repoDir |> Fs.GetFile "repository.yaml"
    if repoConfig.Exists |> not then (true, Seq.empty)
    else 
        try
            use file = System.IO.File.OpenText(repoConfig.FullName)
            let serializer = new SharpYaml.Serialization.Serializer()
            let repoConfig = serializer.Deserialize<RepositoryConfiguration>(file)
            if repoConfig.dependencies |> isNull then failwithf "dependencies list is mandatory"

            (repoConfig.``auto-dependencies``, repoConfig.dependencies |> seq)
        with
            exn -> failwithf "repository.yaml in %A is invalid (%s)" repoDir exn.Message
 