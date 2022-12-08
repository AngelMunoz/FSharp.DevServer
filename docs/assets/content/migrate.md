[jspm generator api]: https://generator.jspm.io/
[plugins]: /#/v1/docs/features/plugins
[testing]: /#/v1/docs/features/testing
[transpilation]: /#/v1/docs/features/transpilation
[package manager]: /#/v1/docs/features/package-manager
[dev server]: /#/v1/docs/features/dev-server
[dev proxy]: /#/v1/docs/features/dev-proxy
[fake globs]: https://fake.build/reference/fake-io-globbing-operators.html
[fsharp.systemcommandline]: https://github.com/JordanMarr/FSharp.SystemCommandLine

# Migrate versions

## From V0 -> V1

There were substantial changes made to the configuration files as well as some changes to the CLI interface while these changes are not complicated you might want to be aware of them

### perla.json

The first vital change was to re-name `perla.jsonc` to `perla.json` in the early days we naively thought there were jsonc compatible parsers ready to use in .NET, this turned out to be not true, we renamed the file to ensure we're not setting expectations of allowing comments within the configuration file

Here's the list of breaking changes for the `perla.json` file

#### Removed Properties

- packages ->
  Removed and replaced: refer to `dependencies` and `devDependencies` nodes

- fable

  - autoStart ->

    Removed: v0 had an `interactive` mode which allowed you to control fable's execution, this is no longer the case as it didn't get much traction and made low sense to even expose this option

- devServer

  - AutoStart ->
    Removed: v0 had an `interactive` mode which allowed you to control fable's execution, this is no longer the case as it didn't get much traction and made low sense to even expose this option
  - mountDirectories ->

    Moved: This has been sourced to the top of the configuration rather than something specific to devServer

  - enableEnv ->

    Moved: This has been sourced to the top of the configuration rather than something specific to devServer

  - watchConfig ->

    Removed: This configuration was used to tune the watch settings for the source files, this configuration was very complex and was prone to break watch mode

- build

  - esBuildPath ->
    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node
  - esbuildVersion ->
    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node
  - copyPaths ->

    Removed: this was replaced by the `includes` and `excludes` nodes within the `build` node

  - target ->
    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node

  - format ->
    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node

  - bundle ->

    Removed: For perla purposes it doesn't make sense to not bundle the build output so this setting was removed

  - minify ->
    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node
  - jsxFactory ->

    Removed: This setting requires you to manually inject jsx helpers into your code, newer esbuild versions allow you to automate these injects and with the import maps you can customize where to pull the dependencies from.

  - jsxFragment ->

    Removed: This setting requires you to manually inject jsx helpers into your code, newer esbuild versions allow you to automate these injects and with the import maps you can customize where to pull the dependencies from.

  - externals ->

    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node

  - injects ->

    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node

  - fileLoaders ->

    Moved to the `esbuild` node:
    This configuration was specific to esbuild so it was moved to the corresponding node

## Config Additions

- runConfiguration ->

  New: perla now supports running/testing apps in production/development mode, the default is production as it will only pick up dependencies required by the application at runtime, development mode is chosen by the test command to pick up libraries that are only relevant for testing, you can also use development mode to add uility libraries that should only run at dev time like rxjs-spy (to debug rxjs observables)

  ```diff
  + "runConfiguration": "production"

  # or
  + "runConfiguration": "development"
  ```

- provider ->

  New: In v0 we had a couple of providers but we had custom mechanisms to fetch dependencies and update the import map, for v1 we will leverage completely the [jspm generator api] so we use the same providers to accomplish the same effect, pease refer to the [package manager] documentation for more information

  ```diff
  + "provider": "jspm"

  # or
  + "provider": "unpkg"

  # or
  + "provider": "skypack"
  ```

- plugins ->

  **_NEW_**: At last! Plugins are here to stay, and while they are not as powerful as the ones existing in other tools like vite, snowpack, rollup, webpack, But given what we've got, it is a start for future versions
  please refer to the [plugins] documentation for more information

- testing ->

  **_NEW_**: Client side testing is making it into v1, this was pretty much needed to have more robust software built with perla and to have the confidence of what you've built won't break that easily, please refer to the [testing] documentation for more information.

- mountDirectories ->

  Moved from `devServer`: This setting was present in the devServer node but it was also used by the `build` node to ensure things were going to be served and built/copied as you would expect the main change in this setting is the following

  ```diff

  # Before: local directory -> server mount path
  -"./src":"/src"

  # To add files in root path we needed it to have it as an empty string
  -"./put-in-root":""

  # After: server mount path -> local directory
  +"/src":"./src"

  # To add files in root path we can simply use "/"
  +"/":"./put-in-root"
  ```

- enableEnv
- envPath ->

  Moved from `devServer`: Use these settings to enable the usage of perla specific environment variables, please refer to the [dev server] documentation for more information.

- dependencies
- devDependencies ->

  **_NEW_**: While the `packages` node existed before it was mostly to inform what was installed in the current application, it was hard to actually make it useful for something as the major driver was the import map itself, in this version the depencies take a more useful role please refer to the [package manager] documentation for more information.

  ```diff
  - "dependencies": {
  -   "rxjs" : "https://unpkg.com/rxjs@7.6.0?module"
  -  }

  # now should be written as

  + "dependencies": [
  +   { "name": "rxjs", "version": "7.6.0" }
  + ]
  ```

  Please note that when you port the dependencies to the new format, it is recommended to remove the existing `perla.json.importmap` file so you don't get outdated resolutions, manual resolutions should be added after the new import map is generated.

