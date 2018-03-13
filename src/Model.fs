module Model

type ProjectFilePath = string
type SourceFilePath = string
type ProjectReferencePath = string

[<RequireQualifiedAccess>]
type ProjectResponseInfo =
    | DotnetSdk of ProjectResponseInfoDotnetSdk
    | Verbose
    | ProjectJson

and ProjectResponseInfoDotnetSdk = {
    IsTestProject: bool
    Configuration: string
    IsPackable: bool
    TargetFramework: string
    TargetFrameworkIdentifier: string
    TargetFrameworkVersion: string
    RestoreSuccess: bool
    TargetFrameworks: string list
    RunCmd: RunCmd option
    IsPublishable: bool option }

and [<RequireQualifiedAccess>] RunCmd = { Command: string; Arguments: string }

type Project = {
    Project: ProjectFilePath
    Files: SourceFilePath list
    Output: string
    References: ProjectReferencePath list
    Logs: Map<string, string>
    OutputType: string
    Info: ProjectResponseInfo
    AdditionalInfo: Map<string, string>
}

type IonideApi = {
    ProjectLoadedEvent: Event<Project>
}