using System.Text;
using Serilog;

namespace Moq2NSubstitute;

public class Converter  
{
    public Task ConvertProjectTests(string targetPath)
    {
        if (!Directory.Exists(targetPath))
        {
            Log.Error("Path {TargetPath} does not exist", targetPath);
            return Task.CompletedTask;
        }
        
        var projectFiles = Directory.GetFiles(targetPath, "*.cs", SearchOption.AllDirectories);
        Log.Information("Found {FileCount} files to convert", projectFiles.Length);

        foreach (var file in projectFiles)
        {
            string content;
            Encoding encoding;
            using (var reader = new StreamReader(file)) {
                content = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }
            
            var replacedContent = ReplaceUsingStatements(content);
            replacedContent = ReplaceMockCreation(replacedContent);
            replacedContent = ReplaceInstanceData(replacedContent);
            replacedContent = ReplaceVerifies(replacedContent);
            replacedContent = ReplaceThrows(replacedContent);
            replacedContent = ReplaceArgs(replacedContent);
            replacedContent = ReplaceMockingKernel(replacedContent);
            replacedContent = ReplaceMockingKernelUsing(replacedContent);
            replacedContent = ReplaceMockingKernelGetMock(replacedContent);
            replacedContent = ReplaceSetup(replacedContent);
            replacedContent = ReplaceDotObject(replacedContent);
            
            if (content != replacedContent)
            {
                File.WriteAllText(file, replacedContent, encoding);
                Log.Information("Modified: {File}", file);
            }
        }
        
        Log.Information("Conversion complete....");

        var packageInstaller = new PackageInstaller(targetPath);
        packageInstaller.RemoveMoq();
        packageInstaller.AddNSubstitute();
        
        return Task.CompletedTask;
    }    
    
    static string ReplaceUsingStatements(string content)
    {
        var pattern = @"using\s+Moq;";
        var replacement = "using NSubstitute;";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceDotObject(string content)
    {
        const string pattern = @".Object";
        const string replacement = "";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceMockCreation(string content)
    {
        var pattern = @"new\s+Mock<(.+?)>\((.*?)\)";
        const string replacement = "Substitute.For<$1>($2)";
        var replacedContent = content.RegexReplace(pattern, replacement);

        pattern = @"Mock.Of<(.+?)>\((.*?)\)";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        return replacedContent;
    }
    
    static string ReplaceInstanceData(string content)
    {
        const string pattern = @"\bMock<(.+?)>";
        const string replacement = "$1";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceVerifies(string content)
    {
        var pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?), Times\.(Once(\(\))?|Exactly\((?<times>\d+)\))\)";
        var replacement = "$1.Received(${times})$3";
        var replacedContent = content.RegexReplace(pattern, replacement);

        pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?)\)\)";
        replacement = "$1.Received()$3)";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);    
        
        pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?), Times\.Never\)";
        replacement = "$1.DidNotReceive()$3";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        return replacedContent;
    }
    
    static string ReplaceThrows(string content)
    {
        const string pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup\(((\w+) => \4(\..?.+?)\))\)\s*\n*\.Throws";
        const string replacement = "$1.When($3).Throw";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceArgs(string content)
    {
        var pattern = @"It.IsAny";
        var replacement = "Arg.Any";
        var replacedContent = content.RegexReplace(pattern, replacement);

        pattern = @"It.Is";
        replacement = "Arg.Is";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        return replacedContent;
    }
    
    static string ReplaceMockingKernel(string content)
    {
        const string pattern = @"MoqMockingKernel";
        const string replacement = "NSubstituteMockingKernel";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceMockingKernelUsing(string content)
    {
        const string pattern = @"using\s+Ninject.MockingKernel.Moq;";
        const string replacement = "using Ninject.MockingKernel.NSubstitute;";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }
    
    static string ReplaceMockingKernelGetMock(string content)
    {
        const string pattern = @"\.GetMock<(.+?)>\(\)";
        const string replacement = ".Get<(.+?)>()";
        var replacedContent = content.RegexReplace(pattern, replacement);

        return replacedContent;
    }

    static string ReplaceSetup(string content)
    {
        var pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup(Get)?\((\w+) => \4(\.?.+?)\)(?=\.R|\s\n)";
        var replacement = "$1$5";
        var replacedContent = content.RegexReplace(pattern, replacement);

        pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup(Get)?\((\w+) => \4(\.?.+?)\).ReturnsAsync(..?.+?\))";
        replacement = "$1$5.Returns(Task.FromResult$6)";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        pattern = @"\.Get<(.+?)>\(\)\.Setup\((\w+) => \2(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @".Get<$1>()$3";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        pattern = @"\.Get<(.+?)>\(\)\.SetupSequence?\((\w+) => \3(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @".Get<$1>()$3";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.SetupSequence?\((\w+) => \3(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @"$1$4";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        pattern = @"\.Get<(.+?)>\(\)\.SetupSequence?\((\w+) => \2(\.?.+?)(\)(?!\)))";
        replacement = @".Get<$1>()$3";
        replacedContent = replacedContent.RegexReplace(pattern, replacement);
        
        return replacedContent;
    }
    
    
}