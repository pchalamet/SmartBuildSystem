// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Generic




open System.Xml.Linq

#nowarn "0077" // op_Explicit


let NsNone = XNamespace.None

let inline (!>) (x : ^a) : ^b = (((^a or ^b) : (static member op_Explicit : ^a -> ^b) x))




let isBuild (x : XElement) =
    let name = !> x.Attribute(NsNone + "name") : string
    (name = "build-file-path")

let isRunner (x : XElement) =
    let name = !> x.Attribute(NsNone + "name") : string
    (name = "runnerArgs") 

let extractSln (s : string) =
    let idx1 = s.IndexOf("&quot;")
    if idx1 <> -1 then
        let idx2 = s.IndexOf("&quot;", idx1+1)
        let sln = s.Substring(idx1, idx2-idx1)
        sln
    else
        let idx2 = s.IndexOf("=")
        let sln = s.Substring(idx2+1)
        sln

let replaceMacro (x : string) =
    x.Replace("%system.teamcity.build.checkoutdir%", "")
     .Replace("%system.teamcity.build.workingdir%", "")
     .Replace("%teamcity.build.checkoutdir%", "")
     .Replace("%system.branchname%", "")
     .Replace("//", "/")
     .Replace(@"\\", @"\")
     .Replace("\"", "")

let replaceLeadingSlash (x : string) =
    if x.StartsWith("/") || x.StartsWith(@"\\") then x.Substring(1) 
    else x

let findSolutions (file : FileInfo) =
    let xdoc = file.FullName |> XDocument.Load
    let buildFilePath = xdoc.Descendants(NsNone + "param") |> Seq.filter isBuild
                                                  |> Seq.map (fun x -> !> x.Attribute(NsNone + "value") : string)
                                                  |> Seq.filter (fun x -> x |> isNull |> not && x.EndsWith(".sln"))
                                                  |> Seq.map (fun x -> x.ToLowerInvariant())
                                                  |> Seq.map replaceMacro
                                                  |> Seq.map replaceLeadingSlash
                                                  |> Seq.map (fun x -> Path.GetFileName x)

    let runnerFilePath = xdoc.Descendants(NsNone + "param") |> Seq.filter isRunner
                                                  |> Seq.map (fun x -> !> x.Attribute(NsNone + "value") : string)
                                                  |> Seq.filter (fun x -> x |> isNull |> not && x.Contains(".sln"))
                                                  |> Seq.map (fun x -> x.ToLowerInvariant())
                                                  |> Seq.map replaceMacro
                                                  |> Seq.map extractSln
                                                  |> Seq.map replaceLeadingSlash
                                                  |> Seq.map (fun x -> Path.GetFileName x)

    buildFilePath |> Seq.append runnerFilePath


[<EntryPoint>]
let main argv =
    let folder = argv.[0] |> DirectoryInfo
    let configs = folder.EnumerateFiles("*.xml", SearchOption.AllDirectories)
    let slns = configs |> Seq.map findSolutions
                       |> Seq.concat
                       |> Seq.distinct
    slns |> Seq.iter (printfn "%A")
    0