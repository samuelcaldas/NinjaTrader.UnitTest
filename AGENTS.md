# Repository Guidelines

## Project Structure & Module Organization
Source is flat at the repo root. Core framework classes live in files like `Assert.cs`, `TestCase.cs`, `TestSuite.cs`, and `TextTestRunner.cs`. Assembly metadata is in `Properties/AssemblyInfo.cs`, and the build entry points are `NinjaTrader.UnitTest.sln` and `NinjaTrader.UnitTest.csproj`. Build outputs land in `bin/Debug` or `bin/Release`.

## Build, Test, and Development Commands
Use Visual Studio or MSBuild to compile the .NET Framework 4.8 library. The project includes a post-build step that copies `NinjaTrader.UnitTest.dll` to `%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom` for local testing.

```powershell
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Debug /p:Platform=AnyCPU
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform=x64
```

If you do not want auto-deploy, remove or edit the PostBuildEvent in `NinjaTrader.UnitTest.csproj`.

## Coding Style & Naming Conventions
Follow existing C# style: 4-space indentation, braces on new lines, PascalCase for public types and methods, camelCase for locals and fields, and file names that match the primary class. Keep types in the `NinjaTrader.UnitTest` namespace unless you are adding a new integration layer.

## Testing Guidelines
Tests are written by consumers as subclasses of `NinjaTrader.UnitTest.TestCase` and executed inside NinjaTrader. Name test methods clearly (for example, `TestAddition`) and pass the method name to the base constructor. Run tests via `TestCase.Run()` or a `TestSuite` and inspect `TestResult.PrintSummary()` output in the NinjaTrader log. There is no standalone CLI test runner in this repo.

## Commit & Pull Request Guidelines
Git history uses short, imperative subjects (for example, `Update README.md`, `Rename TestRunner`). Keep commits focused and avoid multi-topic changes. For PRs, include a clear description, steps to reproduce or validate, and note the NinjaTrader version used; attach log snippets when test behavior changes.

## Configuration & Dependencies
References to `NinjaTrader.Core` and `NinjaTrader.Gui` assume a local NinjaTrader 8 installation under `Program Files\NinjaTrader 8\bin`. If your install path differs, update the `HintPath` entries in `NinjaTrader.UnitTest.csproj`. x64 builds set `LangVersion` to 7.3; prefer that configuration for NinjaTrader 8.
