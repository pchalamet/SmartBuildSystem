module Commands.View
open System.IO
open System.Xml.Linq
open Helpers
open Helpers.Xml
open Helpers.Collections
open Core.Project


type FileProject =
    { File: string
      Project: Project }


let private generateSolution (wsDir : DirectoryInfo) name projectFiles =
    let projects = projectFiles |> List.map FileInfo
    let content = Core.Solution.GenerateSolutionContent (wsDir.FullName) projects
    let slnFileName = sprintf "%s.sln" name
    let sln = wsDir |> Fs.GetFile slnFileName
    File.WriteAllLines(sln.FullName, content)


let Create (cmd : CLI.Commands.CreateView) =

    let rootPath = System.Environment.CurrentDirectory

    let projects = Core.Project.FindProjects rootPath
    File.WriteAllText("projects.json", Helpers.Json.Serialize projects)

    let projects = projects |> Seq.map (fun (KeyValue(file, project)) -> { File = file; Project = project }) |> Set.ofSeq

    // find requested projects
    let repoPath = Path.Combine(rootPath, cmd.Pattern)
    let viewPath = Path.Combine(repoPath, "*") |> Fs.GetFullPath
    let checkViewProject file = Text.Match file viewPath

    let viewProjects = projects |> Set.filter (fun fp -> fp.Project.Files |> Seq.exists checkViewProject)
    let getProjectKey (fp: FileProject) = fp.File
    let getProjectDeps (fp: FileProject) = projects |> Set.filter (fun ofp -> fp.Project.Projects |> Set.contains ofp.File)
    let noProjects (_) = Set.empty
    let closureProjects = Algorithm.Closure viewProjects getProjectKey getProjectDeps noProjects

    let closureTestProjects = 
        let closureTestProjectKeys = closureProjects |> Set.map (fun x -> Path.GetFileNameWithoutExtension(x.File) + ".tests")
        projects |> Set.filter (fun (fp: FileProject) -> let testfile = Path.GetFileNameWithoutExtension(fp.File)
                                                         closureTestProjectKeys |> Set.contains testfile)

    // get latest changes
    let latestChangesResult = rootPath |> DirectoryInfo |> Tools.Git.LatestChanges
    let latestChanges = latestChangesResult.Out |> List.map (fun file -> Path.Combine(rootPath, file) |> Fs.GetFullPath)

    // compute impacted projects
    let fileImpacted filePattern = latestChanges |> List.exists (fun impactedFile -> Text.Match impactedFile filePattern)
    let impactedProjects = projects |> Set.filter (fun fp -> fp.Project.Files |> Seq.exists fileImpacted)
    let impactedProjects = Set.intersect closureProjects impactedProjects

    let projectFiles = (closureProjects + closureTestProjects) |> Set.map (fun p -> p.File) |> List.ofSeq
    generateSolution (DirectoryInfo rootPath) cmd.Name projectFiles

    // compute impacted projects
    if impactedProjects <> Set.empty then
        printfn "INFO: build is not required"
        5
    else
        0
    
let Build (cmd : CLI.Commands.BuildView) =
    let wsDir = System.Environment.CurrentDirectory |> DirectoryInfo
    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Building view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.MsBuild.Build cmd.Clean cmd.Parallel cmd.Config wsDir sln

let Test (cmd : CLI.Commands.TestView) =
    let wsDir = System.Environment.CurrentDirectory |> DirectoryInfo
    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Testing view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.MsBuild.Test cmd.Parallel cmd.Config wsDir sln


let Publish (cmd : CLI.Commands.PublishView) =
    let wsDir = Env.WorkspaceDir()
    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Publishing view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.Publish.Publish wsDir cmd.Name cmd.Config

let Open (cmd : CLI.Commands.OpenView) =
    let wsDir = System.Environment.CurrentDirectory |> DirectoryInfo
    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name
    Exec.Spawn sln.FullName "" "open"

