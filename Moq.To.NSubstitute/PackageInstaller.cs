using System.Diagnostics;
using Serilog;

namespace Moq2NSubstitute;

public class PackageInstaller
{
    private readonly string _targetPath;

    public PackageInstaller(string targetPath)
    {
        _targetPath = targetPath;
    }

    public void RemoveMoq()
    {
        RunDotNet(_targetPath, "remove package moq");
    }

    public void AddNSubstitute()
    {
        RunDotNet(_targetPath, "add package NSubstitute --version 5.0.0");
    }
    
    public bool DoesProjectFileExist()
    {
        var files = Directory.GetFiles(_targetPath, "*.csproj", SearchOption.TopDirectoryOnly);

        return files.Length > 0;
    }

    public void RunDotNet(string directory, string command)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet.exe",
            WorkingDirectory = directory,
            Arguments = $"{command}",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.StartInfo = startInfo;
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        if (!string.IsNullOrEmpty(output))
        {
            Log.Information(output);
        }

        var err = process.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(err))
        {
            Log.Error(err);
        }

        process.WaitForExit();
    }
}