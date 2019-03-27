module Configuration.Repository
open System.IO
open System
open Helpers
open FSharp.Configuration

type RepositoryConfig = YamlConfig<"Examples/repository.yaml">


let Load (repoDir : DirectoryInfo) =
    if repoDir.Exists |> not then failwithf "Repository %A is not cloned" repoDir.Name
    let repoConfig = repoDir |> Fs.GetFile "repository.yaml"
    if repoConfig.Exists |> not then (true, Seq.empty)
    else 
        try
            let config = RepositoryConfig()
            config.Load(repoConfig.FullName)

            (config.repository.``auto-dependencies``, config.repository.dependencies |> seq)
        with
            exn -> failwithf "repository.yaml in %A is invalid (%s)" repoDir exn.Message
 