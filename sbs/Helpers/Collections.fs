﻿module Helpers.Collections
open FSharp.Collections

type set<'T when 'T : comparison> = Set<'T>

let (?) (q: bool) (yes: 'a, no: 'a) = if q then yes else no

module Set =
    let substract a b = Set.difference b a
    let choose f s = s |> Seq.choose f |> Set.ofSeq

module Map =
    let union p q = 
        Map.fold (fun acc key value -> acc |> Map.add key value) p q

    let choose f m =
        m |> Map.fold (fun acc k v -> match f k v with
                                      | Some x -> acc |> Map.add k x
                                      | _ -> acc) Map.empty

    let addRange map1 map2 =
        map2 |> Map.fold (fun acc k v -> acc |> Map.add k v) map1
