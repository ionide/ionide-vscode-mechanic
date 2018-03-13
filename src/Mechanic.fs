module VSCodeMechanic

open Fable.Core.JsInterop
open Fable.PowerPack
open VSCode
open Model

let state = ResizeArray<Project>()
let outputChannel = Vscode.window.createOutputChannel "Mechanic"

let private askFsProjs (projFiles : string seq) = promise {
    return!
        Vscode.window.showQuickPick(!^ (projFiles |> ResizeArray))
        |> Promise.fromThenable
}

let private runInScope (input: ProjectExplorerModel option) = promise {
    match input with
    | Some (ProjectExplorerModel.Project (p,_,_,_,_,_,_)) ->
        do! Mechanic.run p outputChannel
    | _ ->
    match state.Count with
    | 0 ->
        // We didn't find an fsproj in the workspace directory
        // This case should not happen since we activate the extension only if an fsproj is found
        do! Vscode.window.showWarningMessage("Mechanic not run, we didn't find a fsproj in your workspace", [] |> ResizeArray)
            |> Promise.fromThenable
            |> Promise.ignore
    | 1 ->
        // Only one fsproj found, run mechanic directly
        do! Mechanic.run state.[0].Project outputChannel
    | _ ->
        // Several fsproj found, ask the users which one to use
        let! projFile = askFsProjs (state |> Seq.map (fun p -> p.Project))

        match projFile with
        | None ->
            do! Vscode.window.showInformationMessage("No project selected, command canceled", [] |> ResizeArray)
                |> Promise.fromThenable
                |> Promise.ignore
        | Some file ->
            do! Mechanic.run file outputChannel
}

let activate (context : Vscode.ExtensionContext) =
    let ext = Vscode.extensions.getExtension<IonideApi> "Ionide.Ionide-fsharp"
    match ext with
    | None ->
        Vscode.window.showWarningMessage("Mechanic couldn't be activated, Ionide-fsharp is required", [] |> ResizeArray)
        |> ignore
    | Some ext ->
        ext.exports.ProjectLoadedEvent $ (fun (project : Project) ->
            let exist =
                state
                |> Seq.toList
                |> List.exists(fun currentProject ->
                    currentProject.Project = project.Project
                )

            // Only add the project if not already known
            if not exist then
                state.Add project
        ) |> ignore

        Vscode.commands.registerCommand("mechanic.run", fun input ->
            let m = unbox<ProjectExplorerModel option> input
            Promise.start (runInScope m)
            None
        )
        |> context.subscriptions.Add
