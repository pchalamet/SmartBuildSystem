module Helpers.Collections
open FSharp.Collections

type set<'T when 'T : comparison> = Set<'T>

let (?) (q: bool) (yes: 'a, no: 'a) = if q then yes else no


type Set<'T when 'T : comparison>
with
    static member substract a b = Set.difference b a
    static member choose f s = s |> Seq.choose f |> Set.ofSeq

type Map<'v, 'k when 'v : comparison>
with
    static member union (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map.fold (fun acc key value -> acc |> Map.add key value) p q
