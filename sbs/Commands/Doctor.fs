module Commands.Doctor
open Helpers
open Core.Repository


let private findMissingDependencies wsDir (config : Configuration.Master.Configuration) =
    seq {
        // check all dependencies are available for each repository
        for repo in config.Repositories do
            let repoDir = wsDir |> Fs.GetDirectory repo.Name
            if repoDir.Exists then
                let dependencies = { RepositoryName = repo.Name }.ScanDependencies wsDir config
                for dependency in dependencies do
                    let depDir = wsDir |> Fs.GetDirectory dependency.Name
                    if depDir.Exists |> not then
                        yield (dependency.Name, repo.Name)
    }

let Check () =
    try
        let wsDir = Env.WorkspaceDir()
        let config = wsDir |> Configuration.Master.Load
        let missingDeps = findMissingDependencies wsDir config
                            |> Seq.groupBy fst
 
        for (missingRepo, forRepos) in missingDeps do
            let forRepos = System.String.Join(", ", forRepos |> Seq.map snd |> Array.ofSeq)
            printfn "Missing repository %A as dependency for: %s" missingRepo forRepos

        let hasNoError = missingDeps = Seq.empty
        if hasNoError then printfn "No error detected!"   
    with
        exn -> printfn "Failure while running doctor: %s" exn.Message
   
