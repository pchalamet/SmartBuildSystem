module Helpers.IO
open System

type Result =
    { Code: int
      Info : string
      Out: string list
      Error: string list }

let ResultToError execResult =
    if execResult.Code <> 0 then 
        [sprintf "Operation '%s' failed with error %d" execResult.Info execResult.Code]
            @ ("Out:" :: execResult.Out) 
            @ ("Error:" :: execResult.Error)
            |> String.concat Environment.NewLine
            |> Some
    else None

let GetOutput execResult =
    match execResult |> ResultToError with
    | Some error -> failwith error
    | None -> execResult.Out

let CheckResponseCode execResult =
    match execResult |> ResultToError with
    | Some error -> failwith error
    | None -> ()

let AndThen f execResult =
    match execResult |> ResultToError with
    | Some _ -> execResult
    | None -> f()

let IsError execResult =
    let res = execResult |> ResultToError
    res.IsSome

let CheckMultipleResponseCode execResults =
    let errors = execResults |> Seq.choose (fun execResult -> execResult |> ResultToError)
    if errors |> Seq.isEmpty |> not then
        errors |> String.concat System.Environment.NewLine |> failwith
  