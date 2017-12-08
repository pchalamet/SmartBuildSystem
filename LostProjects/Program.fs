// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Generic




let ext2projType = Map [ (".csproj",  "{fae04ec0-301f-11d3-bf4b-00c04f79efbc}")
                         (".fsproj",  "{f2a71f9b-5d33-465a-a702-920d77279786}")
                         (".vbproj",  "{f184b08f-c81c-45f6-a57f-5abd9991f28f}") 
                         (".pssproj", "{f5034706-568f-408a-b7b3-4d38c6db8a32}")
                         (".sqlproj", "{00d1a9c2-b5f0-4af3-8072-f6c62b433612}") ]





let private gatherProjects (dir : DirectoryInfo) =
    let projects = ext2projType |> Seq.map (fun x -> x.Key)
                                |> Seq.fold (fun s t -> s |> Seq.append (dir.EnumerateFiles("*"+ t, SearchOption.AllDirectories))) Seq.empty
    projects

let gatherSolutions (dir : DirectoryInfo) =
    let slns = dir.EnumerateFiles("*.sln", SearchOption.AllDirectories)
    slns


let rec isProject (line : string) (kvps : KeyValuePair<string, string> list) =
    match kvps with
    | kvp :: tail when line.Contains("project(\"" + kvp.Value) -> true
    | kvp :: tail -> tail |> isProject line
    | [] -> false


let rec gatherProjectsFromSolutionContent (slnDir : DirectoryInfo) (lines : string list) =
    seq {
        match lines with
        | line :: tail -> if isProject (line.ToLowerInvariant()) (ext2projType |> List.ofSeq) then
                              let prjItems = line.Split('=').[1]
                              let prjFile = prjItems.Split(',').[1].Trim().Replace("\"", "")
                              let prjFileName = if prjFile.Contains(@":\") then prjFile
                                                else Path.Combine(slnDir.FullName, prjFile)
                                                |> FileInfo

                              if prjFileName.Exists |> not then 
                                  printfn "WARNING: project %A does not exist" prjFileName
                              else 
                                  yield prjFileName
                          yield! tail |> gatherProjectsFromSolutionContent slnDir
        | [] -> ()
    }


let gatherProjectsFromSolution (sln : FileInfo) =
    let lines = File.ReadAllLines (sln.FullName) |> List.ofSeq
    lines |> gatherProjectsFromSolutionContent sln.Directory



[<EntryPoint>]
let main argv =
    let dir = argv.[0] |> DirectoryInfo
    let allPrjs = dir |> gatherProjects 
                      |> Seq.map (fun x -> x.FullName.ToLowerInvariant()) 
                      |> Set.ofSeq

    let slnPrjs = dir |> gatherSolutions 
                      |> Seq.map gatherProjectsFromSolution
                      |> Seq.concat
                      |> Seq.map (fun x -> x.FullName.ToLowerInvariant()) 
                      |> Set.ofSeq

    let lostProjects = allPrjs - slnPrjs
    lostProjects |> Seq.iter (printfn "LOST: %A")

    0 // return an integer exit code
