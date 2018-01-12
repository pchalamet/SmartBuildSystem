﻿module Configuration.Repository
open System.IO
open System
open Helpers


[<AllowNullLiteralAttribute>]
type RepositorySources() = class
    member val ``auto-dependencies`` : bool = true with get, set 
    member val dependencies : string array = null with get, set
end


type RepositoryArtifacts() = class
    member val name : string = null with get, set 
    member val ``type`` : string = null with get, set
end


type RepositoryConfiguration() = class
    member val sources : RepositorySources = null with get, set
    member val artifacts : RepositoryArtifacts array = null with get, set
end


let Load (repoDir : DirectoryInfo) =
    if repoDir.Exists |> not then failwithf "Repository %A is not cloned" repoDir.Name
    let repoConfig = repoDir |> Fs.GetFile "repository.yaml"
    if repoConfig.Exists |> not then (true, Seq.empty)
    else 
        try
            use file = System.IO.File.OpenText(repoConfig.FullName)
            let serializer = new SharpYaml.Serialization.Serializer()
            let repoConfig = serializer.Deserialize<RepositoryConfiguration>(file)
            if repoConfig.sources |> isNull then failwithf "sources is mandatory"
            if repoConfig.sources.dependencies |> isNull then failwithf "dependencies list is mandatory"
            if repoConfig.artifacts |> isNull then failwithf "artifacts is mandatory"

            (repoConfig.sources.``auto-dependencies``, repoConfig.sources.dependencies |> seq)
        with
            exn -> failwithf "repository.yaml in %A is invalid (%s)" repoDir exn.Message
 