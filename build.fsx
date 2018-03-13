// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#I "packages/build/FAKE/tools"
#r "FakeLib.dll"
open System
open System.Diagnostics
open System.IO
open Fake
open Fake.YarnHelper
open Fake.ZipHelper
open Fake.FileUtils

// --------------------------------------------------------------------------------------
// Build the Generator project and run it
// --------------------------------------------------------------------------------------

let MechanicCliSource = "paket-files" </> "github.com" </> "fsprojects" </> "Mechanic" </> "src" </> "Mechanic.CommandLine" </> "bin" </> "Debug" </> "netcoreapp2.0"
let binTools = "release" </> "bin"
let MechanicDestination = binTools </> "Mechanic"

Target "Clean" (fun _ ->
    CleanDir "./temp"
    CleanDir binTools
    CopyFiles "release" ["README.md"; "LICENSE.md"]
    CopyFile "release/CHANGELOG.md" "RELEASE_NOTES.md"
)

Target "YarnInstall" <| fun () ->
    Yarn (fun p -> { p with Command = Install Standard })

Target "DotNetRestore" <| fun () ->
    DotNetCli.Restore (fun p -> { p with WorkingDir = "src" } )

let runFable additionalArgs =
    let cmd = "fable webpack -- " + additionalArgs
    DotNetCli.RunCommand (fun p -> { p with WorkingDir = "src" } ) cmd

let runFableNoTimeout additionalArgs =
    let cmd = "fable webpack -- " + additionalArgs
    DotNetCli.RunCommand (fun p -> { p with WorkingDir = "src"
                                            TimeOut = TimeSpan.MaxValue } ) cmd

Target "Build" (fun _ ->
    // Ideally we would want a production (minized) build but UglifyJS fail on PerMessageDeflate.js as it contains non-ES6 javascript.
    runFable "-p"
)

Target "Watch" (fun _ ->
    runFableNoTimeout "--watch"
)

Target "CopyMechanic" (fun _ ->
    cp_r MechanicCliSource MechanicDestination
)

// --------------------------------------------------------------------------------------
// Run generator by default. Invoke 'build <Target>' to override
// --------------------------------------------------------------------------------------

Target "Default" DoNothing

"Clean"
==> "YarnInstall"
==> "CopyMechanic"
==> "DotNetRestore"
==> "Build"
==> "Default"

RunTargetOrDefault "Default"
