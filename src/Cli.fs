module Mechanic

open Fable.Core.JsInterop
open Fable.Import
open VSCode
open Fable.PowerPack

let inline (</>) a b = a + Node.Exports.path.sep + b

let pluginPath =
    match Vscode.extensions.getExtension "Mechanic.mechanic" with
    | Some extension -> extension.extensionPath
    | None -> failwith "Failed to find `Mechanic.mechanic` extension"

let pluginBinPath = pluginPath </> "bin" </> "Mechanic"

let private raw (projectFile : string) (outputChannel : Vscode.OutputChannel) =
    let options =
        createObj [
            "cwd" ==> Vscode.workspace.rootPath
            "detached" ==> true
            "shell" ==> true
        ]

    printfn "%A" pluginBinPath
    printfn "%A" projectFile

    let prms =
        seq { yield pluginBinPath </> "mech.dll"
              yield projectFile } |> ResizeArray

    Node.Exports.childProcess.spawn(
        "dotnet",
        prms,
        options)
    |> Process.onOutput(fun e -> e.toString () |> outputChannel.append)
    |> Process.onError (fun e -> e.ToString () |> outputChannel.append)
    |> Process.onErrorOutput(fun e -> e.toString () |> outputChannel.append)
    |> Process.toPromise

let run projFile outputChannel =
    promise {
        let! result = raw projFile outputChannel
        match result with
        | 0 ->
            do! Vscode.window.showInformationMessage("Mechanic completed", [] |> ResizeArray)
                |> Promise.fromThenable
                |> Promise.ignore
        | _ ->
            let! result = Vscode.window.showErrorMessage("Mechanic failed", [ "Show output" ] |> ResizeArray)
                          |> Promise.fromThenable

            match result with
            | Some "Show output" ->
                outputChannel.show(true)
            | None | Some _ -> ()
    }
