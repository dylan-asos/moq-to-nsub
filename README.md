# moq-to-nsub

## What is this?

Simple tool for converting a test project from moq to nsubstitute

Published as a dotnet tool to nuget at https://www.nuget.org/packages/Moq2NSubstitute

## What's included

A command line tool with a single command, `convert`

Give it a path that contains test files, it'll perform some regex replaces to switch syntax to NSubstitute.

Many thanks to `@AlbertoMonteiro` for the RegEx work at [https://gist.github.com/AlbertoMonteiro/daeab549df57727ddaa7](https://gist.github.com/AlbertoMonteiro/daeab549df57727ddaa7)

## Usage

The root command is named `moq2nsub`, only one sub-command is available

``` bash
moq2nsub convert --project-path C:\src\my-path-that-contains-unit-tests 
```
`--project-path` - should be the path where a csproj file exists that contains unit tests. 

`This tool amends the files it finds, applies the regex and saves the results, so make sure you've got your files source controlled for easy
rollback if you don't like the results `

## What does this do?

The tool will scan the path you provide for all *.cs files, and will then run a number of find and replace regular expressions
to convert from Moq syntax to NSubstitute. After performing find and replace operations, it will run a dotnet command to uninstall
moq and install nsubstitute.

`Important` - make sure your files are source controlled. The tool will overwrite files with the changes, so have a simple rollback 
plan if you don't like the output

## Contributing

Any contributions welcome, especially any new Regex!



