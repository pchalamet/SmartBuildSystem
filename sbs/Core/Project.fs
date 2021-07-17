module Core.Project
open Helpers.Xml
open Helpers.Collections
open System.Xml.Linq
open Helpers
open System.IO

type Project =
    { Files: string set
      Projects: string set }

type Projects = Map<string, Project>


type FileProject =
    { File: string
      Project: Project }



let FindProjects rootPath =
    let validExtensions = set [ ".fsproj"; ".csproj"; ".sqlproj"; ".vbproj"; ".pssproj"; "dcproj" ]

    let isProjectFile (filename: string) =
        let extension = System.IO.Path.GetExtension(filename)
        validExtensions |> Set.contains extension

    let rec scanProjects (projects: Map<string, Project>) (folder: string) =
        let parseProject projects folder (projectFile: string) =
            let projectFile = Fs.GetFullPath projectFile            
            match projects |> Map.tryFind projectFile with
            | Some _ -> projects
            | None -> let xdoc = XDocument.Load projectFile
                      let queryFiles item =
                            xdoc.Descendants(XNamespace.None + item)
                                |> Seq.map (fun x -> !> x.Attribute(XNamespace.None + "Include") : string)
                                |> Seq.map (fun x -> Path.Combine(folder, x) |> Fs.GetFullPath)
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

    let projects = scanProjects Map.empty rootPath
                        |> Seq.map (fun (KeyValue(file, project)) -> { File = file; Project = project }) |> Set.ofSeq
    projects


let ComputeClosure rootPath selector projects =
    // find requested projects
    let repoPath = Path.Combine(rootPath, selector)
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

    closureProjects, closureTestProjects