- fable

  - sourceMaps ->

    **NEW**: This setting was not exposed before but now you can enable source maps for fable projects

- devServer

  - proxy ->

    **_MOVED_**: Previously you had to have a separate `proxy-config.json` file this has now been integrated into the `perla.json` file the specification of the settings is still the same please refer to the [dev proxy] documentation for more information.

- build

  - includes
  - excludes ->

    **_NEW_**: In v0 we had a `copyPaths` configuration option that kind of served this purpose, but it was hard to configure correctly and prone to errors we have changed the strategy here and allow us to use [FAKE Globs], please note that you don't need to add the FAKE operators only the glob patterns.

    ```diff
    # copy the webmanifest file to outDir/manifest.webmanifest

    - "build": {
    -   "copyPaths": {
    -     "includes": ["./root-files/manifest.webmanifest -> ./manifest.webmanifest"]
    -   }
    - }

    # copy files from ./assets/images/ into outDir/assets/images
    # and ./documents and all of the html files within that directory to outDir/documents

    + "build": {
    +   "includes: [ "assets/images/*", "documents/*/*.html" ]
    +  }

    # New: copy files that live within the virtual file system to the output directory
    + "build": {
    +   "includes: [ "vfs:src/*/*.html" ]
    +  }
    ```

    While we lost the ability to re-target ouput files to different paths we can now use glob patterns and remove some wonkyness in the process.

    For the case of using plugins, you might want to copy the processed files that are not CSS/JS (esbuild takes care of these) like a markdown to html plugin (that's how the docs are built)
    Just prefix the pattern with `vfs:` and perla will take the virtual file system as the root when doing the final copy to the output directory

  - emitEnvFile ->

    **_Moved_**: Enable producing an environment file from the perla specific environment variables, this can be turned off in case you have something else that will serve this file in production.

- esbuild

  - esBuildPath ->

    Moved from the `build` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - version ->

    Moved from the `build.esbuildVersion` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - ecmaVersion ->

    Moved from the `build.target` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - minify ->

    Moved from the `build` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - injects ->

    Moved from the `build` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - externals ->

    Moved from the `build` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - fileLoaders ->

    Moved from the `build` node: The setting works the same as it worked before please check the [transpilation] documentation for more information

  - jsxAutomatic
  - jsxImportSource ->

    **_NEW_**: In cases where you want to use JSX in your sources you can leverage these settings

    ```json
    // For React
    "esbuild": {
        "jsxAutomatic": true
    },
    // or for preact and others
    "esbuild": {
        "jsxAutomatic": true,
        "jsxImportSource": "preact"
    }
    ```

    If you don't specify these settings you have to manually add the corresponding jsx helpers in each jsx/tsx file

### Command Line Interface

The perla CLI was also modified as we moved from argu to [Fsharp.Systemcommandline] the new perla options are the following:

> **_NOTE_**: Please have in mind that you can run `perla [command] --help` to have more information about that specific command.

**_NEW_**:

```text
Description:
  The Perla Dev Server!

Usage:
  Perla [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information
  --info <info>   Brings the Help dialog []

Commands:
  serve                                              My Command
  build                                              Builds the SPA application for distribution
  init                                               Initialized a given directory or perla itself
  search <package> <page>                            Search a package name in the Skypack api, this will bring potential results []
  show <package>                                     Shows information about a package if the name matches an existing one
  remove <package>                                   removes a package from the
  add <package>                                      Shows information about a package if the name matches an existing one
  list                                               Lists the current dependencies in a table or an npm style json string
  restore                                            Restore the import map based on the selected mode, defaults to production
  templates:add <templateRepositoryName> <branch>     Adds a new template from a particular repository
  templates:update <templateRepositoryName> <branch>  Updates an existing template in the templates database
  templates:list <simple|table>                      My Command []
  templates:delete <templateRepositoryName>          Removes a template from the templates database
  new <name> <templateName>                          Creates a new project based on the selected template if it exists
  test                                               Runs client side tests in a headless browser
```

If you want to have a brief overview how the old CLI was here's for comparison:

**_OLD_**:

```text
USAGE: perla.exe [--help] [restore] [list-templates] [remove-template <string>] [--version] [<subcommand> [<options>]]

SUBCOMMANDS:

    serve, s <options>    Starts a development server for modern Javascript development
    build, b <options>    Builds the specified JS and CSS resources for production
    init <options>        Sets perla up to start new projects.
    search, se <options>  Searches a package in the skypack API.
    show <options>        Gets the skypack information about a package.
    add <options>         Generates an entry in the import map.
    remove <options>      Removes an entry in the import map.
    list <options>        Lists entries in the import map.
    new <options>         Creates a new Perla based project.
    add-template <options>
                          Downloads a GitHub repository to the templates directory.
    update-template <options>
                          Downloads a new version of the specified template.

    Use 'perla.exe <subcommand> --help' for additional information.

OPTIONS:

    restore               Restores import map
    list-templates, -lt   Shows existing templates available to scaffold.
    remove-template, -rt <string>
                          Removes an existing templating repository.
    --version, -v         Prints out the cli version to the console.
    --help                display this list of options.
```
