﻿namespace Perla.Plugins.Extensibility

open System
open System.Collections.Concurrent
open System.IO

open FSharp.Compiler.Interactive.Shell
open FSharp.Control

open FsToolkit.ErrorHandling

open Perla.Plugins
open Perla.Logger

type Fsi =
  static member getSession(?argv, ?stdout, ?stderr) =
    let defaultArgv =
      [| "fsi.exe"; "--optinize+"; "--nologo"; "--gui-"; "--readline-" |]

    let argv =
      match argv with
      | Some argv -> [| yield! defaultArgv; yield! argv |]
      | None -> defaultArgv

    let stdout = defaultArg stdout Console.Out
    let stderr = defaultArg stderr Console.Error

    let config = FsiEvaluationSession.GetDefaultConfiguration()

    FsiEvaluationSession.Create(
      config,
      argv,
      new StringReader(""),
      stdout,
      stderr,
      true
    )

type Plugin =

  static member PluginCache =
    lazy (ConcurrentDictionary<PluginInfo, FsiEvaluationSession>())

  static member getCachedPlugins() =
    Plugin.PluginCache.Value |> Seq.map (fun entry -> entry.Key)

  static member fromCache(pluginName: string, path: string) =
    Logger.log $"Attempting to extracting plugin'{pluginName}' from cache"

    Plugin.PluginCache.Value
    |> Seq.tryFind (fun entry ->
      entry.Key.name = pluginName && entry.Key.path = path)
    |> Option.map (fun entry -> entry.Key)

  static member fromText(name: string, content: string, ?skipCache) =
    let skipCache = defaultArg skipCache false
    Logger.log $"Extracting plugin '{name}' from text"

    let Fsi = Fsi.getSession ()

    let evaluation, _ = Fsi.EvalInteractionNonThrowing(content)

    let tryFindBoundValue () =
      match Fsi.TryFindBoundValue "Plugin", Fsi.TryFindBoundValue "plugin" with
      | Some bound, _ -> Some bound.Value
      | None, Some bound -> Some bound.Value
      | None, None -> None

    let logError ex =
      Logger.log ($"An error ocurrer while processing {name}", ex)

    let evaluation =
      evaluation
      |> Result.ofChoice
      |> Result.teeError logError
      |> Result.toOption
      |> Option.defaultWith tryFindBoundValue

    match evaluation with
    | Some value when value.ReflectionType = typeof<PluginInfo> ->
      let plugin = unbox plugin

      if not skipCache then
        if not (Plugin.PluginCache.Value.TryAdd(plugin, Fsi)) then
          Logger.log $"Couldn't add %s{plugin.name}"
        else
          Logger.log $"Added %s{plugin.name} to plugin cache"

      Some plugin
    | _ -> None

  static member loadTextBatch
    (plugins: {| name: string; content: string |} seq)
    =
    seq {
      for entry in plugins do
        match Plugin.fromText (entry.name, entry.content) with
        | Some plugin -> plugin
        | None -> ()
    }

  static member getSupportedPlugins(?plugins: PluginInfo seq) =
    let plugins = defaultArg plugins (Plugin.getCachedPlugins ())

    let chooser (plugin: PluginInfo) =
      match plugin.shouldProcessFile, plugin.transform with
      | ValueSome st, ValueSome t ->
        Some
          { plugin = plugin
            shouldTransform = st
            transform = t }
      | _ -> None

    plugins |> Seq.choose chooser

  static member applyPluginsToFile
    (
      fileInput: FileTransform,
      ?plugins: RunnablePlugin seq
    ) =
    let plugins = defaultArg plugins (Plugin.getSupportedPlugins ())

    let folder result next =
      async {
        match next.shouldTransform result with
        | true -> return! (next.transform result).AsTask() |> Async.AwaitTask
        | false -> return result
      }

    plugins |> AsyncSeq.ofSeq |> AsyncSeq.foldAsync folder fileInput

module Scaffolding =
  [<Literal>]
  let ScaffoldConfiguration = "TemplateConfiguration"

  let getConfigurationFromScript content =
    use session = Fsi.getSession ()

    session.EvalInteractionNonThrowing(content) |> ignore

    match session.TryFindBoundValue ScaffoldConfiguration with
    | Some bound -> Some bound.Value.ReflectionValue
    | None -> None