module Commands.View
open System.IO
open System.Xml.Linq
open Helpers
open Helpers.Xml
open Helpers.Collections



type Project =
    { Files: string set
      Projects: string set }

type Projects = Map<string, Project>



let private generateSolution (wsDir : DirectoryInfo) name (projects : FileInfo seq) =
    let content = Core.Solution.GenerateSolutionContent (wsDir.FullName) projects
    let slnFileName = sprintf "%s.sln" name
    let sln = wsDir |> Fs.GetFile slnFileName
    File.WriteAllLines(sln.FullName, content)




let Create (cmd : CLI.Commands.CreateView) =
    let validExtensions = set [ ".fsproj"; ".csproj"; ".sqlproj"; ".vbproj"; ".pssproj"; "dcproj" ]

    let getFullPath (file: string) = Path.GetFullPath(file.Replace("\\", "/"))

    let isProjectFile (filename: string) =
        let extension = System.IO.Path.GetExtension(filename)
        validExtensions |> Set.contains extension

    let rec scanProjects (projects: Map<string, Project>) (folder: string) =
        let parseProject projects folder (projectFile: string) =
            let projectFile = getFullPath projectFile            
            match projects |> Map.tryFind projectFile with
            | Some _ -> projects
            | None -> let xdoc = XDocument.Load projectFile
                      let queryFiles item =
                            xdoc.Descendants(XNamespace.None + item)
                                |> Seq.map (fun x -> !> x.Attribute(XNamespace.None + "Include") : string)
                                |> Seq.map (fun x -> Path.Combine(folder, x) |> getFullPath)
                                |> Set.ofSeq

                      let files = (queryFiles "None") + (queryFiles "Compile") + (queryFiles "Content") + (set [projectFile])
                      let projectDeps = queryFiles "ProjectReference"
                      let newProject = { Files = files; Projects = projectDeps }
                      projects |> Map.add projectFile newProject

        let projects = System.IO.Directory.EnumerateFiles(folder)
                            |> Seq.filter isProjectFile
                            |> Seq.fold (fun projects projectFile -> parseProject projects folder projectFile) projects

        let projects = System.IO.Directory.EnumerateDirectories(folder)
                            |> Seq.fold (fun projects folder -> scanProjects projects folder) projects
        projects

    let rootPath = System.Environment.CurrentDirectory
    let repoPath = Path.Combine(rootPath, cmd.Pattern)
    let viewPath = Path.Combine(repoPath, "*") |> getFullPath

    // identify all projects
    let projects = scanProjects Map.empty rootPath
    let jsonProjects = Helpers.Json.Serialize projects
    File.WriteAllText("projects.json", jsonProjects)

    // find requested projects
    let checkViewProject file = Text.Match file viewPath

    let viewProjects = projects |> Map.filter (fun _ project -> project.Files |> Seq.exists checkViewProject)
                                |> Seq.map (fun (KeyValue(file, project)) -> {| File = file; Project = project |})
                                |> Set.ofSeq
    let getProjectKey (fp : {| File: string; Project: Project |}) = fp.File
    let getProjectDeps (fp: {| File: string; Project: Project |}) =
        projects |> Seq.choose (fun (KeyValue(file, project)) -> if fp.Project.Projects |> Set.contains file then Some {| File = file; Project = project |}
                                                                 else None) |> Set.ofSeq
    let noProjects (_) = Set.empty
    let closureProjects = Algorithm.Closure viewProjects getProjectKey getProjectDeps noProjects

    let closureTestProjects = 
        let closureTestProjectKeys = closureProjects |> Set.map (fun x -> Path.GetFileNameWithoutExtension(x.File) + ".tests")
        projects |> Seq.choose (fun (KeyValue(file, project)) -> let testfile = Path.GetFileNameWithoutExtension(file)
                                                                 if closureTestProjectKeys |> Set.contains testfile then Some {| File = file; Project = project |}
                                                                 else None)
                 |> Set.ofSeq

    // get latest changes
    let latestChangesResult = Tools.Git.LatestChanges (DirectoryInfo(rootPath))
    let latestChanges = latestChangesResult.Out |> List.map (fun file -> Path.Combine(rootPath, file) |> getFullPath)

    // compute impacted projects
    let fileImpacted filePattern = latestChanges |> List.exists (fun impactedFile -> Text.Match impactedFile filePattern)
    let impactedProjects = projects |> Map.filter (fun _ project -> project.Files |> Seq.exists fileImpacted)
                                    |> Seq.map (fun (KeyValue(file, project)) -> {| File = file; Project = project |})
                                    |> Set.ofSeq
    let impactedProjects = Set.intersect closureProjects impactedProjects

    let projectFiles = (closureProjects + closureTestProjects) |> Seq.map (fun p -> FileInfo(p.File)) |> List.ofSeq
    generateSolution (DirectoryInfo rootPath) cmd.Name projectFiles

    // compute impacted projects
    if impactedProjects <> Set.empty then 0
    else
        printfn "INFO: build is not required"
        5
    
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

