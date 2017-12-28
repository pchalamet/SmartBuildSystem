module Core.Project

let private ext2projType = Map [ (".csproj", "fae04ec0-301f-11d3-bf4b-00c04f79efbc")
                                 (".fsproj", "f2a71f9b-5d33-465a-a702-920d77279786")
                                 (".vbproj", "f184b08f-c81c-45f6-a57f-5abd9991f28f") 
                                 (".pssproj", "f5034706-568f-408a-b7b3-4d38c6db8a32")
                                 (".sqlproj", "00D1A9C2-B5F0-4AF3-8072-F6C62B433612")]


let SupportedProjectExtensions = ext2projType |> Seq.map (fun kvp -> kvp.Key)

let Ext2ProjectType ext =
    match ext2projType |> Map.tryFind ext with
    | Some prjType -> prjType
    | None -> failwithf "Unsupported project type %A" ext
