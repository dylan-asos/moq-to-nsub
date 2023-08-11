using System.CommandLine;
using Moq2NSubstitute;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "{Timestamp:HH:mm} [{Level}] : {JobName} ExecutionId - {ExecutionId}: {Message}{NewLine}{Exception}")
    .CreateLogger();

var rootCommand = new RootCommand( "Moq to NSubstitute conversion tool")
{
    Name = "moq2nsub",
};

var conversionCommand = 
    new Command("convert", "Converts a dotnet test project from using Moq to NSubstitute");

var pathOption = new Option<string>(
    "--project-path",
    description: "Target path that contains test project to convert.")
{
    IsRequired = true
};

conversionCommand.AddOption(pathOption);
var converter = new Converter();

conversionCommand.SetHandler(
    async (path) => { await converter.ConvertProjectTests(path); },
    pathOption);

rootCommand.AddCommand(conversionCommand);

return await rootCommand.InvokeAsync(args);