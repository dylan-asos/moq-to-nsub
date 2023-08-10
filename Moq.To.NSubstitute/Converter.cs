using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace Moq.To.NSubstitute;

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
        
        return Task.CompletedTask;
    }    
    
    static string ReplaceUsingStatements(string content)
    {
        var pattern = @"using\s+Moq;";
        var replacement = "using NSubstitute;";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceDotObject(string content)
    {
        var pattern = @".Object";
        var replacement = "";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceMockCreation(string content)
    {
        var pattern = @"new\s+Mock<(.+?)>\((.*?)\)";
        var replacement = "Substitute.For<$1>($2)";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceInstanceData(string content)
    {
        var pattern = @"\bMock<(.+?)>";
        var replacement = "$1";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceVerifies(string content)
    {
        var pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?), Times\.(Once(\(\))?|Exactly\((?<times>\d+)\))\)";
        var replacement = "$1.Received(${times})$3";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?)";
        replacement = "$1.Received()$3";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);        
        
        pattern = @"(?<!\.)\b(\w+)\.Verify\((\w+) => \2(.+?), Times\.Never\)";
        replacement = "$1.DidNotReceive()$3";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        return replacedContent;
    }
    
    static string ReplaceThrows(string content)
    {
        var pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup\(((\w+) => \4(\..?.+?)\))\)\s*\n*\.Throws";
        var replacement = "$1.When($3).Throw";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceArgs(string content)
    {
        var pattern = @"It.IsAny";
        var replacement = "Arg.Any";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        pattern = @"It.Is";
        replacement = "Arg.Is";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        return replacedContent;
    }
    
    static string ReplaceMockingKernel(string content)
    {
        var pattern = @"MoqMockingKernel";
        var replacement = "NSubstituteMockingKernel";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceMockingKernelUsing(string content)
    {
        var pattern = @"using\s+Ninject.MockingKernel.Moq;";
        var replacement = "using Ninject.MockingKernel.NSubstitute;";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
    
    static string ReplaceMockingKernelGetMock(string content)
    {
        var pattern = @"\.GetMock<(.+?)>\(\)";
        var replacement = ".Get<(.+?)>()";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }

    static string ReplaceSetup(string content)
    {
        var pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup(Get)?\((\w+) => \4(\.?.+?)\)(?=\.R|\s\n)";
        var replacement = "$1$5";
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.Setup(Get)?\((\w+) => \4(\.?.+?)(?=\.R|\s\n).ReturnsAsync(..?.+?\))";
        replacement = "$1$5.Returns(Task.FromResult$6)";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        pattern = @"\.Get<(.+?)>\(\)\.Setup\((\w+) => \2(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @".Get<$1>()$3";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        pattern = @"\.Get<(.+?)>\(\)\.SetupSequence?\((\w+) => \3(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @".Get<$1>()$3";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        pattern = @"(?<!\.)\b(\w+)(\s\n\s*)?\.SetupSequence?\((\w+) => \3(\.?.+?)\)(?=\.R|\s\n)";
        replacement = @"$1$4";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        pattern = @"\.Get<(.+?)>\(\)\.SetupSequence?\((\w+) => \2(\.?.+?)(\)(?!\)))";
        replacement = @".Get<$1>()$3";
        replacedContent = Regex.Replace(replacedContent, pattern, replacement, RegexOptions.Singleline);
        
        return replacedContent;
    }
}