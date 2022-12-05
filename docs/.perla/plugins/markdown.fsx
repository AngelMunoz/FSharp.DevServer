#r "nuget: Markdig, 0.30.4"
#r "nuget: Perla.Plugins, 1.0.0-beta-003"

open Perla.Plugins
open Markdig



let pipeline =
    lazy (MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build())

let shouldProcess: FilePredicate =
    fun extension -> [ ".md"; ".markdown" ] |> List.contains extension

let transform: Transform =
    fun args ->
        { content = Markdown.ToHtml(args.content, pipeline.Value)
          extension = ".html" }

plugin "markdown-plugin" {
    should_process_file shouldProcess
    with_transform transform
}
