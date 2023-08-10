using System.CommandLine;
using Moq.To.NSubstitute;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "{Timestamp:HH:mm} [{Level}] : {JobName} ExecutionId - {ExecutionId}: {Message}{NewLine}{Exception}")
    .CreateLogger();

Log.Information("Hello, dotnet OSS world!");
Log.Information("Enjoying this tool? Don't forget to send me the money");

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

var objectOption = new Option<string>(
    "--object-option",
    description: "Replace .Object usages");

conversionCommand.AddOption(pathOption);
conversionCommand.AddOption(objectOption);

var converter = new Converter();

conversionCommand.SetHandler(
    async (path, objectChoice) => { await converter.ConvertProjectTests(path, objectChoice); },
    pathOption, objectOption);

rootCommand.AddCommand(conversionCommand);

return await rootCommand.InvokeAsync(args);