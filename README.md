# GitUnpacker

![icon](GitUnpacker/Resources/icon.ico)

Simple tool to auto-convert `repoName.git` directories to actual repositories.

Icon is made by **[Pixel perfect from FlatIcon](https://www.flaticon.com/authors/pixel-perfect)**.

[![Build status](https://ci.appveyor.com/api/projects/status/khm2742n75a727r4?svg=true)](https://ci.appveyor.com/project/Gigas002/gitunpacker)

## Current version

Current stable can be found here: [![Release](https://img.shields.io/github/release/Gigas002/gitunpacker.svg)](https://github.com/Gigas002/gitunpacker/releases/latest).

Information about changes since previous releases can be found in [changelog](CHANGELOG.md). This project supports [SemVer 2.0.0](https://semver.org/) (template is `{MAJOR}.{MINOR}.{PATCH}.{BUILD}`).

Previous versions can be found on [releases](https://github.com/Gigas002/GitUnpacker/releases) and [branches](https://github.com/Gigas002/GitUnpacker/branches) pages.

## Requirements

- [Git](https://git-scm.com/downloads) – 2.23.0 or later

## Build

Project is built in **VS2019** (**16.3.2+**), but can also be built in **VSCode** (**1.38.1+**) with **omnisharp-vscode** (**1.21.4+**) extension. Build requirements are libs dependencies and **.NET Core 3.0 SDK**.

The **Release** build is made by `Publish.ps1` script. Take a look at it in the repo. Note, that running this script requires installed **PowerShell** or **[PowerShell Core](https://github.com/PowerShell/PowerShell)** for **Linux**/**OSX** systems.

## Dependencies

- [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/) – 2.6.0;

## Usage

| Short |   Long    |            Description             | Required? |
| :---: | :-------: | :--------------------------------: | :-------: |
|  -i   |  --input  |    Full path to input directory    |    Yes    |
|  -o   | --output  |   Full path to output directory    |    Yes    |
|       | --threads |           Threads count            |    No     |
|       | --version |          Current version           |           |
|       |  --help   | Message about command line options |           |

`-i/--input` is a `string`, representing full path to input directory. Directory should contain authors directories, and actual `repoName.git` directories in them.

`-o/--output` is a `string`, representing full path to ready output directory, where usual repos will be cloned.

`--threads` is a `int`, representing threads count.

Simple example looks like this: `./GitUnpacker -i "../Input" -o "../Output"`.

See `Start.ps1` for automating the work. Note, that running this script requires installed **PowerShell** or **[PowerShell Core](https://github.com/PowerShell/PowerShell)** for **Linux**/**OSX** systems.

## Localization

Localizable strings are located in `Localization/Strings.resx` file. You can add your translation (e.g. added `Strings.Ru.resx` file) and create pull request.

Currently, application is available on **English** and **Russian** languages.

## Contributing

Feel free to contribute, make forks, change some code, add [issues](https://github.com/Gigas002/GitUnpacker/issues), etc.