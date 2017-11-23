module Core.PatternMatching
open Helpers.Collections

let private (|MatchZeroOrMore|_|) c =
    match c with
    | '*' -> Some c
    | _ -> None

let rec private matchRec (content : char list) (pattern : char list) =
    let matchZeroOrMore remainingPattern =
        match content with
        | [] -> matchRec content remainingPattern
        | _ :: t1 -> if matchRec content remainingPattern then true // match 0 time
                     else matchRec t1 pattern // try match one more time

    let matchChar firstPatternChar remainingPattern =
        match content with
        | firstContentChar :: remainingContent when firstContentChar = firstPatternChar -> matchRec remainingContent remainingPattern
        | _ -> false

    match pattern with
    | [] -> content = []
    | MatchZeroOrMore _ :: tail -> matchZeroOrMore tail
    | head :: tail -> matchChar head tail

let Match (content : string) (pattern : string) =
    matchRec (content.ToLowerInvariant() |> Seq.toList) (pattern.ToLowerInvariant() |> Seq.toList)


let FilterMatch<'T when 'T : comparison> (items : 'T set) (strOf : 'T -> string) (filters : string set) : 'T set =
    let mapItems = items |> Set.map (fun x -> (x, (strOf x).ToLowerInvariant()))
    let matchItems filter = mapItems |> Set.filter (fun x -> Match (snd x) filter)
                                     |> Set.map fst
    let matches = filters |> Set.map (fun x -> x.ToLowerInvariant()) 
                          |> Set.map matchItems
                          |> Set.unionMany
    matches
