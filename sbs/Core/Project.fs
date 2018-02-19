module Core.Project

let private ext2projType = Map [ (".csproj",  "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")
                                 (".fsproj",  "F2A71F9B-5D33-465A-A702-920D77279786")
                                 (".vbproj",  "F184B08F-C81C-45F6-A57F-5ABD9991F28F") 
                                 (".pssproj", "F5034706-568F-408A-B7B3-4D38C6DB8A32")
                                 (".sqlproj", "00D1A9C2-B5F0-4AF3-8072-F6C62B433612")
                                 (".dcproj",  "E53339B2-1760-4266-BCC7-CA923CBCF16C")]

let Ext2ProjectType ext =
    match ext2projType |> Map.tryFind ext with
    | Some prjType -> prjType
    | None -> failwithf "Unsupported project type %A" ext
