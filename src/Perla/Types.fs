﻿namespace Perla


module Types =

  let (|RestartFable|StartFable|StopFable|UnknownFable|) =
    function
    | "restart:fable" -> RestartFable
    | "start:fable" -> StartFable
    | "stop:fable" -> StopFable
    | value -> UnknownFable value

  let (|RestartServer|StartServer|StopServer|Clear|Exit|Unknown|) =
    function
    | "restart" -> RestartServer
    | "start" -> StartServer
    | "stop" -> StopServer
    | "clear"
    | "cls" -> Clear
    | "exit"
    | "stop" -> Exit
    | value -> Unknown value

  type FableConfig =
    { autoStart: bool option
      project: string option
      extension: string option
      outDir: string option }

    static member DefaultConfig() =
      { autoStart = Some false
        project = Some "./src/App.fsproj"
        extension = None
        outDir = None }

  type DevServerConfig =
    { autoStart: bool option
      port: int option
      host: string option
      mountDirectories: Map<string, string> option
      useSSL: bool option }

    static member DefaultConfig() =
      { autoStart = Some true
        port = Some 7331
        host = None
        mountDirectories = Map.ofList ([ "./src", "/src" ]) |> Some
        useSSL = Some false }

  type BuildConfig =
    { esBuildPath: string option
      esbuildVersion: string option
      target: string option
      outDir: string option
      bundle: bool option
      format: string option
      minify: bool option
      externals: (string seq) option }


    static member DefaultConfig() =
      { esBuildPath = None
        esbuildVersion = Some "0.12.28"
        target = Some "es2015"
        outDir = Some "./dist"
        bundle = Some true
        format = Some "esm"
        minify = Some true
        externals = None }

  type FdsConfig =
    { ``$schema``: string option
      index: string option
      fable: FableConfig option
      devServer: DevServerConfig option
      build: BuildConfig option
      packages: Map<string, string> option }

    static member DefaultConfig(?withFable: bool) =
      let fable =
        match withFable with
        | Some true -> FableConfig.DefaultConfig() |> Some
        | _ -> None

      { ``$schema`` =
          Some
            "https://raw.githubusercontent.com/AngelMunoz/Perla/main/perla.schema.json"
        index = Some "./index.html"
        fable = fable
        devServer = DevServerConfig.DefaultConfig() |> Some
        build = BuildConfig.DefaultConfig() |> Some
        packages = None }

  type LockDependency =
    { lookUp: string
      pin: string
      import: string }

  type PacakgesLock = Map<string, LockDependency>

  type ImportMap =
    { imports: Map<string, string>
      scopes: Map<string, string> }

  type Source =
    | Skypack = 0
    | Jspm = 1
    | Jsdelivr = 2
    | Unpkg = 3

  type JspmResponse =
    { staticDeps: string seq
      dynamicDeps: string seq
      map: {| imports: Map<string, string> |} }

  type InitOptions =
    { path: string option
      withFable: bool option }

  // https://api.skypack.dev/v1/search?q=package-name&p=1
  type SearchOptions = { package: string option }

  // https://api.skypack.dev/v1/package/package-name
  type ShowPackageOptions = { package: string option }

  // https://cdn.skypack.dev/package-name
  type AddPackageOptions =
    { package: string option
      alias: string option
      source: Source option }

  type RemovePackageOptions = { package: string option }

  exception CommandNotParsedException of string
  exception HelpRequestedException
  exception MissingPackageNameException
  exception MissingImportMapPathException
  exception PackageNotFoundException
  exception HeaderNotFoundException of string