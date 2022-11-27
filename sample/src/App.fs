﻿module App

open Fable.Core

open Sutil
open Sutil.Attr
open Sutil.DOM
open Sutil.Styling

open type Feliz.length

open Types
open Translations

[<ImportMember "./js-file.js">]
let renderLit: unit -> unit = jsNative

let setTranslations
  (fetchTranslations: unit -> TranslationCollection)
  (translationStore: IStore<TranslationCollection option * Language>)
  =
  try
    let importedTranslations = fetchTranslations ()

    Store.modify
      (fun (_, language) -> Some importedTranslations, language)
      translationStore
  with ex ->
    Store.modify (fun (_, language) -> None, language) translationStore

let view () =
  let translations = Store.make (None, EsMx)

  let notifications: IStore<Notification ResizeArray> =
    Store.make (ResizeArray())

  let Tr = T translations
  setTranslations fetchTranslations translations


  Html.app [
    Html.main [
      onCustomEvent Event.Mount (fun _ -> renderLit()) []
      Components.LanguageSelector translations
      Components.NotificationGenerator Tr notifications
      Html.div [ Attr.id "lit-app" ]
      Html.div [ Attr.id "lit-app-2" ]
      Html.custom("my-sample-el", [])
    ]
    Components.NotificationArea notifications
  ]
  |> withStyle [
    rule "label" [ Css.fontSize (em 1.5) ]
    rule "main, div" [ Css.displayFlex; Css.flexDirectionColumn ]
  ]
