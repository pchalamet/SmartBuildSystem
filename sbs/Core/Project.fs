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

    scanProjects Map.empty rootPath
