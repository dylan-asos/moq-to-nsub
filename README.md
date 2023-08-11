# moq-to-nsub

## What is this?

Super simple tool for converting a test project from moq to nsubstitute

## What's included

A command line tool with a single command, `convert`

Give it a path that contains test files, it'll perform some regex replaces to switch syntax to NSubstitute.

Many thanks to `@AlbertoMonteiro` for the RegEx work
at [https://gist.github.com/AlbertoMonteiro/daeab549df57727ddaa7](https://gist.github.com/AlbertoMonteiro/daeab549df57727ddaa7)

## Usage

The root command is named `moq2nsub`, Only one sub-command is available at this time

``` bash
moq2nsub convert --project-path C:\src\my-path-that-contains-unit-tests 
```

## Contributing

Any contributions welcome
