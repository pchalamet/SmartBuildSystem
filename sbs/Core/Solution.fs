﻿module Core.Solution
open System.IO

let GenerateSolutionContent (wsDir : string) (projects : FileInfo seq) =
    let extractRepository (file : string) =
        let partial = file.Substring(wsDir.Length+1)
        let idx = partial.IndexOf(System.IO.Path.DirectorySeparatorChar)
        partial.Substring(0, idx)

    let string2guid s = s |> Helpers.Text.GenerateGuidFromString 
                          |> Helpers.Text.ToVSGuid

    let guids = projects |> Seq.map (fun x -> x.FullName, string2guid x.FullName)
                         |> Map

    let repos = projects |> Seq.map (fun x -> x.FullName, extractRepository x.FullName)
                         |> Map

    let repoGuids = repos |> Seq.map (fun kvp -> kvp.Value, string2guid kvp.Value)
                          |> Map

    seq {
        yield "Microsoft Visual Studio Solution File, Format Version 12.00"
        yield "# Visual Studio 14"

        for project in projects do
            let fileName = project.FullName
            let projectType = project.Extension |> Project.Ext2ProjectType
            match projectType with
            | Some prjType -> yield sprintf @"Project(""{%s}"") = ""%s"", ""%s"", ""{%s}"""
                                  prjType
                                  (fileName |> Path.GetFileNameWithoutExtension)
                                  fileName
                                  (guids.[fileName])
                              yield "EndProject"
            | None -> sprintf "Unsupported project %A" fileName |> Helpers.Console.PrintError

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


