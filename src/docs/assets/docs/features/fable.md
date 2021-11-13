[real world fable]: https://github.com/AngelMunoz/real-world-fable

# Fable |> Perla = ❤️

This is probably one of the main drivers for Perla. F# Users who are doing frontend development today have had some friction with the Node.Js ecosystem in the past and we believe there's a desire of reducing the amount of complexity in the toolchain when it comes to Fable projects.

I believe F# users can start using Perla given that Fable outputs modern javascript and that's the easiest input to process by Perla

As an example for that take a look at the [Real World Fable] and as the repository says

> This codebase was created to demonstrate a fully fledged fullstack application built with Fable, Elmish and F# including CRUD operations, authentication, routing, pagination, and more.

While it's a bit outdated this repo was created on the times of Fable 2 and Perla can make it run today with Fable 3 and produce a production ready build from it. That being said... let's take a look at a normal project structure for a Fable project

# Project Structure

Fable projects are farly straight forward the project structure doesn't differ too much from non-fable projects, the differences reside on the presence of perla as a dotnet tool as well as fable as a dotnet tool plus the F# sources.

```txt
.config/
    dotnet-tools.json
index.html
perla.jsonc
src/
    App.fsproj
    Main.fs
```

You can start of with the `Sutil` or `Feliz` Templates from `https://github.com/angelMunoz/perla-samples` or start from scratch, while _scaffolding_ is part of the project goals, it is not implemented yet in the mean time you need to type some commands if you want to go from scratch

```text
mkdir project
cd project
dotnet new tool-manifest
dotnet tool install perla
dotnet tool install fable
dotnet new classlib -o src -n App
perla init --with-fable true
touch index.html
```

your `index.html` should have something like this

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Perla App</title>
    <!-- import maps shim/polyfill
        untill it is natively supported in the browser -->
    <script
      async
      src="https://ga.jspm.io/npm:es-module-shims@1.0.0/dist/es-module-shims.js"
      crossorigin="anonymous"
    ></script>
  </head>
  <body>
    <div id="fable-app"></div>
    <script data-entry-point src="./src/Main.fs.js" type="module"></script>
  </body>
</html>
```

then you can start with `dotnet perla serve` and it will automatically start the Fable Compilation in watch mode as well as the saturn dev server

## TSX/JSX/JS/TS

Something that is not well known but possible in things like webpack/vite/snowpack Fable setups is that you can mix TSX/JSX/JS/TS files with your Fable sources, Perla allows this as well so feel free to mix and match F# and JS/TS files without worrying about compatibility.

### Migrating from Webpack Based projects

If for some reason you have a spare project based on webpack or you believe in Perla and want to proceed further here's a checklist you need to fill first

- [x] You're not using SASS/LESS or any other CSS pre-processor
- [x] You're not using CSS Modules (the popular solution, not the browser spec)
- [x] You don't depend on Javascript Plugins to transform content at build time (like converting markdown to html)

That's it.

Some things to take into account as well:

- There is no way to run tests at the moment.

  Current node based tooling has test runners like mocha that run unit tests on compiled F# -> JS code, Perla doesn't have an alternative for that yet.

- Your builds won't be "local" anymore
  While your F# code is local to your apps, the dependencies (such as react) are going to be imported from a CDN once built and bundled, you should evaluate if this is a concern for you.

#### Migration Steps

If you want to go ahead, then ensure of the following:

1. Move `index.html` to the root of your project or to the CWD you're going to run the _Perla CLI_
2. Replace css imports from npm packages with a CDN based alternative, example for `Bulma CSS`:

- From: `importSideEffects "bulma/css/bulma.css"`
- To:
  ```html
  <!-- inside your index.html -->
  <link
    rel="stylesheet"
    href="https://cdn.skypack.dev/-/bulma@v0.9.3-qiMIbVYAqsanl5Ue4g6H/dist=es2020,mode=raw/css/bulma.min.css"
  />
  ```
  > If you have your own CSS Files you don't need to do anything they can stay like `importSideEffects "./index.css"` these imports are supported, only external css files are required to be changed.

3. Import your entry point compiled F# -> JS file in the index.html

Your HTML file should look along these lines

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Perla App</title>
    <!-- import maps shim/polyfill
        untill it is natively supported in the browser -->
    <script
      async
      src="https://ga.jspm.io/npm:es-module-shims@1.0.0/dist/es-module-shims.js"
      crossorigin="anonymous"
    ></script>
    <link
      rel="stylesheet"
      href="https://cdn.skypack.dev/-/bulma@v0.9.3-qiMIbVYAqsanl5Ue4g6H/dist=es2020,mode=raw/css/bulma.min.css"
    />
  </head>
  <body>
    <div id="fable-app"></div>
    <!-- data-entry-point is very important when building for production
         this attribute tells perla that it must process the file when building
         you can include multiple `data-entry-pont` scripts -->
    <script data-entry-point src="./src/Main.fs.js" type="module"></script>
  </body>
</html>
```

4. Run `dotnet perla init --with-fable true` in the root of your project or to the CWD you're going to run the _Perla CLI_.

5. Install your dependencies. Example:

   - `dotnet perla add -p react`
   - `dotnet perla add -p react-dom`

   To identify which dependencies are the ones you actually need, they are usually in the `dependencies` object inside your `package.json`

6. Do a test run

   - `dotnet perla serve`

7. Check in the browser's console for errors like

   ```text
   Uncaught TypeError: Failed to resolve module specifier "react-dom".
   Relative references must start with either "/", "./", or "../".
   ```

   That usually means you are missing a package from your npm dependencies or that you are trying to do something like `importSideEffects "./my-module"` when the correct import must include the `.js` extension `importSideEffects "./my-module.js"`

   If at this point you don't see any error and your application is working then you can finally delete any `node_modules`, `package.json`, and `package-lock.json`

## Congratulations, Welcome to Perla :)
