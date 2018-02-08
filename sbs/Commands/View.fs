module Commands.View
open System.IO
open Helpers
open Helpers.Fs
open Core.View


let private generateSolution (wsDir : DirectoryInfo) name (projects : FileInfo seq) =
    let content = Core.Solution.GenerateSolutionContent (wsDir.FullName) projects
    let slnFileName = sprintf "%s.sln" name
    let sln = wsDir |> Fs.GetFile slnFileName
    File.WriteAllLines(sln.FullName, content)


let Create (cmd : CLI.Commands.CreateView) =
    let wsDir = Env.WorkspaceDir()
    let masterConfig = wsDir |> Configuration.Master.Load
    let view = View.Materialize wsDir masterConfig  cmd.Name (cmd.Patterns |> set) cmd.Dependencies
    generateSolution wsDir cmd.Name (view.Projects |> Seq.map FileInfo)
    view.Save wsDir

    if view.Projects = Set.empty then printfn "Warning: empty selection specified"

    
let Build (cmd : CLI.Commands.BuildView) =
    let wsDir = Env.WorkspaceDir()

    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Building view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.MsBuild.Build cmd.Clean cmd.Config wsDir sln

let Publish (cmd : CLI.Commands.PublishView) =
    let wsDir = Env.WorkspaceDir()

    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name

    sprintf "Publishing view %A" cmd.Name |> Helpers.Console.PrintInfo
    Tools.Publish.Publish wsDir cmd.Name cmd.Config

let Open (cmd : CLI.Commands.OpenView) =
    let wsDir = Env.WorkspaceDir()
    let slnFileName = sprintf "%s.sln" cmd.Name
    let sln = wsDir |> Fs.GetFile slnFileName
    if sln.Exists |> not then failwithf "View %A does not exist" cmd.Name
    Exec.Spawn sln.FullName "" "open"

