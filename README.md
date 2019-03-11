# TAP Test Logger

TAP logging extension for Visual Studio Test Platform.

This logger formats the test results using the [Test Anything Protocol](http://testanything.org/), which is used by Heroku buildpacks and several other tools in the testing ecosystem.


## Packages

Tap.TestLogger https://img.shields.io/nuget/v/Tap.TestLogger.svg

## Usage

Add a reference to the logger into your test project.

    dotnet add package Tap.TestLogger

Specify the use of the logger format when running your tests.

    dotnet test --logger:tap

By default the output will be found in `TestResults/TestResults.txt`

## License

MIT
