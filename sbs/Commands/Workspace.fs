module Commands.Workspace
open System.IO
open Helpers
open Helpers.Collections
open Helpers.Fs

let InitWorkspace (cmd : CLI.Commands.InitWorkspace) =
    let wsDir = cmd.Path |> DirectoryInfo
    if wsDir.Exists then failwithf "Workspace already exists"

    // first clone master repository
    let masterRepo = { Configuration.Master.Repository.Name = ".sbs"
                       Configuration.Master.Repository.Uri = Helpers.Env.MasterRepository () }
    Tools.Git.Clone masterRepo wsDir false
        |> Helpers.IO.CheckResponseCode

    let currentDir = System.Environment.CurrentDirectory
    try
        System.Environment.CurrentDirectory <- wsDir.FullName

        // delegate to clone then to get dependencies
        let cloneRepo = { CLI.Commands.CloneRepository.Patterns = [masterRepo.Name]
                          CLI.Commands.CloneRepository.Shallow = false
                          CLI.Commands.CloneRepository.Dependencies = true }
        cloneRepo |> Commands.Sources.Clone
    finally
        System.Environment.CurrentDirectory <- currentDir



let rec private gatherDependencies wsDir (config : Configuration.Master.Configuration) (closure : Configuration.Master.Repository set) =
    let getNewDependencies (repo : Configuration.Master.Repository) =
        let repoConfig = Configuration.Repository.Load wsDir repo.Name config
        repoConfig.Dependencies - repoConfig.Dependencies

    // gather all new dependencies first
    let repoToGather = closure |> Set.fold (fun s t -> s + (getNewDependencies t)) closure
    if repoToGather <> closure then gatherDependencies wsDir config repoToGather
    else closure
    
let private gatherProjects wsDir (repo : Configuration.Master.Repository) =
    let repoDir = wsDir |> GetDirectory repo.Name
    repoDir.EnumerateFiles("*.csproj", SearchOption.AllDirectories) |> List.ofSeq





let private projectToProjectType (file : FileInfo) =
    let ext2projType = Map [ (".csproj", "fae04ec0-301f-11d3-bf4b-00c04f79efbc")
                             (".fsproj", "f2a71f9b-5d33-465a-a702-920d77279786")
                             (".vbproj", "f184b08f-c81c-45f6-a57f-5abd9991f28f") 
                             (".pssproj", "f5034706-568f-408a-b7b3-4d38c6db8a32")
                             (".sqlproj", "00D1A9C2-B5F0-4AF3-8072-F6C62B433612")]
    let prjType = ext2projType.[file.Extension]
    prjType



let private generateSolutionContent (wsDir : string) (projects : FileInfo list) =
    let extractRepository (file : string) =
        let partial = file.Substring(wsDir.Length+1)
        let idx = partial.IndexOf(System.IO.Path.DirectorySeparatorChar)
        partial.Substring(0, idx)

    seq {
        yield "Microsoft Visual Studio Solution File, Format Version 12.00"
        yield "# Visual Studio 14"

        let guids = projects 
                        |> List.map (fun x -> x.FullName, x.FullName 
                                                            |> Helpers.Text.GenerateGuidFromString 
                                                            |> Helpers.Text.ToVSGuid)
                        |> Map

        let repos = projects
                        |> List.map (fun x -> x.FullName, x.FullName |> extractRepository)
                        |> Map

        let repoGuids = repos
                            |> Seq.map (fun kvp -> kvp.Value, kvp.Value |> Helpers.Text.GenerateGuidFromString |> Helpers.Text.ToVSGuid)
                            |> Map

        for project in projects do
            let fileName = project.FullName
            yield sprintf @"Project(""{%s}"") = ""%s"", ""%s"", ""{%s}"""
                  (projectToProjectType project)
                  (fileName |> Path.GetFileNameWithoutExtension)
                  fileName
                  (guids.[fileName])
            yield "EndProject"

        for repository in repoGuids do
            let repo = repository.Key
            let guid = repository.Value

            yield sprintf @"Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = %A, %A, ""{%s}""" repo repo guid
            yield "EndProject"

        yield "Global"
        yield "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution"
        yield "\t\tDebug|Any CPU = Debug|Any CPU"
        yield "\t\tRelease|Any CPU = Release|Any CPU"
        yield "\tEndGlobalSection"
        yield "\tGlobalSection(ProjectConfigurationPlatforms) = postSolution"

        for project in projects do
            let guid = guids.[project.FullName]
            yield sprintf "\t\t{%s}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" guid
            yield sprintf "\t\t{%s}.Debug|Any CPU.Build.0 = Debug|Any CPU" guid
            yield sprintf "\t\t{%s}.Release|Any CPU.ActiveCfg = Release|Any CPU" guid
            yield sprintf "\t\t{%s}.Release|Any CPU.Build.0 = Release|Any CPU" guid

        yield "\tEndGlobalSection"

        yield "\tGlobalSection(NestedProjects) = preSolution"
        for project in projects do
            let projectFileName = project.FullName
            let projectGuid = guids.[projectFileName]
            let repoFileName = repos.[projectFileName]
            let repoGuid = repoGuids.[repoFileName]
            yield sprintf "\t\t{%s} = {%s}" projectGuid repoGuid
        yield "\tEndGlobalSection"

        yield "EndGlobal"
    }




let private generateSolution (wsDir : DirectoryInfo) name (projects : FileInfo list) =
    let content = generateSolutionContent (wsDir.FullName) projects
    let slnFileName = sprintf "%s.sln" name
    let sln = wsDir |> Fs.GetFile slnFileName
    File.WriteAllLines(sln.FullName, content)


let CreateView (cmd : CLI.Commands.CreateView) =
    let wsDir = Env.WorkspaceDir()
    let config = wsDir |> Configuration.Master.Load

    let selectedRepos = Helpers.Text.FilterMatch (config.Repositories) (fun x -> x.Name) (cmd.Patterns |> Set)
    let repos = if cmd.Dependencies then gatherDependencies wsDir config selectedRepos
                else selectedRepos
    let projects = repos |> Seq.fold (fun s t -> s @ (gatherProjects wsDir t)) List.empty
    generateSolution wsDir cmd.Name projects

    
let Build (cmd : CLI.Commands.BuildView) =
    let wsDir = Env.WorkspaceDir()

    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Building view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.MsBuild.Build cmd.Clean cmd.Config wsDir sln
