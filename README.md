The sample for building VSCode extensions using F# and Fable.

### Requirements
 * VSCode
 * Node.js
 * .Net Framework or mono
 * `dotnet` 2.0
 * Yarn

### How to build

1. `yarn install`
2. `cd src && dotnet restore`
3. `cd ..``
4. `build.cmd` on windoes or `build.sh` on Linux/Mac
5. `code .`
6. Press `F5` for single build, or run `Watch` task and `Launch Only` debug configuration for watch mode compilation.