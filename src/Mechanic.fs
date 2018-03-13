module VSCodeMechanic

open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open VSCode

let directoryToExcude = [ ".git"
                          ".paket-files" ]

let findFsProjs _ =
    let rec findFiles dir =
        Node.Exports.fs.readdirSync !^dir
        |> Seq.toList
        |> List.collect (fun inode ->
            try
                let currentPath = dir + Node.Exports.path.sep + inode

                if directoryToExcude |> List.contains inode then
                    []
                elif
                    Node.Exports.fs.statSync(!^currentPath).isDirectory() then findFiles currentPath
                elif
                    currentPath.EndsWith ".fsproj" then [ currentPath ]
                else
                    [ ]
            with
                | _ -> []
        )

    match Vscode.workspace.rootPath with
    | Some path -> findFiles path
    | None -> []

let outputChannel = Vscode.window.createOutputChannel "Mechanic"

let private  askFsProjs projFiles =
    promise {
        return!
            Vscode.window.showQuickPick(!^ (projFiles  |> List.toSeq |> ResizeArray))
            |> Promise.fromThenable
    }

let activate (context : Vscode.ExtensionContext) =
    Vscode.commands.registerCommand("mechanic.run", fun _ ->
        promise {
            let fsprojFiles = findFsProjs ()

            match fsprojFiles.Length with
            // We didn't find an fsproj in the workspace directory
            // This case should not occure because we active the extension only if an fsproj is found
            | 0 ->
                do! Vscode.window.showWarningMessage("Mechanic not run, we didn't find a fsproj in your workspace", [] |> ResizeArray)
                    |> Promise.fromThenable
                    |> Promise.ignore
            // Only one fsproj found, run mechanic directly
            | 1 ->
                do! Mechanic.run fsprojFiles.Head outputChannel
            // Several fsproj found, ask the users which one to use
            | _ ->
                let! projFile = askFsProjs fsprojFiles

                match projFile with
                | None ->
                    do! Vscode.window.showInformationMessage("No project selected, command canceled", [] |> ResizeArray)
                        |> Promise.fromThenable
                        |> Promise.ignore
                | Some file ->
                    do! Mechanic.run file outputChannel
        }
        |> Promise.start

        None
    )
    |> context.subscriptions.Add
